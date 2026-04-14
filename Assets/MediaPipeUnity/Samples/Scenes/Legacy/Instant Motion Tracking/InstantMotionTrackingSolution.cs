// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.InstantMotionTracking
{
  /// <summary>
  ///   Simple Instant Motion Tracking demo.
  ///
  ///   Tap / click on the camera feed to drop an AR cube.
  ///   The graph tracks it across frames using MediaPipe region tracking.
  ///   Each cube is repositioned every frame based on the tracked anchor data.
  /// </summary>
  public class InstantMotionTrackingSolution : LegacySolutionRunner<InstantMotionTrackingGraph>
  {
    [Tooltip("Prefab spawned at each tracked anchor position (falls back to a plain cube).")]
    [SerializeField] private GameObject _stickerPrefab;

    [Tooltip("Distance in front of the camera (world units) where stickers are placed.")]
    [SerializeField] private float _stickerDepth = 5f;

    // ── Internal state ────────────────────────────────────────────────────────

    private Experimental.TextureFramePool _textureFramePool;

    /// <summary>All anchors the user has placed; sent to the graph every frame.</summary>
    private readonly List<StickerAnchor> _anchors = new List<StickerAnchor>();

    /// <summary>GameObjects representing each sticker, keyed by sticker_id.</summary>
    private readonly Dictionary<int, GameObject> _stickerObjects = new Dictionary<int, GameObject>();

    private int _nextStickerId = 0;

    // ── LegacySolutionRunner ──────────────────────────────────────────────────

    public override void Stop()
    {
      base.Stop();

      _textureFramePool?.Dispose();
      _textureFramePool = null;

      foreach (var go in _stickerObjects.Values)
      {
        if (go != null)
        {
          Destroy(go);
        }
      }
      _stickerObjects.Clear();
      _anchors.Clear();
    }

    protected override IEnumerator Run()
    {
      var graphInitRequest = graphRunner.WaitForInit(runningMode);
      var imageSource = ImageSourceProvider.ImageSource;

      yield return imageSource.Play();

      if (!imageSource.isPrepared)
      {
        Debug.LogError("Failed to start ImageSource, exiting...");
        yield break;
      }

      _textureFramePool = new Experimental.TextureFramePool(
        imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      screen.Initialize(imageSource);

      yield return graphInitRequest;
      if (graphInitRequest.isError)
      {
        Debug.LogError(graphInitRequest.error);
        yield break;
      }

      graphRunner.StartRun(imageSource);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);

      // Sentinel for the current frame: -1 = nothing changed.
      var frameSentinel = -1;

      while (true)
      {
        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        // ── Handle tap / click to place a new sticker ────────────────────────
        if (Input.GetMouseButtonDown(0))
        {
          var tapPos = Input.mousePosition;
          var anchor = new StickerAnchor
          {
            x = tapPos.x / UnityEngine.Screen.width,
            y = 1f - (tapPos.y / UnityEngine.Screen.height),  // flip Y: MediaPipe is top-down
            z = 1f,
            stickerId = _nextStickerId++
          };

          _anchors.Add(anchor);
          frameSentinel = anchor.stickerId;

          // Spawn the sticker GameObject
          var go = _stickerPrefab != null
            ? Instantiate(_stickerPrefab)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);
          go.transform.localScale = Vector3.one * 0.3f;
          _stickerObjects[anchor.stickerId] = go;
        }

        // ── Feed the camera frame into the graph ─────────────────────────────
        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), false, imageSource.isVerticallyFlipped);
        yield return waitUntilReqDone;

        if (req.hasError)
        {
          Debug.LogWarning("Failed to read texture from image source");
          yield return new WaitForEndOfFrame();
          continue;
        }

        graphRunner.AddTextureFrameToInputStream(textureFrame);
        graphRunner.SendAnchorInputs(frameSentinel, _anchors.ToArray());
        frameSentinel = -1;

        // ── Read results (synchronous mode) ──────────────────────────────────
        screen.ReadSync(textureFrame);

        var task = graphRunner.WaitNextAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result != null)
        {
          UpdateStickerPositions(task.Result);
        }
      }
    }

    // ── Sticker positioning ───────────────────────────────────────────────────

    /// <summary>
    ///   Re-position each sticker GameObject according to the tracked anchor data.
    ///   Anchor x/y are normalised [0,1] image coords; they are projected onto the
    ///   main camera frustum at <see cref="_stickerDepth"/>.
    /// </summary>
    private void UpdateStickerPositions(List<StickerAnchor> trackedAnchors)
    {
      var cam = Camera.main;
      if (cam == null)
      {
        return;
      }

      foreach (var anchor in trackedAnchors)
      {
        if (!_stickerObjects.TryGetValue(anchor.stickerId, out var go) || go == null)
        {
          continue;
        }

        // Viewport point: x/y normalised, z = distance from camera
        var viewportPoint = new Vector3(anchor.x, 1f - anchor.y, _stickerDepth);
        go.transform.position = cam.ViewportToWorldPoint(viewportPoint);

        // anchor.z is a scale factor centred around 1.0
        go.transform.localScale = Vector3.one * (0.3f * anchor.z);
      }
    }
  }
}

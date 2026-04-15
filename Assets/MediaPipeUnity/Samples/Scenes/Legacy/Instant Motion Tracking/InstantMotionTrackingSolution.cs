// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using Mediapipe.Unity.CoordinateSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.Sample.Holistic
{
  public class InstantMotionTrackingSolution : LegacySolutionRunner<InstantMotionTrackingGraph>
  {
    // [SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;

    private Experimental.TextureFramePool _textureFramePool;

    public override void Stop()
    {
      base.Stop();
      _textureFramePool?.Dispose();
      _textureFramePool = null;
    }
    
    private void Update()
    {
      if (Input.GetMouseButtonDown(0))
      {
        var rectTransform = screen.GetComponent<RectTransform>();

        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
        {
          if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, Camera.main, out var localPoint))
          {
            var isMirrored = ImageSourceProvider.ImageSource.isFrontFacing ^ ImageSourceProvider.ImageSource.isHorizontallyFlipped;
            var normalizedPoint = rectTransform.rect.PointToImageNormalized(localPoint, graphRunner.rotation, isMirrored);
            graphRunner.ResetAnchor(normalizedPoint.x, normalizedPoint.y);
            // _trackedAnchorDataAnnotationController.ResetAnchor();
          }
        }
      }
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

      // Use RGBA32 as the input format.
      // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so the following code must be fixed.
      _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight, TextureFormat.RGBA32, 10);

      // NOTE: The screen will be resized later, keeping the aspect ratio.
      screen.Initialize(imageSource);
      
      yield return graphInitRequest;
      if (graphInitRequest.isError)
      {
        Debug.LogError(graphInitRequest.error);
        yield break;
      }

      if (!runningMode.IsSynchronous())
      {
         graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
      }

      /*
      SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
      _segmentationMaskAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
*/
      graphRunner.ResetAnchor();
      graphRunner.StartRun(imageSource);

      AsyncGPUReadbackRequest req = default;
      var waitUntilReqDone = new WaitUntil(() => req.done);

      // NOTE: we can share the GL context of the render thread with MediaPipe (for now, only on Android)
      var canUseGpuImage = graphRunner.configType == GraphRunner.ConfigType.OpenGLES && GpuManager.GpuResources != null;
      using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

      while (true)
      {
        if (isPaused)
        {
          yield return new WaitWhile(() => isPaused);
        }

        if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
        {
          yield return new WaitForEndOfFrame();
          continue;
        }

        // Copy current image to TextureFrame
        if (canUseGpuImage)
        {
          yield return new WaitForEndOfFrame();
          textureFrame.ReadTextureOnGPU(imageSource.GetCurrentTexture());
        }
        else
        {
          req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), false, imageSource.isVerticallyFlipped);
          yield return waitUntilReqDone;

          if (req.hasError)
          {
            Debug.LogWarning($"Failed to read texture from the image source");
            yield return new WaitForEndOfFrame();
            continue;
          }
        }

        graphRunner.AddTextureFrameToInputStream(textureFrame, glContext);

        if (runningMode.IsSynchronous())
        {
          screen.ReadSync(textureFrame);

          var task = graphRunner.WaitNextAsync();
          yield return new WaitUntil(() => task.IsCompleted);

          var result = task.Result;
          // _poseDetectionAnnotationController.DrawNow(result.poseDetection);
        }
      }
      
    }
    private void OnPoseDetectionOutput(object stream, OutputStream<List<Anchor3d>>.OutputEventArgs eventArgs)
    {
      Debug.Log("hi");
      /*
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(Detection.Parser);
      _poseDetectionAnnotationController.DrawLater(value);
      */
    }
  }
}

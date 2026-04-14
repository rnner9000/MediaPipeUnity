// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mediapipe.Unity.Sample.InstantMotionTracking
{
  public class InstantMotionTrackingGraph : GraphRunner
  {
    // ── Input stream names ──────────────────────────────────────────────────
    private const string _VideoStreamName = "input_video";
    private const string _SentinelStreamName = "sticker_sentinel";
    private const string _InitialAnchorStreamName = "initial_anchor_data";

    // ── Output stream name ──────────────────────────────────────────────────
    private const string _TrackedAnchorStreamName = "tracked_anchor_data";

    private OutputStream<List<StickerAnchor>> _trackedAnchorStream;

    // ── Output event ────────────────────────────────────────────────────────
    public event EventHandler<OutputStream<List<StickerAnchor>>.OutputEventArgs> OnTrackedAnchorsOutput
    {
      add => _trackedAnchorStream.AddListener(value, timeoutMicrosec);
      remove => _trackedAnchorStream.RemoveListener(value);
    }

    // ── GraphRunner overrides ───────────────────────────────────────────────

    public override void StartRun(ImageSource imageSource)
    {
      if (runningMode.IsSynchronous())
      {
        _trackedAnchorStream.StartPolling();
      }
      StartRun(BuildSidePacket(imageSource));
    }

    public override void Stop()
    {
      base.Stop();
      _trackedAnchorStream?.Dispose();
      _trackedAnchorStream = null;
    }

    protected override void ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      _trackedAnchorStream = new OutputStream<List<StickerAnchor>>(calculatorGraph, _TrackedAnchorStreamName, true);
      calculatorGraph.Initialize(config);
    }

    protected override IList<WaitForResult> RequestDependentAssets() => new List<WaitForResult>();

    // ── Input helpers ────────────────────────────────────────────────────────

    /// <summary>Send the camera frame into the graph.</summary>
    public void AddTextureFrameToInputStream(Experimental.TextureFrame textureFrame)
    {
      AddTextureFrameToInputStream(_VideoStreamName, textureFrame);
    }

    /// <summary>
    ///   Send the sticker sentinel and the full list of current anchors.
    ///   Call once per frame after <see cref="AddTextureFrameToInputStream"/>.
    /// </summary>
    /// <param name="sentinelStickerId">
    ///   ID of the sticker whose anchor was just changed/added, or -1 when nothing changed.
    /// </param>
    /// <param name="anchors">All currently active anchors.</param>
    public void SendAnchorInputs(int sentinelStickerId, StickerAnchor[] anchors)
    {
      AddPacketToInputStream(_SentinelStreamName, Packet.CreateIntAt(sentinelStickerId, latestTimestamp));
      AddPacketToInputStream(_InitialAnchorStreamName, PacketAnchorExtension.CreateAnchorVectorAt(anchors, latestTimestamp));
    }

    // ── Output helpers ───────────────────────────────────────────────────────

    /// <summary>Wait for the next result (synchronous running mode only).</summary>
    public async Task<List<StickerAnchor>> WaitNextAsync()
    {
      var result = await _trackedAnchorStream.WaitNextAsync();
      AssertResult(result);

      _ = TryGetValue(result.packet, out var trackedAnchors, (packet) => packet.Get());
      return trackedAnchors;
    }

    // ── Side packets ─────────────────────────────────────────────────────────

    private PacketMap BuildSidePacket(ImageSource imageSource)
    {
      var sidePacket = new PacketMap();
      SetImageTransformationOptions(sidePacket, imageSource);
      return sidePacket;
    }
  }
}

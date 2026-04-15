// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using Google.Protobuf;
using System.Threading.Tasks;

namespace Mediapipe.Unity.Sample.Holistic
{
  public readonly struct InstantMotionTrackingResult
  {
    /*
    public readonly Detection poseDetection;
    */

    public InstantMotionTrackingResult(Detection poseDetection, NormalizedLandmarkList poseLandmarks, NormalizedLandmarkList faceLandmarks, NormalizedLandmarkList leftHandLandmarks,
                                  NormalizedLandmarkList rightHandLandmarks, LandmarkList poseWorldLandmarks, ImageFrame segmentationMask, NormalizedRect poseRoi)
    {
      // this.poseDetection = poseDetection;
    }
  }

  public class InstantMotionTrackingGraph : GraphRunner
  {
    public event EventHandler<OutputStream<List<StickerAnchor>>.OutputEventArgs> OnPoseDetectionOutput
    {
      add => _trackedAnchorDataStream.AddListener(value, timeoutMicrosec);
      remove => _trackedAnchorDataStream.RemoveListener(value);
    }
    
    private const string _InputStreamName = "input_video";
    private const string _StickerSentinelStreamName = "sticker_sentinel";
    private const string _InitialAnchorDataStreamName = "initial_anchor_data";
    private const string _TrackedAnchorDataStreamName = "tracked_anchor_data";

    private OutputStream<List<StickerAnchor>> _trackedAnchorDataStream;
    
    private readonly Anchor3d[] _anchors = new Anchor3d[1];

    public override void StartRun(ImageSource imageSource)
    {
      _trackedAnchorDataStream.StartPolling();
      StartRun(BuildSidePacket(imageSource));
    }

    public override void Stop()
    {
      base.Stop();
      _trackedAnchorDataStream?.Dispose();
      _trackedAnchorDataStream = null;
    }

    private bool _isTracking;
    private int _currentStickerSentinelId = -1;

    public void ResetAnchor(float normalizedX = 0.5f, float normalizedY = 0.5f)
    {
      _anchors[0].stickerId = ++_currentStickerSentinelId;
      _isTracking = false;
      _anchors[0].x = normalizedX;
      _anchors[0].y = normalizedY;
      Logger.LogInfo(TAG, $"New anchor = {_anchors[0]}");
    }
    
    public void AddTextureFrameToInputStream(Experimental.TextureFrame textureFrame, GlContext glContext = null)
    {
      AddTextureFrameToInputStream(_InputStreamName, textureFrame, glContext);
      
      var stickerSentinelId = _isTracking ? -1 : _currentStickerSentinelId;
      AddPacketToInputStream(_StickerSentinelStreamName, Packet.CreateIntAt(stickerSentinelId, latestTimestamp));

      _isTracking = true;
      AddPacketToInputStream(_InitialAnchorDataStreamName, PacketAnchorExtension.CreateAnchorVectorAt(_anchors, latestTimestamp));
    }

    public async Task<List<StickerAnchor>> WaitNextAsync()
    {
      var results = await _trackedAnchorDataStream.WaitNextAsync();
      AssertResult(results);

      _ = TryGetValue(results.packet, out var anchors, (packet) =>
      {
        return PacketAnchorExtension.Get(packet);
      });

      return anchors;
    }

    protected override IList<WaitForResult> RequestDependentAssets()
    {
      return new List<WaitForResult> {
        WaitForAsset("ssdlite_object_detection.bytes"),
      };
    }

    protected override void ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      _trackedAnchorDataStream = new OutputStream<List<StickerAnchor>>(calculatorGraph, _TrackedAnchorDataStreamName, true);
      calculatorGraph.Initialize(config);
    }
    
    private PacketMap BuildSidePacket(ImageSource imageSource)
    {
      var sidePacket = new PacketMap();
      SetImageTransformationOptions(sidePacket, imageSource);
      return sidePacket;
    }
  }
}

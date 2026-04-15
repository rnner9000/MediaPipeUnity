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
    /*
    public event EventHandler<OutputStream<Detection>.OutputEventArgs> OnPoseDetectionOutput
    {
      add => _poseDetectionStream.AddListener(value, timeoutMicrosec);
      remove => _poseDetectionStream.RemoveListener(value);
    }
    */
    
    private const string _InputStreamName = "input_video";
    // private const string _PoseDetectionStreamName = "pose_detection";

    // private OutputStream<Detection> _poseDetectionStream;

    public override void StartRun(ImageSource imageSource)
    {
      if (runningMode.IsSynchronous())
      {
        // _poseDetectionStream.StartPolling();
      }
      // StartRun(BuildSidePacket(imageSource));
    }

    public override void Stop()
    {
      base.Stop();
      /*
      _poseDetectionStream?.Dispose();
      _poseDetectionStream = null;
      */
    }

    public void AddTextureFrameToInputStream(Experimental.TextureFrame textureFrame, GlContext glContext = null)
    {
      AddTextureFrameToInputStream(_InputStreamName, textureFrame, glContext);
    }

    public async Task<InstantMotionTrackingResult> WaitNextAsync()
    {
      return new InstantMotionTrackingResult();
      /*
      var results = await WhenAll(
        _poseDetectionStream.WaitNextAsync(),
      );
      AssertResult(results);

      _ = TryGetValue(results.Item1.packet, out var poseDetection, (packet) =>
      {
        return packet.Get(Detection.Parser);
      });
      _ = TryGetValue(results.Item2.packet, out var poseLandmarks, (packet) =>
      {
        return packet.Get(NormalizedLandmarkList.Parser);
      });
      _ = TryGetValue(results.Item3.packet, out var faceLandmarks, (packet) =>
      {
        return packet.Get(NormalizedLandmarkList.Parser);
      });
      _ = TryGetValue(results.Item4.packet, out var leftHandLandmarks, (packet) =>
      {
        return packet.Get(NormalizedLandmarkList.Parser);
      });
      _ = TryGetValue(results.Item5.packet, out var rightHandLandmarks, (packet) =>
      {
        return packet.Get(NormalizedLandmarkList.Parser);
      });
      _ = TryGetValue(results.Item6.packet, out var poseWorldLandmarks, (packet) =>
      {
        return packet.Get(LandmarkList.Parser);
      });
      _ = TryGetValue(results.Item7.packet, out var segmentationMask, (packet) =>
      {
        return packet.Get();
      });
      _ = TryGetValue(results.Item8.packet, out var poseRoi, (packet) =>
      {
        return packet.Get(NormalizedRect.Parser);
      });

      return new HolisticTrackingResult(poseDetection, poseLandmarks, faceLandmarks, leftHandLandmarks, rightHandLandmarks, poseWorldLandmarks, segmentationMask, poseRoi);
      */
    }

    protected override IList<WaitForResult> RequestDependentAssets()
    {
      return new List<WaitForResult> {
        WaitForAsset("pose_detection.bytes"),
      };
    }

    protected override void ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      // _poseDetectionStream = new OutputStream<Detection>(calculatorGraph, _PoseDetectionStreamName, true);
      
      using (var validatedGraphConfig = new ValidatedGraphConfig())
      {
        validatedGraphConfig.Initialize(config);

        var extensionRegistry = new ExtensionRegistry() { TensorsToDetectionsCalculatorOptions.Extensions.Ext, ThresholdingCalculatorOptions.Extensions.Ext };
        var cannonicalizedConfig = validatedGraphConfig.Config(extensionRegistry);

        var poseDetectionCalculatorPattern = new Regex("__posedetection[a-z]+__TensorsToDetectionsCalculator$");
        var poseTrackingCalculatorPattern = new Regex("tensorstoposelandmarksandsegmentation__ThresholdingCalculator$");
        
        /*
        foreach (var calculator in tensorsToDetectionsCalculators)
        {
          if (calculator.Options.HasExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext))
          {
            var options = calculator.Options.GetExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext);
            options.MinScoreThresh = minDetectionConfidence;
            Debug.Log($"Min Detection Confidence = {minDetectionConfidence}");
          }
        }

*/
        calculatorGraph.Initialize(cannonicalizedConfig);
      }
    }
/*
    private PacketMap BuildSidePacket(ImageSource imageSource)
    {
      var sidePacket = new PacketMap();

      SetImageTransformationOptions(sidePacket, imageSource);

      // TODO: refactoring
      // The orientation of the output image must match that of the input image.
      var isInverted = CoordinateSystem.ImageCoordinate.IsInverted(imageSource.rotation);
      var outputRotation = imageSource.rotation;
      var outputHorizontallyFlipped = !isInverted && imageSource.isHorizontallyFlipped;
      var outputVerticallyFlipped = (!runningMode.IsSynchronous() && imageSource.isVerticallyFlipped) ^ (isInverted && imageSource.isHorizontallyFlipped);

      if ((outputHorizontallyFlipped && outputVerticallyFlipped) || outputRotation == RotationAngle.Rotation180)
      {
        outputRotation = outputRotation.Add(RotationAngle.Rotation180);
        outputHorizontallyFlipped = !outputHorizontallyFlipped;
        outputVerticallyFlipped = !outputVerticallyFlipped;
      }

      sidePacket.Emplace("output_rotation", Packet.CreateInt((int)outputRotation));
      sidePacket.Emplace("output_horizontally_flipped", Packet.CreateBool(outputHorizontallyFlipped));
      sidePacket.Emplace("output_vertically_flipped", Packet.CreateBool(outputVerticallyFlipped));

      Debug.Log($"outtput_rotation = {outputRotation}, output_horizontally_flipped = {outputHorizontallyFlipped}, output_vertically_flipped = {outputVerticallyFlipped}");

      // sidePacket.Emplace("refine_face_landmarks", Packet.CreateBool(refineFaceLandmarks));

      return sidePacket;
    }
    */
  }
}

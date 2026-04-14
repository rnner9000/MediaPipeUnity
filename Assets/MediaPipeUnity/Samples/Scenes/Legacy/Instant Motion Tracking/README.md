# Instant Motion Tracking Sample

This is a MediaPipe Unity sample that demonstrates **instant motion tracking** - a technique for tracking and visualizing moving objects/regions in video using region-based tracking.

## Overview

The Instant Motion Tracking sample is based on the [MediaPipe Instant Motion Tracking graph](https://github.com/google-ai-edge/mediapipe/blob/master/mediapipe/graphs/instant_motion_tracking/instant_motion_tracking.pbtxt). It showcases:

- **Region-based object tracking**: Automatically detects and tracks rectangular regions in video
- **Real-time tracking**: Runs efficiently using multi-threaded processing
- **Cross-platform support**: Works on CPU, GPU, and OpenGL ES (mobile) configurations
- **Live video processing**: Processes video frames in real-time with minimal latency

## Architecture

### Core Components

#### 1. **InstantMotionTrackingGraph.cs**
Extends `GraphRunner` and manages the MediaPipe calculator graph lifecycle:
- Initializes the graph with appropriate configuration files (CPU/GPU/OpenGL ES)
- Handles input/output stream management
- Manages texture frames for efficient GPU processing
- Supports both CPU and GPU-accelerated pipelines

#### 2. **InstantMotionTrackingSolution.cs**
Extends `LegacySolutionRunner<InstantMotionTrackingGraph>` and implements the main solution logic:
- Manages the game loop and coroutine-based execution
- Handles image source playback and frame capture
- Manages texture frame pooling for efficient memory usage
- Processes video frames through the graph
- Handles both CPU and GPU rendering pathways

#### 3. **InstantMotionTrackingConfig.cs**
Extends `ModalContents` for UI configuration:
- Provides a configuration UI panel
- Allows runtime switching between running modes (Sync/Async)
- Configurable timeout for frame processing

### Graph Configurations

Three graph configuration files are provided for different runtime environments:

#### **instant_motion_tracking_cpu.txt**
CPU-based processing:
- Uses ImageFrame for image input/output
- Employs CPU-based Region Tracking Subgraph
- Suitable for development and platforms without GPU acceleration
- Good for debugging and understanding the pipeline

#### **instant_motion_tracking_gpu.txt**
GPU-accelerated processing:
- Uses GpuBuffer (GPU texture) for image input/output
- Reduces CPU-GPU memory transfers
- Faster rendering with GL-based annotation overlays
- Recommended for production on capable devices

#### **instant_motion_tracking_opengles.txt**
Mobile OpenGL ES optimized:
- Optimized for mobile platforms (iOS, Android)
- Uses OpenGL ES for efficient rendering
- Balanced performance and quality
- Recommended for mobile deployment

## Graph Pipeline

The graph follows this processing pipeline:

```
input_video
    ↓
[FlowLimiter]  ← Throttles input to maintain pipeline throughput
    ↓
[ImageTransformation]  ← Applies rotation/flip transformations
    ↓
[RegionTracking]  ← Detects and tracks regions using box tracking
    ↓
[AnnotationOverlay]  ← Visualizes tracked regions on image
    ↓
[ImageTransformation]  ← Applies output transformations
    ↓
output_video
```

## Key Features

### 1. **Region Tracking**
The `RegionTrackingSubgraph` automatically:
- Detects motion in the video
- Initializes tracking anchors for detected regions
- Maintains tracking state across frames
- Handles occlusion and tracking loss

### 2. **Real-time Visualization**
The pipeline renders:
- Bounding boxes around tracked regions
- Region IDs and confidence scores
- Tracking stability indicators
- Smooth visual feedback for tracking quality

### 3. **Platform Support**
- **Desktop (Windows, macOS, Linux)**: CPU and GPU modes
- **Mobile (iOS, Android)**: OpenGL ES optimized
- **Web (Emscripten)**: WebGL-based rendering

## Usage

### In Unity Scene

1. **Add InstantMotionTrackingSolution component** to a GameObject
2. **Attach ImageSourceProvider** to provide video input:
   - Webcam input
   - Video file playback
   - Image sequences
3. **Attach a screen renderer** to display results:
   - Raw Image component for UI display
   - Canvas setup

### Configuration

The solution automatically selects the appropriate graph configuration based on:
- Platform (Windows, macOS, iOS, Android, etc.)
- Available GPU resources
- Project build settings

Runtime configuration can be adjusted via the UI panel:
- **Running Mode**: Switch between Sync (single-threaded) and Async (multi-threaded)
- **Timeout**: Frame processing timeout in milliseconds

## Input/Output Specification

### Inputs
- **input_video** (ImageFrame or GpuBuffer)
  - Color space: RGB or RGBA
  - Dimensions: Any resolution (will be scaled as needed)
  - Frame rate: 30 FPS or higher recommended

### Outputs
- **output_video** (ImageFrame or GpuBuffer)
  - Color space: RGBA
  - Dimensions: Same as input
  - Contains: Original image + tracked region visualization

## Required Assets

The sample automatically loads the following MediaPipe assets:
- `anchor_vertices.bytes` - Anchor vertex data
- `box_trackers_vertices.bytes` - Tracker vertex data
- `box_trackers_triangles.bytes` - Tracker triangle data

These assets must be included in the project's MediaPipe asset bundle.

## Performance Considerations

### CPU Mode
- Suitable for development and lower-end devices
- Processing time: 30-100ms per frame (depending on resolution)
- Memory usage: ~50-100MB

### GPU Mode
- 2-4x faster than CPU mode
- Processing time: 15-30ms per frame
- Requires GPU memory: ~100-200MB
- Recommended for desktop applications

### OpenGL ES Mode
- Optimized for mobile platforms
- Processing time: 20-50ms per frame (mobile)
- Lower memory footprint than GPU
- Recommended for production mobile apps

## Troubleshooting

### Issue: Tracking is unstable or jittery
- **Solution**: Ensure good lighting and stable camera input
- Increase timeout value in configuration if frames are being dropped

### Issue: High CPU usage on CPU mode
- **Solution**: Switch to GPU mode if available, or reduce input resolution

### Issue: Assets not found error
- **Solution**: Ensure MediaPipe assets are properly imported in the project

## Comparison with MediaPipe Video Sample

| Feature | MediaPipe Video | Instant Motion Tracking |
|---------|-----------------|------------------------|
| Purpose | Hand detection & tracking | Region-based object tracking |
| Input | Hand landmarks | Motion/Region detection |
| Output | Hand pose visualization | Tracked region bounding boxes |
| Complexity | Complex hand model | Simpler box tracking |
| Performance | Slower (hand analysis) | Faster (region tracking) |
| Use Cases | Gesture recognition | Physical object tracking |

## References

- [MediaPipe Instant Motion Tracking Graph](https://github.com/google-ai-edge/mediapipe/blob/master/mediapipe/graphs/instant_motion_tracking/instant_motion_tracking.pbtxt)
- [MediaPipe Documentation](https://developers.google.com/mediapipe)
- [Box Tracking Calculator](https://github.com/google-ai-edge/mediapipe/tree/master/mediapipe/graphs/tracking)

## License

Copyright © 2021 homuler

Licensed under the MIT License. See LICENSE file for details.

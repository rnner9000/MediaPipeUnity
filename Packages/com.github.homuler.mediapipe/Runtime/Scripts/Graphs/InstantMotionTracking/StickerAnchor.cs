// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Runtime.InteropServices;

namespace Mediapipe
{
  /// <summary>
  ///   Mirrors the <c>mediapipe::Anchor</c> C++ struct used by the Instant Motion Tracking graph.
  ///   Represents the normalised screen-space anchor for a tracked AR sticker.
  ///   Named <c>StickerAnchor</c> to avoid collision with the proto-generated <c>Mediapipe.Anchor</c>.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct StickerAnchor
  {
    /// <summary>Normalized horizontal position [0.0 – 1.0].</summary>
    public float x;
    /// <summary>Normalized vertical position [0.0 – 1.0].</summary>
    public float y;
    /// <summary>Scale factor centered around 1.0 (current_scale = z * initial_scale).</summary>
    public float z;
    /// <summary>Unique identifier for the sticker this anchor belongs to.</summary>
    public int stickerId;
  }
}

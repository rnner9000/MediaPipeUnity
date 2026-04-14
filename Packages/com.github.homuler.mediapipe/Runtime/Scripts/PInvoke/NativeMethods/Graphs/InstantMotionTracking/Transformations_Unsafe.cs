// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Runtime.InteropServices;

namespace Mediapipe
{
  internal static partial class UnsafeNativeMethods
  {
    #region InstantMotionTracking – Anchor packets

    [DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern MpReturnCode mp__MakeAnchor3dVectorPacket__PA_i(
      [In] StickerAnchor[] value, int size, out IntPtr packet);

    [DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern MpReturnCode mp__MakeAnchor3dVectorPacket_At__PA_i_Rt(
      [In] StickerAnchor[] value, int size, IntPtr timestamp, out IntPtr packet);

    [DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern MpReturnCode mp_Packet__GetAnchor3dVector(
      IntPtr packet, out StructArray<StickerAnchor> value);

    [DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern void mp_Anchor3dArray__delete(IntPtr anchorArrayData);

    #endregion
  }
}

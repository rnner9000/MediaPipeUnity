// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;

namespace Mediapipe
{
  public static class PacketAnchorExtension
  {
    /// <summary>Create a packet containing a vector of <see cref="StickerAnchor"/>.</summary>
    public static Packet<List<StickerAnchor>> CreateAnchorVector(Anchor3d[] value)
    {
      UnsafeNativeMethods.mp__MakeAnchor3dVectorPacket__PA_i(value, value.Length, out var ptr).Assert();
      return new Packet<List<StickerAnchor>>(ptr, true);
    }

    /// <summary>
    ///   Create a timestamped packet containing a vector of <see cref="StickerAnchor"/>.
    /// </summary>
    public static Packet<List<StickerAnchor>> CreateAnchorVectorAt(Anchor3d[] value, long timestampMicrosec)
    {
      using var timestamp = new Timestamp(timestampMicrosec);
      UnsafeNativeMethods.mp__MakeAnchor3dVectorPacket_At__PA_i_Rt(value, value.Length, timestamp.mpPtr, out var ptr).Assert();
      GC.KeepAlive(timestamp);
      return new Packet<List<StickerAnchor>>(ptr, true);
    }

    /// <summary>
    ///   Get the content of the <see cref="Packet"/> as a <see cref="List{StickerAnchor}"/>.
    /// </summary>
    public static List<StickerAnchor> Get(this Packet<List<StickerAnchor>> packet)
    {
      var value = new List<StickerAnchor>();
      Get(packet, value);
      return value;
    }

    /// <summary>
    ///   Fill <paramref name="value"/> with the <see cref="StickerAnchor"/> vector stored in <paramref name="packet"/>.
    /// </summary>
    public static void Get(this Packet<List<StickerAnchor>> packet, List<StickerAnchor> value)
    {
      UnsafeNativeMethods.mp_Packet__GetAnchor3dVector(packet.mpPtr, out var structArray).Assert();
      GC.KeepAlive(packet);
      structArray.CopyTo(value);
      structArray.Dispose();
    }
  }
}

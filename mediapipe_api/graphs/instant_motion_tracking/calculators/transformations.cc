// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

#include "mediapipe_api/graphs/instant_motion_tracking/calculators/transformations.h"

#include <vector>

MpReturnCode mp__MakeAnchor3dVectorPacket__PA_i(const mediapipe::Anchor* value, int size, mediapipe::Packet** packet_out) {
  TRY
    std::vector<mediapipe::Anchor> vector{};
    for (auto i = 0; i < size; ++i) {
      vector.push_back(value[i]);
    }
    *packet_out = new mediapipe::Packet{mediapipe::MakePacket<std::vector<mediapipe::Anchor>>(vector)};
    RETURN_CODE(MpReturnCode::Success);
  CATCH_EXCEPTION
}

MpReturnCode mp__MakeAnchor3dVectorPacket_At__PA_i_Rt(const mediapipe::Anchor* value, int size, mediapipe::Timestamp* timestamp,
                                                      mediapipe::Packet** packet_out) {
  TRY
    std::vector<mediapipe::Anchor> vector{};
    for (auto i = 0; i < size; ++i) {
      vector.push_back(value[i]);
    }
    *packet_out = new mediapipe::Packet{mediapipe::MakePacket<std::vector<mediapipe::Anchor>>(vector).At(*timestamp)};
    RETURN_CODE(MpReturnCode::Success);
  CATCH_EXCEPTION
}

MpReturnCode mp_Packet__GetAnchor3dVector(mediapipe::Packet* packet, mp_api::StructArray<mediapipe::Anchor>* value_out) {
  return mp_Packet__GetStructVector<mediapipe::Anchor>(packet, value_out);
}

void mp_Anchor3dArray__delete(mediapipe::Anchor* anchor_vector_data) { delete[] anchor_vector_data; }

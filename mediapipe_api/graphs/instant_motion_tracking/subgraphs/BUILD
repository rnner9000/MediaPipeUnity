# Copyright (c) 2021 homuler
#
# Use of this source code is governed by an MIT-style
# license that can be found in the LICENSE file or at
# https://opensource.org/licenses/MIT.

load(
    "@mediapipe//mediapipe/framework/tool:mediapipe_graph.bzl",
    "mediapipe_simple_subgraph",
)

package(default_visibility = ["//visibility:public"])

mediapipe_simple_subgraph(
    name = "region_tracking_cpu",
    graph = "region_tracking_cpu.pbtxt",
    register_as = "RegionTrackingSubgraphCpu",
    deps = [
        "@mediapipe//mediapipe/graphs/instant_motion_tracking/calculators:tracked_anchor_manager_calculator",
        "@mediapipe//mediapipe/graphs/tracking/subgraphs:box_tracking_cpu",
    ],
)

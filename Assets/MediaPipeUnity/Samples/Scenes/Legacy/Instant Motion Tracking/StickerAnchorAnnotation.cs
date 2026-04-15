// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Unity.CoordinateSystem;
using UnityEngine;

namespace Mediapipe.Unity
{
  public class StickerAnchorAnnotation : MonoBehaviour
  {
    public Screen screen;
    
    public void UpdateTracker(StickerAnchor? target, RotationAngle rotationAngle, Vector3 cameraPosition, float defaultDepth)
    {
      var anchor3d = (StickerAnchor)target;
      anchor3d.x = 1f - anchor3d.x;

      // Get the four world-space corners of the screen rect.
      // GetWorldCorners order: [0]=bottom-left, [1]=top-left, [2]=top-right, [3]=bottom-right
      var corners = new Vector3[4];
      screen.GetComponent<RectTransform>().GetWorldCorners(corners);
      var bottomLeft  = corners[0];
      var topLeft     = corners[1];
      var topRight    = corners[2];
      var bottomRight = corners[3];

      // anchor3d.x is [0,1] left→right, anchor3d.y is [0,1] top→bottom.
      // Bilinear interpolation across the screen rect.
      var leftEdge  = Vector3.LerpUnclamped(topLeft,  bottomLeft,  anchor3d.y);
      var rightEdge = Vector3.LerpUnclamped(topRight, bottomRight, anchor3d.y);
      var screenPoint = Vector3.LerpUnclamped(leftEdge, rightEdge, anchor3d.x);

      // Ray from camera through the matched screen point, at the desired depth.
      var cam = Camera.main;
      var direction = (screenPoint - cam.transform.position).normalized;
      transform.position = cam.transform.position + direction * (anchor3d.z * defaultDepth);
    }

    private UnityEngine.Rect GetScreenRect()
    {
      return screen.transform.GetComponent<RectTransform>().rect;
    }
    
    public static Vector2 GetPoint(UnityEngine.Rect rectangle, StickerAnchor anchor3d, RotationAngle imageRotation = RotationAngle.Rotation0, bool isMirrored = false)
    {
      return ImageCoordinate.ImageNormalizedToPoint(rectangle, anchor3d.x, anchor3d.y, imageRotation, isMirrored);
    }
    
    private Vector3 GetAnchorPositionInRay(Vector2 anchorPosition, float anchorDepth, Vector3 cameraPosition)
    {
      if (Mathf.Approximately(cameraPosition.z, 0.0f))
      {
        throw new System.ArgumentException("Z value of the camera position must not be zero");
      }

      var cameraDepth = Mathf.Abs(cameraPosition.z);
      var x = ((anchorPosition.x - cameraPosition.x) * anchorDepth / cameraDepth) + cameraPosition.x;
      var y = ((anchorPosition.y - cameraPosition.y) * anchorDepth / cameraDepth) + cameraPosition.y;
      var z = cameraPosition.z > 0 ? cameraPosition.z - anchorDepth : cameraPosition.z + anchorDepth;
      return new Vector3(x, y, z);
    }
  }
}

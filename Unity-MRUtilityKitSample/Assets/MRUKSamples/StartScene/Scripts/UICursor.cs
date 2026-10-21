// Copyright (c) Meta Platforms, Inc. and affiliates.


using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.StartScene
{
    [MetaCodeSample("MRUKSample-StartScene")]
    public class UICursor : OVRCursor
    {
        private Vector3 _forward;
        private Vector3 _endPoint;
        private Vector3 _normal;
        private bool _hit;

        /// <summary>
        /// Overriding the <see cref="SetCursorRay"/> from <see cref="OVRCursor"/>, setting the cursor ray
        /// direction based on the transform's forward direction.
        /// </summary>
        /// <param name="t"><see cref="Transform"/> used to determine the cursor ray direction</param>
        public override void SetCursorRay(Transform t)
        {
            _forward = t.forward;
            _normal = _forward;
            _hit = false;
        }

        /// <summary>
        /// Overriding the <see cref="SetCursorStartDest"/> from <see cref="OVRCursor"/>, setting the starting point
        /// and the destination point of the cursor.
        /// </summary>
        /// <param name="start"><see cref="Vector3"/> of the starting point position</param>
        /// <param name="dest"><see cref="Vector3"/> of the destination point position</param>
        /// <param name="normal"><see cref="Vector3"/> representing the normal of the cursor</param>
        public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
        {
            _endPoint = dest;
            _normal = normal;
            _hit = true;
        }

        private void LateUpdate()
        {
            if (_hit)
            {
                transform.position = _endPoint;
                transform.rotation = Quaternion.LookRotation(_normal, Vector3.up);
            }
        }
    }
}

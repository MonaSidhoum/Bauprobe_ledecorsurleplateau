// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Serialization;

namespace MRUtilityKitSample.FindFloorZone
{
    public class FishJointDelay : MonoBehaviour
    {
        [FormerlySerializedAs("_target")] public Transform Target;

        [SerializeField] private Vector3 Offset;

        [FormerlySerializedAs("_joint1")] public Transform Joint1; // The first joint (root, the one you move/rotate)
        [FormerlySerializedAs("_joint2")] public Transform Joint2;
        [FormerlySerializedAs("_joint3")] public Transform Joint3;
        [FormerlySerializedAs("_delayTime")] public float DelayTime = 0.9f; // Delay in seconds for each joint
        private Quaternion _joint2DelayedRotation;
        private Quaternion _joint3DelayedRotation;
        private Quaternion _joint2PrevRotation;
        private Quaternion _joint3PrevRotation;

        private void Start()
        {
            if (Joint1 && Joint2 && Joint3)
            {
                return;
            }

            Debug.LogError("Please assign all joint transforms in the Inspector!");
            enabled = false;
        }

        private void Update()
        {
            if (Target)
            {
                Joint1.rotation = Target.rotation * Quaternion.Euler(Offset.x, Offset.y, Offset.z);
                Joint1.position = Target.position;
            }

            _joint2DelayedRotation =
                Quaternion.Slerp(Joint1.rotation, _joint2PrevRotation, DelayTime);
            Joint2.rotation = _joint2DelayedRotation;

            _joint3DelayedRotation =
                Quaternion.Slerp(Joint2.rotation, _joint3PrevRotation, DelayTime);
            Joint3.rotation = _joint3DelayedRotation;

            _joint2PrevRotation = Joint2.rotation;
            _joint3PrevRotation = Joint3.rotation;
        }
    }
}

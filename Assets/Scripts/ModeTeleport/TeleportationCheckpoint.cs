using System.Collections.Generic;
using UnityEngine;


public class TeleportToCheckpointWithoutRayInteractor : MonoBehaviour
{
    [Header("Références")]
    public Transform controllerTransform;
    public LayerMask teleportationMask;
    public List<Transform> teleportCheckpoints;
    public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;

    [Header("Paramètres")]
    public float maxRayDistance = 15f;
    public float detectionRadius = 1.5f;

    void Update()
    {
        if (controllerTransform == null || teleportationProvider == null) return;

        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, teleportationMask))
        {
            Transform closest = null;
            float minDist = Mathf.Infinity;

            foreach (var point in teleportCheckpoints)
            {
                float dist = Vector3.Distance(hit.point, point.position);
                if (dist < detectionRadius && dist < minDist)
                {
                    closest = point;
                    minDist = dist;
                }
            }

            if (closest != null && TryGetUserInput())
            {
                UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest request = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest()
                {
                    destinationPosition = closest.position,
                    matchOrientation = UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.MatchOrientation.WorldSpaceUp
                };
                teleportationProvider.QueueTeleportRequest(request);
            }
        }
    }

    private bool TryGetUserInput()
    {
        return Input.GetKeyDown(KeyCode.T);
    }
}

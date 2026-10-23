using UnityEngine;
using UnityEngine.XR;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class SmartConstrainToPlane : MonoBehaviour
{
    public enum LockedPlane { XZ, XY, YZ }
    public LockedPlane basePlane = LockedPlane.XZ;
    public float fixedValue = 0f;
    public XRNode controllerNode = XRNode.RightHand;

    private Vector3 cachedSnapOffset = Vector3.zero;
    private bool snapOffsetCalculated = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Transform attach;
    private Rigidbody rb;

    private bool lastA, lastB;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grab.attachTransform == null)
        {
            GameObject attachObj = new GameObject("AttachPoint");
            attachObj.transform.SetParent(transform, false);
            grab.attachTransform = attachObj.transform;
        }

        attach = grab.attachTransform;
    }

    private void LateUpdate()
    {
        if (!grab.isSelected) return;
        // Enregistrement offset
        Vector3 offset = GetSnapOffset();

        // Lecture des boutons
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool A = false, B = false;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out A);
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out B);

        // Reset vitesses quand on change d’état
        if ((A != lastA || B != lastB) && rb != null)
        {
            bool wasKinematic = rb.isKinematic;
            rb.isKinematic = false; // autorise la modif
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = wasKinematic; // restaure l’état initial
        }

        lastA = A;
        lastB = B;

        // Aucun bouton : objet figé à sa position actuelle
        if (!A && !B)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }

        // Commence par libérer rotation (on ne bloque que positions)
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 worldPos = attach.position;
        Vector3 objPos = transform.position;

        // --- Gestion claire par switch-case selon plan sélectionné ---
        switch (basePlane)
        {
            case LockedPlane.XZ:
                switch ((A ? 1 : 0) + (B ? 2 : 0))
                {
                    case 1: // A seul → mouvement dans le plan XZ
                        worldPos.y = objPos.y; // garde la hauteur actuelle
                        rb.constraints |= RigidbodyConstraints.FreezePositionY;
                        break;

                    case 2: // B seul → mouvement sur l’axe Y
                        worldPos.x = objPos.x;
                        worldPos.z = objPos.z;
                        rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                        break;

                    case 3: // A+B → snap au sol, tout figé
                        worldPos.x = objPos.x;
                        worldPos.z = objPos.z;
                        worldPos.y = offset.y;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                        break;
                }
                break;

            case LockedPlane.XY:
                switch ((A ? 1 : 0) + (B ? 2 : 0))
                {
                    case 1: // A seul → mouvement dans XY
                        worldPos.z = objPos.z;
                        rb.constraints |= RigidbodyConstraints.FreezePositionZ;
                        break;

                    case 2: // B seul → mouvement sur Z
                        worldPos.x = objPos.x;
                        worldPos.y = objPos.y;
                        rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
                        break;

                    case 3: // A+B → snap sur plan Z=fixedValue
                        worldPos.x = objPos.x;
                        worldPos.y = objPos.y;
                        worldPos.z = offset.z;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                        break;
                }
                break;

            case LockedPlane.YZ:
                switch ((A ? 1 : 0) + (B ? 2 : 0))
                {
                    case 1: // A seul → mouvement dans YZ
                        worldPos.x = objPos.x;
                        rb.constraints |= RigidbodyConstraints.FreezePositionX;
                        break;

                    case 2: // B seul → mouvement sur X
                        worldPos.y = objPos.y;
                        worldPos.z = objPos.z;
                        rb.constraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                        break;

                    case 3: // A+B → snap sur plan X=fixedValue
                        worldPos.x = offset.x;
                        worldPos.y = objPos.y;
                        worldPos.z = objPos.z;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                        break;
                }
                break;
        }

        // Appliquer position modifiée au point d’attache (évite “clignotement”)
        attach.position = worldPos;
    }

    // Permet de calculer l'offset nécessaire, et de le stocker 
    private Vector3 GetSnapOffset()
    {
        if (snapOffsetCalculated) return cachedSnapOffset;

        Renderer[] rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0)
        {
            snapOffsetCalculated = true;
            cachedSnapOffset = Vector3.zero;
            return cachedSnapOffset;
        }

        Bounds totalBounds = rends[0].bounds;
        foreach (Renderer r in rends)
            totalBounds.Encapsulate(r.bounds);

        switch (basePlane)
        {
            case LockedPlane.XZ:
                {
                    float minY = totalBounds.min.y;
                    cachedSnapOffset = new Vector3(0f, fixedValue - minY, 0f);
                    break;
                }

            case LockedPlane.XY:
                {
                    float minZ = totalBounds.min.z;
                    cachedSnapOffset = new Vector3(0f, 0f, fixedValue - minZ);
                    break;
                }

            case LockedPlane.YZ:
                {
                    float minX = totalBounds.min.x;
                    cachedSnapOffset = new Vector3(fixedValue - minX, 0f, 0f);
                    break;
                }
        }

        snapOffsetCalculated = true;
        return cachedSnapOffset;
    }
}

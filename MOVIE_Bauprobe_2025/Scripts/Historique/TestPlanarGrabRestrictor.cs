using UnityEngine;
using UnityEngine.XR;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class TestConstrainToPlane : MonoBehaviour
{
    public enum LockedPlane { XZ, XY, YZ }
    public LockedPlane basePlane = LockedPlane.XZ; // plan de base choisi en amont
    public float fixedValue = 0f; // valeur pour l'axe bloqué si on fixe complètement l'objet

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Rigidbody rb;
    public XRNode controllerNode = XRNode.RightHand;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (grab == null || rb == null) return;

        // Lire l'état du grab
        if (!grab.isSelected)
        {
            // Si l'objet n'est pas saisi → pas de changement
            return;
        }

        // Lire les boutons
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool buttonA = false, buttonB = false;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out buttonA);
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out buttonB);

        // Réinitialiser les contraintes
        rb.constraints = RigidbodyConstraints.None;

        if (!buttonA && !buttonB)
        {
            // Aucun bouton → bloque tout le mouvement (immobile)
            rb.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }

        // Définir les axes selon le plan
        switch (basePlane)
        {
            case LockedPlane.XZ:
                rb.constraints = RigidbodyConstraints.FreezeRotation; // on ne bloque pas Y si on bouge dans XZ
                if (!buttonA && buttonB) rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                if (buttonA && buttonB)
                {
                    rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    rb.position = new Vector3(rb.position.x, fixedValue, rb.position.z);
                }
                if (buttonA && !buttonB) rb.constraints |= RigidbodyConstraints.FreezePositionY;
                break;

            case LockedPlane.XY:
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                if (!buttonA && buttonB) rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
                if (buttonA && buttonB)
                {
                    rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    rb.position = new Vector3(rb.position.x, rb.position.y, fixedValue);
                }
                if (buttonA && !buttonB) rb.constraints |= RigidbodyConstraints.FreezePositionZ;
                break;

            case LockedPlane.YZ:
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                if (!buttonA && buttonB) rb.constraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                if (buttonA && buttonB)
                {
                    rb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    rb.position = new Vector3(fixedValue, rb.position.y, rb.position.z);
                }
                if (buttonA && !buttonB) rb.constraints |= RigidbodyConstraints.FreezePositionX;
                break;
        }
    }
}

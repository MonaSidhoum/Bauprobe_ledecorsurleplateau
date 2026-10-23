using UnityEngine;
using UnityEngine.XR;

public class ScalerController : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayLength = 10f;
    public LayerMask interactableLayer; // couche des objets scalables (à définir dans l’inspecteur)
    public LineRenderer lineRenderer;   // optionnel, pour visualiser le rayon

    [Header("Controller")]
    public XRNode controllerNode = XRNode.RightHand;

    private InputDevice device;
    private Scaling targetedScaler; // objet actuellement visé
    private bool lastAButtonState = false;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
        }
    }

    void Update()
    {
        // Vérifie le contrôleur
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (!device.isValid) return;
        }

        // Raycast depuis le contrôleur
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit, rayLength, interactableLayer);

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * (hitSomething ? hit.distance : rayLength));
        }

        if (hitSomething)
        {
            targetedScaler = hit.collider.GetComponent<Scaling>();
        }
        else
        {
            targetedScaler = null;
        }

        // Lecture des entrées XR
        device.TryGetFeatureValue(CommonUsages.gripButton, out bool isGripPressed);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickAxis);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed); // bouton A

        // Si un objet est visé
        if (targetedScaler != null)
        {
            // Changement de mode
            if (isAPressed && !lastAButtonState)
            {
                //targetedScaler.CycleScaleMode();
            }

            // Application du scaling
            if (isGripPressed)
            {
                float verticalInput = joystickAxis.y;
                if (Mathf.Abs(verticalInput) > 0.1f)
                {
                   // targetedScaler.ApplyScaling(verticalInput);
                }
            }
        }

        lastAButtonState = isAPressed;
    }
}

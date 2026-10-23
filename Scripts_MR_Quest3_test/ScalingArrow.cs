using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ScalingArrow : MonoBehaviour
{
    public enum AxisToScale { Global, X, Z, Y }
    public AxisToScale ScaleAxis = AxisToScale.Global;

    [Header("Paramètres")]
    public XRNode controllerNode = XRNode.RightHand;
    public float scaleSpeed = 1f;
    public float sensitivity = 0.1f;
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    public Vector3 maxScale = new Vector3(5f, 5f, 5f);

    public GameObject arrowPrefab;
    private Rigidbody rb;
    private GameObject currentArrow;
    private Transform interactorTransform;
    private bool lastAButtonState = false;
    private GameObject deplacement;
    private Vector3 pivotWorld;
    private bool isGrabbed = false;

    // MODIF
    private void UpdateArrowDirection()
    {
        if (currentArrow == null)
            return;

        // Réinitialise la position de la flèche à la position du pivot
        currentArrow.transform.position = pivotWorld;

        // Récupère l'orientation de l'objet (les angles de rotation)
        Vector3 eulerRotation = transform.rotation.eulerAngles;

        // Variables pour l'orientation de la flèche
        Vector3 direction = Vector3.zero;

        // Selon l'axe de mise à l'échelle choisi, on calcule la direction de la flèche
        switch (ScaleAxis)
        {
            case AxisToScale.X:
                // La flèche doit pointer selon l'axe X de l'environnement (tenant compte de la rotation de l'objet sur Y)
                direction = Vector3.right;
                break;
            case AxisToScale.Z:
                // La flèche doit pointer selon l'axe Z de l'environnement (tenant compte de la rotation de l'objet sur Y)
                direction = Vector3.forward;
                break;
            case AxisToScale.Y:
                // La flèche doit pointer vers l'axe Y (vertical) de l'environnement
                direction = Vector3.up;
                break;
            case AxisToScale.Global:
                // Ne rien faire en mode global
                currentArrow.SetActive(false);
                return;
        }

        // Active la flèche si elle est en mode X, Y ou Z
        if (!currentArrow.activeSelf)
            currentArrow.SetActive(true);

        // Applique la direction calculée et ajuste la rotation de la flèche
        currentArrow.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        deplacement = GameObject.Find("Locomotion");

        // Empêche tout déplacement quand on grab
        grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;
        grab.trackPosition = false;
        grab.trackRotation = false;

        rb.isKinematic = true;

        grab.selectEntered.AddListener(OnGrabStart);
        grab.selectExited.AddListener(OnGrabEnd);
    }

    private void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabStart);
            grab.selectExited.RemoveListener(OnGrabEnd);
        }
        if (deplacement != null) deplacement.SetActive(true);
    }

    private void OnGrabStart(SelectEnterEventArgs args)
    {
        interactorTransform = args.interactorObject.transform;
        isGrabbed = true;

        // Calcul une seule fois du pivot de scaling (bas du mesh)
        pivotWorld = GetBottomCenter();

        if (deplacement != null) deplacement.SetActive(false);

        // Crée l'instance de la flèche si elle n'existe pas encore
        if (currentArrow == null)
        {
            currentArrow = Instantiate(arrowPrefab);
            currentArrow.SetActive(true);
        }
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        interactorTransform = null;
        isGrabbed = false;

        if (deplacement != null) deplacement.SetActive(true);

        // Désactive la flèche quand on arrête de saisir
        if (currentArrow != null)
            currentArrow.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (!isGrabbed)
            return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool A = false, grip = false;
        Vector2 joystick = Vector2.zero;

        device.TryGetFeatureValue(CommonUsages.primaryButton, out A);
        device.TryGetFeatureValue(CommonUsages.gripButton, out grip);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);

        // Changement d’axe avec A + Grip
        if (A && !lastAButtonState && grip)
        {
            switch (ScaleAxis)
            {
                case AxisToScale.Global: ScaleAxis = AxisToScale.X; break;
                case AxisToScale.X: ScaleAxis = AxisToScale.Z; break;
                case AxisToScale.Z: ScaleAxis = AxisToScale.Y; break;
                case AxisToScale.Y: ScaleAxis = AxisToScale.Global; break;
            }
            // Actualisation de l'orientation de la flèche
            UpdateArrowDirection();
        }
        lastAButtonState = A;

        // Scaling avec joystick vertical + grip
        if (grip && Mathf.Abs(joystick.y) > sensitivity)
        {
            float factor = Mathf.Max(0.0001f, 1f + joystick.y * scaleSpeed * Time.fixedDeltaTime);
            Vector3 newScale = transform.localScale;

            switch (ScaleAxis)
            {
                case AxisToScale.Global:
                    newScale *= factor;
                    break;
                case AxisToScale.X:
                    newScale.x *= factor;
                    break;
                case AxisToScale.Y:
                    newScale.y *= factor;
                    break;
                case AxisToScale.Z:
                    newScale.z *= factor;
                    break;
            }

            newScale = new Vector3(
                Mathf.Clamp(newScale.x, minScale.x, maxScale.x),
                Mathf.Clamp(newScale.y, minScale.y, maxScale.y),
                Mathf.Clamp(newScale.z, minScale.z, maxScale.z)
            );

            ScaleAround(transform, pivotWorld, newScale);

            // Actualisation de l'orientation de la flèche pendant le scaling
            UpdateArrowDirection();
        }
    }

    // Calcul du point le plus bas du mesh (pivot automatique)
    private Vector3 GetBottomCenter()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0)
            return transform.position;

        Vector3 globalMin = Vector3.positiveInfinity;
        List<Vector3> allVertices = new List<Vector3>();

        foreach (Renderer r in rends)
        {
            Mesh mesh = null;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                mesh = mf.sharedMesh;
            else if (r is SkinnedMeshRenderer smr && smr.sharedMesh != null)
                mesh = smr.sharedMesh;

            // Mesh illisible (Read/Write désactivé, cas fréquent sur Quest) : boîte englobante à la place.
            if (mesh == null || !mesh.isReadable)
            {
                Bounds b = r.bounds;
                Vector3 low = new Vector3(b.center.x, b.min.y, b.center.z);
                allVertices.Add(low);
                globalMin = Vector3.Min(globalMin, low);
                continue;
            }

            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 worldV = r.transform.TransformPoint(v);
                allVertices.Add(worldV);
                globalMin = Vector3.Min(globalMin, worldV);
            }
        }

        float minY = globalMin.y;
        const float tolerance = 0.001f;
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Vector3 v in allVertices)
        {
            if (Mathf.Abs(v.y - minY) < tolerance)
            {
                sum += v;
                count++;
            }
        }

        if (count == 0)
            return new Vector3(globalMin.x, minY, globalMin.z);

        Vector3 barycenter = sum / count;
        return new Vector3(barycenter.x, minY, barycenter.z);
    }

    // Scaling stable autour d’un pivot monde
    private void ScaleAround(Transform target, Vector3 pivotWorld, Vector3 newScale)
    {
        Vector3 pivotLocal = target.InverseTransformPoint(pivotWorld);
        Vector3 oldScale = target.localScale;
        target.localScale = newScale;
        Vector3 pivotWorldAfter = target.TransformPoint(pivotLocal);
        target.position += pivotWorld - pivotWorldAfter;
    }
}
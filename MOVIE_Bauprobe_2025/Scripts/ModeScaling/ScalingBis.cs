using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class ScalingBis : MonoBehaviour
{
    public enum AxisToScale { Global, X, Z, Y }
    public AxisToScale ScaleAxis = AxisToScale.Global;

    [Header("Paramètres")]
    public XRNode controllerNode = XRNode.RightHand;
    public float scaleSpeed = 1f;
    public float sensitivity = 0.1f;
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
// MODIF

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    public Vector3 maxScale = new Vector3(5f, 5f, 5f);

// MODIF
    public GameObject arrowPrefab; 
    private Rigidbody rb;
    private GameObject currentArrow; 
    private Transform interactorTransform;
    private bool lastAButtonState = false;
    private GameObject deplacement;
    private Vector3 pivotWorld;
    private bool isGrabbed = false;

    //Mode de jeu
    private myControllerInputActionManager my_ControllerInput;

    // MODIF
    private void UpdateArrowDirection()
    {
        if (currentArrow == null)
            return;

        // Réinitialise la position de la flèche à la position du pivot
        currentArrow.transform.position = GameObject.Find("Right Controller Visual").gameObject.transform.position;

        // Récupère l'orientation de l'objet (les angles de rotation)
        Vector3 eulerRotation = transform.rotation.eulerAngles;

        // Variables pour l'orientation de la flèche
        Vector3 direction = Vector3.zero;

        // Selon l'axe de mise à l'échelle choisi, on calcule la direction de la flèche
        switch (ScaleAxis)
        {
            case AxisToScale.X:
                direction = transform.right;
                currentArrow.transform.up = direction;
                break;
            case AxisToScale.Z: 
                direction = transform.forward;
                currentArrow.transform.up = direction;
                break;
            case AxisToScale.Y:
                direction = transform.up;
                currentArrow.transform.up = direction;  
                break;
            case AxisToScale.Global:
                currentArrow.SetActive(false); 
                return;
        }

        // Active la flèche si elle est en mode X, Y ou Z
        if (!currentArrow.activeSelf)
            currentArrow.SetActive(true);

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

        my_ControllerInput = GameObject.Find("Right Controller").gameObject.GetComponent<myControllerInputActionManager>();
    }

    private void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrabStart);
        grab.selectExited.RemoveListener(OnGrabEnd);
        deplacement.SetActive(true);
    }

private void OnGrabStart(SelectEnterEventArgs args)
{
    interactorTransform = args.interactorObject.transform;
    isGrabbed = true;

    pivotWorld = GetBottomCenter();

    deplacement.SetActive(false);

    // Crée l'instance de la flèche si elle n'existe pas encore
    if ((my_ControllerInput.my_ControllerState == myControllerInputActionManager.ControllerEdittingState.Scaling) && (currentArrow == null)) 
    {
        currentArrow = Instantiate(arrowPrefab);
        currentArrow.transform.parent = transform ;
        currentArrow.transform.localPosition = Vector3.zero;
        currentArrow.transform.localRotation = Quaternion.identity;
        currentArrow.SetActive(true);
    }
}

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        interactorTransform = null;
        isGrabbed = false;

        deplacement.SetActive(true);

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
                case AxisToScale.X: ScaleAxis = AxisToScale.Y; break;
                case AxisToScale.Y: ScaleAxis = AxisToScale.Z; break;
                case AxisToScale.Z: ScaleAxis = AxisToScale.Global; break;
            }
            UpdateArrowDirection();
            Vector3 currentScale = transform.localScale;
            Vector3 newArrowScale = currentArrow.transform.localScale;

            switch (ScaleAxis)
            {
                case AxisToScale.X: 
                    newArrowScale.y = 0.1f / currentScale.x; 
                    newArrowScale.z = 0.1f / currentScale.y; 
                    newArrowScale.x = 0.1f / currentScale.z; 
                    break;
                case AxisToScale.Y: 
                    newArrowScale.y = 0.1f / currentScale.y; 
                    newArrowScale.x = 0.1f / currentScale.x; 
                    newArrowScale.z = 0.1f / currentScale.z; 
                    break;
                case AxisToScale.Z: 
                    newArrowScale.y = 0.1f / currentScale.z; 
                    newArrowScale.z = 0.1f / currentScale.y; 
                    newArrowScale.x = 0.1f / currentScale.x; 
                    break;
            }

            currentArrow.transform.localScale = newArrowScale;
        }
        lastAButtonState = A;

        // Scaling avec joystick vertical + grip
        if (grip && Mathf.Abs(joystick.y) > sensitivity)
        {
            UpdateArrowDirection();
            float factor = Mathf.Max(0.0001f, 1f + joystick.y * scaleSpeed * Time.fixedDeltaTime);
            Vector3 newScale = transform.localScale;
            Vector3 newArrowScale = currentArrow.transform.localScale;

            switch (ScaleAxis)
            {
                case AxisToScale.Global:
                    newScale *= factor;
                    newScale = new Vector3(
                        Mathf.Clamp(newScale.x, minScale.x, maxScale.x),
                        Mathf.Clamp(newScale.y, minScale.y, maxScale.y),
                        Mathf.Clamp(newScale.z, minScale.z, maxScale.z)
                    );
                    break;

                case AxisToScale.X:
                    newScale.x *= factor;
                    newScale = new Vector3(
                        Mathf.Clamp(newScale.x, minScale.x, maxScale.x),
                        Mathf.Clamp(newScale.y, minScale.y, maxScale.y),
                        Mathf.Clamp(newScale.z, minScale.z, maxScale.z)
                    );
                    newArrowScale.y = 0.1f / newScale.x;
                    break;

                case AxisToScale.Z:
                    newScale.z *= factor;
                    newScale = new Vector3(
                        Mathf.Clamp(newScale.x, minScale.x, maxScale.x),
                        Mathf.Clamp(newScale.y, minScale.y, maxScale.y),
                        Mathf.Clamp(newScale.z, minScale.z, maxScale.z)
                    );
                    newArrowScale.y = 0.1f / newScale.z;
                    break;

                case AxisToScale.Y:
                    newScale.y *= factor;
                    newScale = new Vector3(
                        Mathf.Clamp(newScale.x, minScale.x, maxScale.x),
                        Mathf.Clamp(newScale.y, minScale.y, maxScale.y),
                        Mathf.Clamp(newScale.z, minScale.z, maxScale.z)
                    );
                    newArrowScale.y = 0.1f / newScale.y;
                    break;

            }

            ScaleAround(transform, pivotWorld, newScale);
            currentArrow.transform.localScale = newArrowScale;
            
        }
    }


private Vector3 GetBottomCenter()
{
    // Récupère le Renderer de l'objet principal (celui auquel ce script est attaché)
    Renderer rend = GetComponent<Renderer>();
    if (rend == null)
    {
        // Si l'objet n'a pas de Renderer, on retourne simplement sa position
        return transform.position;
    }

    // Récupère le Mesh du Renderer de l'objet principal
    MeshFilter meshFilter = rend.GetComponent<MeshFilter>();
    if (meshFilter == null || meshFilter.sharedMesh == null)
    {
        // Si l'objet n'a pas de MeshFilter ou de mesh, on retourne la position de l'objet
        return transform.position;
    }

    Mesh mesh = meshFilter.sharedMesh;
    
    // Initialisation de la variable pour le point minimum (en Y)
    Vector3 globalMin = Vector3.positiveInfinity;
    List<Vector3> allVertices = new List<Vector3>();

    // Récupérer toutes les coordonnées des vertices
    foreach (Vector3 v in mesh.vertices)
    {
        // Convertir les vertices locaux en coordonnées mondiales
        Vector3 worldV = rend.transform.TransformPoint(v);
        allVertices.Add(worldV);

        // Trouver le vertex avec le Y le plus bas
        if (worldV.y < globalMin.y)
        {
            globalMin = worldV;
        }
    }

    // Calculer la moyenne des vertices proches du minimum Y pour obtenir le "centre bas"
    Vector3 bottomCenter = Vector3.zero;
    int count = 0;
    foreach (Vector3 v in allVertices)
    {
        // On sélectionne les vertices proches du minimum Y (la "base" de l'objet)
        if (Mathf.Abs(v.y - globalMin.y) < 0.001f)
        {
            bottomCenter += v;
            count++;
        }
    }

    // Si on a des vertices proches de la base, on retourne leur barycentre projeté sur le plan XZ
    if (count > 0)
    {
        bottomCenter /= count;
        return new Vector3(bottomCenter.x, globalMin.y, bottomCenter.z);
    }

    // Si aucun vertex n'est trouvé (cas de l'objet sans mesh ou autre), on retourne la position de l'objet
    return transform.position;
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
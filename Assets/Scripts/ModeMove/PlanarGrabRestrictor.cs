using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class SmartConstrainToPlane_Fixed : MonoBehaviour
{
    public enum LockedPlane { XZ, XY, YZ }
    public LockedPlane basePlane = LockedPlane.XZ;
    public float fixedValue = 0f;
    public XRNode controllerNode = XRNode.RightHand;
    public float moveSpeed = 4.0f;
    public float rotationSpeed = 30f;
    public float rotationSeuil = 0.1f;
    private Canvas coordinateCanvas;
    private TMP_Text coordinateText;
    private GameObject rightController;




    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Rigidbody rb;
    private Transform interactorTransform;

    // Mouvements relatifs
    private Vector3 grabStartPos;
    private Vector3 controllerStartPos;
    private bool aPressedLastFrame = false;
    private bool bPressedLastFrame = false;
    private Vector3 snapOffset = Vector3.zero;
    private bool snapOffsetCalculated = false;
    private Vector3 upVector = Vector3.up;
    private Vector3 objectBottomCenter = Vector3.zero;

    // Références vers les mouvements du joueur
    private GameObject deplacement;

    //Mode de jeu
    private myControllerInputActionManager my_ControllerInput;

    private void Awake()
    {   
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        rightController = GameObject.Find("Right Controller").gameObject;
        coordinateCanvas = rightController.transform.Find("coordinateCanvas").GetComponent<Canvas>();
        coordinateText = coordinateCanvas.transform.Find("coordinateText").GetComponent<TMP_Text>();

        if (coordinateCanvas != null)
        {
            coordinateCanvas.gameObject.SetActive(false);
        }

        grab.selectEntered.AddListener(OnGrabStart);
        grab.selectExited.AddListener(OnGrabEnd);
        switch (basePlane)
        {
            case LockedPlane.XY: upVector = new Vector3(0f, 0f, 1f); break;
            case LockedPlane.YZ: upVector = new Vector3(1f, 0f, 0f); break;
            case LockedPlane.XZ: upVector = new Vector3(0f, 1f, 0f); break;
        }

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
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        deplacement = GameObject.Find("Locomotion");


        interactorTransform = args.interactorObject.transform;
        objectBottomCenter = GetBottomCenter();
        deplacement.SetActive(false);
        // Afficher les coordonnées sur le Canvas
        if ((coordinateCanvas) != null && (my_ControllerInput.my_ControllerState == myControllerInputActionManager.ControllerEdittingState.Moving))
        {
            coordinateCanvas.gameObject.SetActive(true);
        }
        // Mettre à jour le texte avec les coordonnées actuelles de l'objet
        if (coordinateText != null)
        {
            coordinateText.text = $" x = {rb.position.x:F2} \n y = {rb.position.z:F2} \n z = {rb.position.y:F2}";
        }
    }

    private void OnGrabEnd(SelectExitEventArgs args)
    {
        interactorTransform = null;
        snapOffsetCalculated = false;

        deplacement.SetActive(true);

        // Cacher le Canvas lorsque l'objet est dégrabé
        if (coordinateCanvas != null)
        {
            coordinateCanvas.gameObject.SetActive(false);
        }

        rb = null;
        deplacement = null;
    }

    private void Update()
    {
        // Mettre à jour les coordonnées sur la manette gauche pendant le grab
        if (coordinateCanvas != null && coordinateCanvas.gameObject.activeSelf && coordinateText != null)
        {
            coordinateText.text = $" x = {rb.position.x:F2} \n y = {rb.position.z:F2} \n z = {rb.position.y:F2}";
        }
    }


    private void FixedUpdate()
    {
        if (interactorTransform == null) return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool A = false, B = false, trigger = false, grip = false;
        device.TryGetFeatureValue(CommonUsages.primaryButton, out A);
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out B);
        device.TryGetFeatureValue(CommonUsages.triggerButton, out trigger);
        device.TryGetFeatureValue(CommonUsages.gripButton, out grip);

        Vector3 newPos = rb.position;

        // Joystick rotation (axe X du stick = gauche/droite)
        Vector2 joystick;
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick);

        // Aucun bouton → figé
        if ((!A && !B && !trigger) || (A && B))
        {
            rb.MovePosition(rb.position);
            if (!rb.isKinematic) { 
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; 
                }
            aPressedLastFrame = false;
            bPressedLastFrame = false;
            snapOffsetCalculated = false;
            // rotation autour de l’axe Y
            float rotationInput = joystick.x;
            if (Mathf.Abs(rotationInput) > rotationSeuil && grip)
            {
                float angle = rotationInput * rotationSpeed * Time.fixedDeltaTime;
                transform.RotateAround(objectBottomCenter, upVector, -angle);
            }
            return;
        }

        // Première frame d'appui sur un bouton → mémoriser positions de départ
        if ((A || B || trigger) && (!aPressedLastFrame && !bPressedLastFrame))
        {
            grabStartPos = rb.position;
            controllerStartPos = interactorTransform.position;
            snapOffsetCalculated = false;
        }
    
        Vector3 controllerDelta = moveSpeed * (interactorTransform.position - controllerStartPos);

        // Priorité : trigger pour snap, sinon A ou B
        if (trigger && grip)
        {
            if (!snapOffsetCalculated)
            {
                Bounds bounds = GetTotalBounds();
                switch (basePlane)
                {
                    case LockedPlane.XZ:
                        snapOffset = new Vector3(0f, fixedValue - bounds.min.y, 0f);
                        break;
                    case LockedPlane.XY:
                        snapOffset = new Vector3(0f, 0f, fixedValue - bounds.min.z);
                        break;
                    case LockedPlane.YZ:
                        snapOffset = new Vector3(fixedValue - bounds.min.x, 0f, 0f);
                        break;
                }
                snapOffsetCalculated = true;
                newPos = grabStartPos + snapOffset;
            }
        }
        else if (B && grip)
        {
            // Mouvement vertical
            switch (basePlane)
            {
                case LockedPlane.XZ:
                    newPos.y = grabStartPos.y + controllerDelta.y;
                    newPos.x = grabStartPos.x;
                    newPos.z = grabStartPos.z;
                    break;
                case LockedPlane.XY:
                    newPos.y = grabStartPos.y;
                    newPos.x = grabStartPos.x;
                    newPos.z = grabStartPos.z + controllerDelta.z;
                    break;
                case LockedPlane.YZ:
                    newPos.y = grabStartPos.y;
                    newPos.z = grabStartPos.z;
                    newPos.x = grabStartPos.x + controllerDelta.x;
                    break;
            }
        }
        else if (A && grip)
        {
            // Mouvement horizontal
            switch (basePlane)
            {
                case LockedPlane.XZ:
                    newPos.x = grabStartPos.x + controllerDelta.x;
                    newPos.z = grabStartPos.z + controllerDelta.z;
                    newPos.y = grabStartPos.y;
                    break;
                case LockedPlane.XY:
                    newPos.x = grabStartPos.x + controllerDelta.x;
                    newPos.y = grabStartPos.y + controllerDelta.y;
                    newPos.z = grabStartPos.z;
                    break;
                case LockedPlane.YZ:
                    newPos.x = grabStartPos.x;
                    newPos.z = grabStartPos.z + controllerDelta.z;
                    newPos.y = grabStartPos.y + controllerDelta.y;
                    break;
            }
        }
        rb.MovePosition(newPos);

        aPressedLastFrame = A;
        bPressedLastFrame = B;
    }

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

            if (mesh == null) continue;

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
    private Bounds GetTotalBounds()
    {
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return new Bounds(transform.position, Vector3.zero);

        Bounds total = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            total.Encapsulate(rends[i].bounds);

        return total;
    }
}

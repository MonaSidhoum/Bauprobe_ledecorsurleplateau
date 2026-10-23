/*

using UnityEngine;
using UnityEngine.XR;

public class Scaler : MonoBehaviour
{
    [Header("Paramètres du Contrôleur")]
    [Tooltip("Nœud XR du contrôleur (main droite ou main gauche).")]
    public XRNode controllerNode = XRNode.RightHand;

    [Header("Sensibilité de l'Échelle")]
    [Tooltip("Vitesse à laquelle l'échelle augmente ou diminue.")]
    public float scaleSpeed = 5f;

    [Tooltip("Échelle minimale/maximale pour éviter que l'objet ne disparaisse ou devienne trop grand.")]
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 maxScale = new Vector3(5.0f, 5.0f, 5.0f);

    private InputDevice device;

    private Vector3 currentScale; // pour suivre l'échelle actuelle

    private bool isXScaleMode = false;
    private bool lastAButtonState = false;

    void Start()
    {
        // On récupère le périphérique au démarrage
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        
        // Initialisation de l'échelle actuelle
        currentScale = transform.localScale;
    }

    void Update()
    {
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (!device.isValid) return;
        }

        //bool isGripPressed = false;
        //bool isBPressed = false;
        //Vector2 joystickAxis = Vector2.zero;

        // 1. Lire les entrées
        device.TryGetFeatureValue(CommonUsages.gripButton, out bool isGripPressed);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickAxis);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed);

        // --- Détection du changement de mode (A pressé puis relâché) ---
        if (isAPressed && !lastAButtonState)
        {
            isXScaleMode = !isXScaleMode; // bascule le mode
            Debug.Log(isXScaleMode ? "Mode Scaling Axe X activé" : "Mode Scaling Global activé");
        }
        lastAButtonState = isAPressed;

        // 2. Vérifier la combinaison de touches : Grip (Gâchette Intérieure) + B
        if (isGripPressed)
        {
            // Le mouvement vertical du joystick (Haut/Bas) est l'axe Y de Vector2
            float verticalInput = joystickAxis.y;

            // Si le joystick est poussé vers le haut ou le bas
            if (Mathf.Abs(verticalInput) > 0.1f) 
            {
                // Calcul du changement d'échelle
                // Time.deltaTime pour rendre le scaling indépendant de la fréquence d'images
                float scaleChange = verticalInput * scaleSpeed * Time.deltaTime;

                if (isXScaleMode)
                {
                    // Mode axe X uniquement
                    currentScale.x += scaleChange;
                    currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                }

                else
                {
                    // Mode global
                    currentScale += new Vector3(scaleChange, scaleChange, scaleChange);
                    currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                    currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
                    currentScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);
                }
            }

            transform.localScale = currentScale;

        }

        // Toujours appliquer la dernière échelle connue
        transform.localScale = currentScale;
    }
}
*/


/*
using UnityEngine;
using UnityEngine.XR;

public class Scaler : MonoBehaviour
{
    [Header("Paramètres du Contrôleur")]
    public XRNode controllerNode = XRNode.RightHand;

    [Header("Sensibilité de l'Échelle")]
    public float scaleSpeed = 5f;
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 maxScale = new Vector3(5.0f, 5.0f, 5.0f);

    private InputDevice device;
    private Vector3 currentScale;

    // --- Modes de scaling ---
    private enum ScaleMode { Global, X, Z, Y }
    private ScaleMode currentMode = ScaleMode.Global;

    private bool lastAButtonState = false;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        currentScale = transform.localScale;
    }

    void Update()
    {
        // Vérifier que le contrôleur est bien connecté
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (!device.isValid) return;
        }

        // Lecture des entrées
        device.TryGetFeatureValue(CommonUsages.gripButton, out bool isGripPressed);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickAxis);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed); // Bouton A

        // --- Cycle de mode via le bouton A ---
        if (isAPressed && !lastAButtonState)
        {
            switch (currentMode)
            {
                case ScaleMode.Global: currentMode = ScaleMode.X; break;
                case ScaleMode.X: currentMode = ScaleMode.Z; break;
                case ScaleMode.Z: currentMode = ScaleMode.Y; break;
                case ScaleMode.Y: currentMode = ScaleMode.Global; break;
            }

            Debug.Log($"Mode de scaling : {currentMode}");
        }
        lastAButtonState = isAPressed;

        // --- Application du scaling ---
        if (isGripPressed)
        {
            float verticalInput = joystickAxis.y;
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                float scaleChange = verticalInput * scaleSpeed * Time.deltaTime;

                switch (currentMode)
                {
                    case ScaleMode.Global:
                        currentScale += new Vector3(scaleChange, scaleChange, scaleChange);
                        break;

                    case ScaleMode.X:
                        currentScale.x += scaleChange;
                        break;

                    case ScaleMode.Y:
                        currentScale.y += scaleChange;
                        break;

                    case ScaleMode.Z:
                        currentScale.z += scaleChange;
                        break;
                }

                // Clamp de sécurité
                currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
                currentScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);
            }
        }

        // Toujours appliquer la dernière échelle connue
        transform.localScale = currentScale;
    }
}
*/


// VERSION QUASI FONCTIONNELLE MAIS TOUT EST GROSSI => QUELQUES MODIFS DU CHAT

/*
using UnityEngine;
using UnityEngine.XR;

public class Scaling : MonoBehaviour
{
    [Header("Paramètres du Contrôleur")]
    public XRNode controllerNode = XRNode.RightHand;

    [Header("Sensibilité de l'Échelle")]
    public float scaleSpeed = 5f;
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 maxScale = new Vector3(5.0f, 5.0f, 5.0f);

    public void SetGrabbedObject(Transform target)
    {
        grabbedObject = target;

        if (grabbedObject != null)
            currentScale = grabbedObject.localScale;

        Debug.Log(grabbedObject != null
            ? $"Objet grab : {grabbedObject.name}"
            : "Aucun objet grab actuellement");
    }



    private InputDevice device;
    private Vector3 currentScale;

    // --- Modes de scaling ---
    private enum ScaleMode { Global, X, Z, Y }
    private ScaleMode currentMode = ScaleMode.Global;

    private bool lastAButtonState = false;

    private Transform grabbedObject;

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        currentScale = transform.localScale;
    }

    void Update()
    {
        // Vérifier que le contrôleur est bien connecté
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (!device.isValid) return;
        }

        // Lecture des entrées
        device.TryGetFeatureValue(CommonUsages.gripButton, out bool isGripPressed);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickAxis);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed); // Bouton A

        // --- Cycle de mode via le bouton A ---
        if (isAPressed && !lastAButtonState)
        {
            switch (currentMode)
            {
                case ScaleMode.Global: currentMode = ScaleMode.X; break;
                case ScaleMode.X: currentMode = ScaleMode.Z; break;
                case ScaleMode.Z: currentMode = ScaleMode.Y; break;
                case ScaleMode.Y: currentMode = ScaleMode.Global; break;
            }

            Debug.Log($"Mode de scaling : {currentMode}");
        }
        lastAButtonState = isAPressed;

        // --- Application du scaling ---
        if (isGripPressed)
        {
            float verticalInput = joystickAxis.y;
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                float scaleChange = verticalInput * scaleSpeed * Time.deltaTime;

                switch (currentMode)
                {
                    case ScaleMode.Global:
                        currentScale += new Vector3(scaleChange, scaleChange, scaleChange);
                        break;

                    case ScaleMode.X:
                        currentScale.x += scaleChange;
                        break;

                    case ScaleMode.Y:
                        currentScale.y += scaleChange;
                        break;

                    case ScaleMode.Z:
                        currentScale.z += scaleChange;
                        break;
                }

                // Clamp de sécurité
                currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
                currentScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);
            }
        }

        // Toujours appliquer la dernière échelle connue
        if (grabbedObject != null)
            grabbedObject.localScale = currentScale;

    }
}

*/

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit; // Rajout

public class Scaling : MonoBehaviour
{
    [Header("Paramètres du Contrôleur")]
    public XRNode controllerNode = XRNode.RightHand;
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor; // Rajout

    [Header("Sensibilité de l'Échelle")]
    public float scaleSpeed = 5f;
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector3 maxScale = new Vector3(5f, 5f, 5f);

    private InputDevice device;
    private Vector3 currentScale;

    private Transform grabbedObject;

    // --- Modes de scaling ---
    private enum ScaleMode { Global, X, Z, Y }
    private ScaleMode currentMode = ScaleMode.Global;

    private bool lastAButtonState = false;

    // --- Public API pour le pont ---
    public void SetGrabbedObject(Transform target)
    {
        grabbedObject = target;
        if (grabbedObject != null)
            currentScale = grabbedObject.localScale;
    }

    private void OnDestroy()
    {
        if (interactor != null)
        {
            interactor.selectEntered.RemoveListener(OnGrab);
            interactor.selectExited.RemoveListener(OnRelease);
        }
    }

    /*
        // --- Flèche pour indiquer la direction du scaling ---
        public GameObject arrow; // Flèche à afficher en mode X, Y, Z
        private Vector3 arrowDirection;
    */

    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        // Rajout
        if (interactor == null)
        {
            Debug.LogError("⚠ Aucun interactor assigné à Scaling !");
            return;
        }

        // Écoute des événements du grab XR
        interactor.selectEntered.AddListener(OnGrab);
        interactor.selectExited.AddListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        GameObject deplacement = GameObject.Find("Locomotion");
        if (deplacement != null)
        {
            deplacement.SetActive(false);
        }
        grabbedObject = args.interactableObject.transform;
        if (grabbedObject != null)
            currentScale = grabbedObject.localScale;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        GameObject deplacement = GameObject.Find("Locomotion");
        if (deplacement != null)
        {
            deplacement.SetActive(true);
        }
        grabbedObject = null;
    }

    void FixedUpdate()
    {
        // Si aucun objet grabé, rien à faire
        if (grabbedObject == null) return;

        // Vérifier que le contrôleur est bien connecté
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (!device.isValid) return;
        }

        // Lecture des entrées
        device.TryGetFeatureValue(CommonUsages.gripButton, out bool isGripPressed);
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickAxis);
        device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed); // Bouton A

        // --- Cycle de mode via le bouton A ---
        if (isAPressed && !lastAButtonState)
        {
            currentMode = currentMode switch
            {
                ScaleMode.Global => ScaleMode.X,
                ScaleMode.X => ScaleMode.Z,
                ScaleMode.Z => ScaleMode.Y,
                ScaleMode.Y => ScaleMode.Global,
                _ => ScaleMode.Global
            };

            Debug.Log($"Mode de scaling : {currentMode}");
        }
        lastAButtonState = isAPressed;

        // --- Application du scaling ---

        if (isGripPressed)
        {
            float verticalInput = joystickAxis.y;

            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                float scaleChange = verticalInput * scaleSpeed * Time.deltaTime;

                switch (currentMode)
                {
                    case ScaleMode.Global:
                        currentScale += new Vector3(scaleChange, scaleChange, scaleChange);
                        break;
                    case ScaleMode.X:
                        currentScale.x += scaleChange;
                        break;
                    case ScaleMode.Y:
                        currentScale.y += scaleChange;
                        break;
                    case ScaleMode.Z:
                        currentScale.z += scaleChange;
                        break;
                }

                // Clamp pour sécurité
                currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
                currentScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);

                // Appliquer au grab
                grabbedObject.localScale = currentScale;
            }

        }
        if (grabbedObject != null)
            {grabbedObject.localScale = currentScale;}


    }
}



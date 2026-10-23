using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Linq; // Nécessaire pour les opérations de tableau (même si non strictement utilisé, c'est une bonne pratique)

// Utilisez le même namespace que l'original pour la compatibilité
namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    [AddComponentMenu("XR/Controller Input Action Manager")]
    public class MyNewControllerInputActionManager : MonoBehaviour // ⬅️ NOUVEAU NOM DE CLASSE
    {
        public enum ControllerEdittingState { Deleting, Scaling, Moving };

        // ------------------------------------------------------------------
        // NOUVELLE DECLARATION : Liste de tous les objets interactifs dans la scène
        // ------------------------------------------------------------------
        private ModeInteractable[] m_AllModeInteractables;

        // ------------------------------------------------------------------
        // Reste des déclarations...
        // ------------------------------------------------------------------

        [Space]
        [Header("Interactors")]

        [SerializeField]
        [Tooltip("The interactor used for distant/ray manipulation. Use this or Near-Far Interactor, not both.")]
        XRRayInteractor m_RayInteractor;

        [SerializeField]
        [Tooltip("Near-Far Interactor used for distant/ray manipulation. Use this or Ray Interactor, not both.")]
        NearFarInteractor m_NearFarInteractor;

        [SerializeField]
        [Tooltip("The interactor used for teleportation.")]
        XRRayInteractor m_TeleportInteractor;

        [Space]
        [Header("Controller Actions")]

        [SerializeField]
        [Tooltip("The reference to the action to start the teleport aiming mode for this controller.")]
        [FormerlySerializedAs("m_TeleportModeActivate")]
        InputActionReference m_TeleportMode;

        [SerializeField]
        [Tooltip("The reference to the action to cancel the teleport aiming mode for this controller.")]
        InputActionReference m_TeleportModeCancel;

        [SerializeField]
        [Tooltip("The reference to the action of continuous turning the XR Origin with this controller.")]
        InputActionReference m_Turn;

        [SerializeField]
        [Tooltip("The reference to the action of snap turning the XR Origin with this controller.")]
        InputActionReference m_SnapTurn;

        [SerializeField]
        [Tooltip("The reference to the action of moving the XR Origin with this controller.")]
        InputActionReference m_Move;

        [SerializeField]
        [Tooltip("The reference to the action of scrolling UI with this controller.")]
        InputActionReference m_UIScroll;

        [Space]
        [Header("Locomotion Settings")]

        [SerializeField]
        [Tooltip("If true, continuous movement will be enabled. If false, teleport will be enabled.")]
        bool m_SmoothMotionEnabled;

        [SerializeField]
        [Tooltip("If true, continuous turn will be enabled. If false, snap turn will be enabled. Note: If smooth motion is enabled and enable strafe is enabled on the continuous move provider, turn will be overriden in favor of strafe.")]
        bool m_SmoothTurnEnabled;

        [SerializeField]
        [Tooltip("With the Near-Far Interactor, if true, teleport will be enabled during near interaction. If false, teleport will be disabled during near interaction.")]
        bool m_NearFarEnableTeleportDuringNearInteraction = true;

        [Space]
        [Header("UI Settings")]

        [SerializeField]
        [Tooltip("If true, UI scrolling will be enabled. Locomotion will be disabled when pointing at UI to allow it to be scrolled.")]
        bool m_UIScrollingEnabled = true;

        [Space]
        [Header("Mediation Events")]

        [SerializeField]
        [Tooltip("Event fired when the active ray interactor changes between interaction and teleport.")]
        UnityEvent<IXRRayProvider> m_RayInteractorChanged;

        private ControllerEdittingState my_ControllerState = ControllerEdittingState.Moving;

        public bool smoothMotionEnabled
        {
            get => m_SmoothMotionEnabled;
            set
            {
                m_SmoothMotionEnabled = value;
                UpdateLocomotionActions();
            }
        }

        public bool smoothTurnEnabled
        {
            get => m_SmoothTurnEnabled;
            set
            {
                m_SmoothTurnEnabled = value;
                UpdateLocomotionActions();
            }
        }

        public bool uiScrollingEnabled
        {
            get => m_UIScrollingEnabled;
            set
            {
                m_UIScrollingEnabled = value;
                UpdateUIActions();
            }
        }

        bool m_StartCalled;
        bool m_PostponedDeactivateTeleport;
        bool m_HoveringScrollableUI;

        readonly HashSet<InputAction> m_LocomotionUsers = new HashSet<InputAction>();
        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        void SetupInteractorEvents()
        {
            // [EXISTANT]
            if (m_NearFarInteractor != null)
            {
                m_NearFarInteractor.uiHoverEntered.AddListener(OnUIHoverEntered);
                m_NearFarInteractor.uiHoverExited.AddListener(OnUIHoverExited);
                m_BindingsGroup.AddBinding(m_NearFarInteractor.selectionRegion.Subscribe(OnNearFarSelectionRegionChanged));
            }

            if (m_RayInteractor != null)
            {
                m_RayInteractor.selectEntered.AddListener(OnRaySelectEntered);
                m_RayInteractor.selectExited.AddListener(OnRaySelectExited);
                m_RayInteractor.uiHoverEntered.AddListener(OnUIHoverEntered);
                m_RayInteractor.uiHoverExited.AddListener(OnUIHoverExited);
            }

            var teleportModeAction = GetInputAction(m_TeleportMode);
            if (teleportModeAction != null)
            {
                teleportModeAction.performed += OnStartTeleport;
                teleportModeAction.performed += OnStartLocomotion;
                teleportModeAction.canceled += OnCancelTeleport;
                teleportModeAction.canceled += OnStopLocomotion;
            }

            var teleportModeCancelAction = GetInputAction(m_TeleportModeCancel);
            if (teleportModeCancelAction != null)
            {
                teleportModeCancelAction.performed += OnCancelTeleport;
            }

            var moveAction = GetInputAction(m_Move);
            if (moveAction != null)
            {
                moveAction.started += OnStartLocomotion;
                moveAction.canceled += OnStopLocomotion;
            }

            var turnAction = GetInputAction(m_Turn);
            if (turnAction != null)
            {
                turnAction.started += OnStartLocomotion;
                turnAction.canceled += OnStopLocomotion;
            }

            var snapTurnAction = GetInputAction(m_SnapTurn);
            if (snapTurnAction != null)
            {
                snapTurnAction.started += OnStartLocomotion;
                snapTurnAction.canceled += OnStopLocomotion;
            }
        }

        void TeardownInteractorEvents()
        {
            // [EXISTANT]
            m_BindingsGroup.Clear();

            if (m_NearFarInteractor != null)
            {
                m_NearFarInteractor.uiHoverEntered.RemoveListener(OnUIHoverEntered);
                m_NearFarInteractor.uiHoverExited.RemoveListener(OnUIHoverExited);
            }

            if (m_RayInteractor != null)
            {
                m_RayInteractor.selectEntered.RemoveListener(OnRaySelectEntered);
                m_RayInteractor.selectExited.RemoveListener(OnRaySelectExited);
                m_RayInteractor.uiHoverEntered.RemoveListener(OnUIHoverEntered);
                m_RayInteractor.uiHoverExited.RemoveListener(OnUIHoverExited);
            }

            var teleportModeAction = GetInputAction(m_TeleportMode);
            if (teleportModeAction != null)
            {
                teleportModeAction.performed -= OnStartTeleport;
                teleportModeAction.performed -= OnStartLocomotion;
                teleportModeAction.canceled -= OnCancelTeleport;
                teleportModeAction.canceled -= OnStopLocomotion;
            }

            var teleportModeCancelAction = GetInputAction(m_TeleportModeCancel);
            if (teleportModeCancelAction != null)
            {
                teleportModeCancelAction.performed -= OnCancelTeleport;
            }

            var moveAction = GetInputAction(m_Move);
            if (moveAction != null)
            {
                moveAction.started -= OnStartLocomotion;
                moveAction.canceled -= OnStopLocomotion;
            }

            var turnAction = GetInputAction(m_Turn);
            if (turnAction != null)
            {
                turnAction.started -= OnStartLocomotion;
                turnAction.canceled -= OnStopLocomotion;
            }

            var snapTurnAction = GetInputAction(m_SnapTurn);
            if (snapTurnAction != null)
            {
                snapTurnAction.started -= OnStartLocomotion;
                snapTurnAction.canceled -= OnStopLocomotion;
            }
        }

        void OnStartTeleport(InputAction.CallbackContext context)
        {
            // [EXISTANT]
            m_PostponedDeactivateTeleport = false;

            if (m_TeleportInteractor != null)
                m_TeleportInteractor.gameObject.SetActive(true);

            if (m_RayInteractor != null)
                m_RayInteractor.gameObject.SetActive(false);

            if (m_NearFarInteractor != null && m_NearFarInteractor.selectionRegion.Value != NearFarInteractor.Region.Near)
                m_NearFarInteractor.gameObject.SetActive(false);

            m_RayInteractorChanged?.Invoke(m_TeleportInteractor);
        }

        void OnCancelTeleport(InputAction.CallbackContext context)
        {
            // [EXISTANT]
            m_PostponedDeactivateTeleport = true;

            if (m_RayInteractor != null && my_ControllerState == ControllerEdittingState.Deleting)
                m_RayInteractor.gameObject.SetActive(true);

            if (m_NearFarInteractor != null && my_ControllerState != ControllerEdittingState.Deleting)
                m_NearFarInteractor.gameObject.SetActive(true);

            m_RayInteractorChanged?.Invoke(m_RayInteractor);
        }

        void OnNearFarSelectionRegionChanged(NearFarInteractor.Region selectionRegion)
        {
            // [EXISTANT]
            if (selectionRegion == NearFarInteractor.Region.Far ||
                (selectionRegion == NearFarInteractor.Region.Near && !m_NearFarEnableTeleportDuringNearInteraction))
                DisableTeleportActions();
            else
                UpdateLocomotionActions();
        }

        void OnStartLocomotion(InputAction.CallbackContext context)
        {
            // [EXISTANT]
            m_LocomotionUsers.Add(context.action);
        }

        void OnStopLocomotion(InputAction.CallbackContext context)
        {
            // [EXISTANT]
            m_LocomotionUsers.Remove(context.action);

            if (m_LocomotionUsers.Count == 0 && m_HoveringScrollableUI)
            {
                DisableAllLocomotionActions();
                UpdateUIActions();
            }
        }

        void OnRaySelectEntered(SelectEnterEventArgs args)
        {
            // [EXISTANT]
            if (m_RayInteractor.manipulateAttachTransform)
            {
                // Disable locomotion and turn actions
                DisableAllLocomotionActions();
            }
        }

        void OnRaySelectExited(SelectExitEventArgs args)
        {
            // [EXISTANT]
            if (m_RayInteractor.manipulateAttachTransform)
            {
                // Re-enable the locomotion and turn actions
                UpdateLocomotionActions();
            }
        }

        void OnUIHoverEntered(UIHoverEventArgs args)
        {
            // [EXISTANT]
            m_HoveringScrollableUI = m_UIScrollingEnabled && args.deviceModel.isScrollable;
            UpdateUIActions();

            // If locomotion is occurring, wait
            if (m_HoveringScrollableUI && m_LocomotionUsers.Count == 0)
            {
                // Disable locomotion and turn actions
                DisableAllLocomotionActions();
            }
        }

        void OnUIHoverExited(UIHoverEventArgs args)
        {
            // [EXISTANT]
            m_HoveringScrollableUI = false;
            UpdateUIActions();

            // Re-enable the locomotion and turn actions
            UpdateLocomotionActions();
        }

        protected void OnEnable()
        {
            // [EXISTANT]
            if (m_RayInteractor != null && m_NearFarInteractor != null)
            {
                m_RayInteractor.gameObject.SetActive(my_ControllerState == ControllerEdittingState.Deleting);
                m_NearFarInteractor.gameObject.SetActive(my_ControllerState != ControllerEdittingState.Deleting);
            }

            if (m_TeleportInteractor != null)
                m_TeleportInteractor.gameObject.SetActive(false);

            // Allow the actions to be refreshed when this component is re-enabled.
            // See comments in Start for why we wait until Start to enable/disable actions.
            if (m_StartCalled)
            {
                UpdateLocomotionActions();
                UpdateUIActions();
            }

            SetupInteractorEvents();
        }

        protected void OnDisable()
        {
            // [EXISTANT]
            TeardownInteractorEvents();
        }

        protected void Start()
        {
            m_StartCalled = true;

            // ------------------------------------------------------------------
            // MODIFICATION 1 : Récupérer les objets interactifs (méthode moderne)
            // ------------------------------------------------------------------
            m_AllModeInteractables = FindObjectsByType<ModeInteractable>(FindObjectsSortMode.None);

            UpdateLocomotionActions();
            UpdateUIActions();

            // ------------------------------------------------------------------
            // MODIFICATION 2 : Appliquer l'état initial des Interactables
            // ------------------------------------------------------------------
            SetObjectInteractivity(my_ControllerState);
        }

        protected void Update()
        {
            // [EXISTANT]
            if (m_PostponedDeactivateTeleport)
            {
                if (m_TeleportInteractor != null)
                    m_TeleportInteractor.gameObject.SetActive(false);

                m_PostponedDeactivateTeleport = false;
            }
        }

        void UpdateLocomotionActions()
        {
            // [EXISTANT]
            SetEnabled(m_Move, m_SmoothMotionEnabled);
            SetEnabled(m_TeleportMode, !m_SmoothMotionEnabled);
            SetEnabled(m_TeleportModeCancel, !m_SmoothMotionEnabled);

            SetEnabled(m_Turn, !m_SmoothMotionEnabled && m_SmoothTurnEnabled);
            SetEnabled(m_SnapTurn, !m_SmoothMotionEnabled && !m_SmoothTurnEnabled);
        }

        void DisableTeleportActions()
        {
            // [EXISTANT]
            DisableAction(m_TeleportMode);
            DisableAction(m_TeleportModeCancel);
        }

        void DisableMoveAndTurnActions()
        {
            // [EXISTANT]
            DisableAction(m_Move);
            DisableAction(m_Turn);
            DisableAction(m_SnapTurn);
        }

        void DisableAllLocomotionActions()
        {
            // [EXISTANT]
            DisableTeleportActions();
            DisableMoveAndTurnActions();
        }

        void UpdateUIActions()
        {
            // [EXISTANT]
            SetEnabled(m_UIScroll, m_UIScrollingEnabled && m_HoveringScrollableUI && m_LocomotionUsers.Count == 0);
        }

        static void SetEnabled(InputActionReference actionReference, bool enabled)
        {
            // [EXISTANT]
            if (enabled)
                EnableAction(actionReference);
            else
                DisableAction(actionReference);
        }

        static void EnableAction(InputActionReference actionReference)
        {
            // [EXISTANT]
            var action = GetInputAction(actionReference);
            if (action != null && !action.enabled)
                action.Enable();
        }

        static void DisableAction(InputActionReference actionReference)
        {
            // [EXISTANT]
            var action = GetInputAction(actionReference);
            if (action != null && action.enabled)
                action.Disable();
        }

        static InputAction GetInputAction(InputActionReference actionReference)
        {
            // [EXISTANT]
            return actionReference != null ? actionReference.action : null;
        }

        // ------------------------------------------------------------------
        // NOUVELLE MÉTHODE : Contrôle l'interactivité des objets en fonction du mode
        // ------------------------------------------------------------------
        /// <summary>
        /// Active ou désactive le composant XRGrabInteractable sur tous les objets de la scène
        /// en fonction de l'état d'édition actuel.
        /// </summary>
        void SetObjectInteractivity(ControllerEdittingState state)
        {
            // Les objets sont interactifs si le mode est Moving ou Scaling
            bool shouldBeInteractable = (state == ControllerEdittingState.Moving || state == ControllerEdittingState.Scaling);

            // Appliquer l'état à tous les objets
            if (m_AllModeInteractables != null)
            {
                foreach (var interactable in m_AllModeInteractables)
                {
                    // La méthode SetInteractableEnabled doit exister dans le script ModeInteractable.cs
                    interactable.SetInteractableEnabled(shouldBeInteractable);
                }
            }
        }

        // ------------------------------------------------------------------
        // MODIFICATIONS des Fonctions d'Activation de Mode
        // ------------------------------------------------------------------

        public void EnableDeleting()
        {
            my_ControllerState = ControllerEdittingState.Deleting;
            m_RayInteractor.gameObject.SetActive(true);
            m_NearFarInteractor.gameObject.SetActive(false);
            SetObjectInteractivity(my_ControllerState); // ⬅️ NOUVEL APPEL
            Debug.Log("mode supprimer");
        }

        public void EnableMoving()
        {
            my_ControllerState = ControllerEdittingState.Moving;
            m_RayInteractor.gameObject.SetActive(false);
            m_NearFarInteractor.gameObject.SetActive(true);
            SetObjectInteractivity(my_ControllerState); // ⬅️ NOUVEL APPEL
            Debug.Log("mode move");
        }

        public void EnableScaling()
        {
            my_ControllerState = ControllerEdittingState.Scaling;
            m_RayInteractor.gameObject.SetActive(false);
            m_NearFarInteractor.gameObject.SetActive(true);
            SetObjectInteractivity(my_ControllerState); // ⬅️ NOUVEL APPEL
            Debug.Log("mode scale");
        }
    }
}
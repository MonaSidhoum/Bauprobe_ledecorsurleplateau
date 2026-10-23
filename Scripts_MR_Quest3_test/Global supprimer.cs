using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using static UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.myControllerInputActionManager;
public class Globalsupprimer : MonoBehaviour
{
    public InputActionReference selectAction;
    public XRRayInteractor RayInteractor;

    [Tooltip("Gestionnaire de mode de la manette droite. Si vide, il est cherché sur l'objet 'Right Controller'.")]
    public myControllerInputActionManager my_ControllerInput;

    private void Awake()
    {
        // CORRECTION : avant, my_ControllerState était un champ privé jamais renseigné,
        // donc le mode Deleting n'était jamais réellement détecté.
        if (my_ControllerInput == null)
        {
            GameObject rightController = GameObject.Find("Right Controller");
            if (rightController != null)
                my_ControllerInput = rightController.GetComponent<myControllerInputActionManager>();
        }
    }

    private void OnEnable()
    {
        if (selectAction != null && selectAction.action != null)
        {
            selectAction.action.performed += OnClickPerformed;
            selectAction.action.Enable();
        }
        else
        {
            Debug.LogError("Select Action is not assigned in XRClickHandler.");
        }
    }

    private void OnDisable()
    {
        if (selectAction != null && selectAction.action != null)
        {
            selectAction.action.performed -= OnClickPerformed;
            selectAction.action.Disable();
        }
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (my_ControllerInput == null || RayInteractor == null) return;

        if (my_ControllerInput.my_ControllerState == ControllerEdittingState.Deleting)
        {
            if (RayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject cible = hit.collider.gameObject;
                Destroy_Object_On_Click script = cible.GetComponent<Destroy_Object_On_Click>();

                // CORRECTION : plus de NullReference si l'objet visé n'a pas le script
                if (script != null && script.supprimable)
                {
                    Debug.Log("XR Click on object detected!");

                    Destroy(cible);
                }
            }
        }
    }

}

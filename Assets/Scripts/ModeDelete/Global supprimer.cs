using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets.myControllerInputActionManager;
public class Globalsupprimer : MonoBehaviour
{
    public InputActionReference selectAction;
    private ControllerEdittingState my_ControllerState;
    public XRRayInteractor RayInteractor;

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
        if (my_ControllerState == ControllerEdittingState.Deleting)
        {
            if (RayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                Debug.Log(hit);
                GameObject cible = hit.collider.gameObject;
                Destroy_Object_On_Click script = cible.GetComponent<Destroy_Object_On_Click>();
                if (script.supprimable)
                {
                    Debug.Log("XR Click on object detected!");

                    Destroy(cible);
                }
            }

        }
    }

}
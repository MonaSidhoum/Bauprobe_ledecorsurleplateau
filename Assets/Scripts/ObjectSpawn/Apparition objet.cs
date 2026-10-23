using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Apparitionobjet : MonoBehaviour
{
    public Transform placeToInstantiate;
    public GameObject prefabToInstantiate;
    public myControllerInputActionManager my_ControllerInput;
    public void DoEffect()
    {
        GameObject new_decor = Instantiate(prefabToInstantiate, placeToInstantiate.position, Quaternion.identity);
        if (my_ControllerInput.my_ControllerState == myControllerInputActionManager.ControllerEdittingState.Moving )
        {
            new_decor.GetComponent<SmartConstrainToPlane_Fixed>().enabled = true;
        }
        else { if (my_ControllerInput.my_ControllerState == myControllerInputActionManager.ControllerEdittingState.Scaling)
                {
                    new_decor.GetComponent<ScalingBis>().enabled = true;
                }
        }
    }
}

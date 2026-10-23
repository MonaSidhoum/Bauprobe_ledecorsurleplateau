using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Apparitionobjet : MonoBehaviour
{
    public Transform placeToInstantiate;
    public GameObject prefabToInstantiate;
    public myControllerInputActionManager my_ControllerInput;
    public void DoEffect()
    {
        if (prefabToInstantiate == null || placeToInstantiate == null)
        {
            Debug.LogWarning("Apparitionobjet : prefabToInstantiate ou placeToInstantiate non assigné.");
            return;
        }

        GameObject new_decor = Instantiate(prefabToInstantiate, placeToInstantiate.position, Quaternion.identity);
        if (my_ControllerInput == null) return;

        // CORRECTION : plus de NullReference si le prefab n'a pas le script du mode correspondant
        if (my_ControllerInput.my_ControllerState == myControllerInputActionManager.ControllerEdittingState.Moving)
        {
            SmartConstrainToPlane_Fixed move = new_decor.GetComponent<SmartConstrainToPlane_Fixed>();
            if (move != null) move.enabled = true;
        }
        else if (my_ControllerInput.my_ControllerState == myControllerInputActionManager.ControllerEdittingState.Scaling)
        {
            ScalingBis scaling = new_decor.GetComponent<ScalingBis>();
            if (scaling != null) scaling.enabled = true;
        }
    }
}

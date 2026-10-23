using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ModeInteractable : MonoBehaviour
{
    private XRGrabInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        if (interactable == null)
        {
            Debug.LogError($"L'objet {gameObject.name} n'a pas de composant XRGrabInteractable.");
        }
    }

    public void SetInteractableEnabled(bool state)
    {
        if (interactable != null)
        {
            interactable.enabled = state;
        }
    }
}
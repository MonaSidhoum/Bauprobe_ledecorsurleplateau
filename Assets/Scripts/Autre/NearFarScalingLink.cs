using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(NearFarInteractor))]
public class NearFarScalingLink : MonoBehaviour
{
    private Scaling scaling;
    private NearFarInteractor interactor;

    void Awake()
    {
        interactor = GetComponent<NearFarInteractor>();
        scaling = GetComponent<Scaling>();
    }

    void OnEnable()
    {
        interactor.selectEntered.AddListener(OnSelectEntered);
        interactor.selectExited.AddListener(OnSelectExited);
    }

    void OnDisable()
    {
        interactor.selectEntered.RemoveListener(OnSelectEntered);
        interactor.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject is UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
        {
            scaling.SetGrabbedObject(interactable.transform);
            Debug.Log($"[ScalingLink] Grab : {interactable.name}");
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        scaling.SetGrabbedObject(null);
        Debug.Log("[ScalingLink] Release");
    }
}


using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlanarGrabInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    public enum LockedAxis { XZ, XY, YZ }

    [Header("Contrainte de plan")]
    public LockedAxis plane = LockedAxis.XZ;
    public float fixedValue = 0f;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // Repositionner le point d’attache au centre de l’objet
        if (attachTransform != null)
        {
            attachTransform.position = transform.position;
        }
    }

    private void Update()
    {
        // Appliquer la contrainte de plan pendant le grab
        if (isSelected)
        {
            Vector3 pos = transform.position;

            switch (plane)
            {
                case LockedAxis.XY:
                    pos.z = fixedValue;
                    break;
                case LockedAxis.XZ:
                    pos.y = fixedValue;
                    break;
                case LockedAxis.YZ:
                    pos.x = fixedValue;
                    break;
            }

            transform.position = pos;
        }
    }
}

using UnityEngine;

public class QRSceneCoordinator : MonoBehaviour
{
    [Header("Repère du décor")]
    [SerializeField] private Transform qrAnchor;
    [SerializeField] private Transform stageRoot;

    public bool IsQrLocated { get; private set; }

    public void OnQrLocated(Transform detectedQr)
    {
        if (detectedQr == null)
            return;

        // Positionner notre repère sur le QR détecté.
        qrAnchor.SetPositionAndRotation(
            detectedQr.position,
            detectedQr.rotation
        );

        IsQrLocated = true;

        Debug.Log("QR détecté : décor recalé.");
    }

    public void OnQrLost()
    {
        IsQrLocated = false;

        // Ne pas détruire le décor.
        Debug.Log("QR perdu : décor conservé.");
    }
}

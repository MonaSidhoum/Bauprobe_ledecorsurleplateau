using UnityEngine;

public class CanvasToggleController : MonoBehaviour
{
    [Header("R�f�rence du Canvas")]
    public GameObject targetCanvas; // Le Canvas à afficher/masquer

    /// Affiche le Canvas
    public void ShowCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Canvas non assign� dans l'Inspector !");
        }
    }

    /// Masque le Canvas
    public void HideCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false);
        }
    }
}
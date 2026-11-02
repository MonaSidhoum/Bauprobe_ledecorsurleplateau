using UnityEngine;

namespace Bauprobe.Core
{
    /// <summary>
    /// Réglages du plateau, enregistrés dans le fichier <c>Assets/_Bauprobe/Core/Resources/StageSettings.asset</c>
    /// (suivi par Git : on peut le modifier dans l'Inspector de Unity ou directement dans le fichier).
    /// </summary>
    [CreateAssetMenu(menuName = "Bauprobe/Réglages du plateau", fileName = "StageSettings")]
    public class StageSettings : ScriptableObject
    {
        /// <summary>Chemin du fichier de réglages dans un dossier Resources (sans extension).</summary>
        public const string ResourcePath = "StageSettings";

        [Tooltip("Distance (en mètres) entre le BAS du QR code et le sol du plateau, mesurée au mètre.\n" +
                 "0 = QR code posé à plat sur le sol. Exemple : QR code collé sur un mur, bord du bas à 1,20 m du sol → 1.2")]
        [SerializeField, Min(0f)] private float qrBottomHeightAboveFloor = 0f;

        /// <summary>Distance (m) entre le bas du QR code et le sol du plateau.</summary>
        public float QrBottomHeightAboveFloor => qrBottomHeightAboveFloor;

        /// <summary>Hauteur du sol (repère du monde) à partir de la hauteur du bas du QR code.</summary>
        public float FloorHeight(float qrBottomY) => FloorHeight(qrBottomY, qrBottomHeightAboveFloor);

        /// <summary>Hauteur du sol : hauteur du bas du QR code moins sa distance au sol.</summary>
        public static float FloorHeight(float qrBottomY, float qrBottomHeightAboveFloor) => qrBottomY - qrBottomHeightAboveFloor;

        /// <summary>Charge le fichier de réglages du projet (null s'il n'existe pas).</summary>
        public static StageSettings LoadDefault() => Resources.Load<StageSettings>(ResourcePath);
    }
}

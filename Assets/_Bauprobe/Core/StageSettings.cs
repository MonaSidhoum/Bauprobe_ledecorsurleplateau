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

        [Header("Offset du QR code (point zéro du bureau d'études)")]
        [Tooltip("Position X du QR code dans le repère du bureau d'études, en mètres (X+ = à droite, par ex. côté cour). " +
                 "On mesure le point du sol sous le centre du QR code.\n" +
                 "0 / 0 / 0 = le QR code EST le point zéro : positions du tableau = positions par rapport au QR code.")]
        [SerializeField] private float qrOffsetX = 0f;

        [Tooltip("Position Y du QR code dans le repère du bureau d'études, en mètres (Y+ = vers l'avant, par ex. le lointain). " +
                 "Exemple : point zéro au nez de scène, QR code 3 m plus loin vers le lointain → 3")]
        [SerializeField] private float qrOffsetY = 0f;

        [Tooltip("Angle du QR code par rapport aux axes du bureau d'études, en degrés, sens inverse des aiguilles d'une " +
                 "montre vu de dessus. 0 = l'avant du QR code (son bord du haut s'il est au sol, sa perpendiculaire s'il est " +
                 "vertical) pointe exactement vers le Y+ du bureau d'études.")]
        [SerializeField] private float qrOffsetAngle = 0f;

        /// <summary>Distance (m) entre le bas du QR code et le sol du plateau.</summary>
        public float QrBottomHeightAboveFloor => qrBottomHeightAboveFloor;

        /// <summary>Position du QR code (point du sol sous son centre) vers la droite du point zéro, en mètres.</summary>
        public float QrOffsetX => qrOffsetX;

        /// <summary>Position du QR code (point du sol sous son centre) vers l'avant du point zéro, en mètres.</summary>
        public float QrOffsetY => qrOffsetY;

        /// <summary>Angle du QR code par rapport aux axes du bureau d'études, en degrés.</summary>
        public float QrOffsetAngle => qrOffsetAngle;

        /// <summary>
        /// Position et rotation à donner à StageRoot (enfant de QRAnchor) pour qu'il soit au point zéro
        /// du bureau d'études.
        /// </summary>
        public Pose ReferenceInQrFrame() => StagePlan.ReferenceInQrFrame(qrOffsetX, qrOffsetY, qrOffsetAngle);

        /// <summary>Hauteur du sol (repère du monde) à partir de la hauteur du bas du QR code.</summary>
        public float FloorHeight(float qrBottomY) => FloorHeight(qrBottomY, qrBottomHeightAboveFloor);

        /// <summary>Hauteur du sol : hauteur du bas du QR code moins sa distance au sol.</summary>
        public static float FloorHeight(float qrBottomY, float qrBottomHeightAboveFloor) => qrBottomY - qrBottomHeightAboveFloor;

        /// <summary>Charge le fichier de réglages du projet (null s'il n'existe pas).</summary>
        public static StageSettings LoadDefault() => Resources.Load<StageSettings>(ResourcePath);
    }
}

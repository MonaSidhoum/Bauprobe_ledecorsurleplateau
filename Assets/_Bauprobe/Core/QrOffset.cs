using System;
using UnityEngine;

namespace Bauprobe.Core
{
    /// <summary>
    /// Où est le QR code par rapport au point zéro de la scène (du bureau d'études), saisi dans le casque.
    /// Repère du plan (voir <see cref="StagePlan"/>) : X+ à droite (cour), Y+ vers l'avant (lointain).
    /// X = Y = 0 : le QR code EST le point zéro (en plan). La hauteur sert à retrouver le sol.
    /// </summary>
    [Serializable]
    public class QrOffset
    {
        /// <summary>Position X du QR code (point du sol sous son centre) depuis le point zéro, en mètres.</summary>
        public float x;

        /// <summary>Position Y du QR code (point du sol sous son centre) depuis le point zéro, en mètres.</summary>
        public float y;

        /// <summary>Distance verticale entre le BAS du QR code et le sol du plateau, en mètres (0 = posé au sol).</summary>
        public float height;

        public QrOffset() { }

        public QrOffset(float x, float y, float height)
        {
            this.x = x;
            this.y = y;
            this.height = Mathf.Max(0f, height);
        }

        public bool IsZero => IsSameAs(new QrOffset());

        public bool IsSameAs(QrOffset other, float tolerance = 1e-3f) =>
            other != null &&
            Mathf.Abs(x - other.x) < tolerance &&
            Mathf.Abs(y - other.y) < tolerance &&
            Mathf.Abs(height - other.height) < tolerance;

        /// <summary>Position et rotation du point zéro (StageRoot) dans le repère du QR code (QRAnchor).</summary>
        public Pose ReferenceInQrFrame() => StagePlan.ReferenceInQrFrame(x, y);

        /// <summary>Hauteur du sol (repère du monde) : hauteur du bas du QR code moins sa distance au sol.</summary>
        public float FloorHeight(float qrBottomY) => FloorHeight(qrBottomY, height);

        /// <summary>Hauteur du sol : hauteur du bas du QR code moins sa distance au sol.</summary>
        public static float FloorHeight(float qrBottomY, float qrBottomHeightAboveFloor) => qrBottomY - qrBottomHeightAboveFloor;

        public QrOffset Copy() => new QrOffset(x, y, height);

        public override string ToString() => $"X {x:0.###} m ; Y {y:0.###} m ; hauteur {height:0.###} m";
    }
}

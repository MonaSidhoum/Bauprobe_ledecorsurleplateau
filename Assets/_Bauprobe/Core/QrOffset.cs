using System;
using UnityEngine;

namespace Bauprobe.Core
{
    /// <summary>
    /// Offset du QR code : où est le QR code par rapport au point zéro de la scène (du bureau d'études).
    /// Repère du plan (voir <see cref="StagePlan"/>) : X+ à droite (cour), Y+ vers l'avant (lointain).
    /// Tout à 0 : le QR code EST le point zéro.
    /// </summary>
    [Serializable]
    public class QrOffset
    {
        /// <summary>Position X du QR code (point du sol sous son centre) depuis le point zéro, en mètres.</summary>
        public float x;

        /// <summary>Position Y du QR code (point du sol sous son centre) depuis le point zéro, en mètres.</summary>
        public float y;

        /// <summary>
        /// Angle de l'avant du QR code par rapport au Y+ du point zéro, en degrés, sens inverse des aiguilles
        /// d'une montre vu de dessus. L'avant est le bord du haut d'un QR code posé au sol, la perpendiculaire
        /// d'un QR code vertical.
        /// </summary>
        public float angle;

        public QrOffset() { }

        public QrOffset(float x, float y, float angle)
        {
            this.x = x;
            this.y = y;
            this.angle = angle;
        }

        public bool IsZero => IsSameAs(new QrOffset());

        public bool IsSameAs(QrOffset other, float tolerance = 1e-3f) =>
            other != null &&
            Mathf.Abs(x - other.x) < tolerance &&
            Mathf.Abs(y - other.y) < tolerance &&
            Mathf.Abs(angle - other.angle) < tolerance;

        /// <summary>Position et rotation du point zéro (StageRoot) dans le repère du QR code (QRAnchor).</summary>
        public Pose ReferenceInQrFrame() => StagePlan.ReferenceInQrFrame(x, y, angle);

        public QrOffset Copy() => new QrOffset(x, y, angle);

        public override string ToString() => $"X {x:0.###} m ; Y {y:0.###} m ; angle {angle:0.#}°";
    }
}

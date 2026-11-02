using System;
using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// Une face de la boîte englobante d'un objet de décor, dans son repère local :
    /// l'axe (X, Y ou Z) et le côté (+ ou -). Sert à « étirer » l'objet par une face.
    /// </summary>
    public readonly struct DecorFace : IEquatable<DecorFace>
    {
        /// <summary>Axe local : 0 = X (gauche / droite), 1 = Y (bas / haut), 2 = Z (arrière / avant).</summary>
        public readonly int Axis;

        /// <summary>+1 = face du côté positif de l'axe, -1 = côté négatif.</summary>
        public readonly int Sign;

        public DecorFace(int axis, int sign)
        {
            Axis = Mathf.Clamp(axis, 0, 2);
            Sign = sign >= 0 ? 1 : -1;
        }

        /// <summary>Normale de la face (vers l'extérieur), dans le repère local de l'objet.</summary>
        public Vector3 LocalNormal
        {
            get
            {
                var n = Vector3.zero;
                n[Axis] = Sign;
                return n;
            }
        }

        public bool Equals(DecorFace other) => Axis == other.Axis && Sign == other.Sign;
        public override bool Equals(object obj) => obj is DecorFace other && Equals(other);
        public override int GetHashCode() => Axis * 2 + (Sign > 0 ? 1 : 0);
        public override string ToString() => (Sign > 0 ? "+" : "-") + "XYZ"[Axis];
    }
}

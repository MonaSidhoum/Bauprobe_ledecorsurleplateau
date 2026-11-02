using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// Limites imposées par le cahier des charges (section 5, Contraintes).
    /// </summary>
    public static class DecorLimits
    {
        /// <summary>Aucun élément de décor ne doit dépasser cette taille (en mètres), dans aucune dimension.</summary>
        public const float MaxSizeMeters = 18f;

        /// <summary>Échelle minimale d'un objet sans fiche de catalogue (les décors du catalogue ont leurs propres limites).</summary>
        public const float DefaultMinScale = 0.01f;

        /// <summary>Échelle maximale d'un objet sans fiche de catalogue.</summary>
        public const float DefaultMaxScale = 100f;

        /// <summary>Longueur minimale (en mètres) d'un objet quand on le raccourcit en étirant une face.</summary>
        public const float MinFaceLength = 0.05f;

        /// <summary>
        /// Garde chaque axe de l'échelle entre <paramref name="min"/> et <paramref name="max"/>,
        /// en gardant les proportions de l'objet (un objet étiré reste étiré).
        /// </summary>
        public static Vector3 ClampScaleToRange(Vector3 scale, float min, float max)
        {
            var largest = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            var smallest = Mathf.Min(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            if (largest <= 0f)
            {
                return scale;
            }

            var factor = 1f;
            if (largest > max)
            {
                factor = max / largest;
            }
            if (smallest * factor < min && smallest > 0f)
            {
                factor = min / smallest; // la taille minimale passe avant la maximale
            }

            return scale * factor;
        }

        /// <summary>
        /// Réduit l'échelle <paramref name="scale"/> si l'objet dépasse <see cref="MaxSizeMeters"/>
        /// sur sa plus grande dimension. Les proportions sont gardées.
        /// </summary>
        /// <param name="localSize">Taille de l'objet à l'échelle 1 (sans son échelle), en mètres.</param>
        public static Vector3 ClampScaleToMaxSize(Vector3 localSize, Vector3 scale)
        {
            var largest = Mathf.Max(
                Mathf.Abs(localSize.x * scale.x),
                Mathf.Abs(localSize.y * scale.y),
                Mathf.Abs(localSize.z * scale.z));

            if (largest <= MaxSizeMeters)
            {
                return scale;
            }

            return scale * (MaxSizeMeters / largest);
        }
    }
}

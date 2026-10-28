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

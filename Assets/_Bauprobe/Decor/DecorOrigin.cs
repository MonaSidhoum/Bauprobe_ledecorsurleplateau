using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>Comment l'élément est accroché sur le plateau (cahier des charges, section 4).</summary>
    public enum DecorAttachment
    {
        /// <summary>Posé au sol : le point d'origine est le point le plus bas, au centre de l'objet.</summary>
        Sol = 0,

        /// <summary>Accroché au cintre : le point d'origine est le point le plus haut, au centre de l'objet.</summary>
        Cintre = 1,
    }

    /// <summary>
    /// Calcule le "point d'origine" d'un élément de décor à partir de ses modèles 3D.
    ///
    /// Grâce à ce calcul, le pivot des modèles du bureau d'études peut être n'importe où :
    /// on n'a pas besoin de le corriger à la main dans Blender ou dans Unity.
    /// </summary>
    public static class DecorOrigin
    {
        /// <summary>
        /// Boîte englobante des modèles 3D de l'objet, exprimée dans le repère local de <paramref name="root"/>
        /// (donc sans l'échelle, la rotation ni la position de <paramref name="root"/>).
        /// Renvoie false si l'objet n'a aucun modèle 3D visible.
        /// </summary>
        public static bool TryGetLocalBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            var found = false;
            var toRoot = root.worldToLocalMatrix;

            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (filter.sharedMesh == null || renderer == null || !renderer.enabled)
                {
                    continue;
                }

                Encapsulate(ref bounds, ref found, filter.sharedMesh.bounds, toRoot * filter.transform.localToWorldMatrix);
            }

            foreach (var skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinned.sharedMesh == null || !skinned.enabled)
                {
                    continue;
                }

                // Les bounds d'un SkinnedMeshRenderer sont exprimées dans le repère de son os racine.
                var space = skinned.rootBone != null ? skinned.rootBone : skinned.transform;
                Encapsulate(ref bounds, ref found, skinned.localBounds, toRoot * space.localToWorldMatrix);
            }

            return found;
        }

        /// <summary>
        /// Point d'origine dans le repère local de <paramref name="root"/> :
        /// centre de la boîte en X et Z ; en Y, le bas (sol) ou le haut (cintre).
        /// Sans modèle 3D, renvoie le pivot (0, 0, 0).
        /// </summary>
        public static Vector3 ComputeLocalOrigin(Transform root, DecorAttachment attachment)
        {
            if (!TryGetLocalBounds(root, out var b))
            {
                return Vector3.zero;
            }

            var y = attachment == DecorAttachment.Cintre ? b.max.y : b.min.y;
            return new Vector3(b.center.x, y, b.center.z);
        }

        // Ajoute les 8 coins d'une boîte (transformés par "matrix") à "bounds".
        private static void Encapsulate(ref Bounds bounds, ref bool found, Bounds box, Matrix4x4 matrix)
        {
            var min = box.min;
            var max = box.max;
            for (var i = 0; i < 8; i++)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? min.x : max.x,
                    (i & 2) == 0 ? min.y : max.y,
                    (i & 4) == 0 ? min.z : max.z);
                var p = matrix.MultiplyPoint3x4(corner);

                if (!found)
                {
                    bounds = new Bounds(p, Vector3.zero);
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(p);
                }
            }
        }
    }
}

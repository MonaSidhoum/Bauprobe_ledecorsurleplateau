using UnityEditor;
using UnityEngine;

namespace Bauprobe.Decor.EditorTools
{
    /// <summary>
    /// Deux raccourcis pour préparer un prefab de décor, accessibles par clic droit
    /// sur le composant (ou ses ⋮) dans l'Inspector, en mode Prefab :
    ///
    ///  - Transform   > « Bauprobe : poser les modèles sur le pivot (centre-bas) »
    ///      Déplace les modèles enfants pour que le pivot de la racine soit au centre de la base du décor.
    ///      Indispensable : DecorConstraints colle le pivot de la racine au sol du plateau.
    ///
    ///  - BoxCollider > « Bauprobe : ajuster aux modèles 3D enfants »
    ///      Englobe tous les modèles enfants et affiche leurs dimensions en mètres dans la Console
    ///      (pratique pour vérifier l'échelle par rapport aux plans).
    /// </summary>
    static class DecorPrefabTools
    {
        [MenuItem("CONTEXT/Transform/Bauprobe : poser les modèles sur le pivot (centre-bas)")]
        static void PlaceModelsOnPivot(MenuCommand command)
        {
            var root = (Transform)command.context;
            if (!TryGetLocalBounds(root, out var b)) return;

            // Décalage exprimé dans le repère de la racine (même repère que localPosition des enfants directs)
            var offset = new Vector3(-b.center.x, -b.min.y, -b.center.z);

            foreach (Transform child in root)
            {
                // On ne déplace que les enfants qui contiennent du visuel (pas ISDK_HandGrabInteraction, etc.)
                if (child.GetComponentInChildren<Renderer>(true) == null) continue;

                Undo.RecordObject(child, "Poser les modèles sur le pivot");
                child.localPosition += offset;
            }

            Debug.Log($"[Decor] {root.name} : modèles décalés de {offset} m.", root);
        }

        [MenuItem("CONTEXT/BoxCollider/Bauprobe : ajuster aux modèles 3D enfants")]
        static void FitBoxCollider(MenuCommand command)
        {
            var box = (BoxCollider)command.context;
            if (!TryGetLocalBounds(box.transform, out var b)) return;

            Undo.RecordObject(box, "Ajuster le BoxCollider");
            box.center = b.center;
            box.size = b.size;

            Debug.Log($"[Decor] {box.name} : {b.size.x:0.00} m (largeur) x {b.size.y:0.00} m (hauteur) x {b.size.z:0.00} m (profondeur).", box);
        }

        // Boîte englobant tous les Renderers enfants, exprimée dans le repère local de 'root'.
        static bool TryGetLocalBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            bool found = false;
            Matrix4x4 worldToRoot = root.worldToLocalMatrix;

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                Bounds source;
                Matrix4x4 toRoot;

                var filter = r.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    source = filter.sharedMesh.bounds;                       // boîte du maillage, repère du maillage
                    toRoot = worldToRoot * r.transform.localToWorldMatrix;
                }
                else
                {
                    source = r.bounds;                                       // boîte en repère monde
                    toRoot = worldToRoot;
                }

                Vector3 c = source.center, e = source.extents;
                for (int i = 0; i < 8; i++)
                {
                    var corner = c + Vector3.Scale(e, new Vector3(
                        (i & 1) == 0 ? -1f : 1f,
                        (i & 2) == 0 ? -1f : 1f,
                        (i & 4) == 0 ? -1f : 1f));
                    var p = toRoot.MultiplyPoint3x4(corner);

                    if (!found) { bounds = new Bounds(p, Vector3.zero); found = true; }
                    else bounds.Encapsulate(p);
                }
            }

            if (!found)
                Debug.LogWarning($"[Decor] {root.name} : aucun modèle 3D trouvé dans les enfants.", root);

            return found;
        }
    }
}

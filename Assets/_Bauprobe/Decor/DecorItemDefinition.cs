using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// La "fiche" d'un élément de décor (une table, un escalier, un mur...).
    ///
    /// Pour en créer une dans Unity : clic droit dans le dossier Decor/Catalog
    /// > Create > Bauprobe > Élément de décor. Pas besoin d'écrire de code.
    /// </summary>
    [CreateAssetMenu(menuName = "Bauprobe/Élément de décor", fileName = "NouveauDecor")]
    public class DecorItemDefinition : ScriptableObject
    {
        [Tooltip("Identifiant unique utilisé dans les sauvegardes. Ne plus le changer une fois utilisé.")]
        [SerializeField] private string id;

        [Tooltip("Nom affiché dans le menu.")]
        [SerializeField] private string displayName;

        [Tooltip("Petite image affichée dans le menu (optionnelle).")]
        [SerializeField] private Sprite thumbnail;

        [Tooltip("Le prefab 3D à faire apparaître, à l'échelle 1:1 en mètres. Son pivot peut être n'importe où : " +
                 "le point d'origine est calculé automatiquement à partir des modèles 3D.")]
        [SerializeField] private GameObject prefab;

        [Header("Point d'origine")]
        [Tooltip("Sol : le point d'origine est le point le plus bas, au centre de l'objet (il est posé sur le plateau).\n" +
                 "Cintre : le point d'origine est le point le plus haut, au centre de l'objet (il est accroché).")]
        [SerializeField] private DecorAttachment attachment = DecorAttachment.Sol;

        [Tooltip("Pour un élément au cintre : hauteur (en mètres au-dessus du plateau) de son point le plus haut " +
                 "quand on l'importe depuis le menu.")]
        [SerializeField, Min(0f)] private float hangHeight = 3f;

        [Header("Limites de taille")]
        [Tooltip("Échelle minimale de ce décor (1 = taille réelle du modèle). Seule limite utilisée par le Scale, " +
                 "à la manette comme à la main.")]
        [SerializeField, Min(0.01f)] private float minScale = 0.1f;

        [Tooltip("Échelle maximale de ce décor (1 = taille réelle du modèle). En plus, aucun décor ne dépasse jamais 18 m " +
                 "(cahier des charges).")]
        [SerializeField, Min(0.01f)] private float maxScale = 10f;

        public string Id => id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Sprite Thumbnail => thumbnail;
        public GameObject Prefab => prefab;
        public DecorAttachment Attachment => attachment;
        public float HangHeight => hangHeight;
        public float MinScale => minScale;
        public float MaxScale => maxScale;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }

            if (maxScale < minScale)
            {
                maxScale = minScale;
            }
        }
    }
}

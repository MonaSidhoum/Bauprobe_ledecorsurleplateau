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

        [Tooltip("Le prefab 3D à faire apparaître. Mettre son pivot au sol, à l'échelle 1:1 en mètres.")]
        [SerializeField] private GameObject prefab;

        [Header("Limites de taille")]
        [SerializeField, Min(0.01f)] private float minScale = 0.1f;
        [SerializeField, Min(0.01f)] private float maxScale = 10f;

        public string Id => id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Sprite Thumbnail => thumbnail;
        public GameObject Prefab => prefab;
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

using System.Collections.Generic;
using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// La liste de tous les éléments de décor disponibles dans l'appli.
    /// Clic droit > Create > Bauprobe > Catalogue de décor.
    /// </summary>
    [CreateAssetMenu(menuName = "Bauprobe/Catalogue de décor", fileName = "DecorCatalog")]
    public class DecorCatalog : ScriptableObject
    {
        [SerializeField] private List<DecorItemDefinition> items = new List<DecorItemDefinition>();

        public IReadOnlyList<DecorItemDefinition> Items => items;

        /// <summary>Retrouve une fiche par son identifiant (null si absente).</summary>
        public DecorItemDefinition Find(string id)
        {
            foreach (var item in items)
            {
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }
    }
}

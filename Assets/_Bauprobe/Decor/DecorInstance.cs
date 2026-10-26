using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// Ajouté automatiquement sur chaque objet de décor posé sur le plateau.
    /// Il se souvient de quelle fiche (DecorItemDefinition) l'objet vient.
    /// </summary>
    public class DecorInstance : MonoBehaviour
    {
        public DecorItemDefinition Definition { get; private set; }

        public void Initialize(DecorItemDefinition definition)
        {
            Definition = definition;
        }
    }
}

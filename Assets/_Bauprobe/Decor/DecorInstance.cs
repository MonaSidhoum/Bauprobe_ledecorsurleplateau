using System;
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

        /// <summary>Appelé quand l'objet est détruit (par ex. bouton « Supprimer » du menu de l'objet).</summary>
        public event Action<DecorInstance> Destroyed;

        public void Initialize(DecorItemDefinition definition)
        {
            Definition = definition;
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
    }
}

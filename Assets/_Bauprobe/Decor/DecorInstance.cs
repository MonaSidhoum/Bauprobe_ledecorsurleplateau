using System;
using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// Ajouté automatiquement sur chaque objet de décor posé sur le plateau.
    /// Il se souvient de quelle fiche (DecorItemDefinition) l'objet vient,
    /// et connaît son "point d'origine" (bas au centre pour le sol, haut au centre pour le cintre).
    /// </summary>
    public class DecorInstance : MonoBehaviour
    {
        public DecorItemDefinition Definition { get; private set; }

        /// <summary>Sol ou cintre (Sol si l'objet n'a pas de fiche).</summary>
        public DecorAttachment Attachment => Definition != null ? Definition.Attachment : DecorAttachment.Sol;

        /// <summary>Appelé quand l'objet est détruit (par ex. bouton « Supprimer » du menu de l'objet).</summary>
        public event Action<DecorInstance> Destroyed;

        private Vector3 _originLocal;
        private Vector3 _localSize;
        private bool _originComputed;

        public void Initialize(DecorItemDefinition definition)
        {
            Definition = definition;
            RecomputeOrigin();
        }

        /// <summary>
        /// Point d'origine dans le repère local de l'objet (sans son échelle).
        /// Calculé une fois, à partir des modèles 3D de l'objet.
        /// </summary>
        public Vector3 OriginLocal
        {
            get
            {
                if (!_originComputed)
                {
                    RecomputeOrigin();
                }
                return _originLocal;
            }
        }

        /// <summary>
        /// Taille de l'objet à l'échelle 1 (sans son échelle), en mètres, calculée à partir de ses modèles 3D.
        /// Sa taille réelle sur le plateau est cette taille multipliée par son échelle.
        /// </summary>
        public Vector3 LocalSize
        {
            get
            {
                if (!_originComputed)
                {
                    RecomputeOrigin();
                }
                return _localSize;
            }
        }

        /// <summary>Taille réelle de l'objet sur le plateau, en mètres.</summary>
        public Vector3 StageSize => Vector3.Scale(LocalSize, transform.localScale);

        /// <summary>
        /// Réduit l'échelle demandée si l'objet dépasserait 18 m sur sa plus grande dimension
        /// (cahier des charges). Les proportions sont gardées.
        /// </summary>
        public Vector3 ClampScaleToMaxSize(Vector3 localScale) => DecorLimits.ClampScaleToMaxSize(LocalSize, localScale);

        /// <summary>À rappeler si les modèles 3D de l'objet changent.</summary>
        public void RecomputeOrigin()
        {
            if (DecorOrigin.TryGetLocalBounds(transform, out var bounds))
            {
                _originLocal = DecorOrigin.OriginFromBounds(bounds, Attachment);
                _localSize = bounds.size;
            }
            else
            {
                _originLocal = Vector3.zero;
                _localSize = Vector3.zero;
            }
            _originComputed = true;
        }

        /// <summary>Position du point d'origine dans le monde.</summary>
        public Vector3 OriginWorldPosition => transform.TransformPoint(OriginLocal);

        /// <summary>Déplace l'objet pour que son point d'origine soit à cette position (monde).</summary>
        public void SetOriginWorldPosition(Vector3 world)
        {
            transform.position += world - OriginWorldPosition;
        }

        /// <summary>
        /// Position du point d'origine dans le repère du plateau (le parent de l'objet).
        /// C'est cette position qui est sauvegardée et qui ira dans le tableau de positionnement.
        /// </summary>
        public Vector3 OriginStagePosition
        {
            get
            {
                var parent = transform.parent;
                return parent != null ? parent.InverseTransformPoint(OriginWorldPosition) : OriginWorldPosition;
            }
        }

        /// <summary>Déplace l'objet pour que son point d'origine soit à cette position (repère du plateau).</summary>
        public void SetOriginStagePosition(Vector3 stagePosition)
        {
            var parent = transform.parent;
            SetOriginWorldPosition(parent != null ? parent.TransformPoint(stagePosition) : stagePosition);
        }

        /// <summary>
        /// Change l'échelle de l'objet sans bouger son point d'origine :
        /// un objet au sol grandit vers le haut, un objet au cintre grandit vers le bas.
        /// </summary>
        public void SetScaleAroundOrigin(Vector3 localScale)
        {
            var origin = OriginWorldPosition;
            transform.localScale = localScale;
            SetOriginWorldPosition(origin);
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
    }
}

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
        /// Si l'objet est penché, son point le plus bas (sol) ou le plus haut (cintre) reste aussi à la même hauteur.
        /// </summary>
        public void SetScaleAroundOrigin(Vector3 localScale)
        {
            var height = AttachmentHeight();
            var origin = OriginWorldPosition;
            transform.localScale = localScale;
            SetOriginWorldPosition(origin);
            RestoreAttachmentHeight(height);
        }

        /// <summary>
        /// Tourne l'objet autour de son centre. Son point le plus bas (sol) ou le plus haut (cintre)
        /// reste à la même hauteur : un objet posé au sol que l'on penche reste posé, sans s'enfoncer.
        /// </summary>
        /// <param name="worldAxis">Axe de rotation, dans le repère du monde.</param>
        /// <param name="degrees">Angle en degrés.</param>
        public void RotateAroundCenter(Vector3 worldAxis, float degrees)
        {
            if (Mathf.Approximately(degrees, 0f))
            {
                return;
            }

            var height = AttachmentHeight();
            var center = DecorOrigin.TryGetBounds(transform, Matrix4x4.identity, out var world) ? world.center : transform.position;
            transform.RotateAround(center, worldAxis, degrees);
            RestoreAttachmentHeight(height);
        }

        /// <summary>
        /// Empêche l'objet de passer sous le plateau : si son point le plus bas est sous le sol,
        /// il est remonté juste au niveau du sol.
        /// </summary>
        public void KeepAboveStage()
        {
            if (TryGetStageBounds(out var b) && b.min.y < 0f)
            {
                transform.localPosition += new Vector3(0f, -b.min.y, 0f);
            }
        }

        /// <summary>Pose le point le plus bas de l'objet exactement sur le plateau.</summary>
        public void RestOnStage()
        {
            if (TryGetStageBounds(out var b) && !Mathf.Approximately(b.min.y, 0f))
            {
                transform.localPosition += new Vector3(0f, -b.min.y, 0f);
            }
        }

        /// <summary>
        /// Boîte englobante de l'objet dans le repère du plateau (le parent), en tenant compte
        /// de sa position, de sa rotation et de son échelle.
        /// </summary>
        public bool TryGetStageBounds(out Bounds bounds)
        {
            var parent = transform.parent;
            return DecorOrigin.TryGetBounds(transform, parent != null ? parent.worldToLocalMatrix : Matrix4x4.identity, out bounds);
        }

        // Hauteur (repère du plateau) du point qui doit rester fixe : le plus bas pour le sol, le plus haut pour le cintre.
        private float? AttachmentHeight()
        {
            if (!TryGetStageBounds(out var b))
            {
                return null;
            }
            return Attachment == DecorAttachment.Cintre ? b.max.y : b.min.y;
        }

        private void RestoreAttachmentHeight(float? height)
        {
            var now = AttachmentHeight();
            if (height.HasValue && now.HasValue)
            {
                transform.localPosition += new Vector3(0f, height.Value - now.Value, 0f);
            }
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke(this);
        }
    }
}

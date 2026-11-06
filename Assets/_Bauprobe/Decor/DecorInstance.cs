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
        private Bounds _localBounds;
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

        /// <summary>Boîte englobante des modèles 3D, dans le repère local de l'objet (sans son échelle).</summary>
        public Bounds LocalBounds
        {
            get
            {
                if (!_originComputed)
                {
                    RecomputeOrigin();
                }
                return _localBounds;
            }
        }

        /// <summary>Taille réelle de l'objet sur le plateau, en mètres.</summary>
        public Vector3 StageSize => Vector3.Scale(LocalSize, transform.localScale);

        /// <summary>À rappeler si les modèles 3D de l'objet changent.</summary>
        public void RecomputeOrigin()
        {
            if (DecorOrigin.TryGetLocalBounds(transform, out var bounds))
            {
                _originLocal = DecorOrigin.OriginFromBounds(bounds, Attachment);
                _localSize = bounds.size;
                _localBounds = bounds;
            }
            else
            {
                _originLocal = Vector3.zero;
                _localSize = Vector3.zero;
                _localBounds = new Bounds(Vector3.zero, Vector3.zero);
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
        /// Aligne l'objet avec le plateau en gardant la bascule la plus proche : parmi les 6 directions de l'objet
        /// (haut, bas, avant, arrière, gauche, droite), celle qui est la plus proche de la verticale devient
        /// exactement verticale. L'objet est donc posé à plat sur la face la plus proche de sa position actuelle
        /// (un objet penché à 20° se redresse, un objet penché à 70° se couche sur le côté), avec la plus petite
        /// rotation possible. La rotation se fait autour de son centre ; son point le plus bas (sol) ou le plus
        /// haut (cintre) reste à la même hauteur.
        /// </summary>
        public void LevelWithStage()
        {
            var up = transform.parent != null ? transform.parent.up : Vector3.up;

            // Direction de l'objet la plus proche de la verticale
            Vector3[] axes =
            {
                transform.up, -transform.up, transform.right, -transform.right, transform.forward, -transform.forward,
            };
            var closest = axes[0];
            foreach (var axis in axes)
            {
                if (Vector3.Dot(axis, up) > Vector3.Dot(closest, up))
                {
                    closest = axis;
                }
            }

            var delta = Quaternion.FromToRotation(closest, up);
            if (Quaternion.Angle(delta, Quaternion.identity) < 0.01f)
            {
                return; // déjà à plat
            }

            delta.ToAngleAxis(out var degrees, out var rotationAxis);
            RotateAroundCenter(rotationAxis, degrees);
        }

        // ------------------------------------------------------------------
        // Étirer une face (« extrusion » de la boîte englobante)
        // ------------------------------------------------------------------

        /// <summary>Longueur réelle de l'objet le long d'un de ses axes locaux (0 = X, 1 = Y, 2 = Z), en mètres.</summary>
        public float GetLength(int axis) => LocalSize[axis] * Mathf.Abs(transform.localScale[axis]);

        /// <summary>
        /// Face de l'objet la plus proche d'un point (par ex. l'endroit touché par le laser).
        /// Renvoie false si l'objet n'a pas de modèle 3D.
        /// </summary>
        public bool TryGetFace(Vector3 worldPoint, out DecorFace face)
        {
            face = default;
            var b = LocalBounds;
            var size = b.size;
            var scale = transform.localScale;
            var p = transform.InverseTransformPoint(worldPoint);
            var best = float.MaxValue;

            for (var i = 0; i < 3; i++)
            {
                if (size[i] < 1e-5f)
                {
                    continue;
                }

                // Distances en mètres (on tient compte de l'échelle de l'objet)
                var k = Mathf.Abs(scale[i]);
                var toMax = Mathf.Abs(p[i] - b.max[i]) * k;
                var toMin = Mathf.Abs(p[i] - b.min[i]) * k;
                if (toMax < best) { best = toMax; face = new DecorFace(i, 1); }
                if (toMin < best) { best = toMin; face = new DecorFace(i, -1); }
            }

            return best < float.MaxValue;
        }

        /// <summary>
        /// Position et dimensions d'une face, dans le monde : son centre, sa normale (vers l'extérieur)
        /// et ses deux côtés (vecteurs dont la longueur est celle du côté).
        /// </summary>
        public void GetFaceGeometry(DecorFace face, out Vector3 center, out Vector3 normal, out Vector3 sideU, out Vector3 sideV)
        {
            var b = LocalBounds;
            int i = face.Axis, u = (i + 1) % 3, v = (i + 2) % 3;

            var c = b.center;
            c[i] = face.Sign > 0 ? b.max[i] : b.min[i];
            center = transform.TransformPoint(c);

            var du = Vector3.zero;
            du[u] = b.size[u];
            var dv = Vector3.zero;
            dv[v] = b.size[v];
            sideU = transform.TransformVector(du);
            sideV = transform.TransformVector(dv);

            // Une échelle négative retourne la face : on en tient compte pour garder la normale vers l'extérieur.
            var localNormal = face.LocalNormal * Mathf.Sign(transform.localScale[i] == 0f ? 1f : transform.localScale[i]);
            normal = (transform.rotation * localNormal).normalized;
        }

        /// <summary>
        /// Étire (ou raccourcit) l'objet en poussant une de ses faces : la face opposée ne bouge pas.
        /// Un cube devient ainsi un pavé (par ex. pour prolonger une porte).
        /// <paramref name="deltaMeters"/> &gt; 0 allonge, &lt; 0 raccourcit.
        /// La longueur ne descend pas sous 5 cm ; pas de longueur maximale.
        /// Un objet au sol reste posé (un objet au cintre reste accroché), même si on tire sa face du bas (du haut).
        /// Renvoie la nouvelle longueur, en mètres.
        /// </summary>
        public float ExtrudeFace(DecorFace face, float deltaMeters)
        {
            var i = face.Axis;
            var b = LocalBounds;
            var size = b.size[i];
            if (size < 1e-5f)
            {
                return 0f;
            }

            var scale = transform.localScale;
            var length = size * Mathf.Abs(scale[i]);
            var newLength = Mathf.Max(DecorLimits.MinFaceLength, length + deltaMeters);
            if (Mathf.Approximately(newLength, length))
            {
                return length;
            }

            // Centre de la face opposée : ce point ne doit pas bouger.
            var fixedPoint = b.center;
            fixedPoint[i] = face.Sign > 0 ? b.min[i] : b.max[i];
            var fixedWorld = transform.TransformPoint(fixedPoint);
            var height = AttachmentHeight();

            scale[i] = (scale[i] < 0f ? -1f : 1f) * newLength / size;
            transform.localScale = scale;
            transform.position += fixedWorld - transform.TransformPoint(fixedPoint);

            RestoreAttachmentHeight(height);
            return newLength;
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

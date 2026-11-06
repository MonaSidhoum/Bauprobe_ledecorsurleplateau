using Bauprobe.Decor;
using Oculus.Interaction;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Comportement de la saisie « grab » (à une ou deux mains) de l'Interaction SDK de Meta, pour les décors.
    ///
    /// Remplace le comportement par défaut du SDK, qui posait deux problèmes :
    ///   1. à deux mains, l'objet revenait à sa taille de départ : toutes les modifications (Scale, EXTRUDE…)
    ///      étaient perdues. Ici, on part TOUJOURS de la taille et de la position actuelles de l'objet ;
    ///   2. l'objet tournait dans tous les sens (360°). Ici, il reste parallèle au sol : seule la rotation
    ///      autour de la verticale suit la main (s'il était penché avec le mode Rotation, il le reste).
    ///
    /// Une main : l'objet suit la main (position + rotation autour de la verticale).
    /// Deux mains : il suit le milieu des deux mains, tourne autour de la verticale avec elles,
    /// et s'agrandit / rétrécit quand on écarte / rapproche les mains (en gardant ses proportions).
    /// Les calculs sont dans <see cref="GrabMath"/>.
    ///
    /// Ajouté automatiquement par <see cref="InteractableObject"/> sur chaque objet qui a un Grabbable :
    /// rien à ajouter dans les prefabs.
    /// </summary>
    public class DecorGrabTransformer : MonoBehaviour, ITransformer
    {
        [Tooltip("Garde l'objet parallèle au sol pendant le grab : seule la rotation autour de la verticale suit la main.")]
        [SerializeField] bool keepLevel = true;

        [Tooltip("À deux mains, écarter / rapprocher les mains agrandit / rétrécit l'objet.")]
        [SerializeField] bool twoHandScale = true;

        [Tooltip("Empêche l'objet de passer sous le sol du plateau pendant le grab.")]
        [SerializeField] bool keepAboveStage = true;

        IGrabbable _grabbable;
        DecorInstance _decor;
        int _pointCount;

        // État au début de la saisie (ou quand le nombre de mains change)
        Pose _startObject;
        Vector3 _startScale;
        Vector3 _offsetPosition;      // une main
        Quaternion _offsetRotation;   // une main
        Vector3 _startA, _startB;     // deux mains

        /// <summary>Remplace le comportement de saisie d'un Grabbable par celui-ci. À appeler dans un Awake.</summary>
        public static DecorGrabTransformer Install(Grabbable grabbable)
        {
            if (grabbable == null) return null;

            var transformer = grabbable.GetComponent<DecorGrabTransformer>();
            if (transformer == null) transformer = grabbable.gameObject.AddComponent<DecorGrabTransformer>();

            // Le Grabbable appelle Initialize(this) dans son Start sur les transformers qu'on lui donne
            // (et n'ajoute plus le sien par défaut).
            grabbable.InjectOptionalOneGrabTransformer(transformer);
            grabbable.InjectOptionalTwoGrabTransformer(transformer);
            return transformer;
        }

        public void Initialize(IGrabbable grabbable)
        {
            _grabbable = grabbable;
            _decor = grabbable.Transform.GetComponent<DecorInstance>();
        }

        public void BeginTransform()
        {
            Capture();
        }

        public void UpdateTransform()
        {
            var points = _grabbable.GrabPoints;
            if (points.Count == 0) return;

            // Une main en plus ou en moins : on repart de l'état actuel (pas de saut, pas de retour en arrière)
            if (points.Count != _pointCount) Capture();

            var target = _grabbable.Transform;
            if (_pointCount == 1)
            {
                var pose = GrabMath.OneHand(points[0], _offsetPosition, _offsetRotation, keepLevel);
                target.SetPositionAndRotation(pose.position, pose.rotation);
            }
            else
            {
                GrabMath.TwoHands(_startObject, _startScale, _startA, _startB, points[0].position, points[1].position,
                                  keepLevel, twoHandScale, out var pose, out var scale);
                target.localScale = scale;
                target.SetPositionAndRotation(pose.position, pose.rotation);
            }

            if (keepAboveStage && _decor != null) _decor.KeepAboveStage();
        }

        public void EndTransform()
        {
            _pointCount = 0;
        }

        // Mémorise l'état actuel de l'objet et des mains.
        void Capture()
        {
            var target = _grabbable.Transform;
            var points = _grabbable.GrabPoints;
            _pointCount = points.Count;

            _startObject = new Pose(target.position, target.rotation);
            _startScale = target.localScale;

            if (_pointCount == 1)
            {
                GrabMath.OneHandOffsets(points[0], _startObject, keepLevel, out _offsetPosition, out _offsetRotation);
            }
            else if (_pointCount >= 2)
            {
                _startA = points[0].position;
                _startB = points[1].position;
            }
        }
    }
}

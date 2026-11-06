using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Calculs de la saisie à la main (grab) utilisés par <see cref="DecorGrabTransformer"/>.
    /// Séparés pour pouvoir être testés sans casque ni Interaction SDK.
    /// </summary>
    public static class GrabMath
    {
        /// <summary>
        /// Repère de la main : sa rotation complète, ou (keepLevel) seulement sa rotation autour de la verticale.
        /// </summary>
        public static Quaternion HandFrame(Quaternion handRotation, bool keepLevel)
        {
            if (!keepLevel) return handRotation;

            Vector3 forward = Vector3.ProjectOnPlane(handRotation * Vector3.forward, Vector3.up);
            if (forward.sqrMagnitude < 1e-6f)
            {
                // Main tournée vers le haut ou le bas : on prend son axe « haut » à la place
                forward = Vector3.ProjectOnPlane(handRotation * Vector3.up, Vector3.up);
            }
            return forward.sqrMagnitude < 1e-6f ? Quaternion.identity : Quaternion.LookRotation(forward, Vector3.up);
        }

        /// <summary>Une main : position et rotation de l'objet dans le repère de la main, au début de la saisie.</summary>
        public static void OneHandOffsets(Pose hand, Pose obj, bool keepLevel, out Vector3 offsetPosition, out Quaternion offsetRotation)
        {
            var inverse = Quaternion.Inverse(HandFrame(hand.rotation, keepLevel));
            offsetPosition = inverse * (obj.position - hand.position);
            offsetRotation = inverse * obj.rotation;
        }

        /// <summary>Une main : nouvelle pose de l'objet quand la main a bougé.</summary>
        public static Pose OneHand(Pose hand, Vector3 offsetPosition, Quaternion offsetRotation, bool keepLevel)
        {
            var frame = HandFrame(hand.rotation, keepLevel);
            return new Pose(hand.position + frame * offsetPosition, frame * offsetRotation);
        }

        /// <summary>
        /// Deux mains : l'objet suit le milieu des mains, tourne avec elles (autour de la verticale si keepLevel)
        /// et change de taille avec leur écart (si allowScale), à partir de son état au début de la saisie.
        /// </summary>
        /// <param name="startObject">Pose de l'objet au début de la saisie.</param>
        /// <param name="startScale">Échelle de l'objet au début (gardée telle quelle : un objet étiré reste étiré).</param>
        /// <param name="a0">Main 1 au début.</param>
        /// <param name="b0">Main 2 au début.</param>
        /// <param name="a">Main 1 maintenant.</param>
        /// <param name="b">Main 2 maintenant.</param>
        public static void TwoHands(Pose startObject, Vector3 startScale, Vector3 a0, Vector3 b0, Vector3 a, Vector3 b,
                                    bool keepLevel, bool allowScale, out Pose result, out Vector3 scale)
        {
            Vector3 startCenter = (a0 + b0) * 0.5f;
            Vector3 center = (a + b) * 0.5f;

            Vector3 startDirection = keepLevel ? Vector3.ProjectOnPlane(b0 - a0, Vector3.up) : b0 - a0;
            Vector3 direction = keepLevel ? Vector3.ProjectOnPlane(b - a, Vector3.up) : b - a;

            Quaternion turn = Quaternion.identity;
            if (startDirection.sqrMagnitude > 1e-6f && direction.sqrMagnitude > 1e-6f)
            {
                turn = keepLevel
                    ? Quaternion.AngleAxis(Vector3.SignedAngle(startDirection, direction, Vector3.up), Vector3.up)
                    : Quaternion.FromToRotation(startDirection, direction);
            }

            float factor = 1f;
            if (allowScale)
            {
                float startDistance = Mathf.Max(1e-3f, (b0 - a0).magnitude);
                factor = Mathf.Max(0.01f, (b - a).magnitude / startDistance);
            }

            scale = startScale * factor;
            result = new Pose(center + turn * ((startObject.position - startCenter) * factor), turn * startObject.rotation);
        }
    }
}

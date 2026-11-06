using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Règles "métier" appliquées pendant qu'on manipule un objet de décor.
    ///
    /// La saisie (attraper, déplacer, tourner, agrandir à deux mains) est faite par
    /// l'Interaction SDK de Meta (composant Grabbable). Ce script passe juste après
    /// pour garder l'objet posé au sol et droit. La taille n'est pas limitée.
    ///
    /// "Posé au sol" utilise le point d'origine de l'objet (le bas de ses modèles 3D) :
    /// le pivot du modèle peut donc être n'importe où.
    /// À mettre sur le prefab de chaque décor, à côté du Grabbable.
    /// </summary>
    public class DecorConstraints : MonoBehaviour
    {
        [Tooltip("Garde le bas de l'objet sur le plateau (sans effet sur un élément au cintre).")]
        [SerializeField] private bool keepOnFloor = true;

        [Tooltip("Empêche de pencher l'objet : il ne peut tourner que sur lui-même. " +
                 "Décoché, on peut aussi le basculer avec le joystick haut / bas en mode Rotation.")]
        [SerializeField] private bool keepUpright = true;

        private DecorInstance _instance;

        private void Awake()
        {
            _instance = GetComponent<DecorInstance>();
        }

        private void LateUpdate()
        {
            // Les positions sont locales : on raisonne dans le repère du plateau (le parent).
            var t = transform;

            // DecorInstance est ajouté au moment de l'apparition, après Awake.
            if (_instance == null)
            {
                _instance = GetComponent<DecorInstance>();
            }

            if (keepUpright)
            {
                t.localRotation = Quaternion.Euler(0f, t.localEulerAngles.y, 0f);
            }

            if (keepOnFloor)
            {
                KeepOnFloor(t);
            }
        }

        private void KeepOnFloor(Transform t)
        {
            if (_instance == null)
            {
                // Pas de point d'origine connu : on se rabat sur le pivot.
                var p = t.localPosition;
                t.localPosition = new Vector3(p.x, 0f, p.z);
                return;
            }

            if (_instance.Attachment != DecorAttachment.Sol)
            {
                return;
            }

            // Le point le plus bas de l'objet (même s'il est penché) touche le plateau.
            _instance.RestOnStage();
        }
    }
}

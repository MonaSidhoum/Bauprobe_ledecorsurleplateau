using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Règles "métier" appliquées pendant qu'on manipule un objet de décor.
    ///
    /// La saisie (attraper, déplacer, tourner, agrandir à deux mains) est faite par
    /// l'Interaction SDK de Meta (composant Grabbable). Ce script passe juste après
    /// pour garder l'objet posé au sol, droit, et dans des tailles raisonnables.
    /// À mettre sur le prefab de chaque décor, à côté du Grabbable.
    /// </summary>
    public class DecorConstraints : MonoBehaviour
    {
        [Tooltip("Garde l'objet posé sur le plateau (le pivot du modèle doit être en bas).")]
        [SerializeField] private bool keepOnFloor = true;

        [Tooltip("Empêche de pencher l'objet : il ne peut tourner que sur lui-même.")]
        [SerializeField] private bool keepUpright = true;

        [Tooltip("Garde les mêmes proportions quand on agrandit l'objet.")]
        [SerializeField] private bool uniformScale = true;

        private DecorInstance _instance;

        private void Awake()
        {
            _instance = GetComponent<DecorInstance>();
        }

        private void LateUpdate()
        {
            // Les positions sont locales : on raisonne dans le repère du plateau (le parent).
            var t = transform;

            if (keepOnFloor)
            {
                var p = t.localPosition;
                t.localPosition = new Vector3(p.x, 0f, p.z);
            }

            if (keepUpright)
            {
                t.localRotation = Quaternion.Euler(0f, t.localEulerAngles.y, 0f);
            }

            ClampScale(t);
        }

        private void ClampScale(Transform t)
        {
            // DecorInstance est ajouté au moment de l'apparition, après Awake.
            if (_instance == null)
            {
                _instance = GetComponent<DecorInstance>();
            }

            var min = _instance != null && _instance.Definition != null ? _instance.Definition.MinScale : 0.01f;
            var max = _instance != null && _instance.Definition != null ? _instance.Definition.MaxScale : 100f;

            var s = t.localScale;
            if (uniformScale)
            {
                var uniform = Mathf.Clamp(s.x, min, max);
                t.localScale = new Vector3(uniform, uniform, uniform);
            }
            else
            {
                t.localScale = new Vector3(
                    Mathf.Clamp(s.x, min, max),
                    Mathf.Clamp(s.y, min, max),
                    Mathf.Clamp(s.z, min, max));
            }
        }
    }
}

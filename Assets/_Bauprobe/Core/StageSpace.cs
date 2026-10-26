using System;
using UnityEngine;

namespace Bauprobe.Core
{
    /// <summary>
    /// Le repère du plateau. À mettre sur l'objet "StageRoot".
    ///
    /// Tous les objets de décor sont enfants de cet objet. Leurs positions sont
    /// donc exprimées par rapport au plateau (au QR code), et pas par rapport au
    /// casque : c'est ce qui permet de sauvegarder une implantation et de la
    /// retrouver au même endroit le lendemain.
    /// </summary>
    public class StageSpace : MonoBehaviour
    {
        /// <summary>Vrai dès que le QR code du plateau a été trouvé au moins une fois.</summary>
        public bool IsCalibrated { get; private set; }

        /// <summary>Appelé chaque fois que le repère du plateau est recalé sur le QR code.</summary>
        public event Action Calibrated;

        /// <summary>Appelé par le module Calibration quand le repère vient d'être recalé.</summary>
        public void NotifyCalibrated()
        {
            IsCalibrated = true;
            Calibrated?.Invoke();
        }

        /// <summary>Convertit une pose "monde" (casque) en pose "plateau".</summary>
        public Pose WorldToStage(Pose world)
        {
            return new Pose(
                transform.InverseTransformPoint(world.position),
                Quaternion.Inverse(transform.rotation) * world.rotation);
        }

        /// <summary>Convertit une pose "plateau" en pose "monde" (casque).</summary>
        public Pose StageToWorld(Pose stage)
        {
            return new Pose(
                transform.TransformPoint(stage.position),
                transform.rotation * stage.rotation);
        }
    }
}

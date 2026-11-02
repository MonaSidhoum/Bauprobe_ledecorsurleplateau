using System;
using UnityEngine;

namespace Bauprobe.Core
{
    /// <summary>
    /// L'offset du QR code en cours, saisi dans le casque (écran « Offset du QR code » du menu général)
    /// et gardé d'un lancement à l'autre sur le casque (PlayerPrefs) : pas besoin de refaire un Build
    /// quand le QR code change de place. Au tout premier lancement, l'offset est nul.
    ///
    /// StageCalibrator écoute <see cref="Changed"/> pour replacer le point zéro ; LayoutManager recopie
    /// l'offset dans chaque sauvegarde (pour information).
    /// </summary>
    public static class QrOffsetStore
    {
        private const string KeyX = "Bauprobe.QrOffset.X";
        private const string KeyY = "Bauprobe.QrOffset.Y";
        private const string KeyAngle = "Bauprobe.QrOffset.Angle";

        /// <summary>Appelé quand l'offset change (avec le nouvel offset).</summary>
        public static event Action<QrOffset> Changed;

        /// <summary>Offset actuel (une copie : la modifier ne change rien, utiliser <see cref="Set"/>).</summary>
        public static QrOffset Current => new QrOffset(
            PlayerPrefs.GetFloat(KeyX, 0f),
            PlayerPrefs.GetFloat(KeyY, 0f),
            PlayerPrefs.GetFloat(KeyAngle, 0f));

        /// <summary>Enregistre le nouvel offset sur le casque et prévient les autres modules.</summary>
        public static void Set(QrOffset offset)
        {
            offset ??= new QrOffset();
            PlayerPrefs.SetFloat(KeyX, offset.x);
            PlayerPrefs.SetFloat(KeyY, offset.y);
            PlayerPrefs.SetFloat(KeyAngle, offset.angle);
            PlayerPrefs.Save();

            Debug.Log("Offset du QR code : " + offset);
            Changed?.Invoke(offset.Copy());
        }

        /// <summary>Oublie l'offset enregistré (pour les tests) : il redevient nul.</summary>
        public static void Clear()
        {
            PlayerPrefs.DeleteKey(KeyX);
            PlayerPrefs.DeleteKey(KeyY);
            PlayerPrefs.DeleteKey(KeyAngle);
            PlayerPrefs.Save();
        }
    }
}

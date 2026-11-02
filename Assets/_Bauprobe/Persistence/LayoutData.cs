using System;
using System.Collections.Generic;
using Bauprobe.Core;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>
    /// Une "implantation" : la liste des objets posés sur le plateau, avec leur
    /// position dans le repère du plateau. C'est ce qui est écrit dans le fichier JSON.
    ///
    /// Versions du format :
    ///   1 : "position" = position du pivot de l'objet ;
    ///   2 : "position" = position du point d'origine (bas au centre pour le sol, haut au centre pour le cintre),
    ///       comme dans le tableau de positionnement du cahier des charges ;
    ///   3 : + "scene", "qrOffset" (offset du QR code utilisé), et pour chaque objet "name", "attachment", "size".
    ///       Les positions sont mesurées depuis le point zéro du bureau d'études (= le QR code si l'offset est nul).
    /// </summary>
    [Serializable]
    public class LayoutData
    {
        /// <summary>Version écrite par cette version de l'appli.</summary>
        public const int CurrentVersion = 3;

        public int version = CurrentVersion;
        public string savedAt;

        /// <summary>Nom de la scène (pour l'instant : le nom de la sauvegarde).</summary>
        public string scene;

        /// <summary>
        /// Offset du QR code au moment de la sauvegarde (pour information : les positions ne dépendent pas
        /// de l'endroit où était le QR code, elles sont mesurées depuis le point zéro).
        /// </summary>
        public QrOffset qrOffset = new QrOffset();

        public List<LayoutEntry> items = new List<LayoutEntry>();
    }

    [Serializable]
    public class LayoutEntry
    {
        public string itemId;

        /// <summary>Nom affiché du décor (pour information, l'objet est retrouvé grâce à itemId).</summary>
        public string name;

        /// <summary>« Sol » ou « Cintre ».</summary>
        public string attachment;

        /// <summary>
        /// Position du point d'origine dans le repère du plateau (repère Unity : Y = hauteur), en mètres.
        /// Depuis la version 2 : point d'origine ; version 1 : pivot.
        /// </summary>
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;

        /// <summary>
        /// Taille réelle de l'objet, en mètres, le long de ses propres axes (repère Unity) :
        /// x = largeur, y = hauteur, z = profondeur. Pour information (recalculée à partir du modèle au chargement).
        /// </summary>
        public Vector3 size;
    }
}

using System;
using System.Collections.Generic;
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
    ///       comme dans le tableau de positionnement du cahier des charges.
    /// </summary>
    [Serializable]
    public class LayoutData
    {
        /// <summary>Version écrite par cette version de l'appli.</summary>
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public string savedAt;
        public List<LayoutEntry> items = new List<LayoutEntry>();
    }

    [Serializable]
    public class LayoutEntry
    {
        public string itemId;

        /// <summary>Position du point d'origine dans le repère du plateau, en mètres (version 2).</summary>
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
    }
}

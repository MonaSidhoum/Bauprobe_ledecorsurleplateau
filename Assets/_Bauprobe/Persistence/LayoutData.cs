using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>
    /// Une "implantation" : la liste des objets posés sur le plateau, avec leur
    /// position dans le repère du plateau. C'est ce qui est écrit dans le fichier JSON.
    /// </summary>
    [Serializable]
    public class LayoutData
    {
        public int version = 1;
        public string savedAt;
        public List<LayoutEntry> items = new List<LayoutEntry>();
    }

    [Serializable]
    public class LayoutEntry
    {
        public string itemId;
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
    }
}

using System;
using System.Collections.Generic;
using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>Transforme le décor posé sur le plateau en texte JSON, et inversement.</summary>
    public static class LayoutSerializer
    {
        /// <summary>Prend une "photo" des objets posés (positions relatives au plateau).</summary>
        public static LayoutData Capture(IEnumerable<DecorInstance> instances)
        {
            var data = new LayoutData { savedAt = DateTime.Now.ToString("s") };

            foreach (var instance in instances)
            {
                if (instance == null || instance.Definition == null)
                {
                    continue;
                }

                var t = instance.transform;
                data.items.Add(new LayoutEntry
                {
                    itemId = instance.Definition.Id,
                    position = instance.OriginStagePosition,
                    rotation = t.localRotation,
                    scale = t.localScale,
                });
            }

            return data;
        }

        public static string ToJson(LayoutData data) => JsonUtility.ToJson(data, prettyPrint: true);

        public static LayoutData FromJson(string json) => JsonUtility.FromJson<LayoutData>(json);
    }
}

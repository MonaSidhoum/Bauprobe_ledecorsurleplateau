using System;
using System.Collections.Generic;
using Bauprobe.Core;
using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>Transforme le décor posé sur le plateau en texte JSON, et inversement.</summary>
    public static class LayoutSerializer
    {
        /// <summary>Prend une "photo" des objets posés (positions relatives au plateau).</summary>
        /// <param name="scene">Nom de la scène (écrit dans le fichier et dans la colonne « Scène » du CSV).</param>
        /// <param name="qrOffset">Offset du QR code utilisé (null = pas d'offset).</param>
        public static LayoutData Capture(IEnumerable<DecorInstance> instances, string scene = null, QrOffset qrOffset = null)
        {
            var data = new LayoutData
            {
                savedAt = DateTime.Now.ToString("s"),
                scene = scene ?? "",
                qrOffset = qrOffset ?? new QrOffset(),
            };

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
                    name = instance.Definition.DisplayName,
                    attachment = instance.Attachment.ToString(),
                    position = instance.OriginStagePosition,
                    rotation = t.localRotation,
                    scale = t.localScale,
                    baseScale = instance.BaseScale,
                    size = instance.StageSize,
                    hasColor = instance.CustomColor.HasValue,
                    color = instance.CustomColor ?? Color.white,
                });
            }

            return data;
        }

        public static string ToJson(LayoutData data) => JsonUtility.ToJson(data, prettyPrint: true);

        public static LayoutData FromJson(string json) => JsonUtility.FromJson<LayoutData>(json);
    }
}

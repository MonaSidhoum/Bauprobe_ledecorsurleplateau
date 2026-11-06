using Bauprobe.Core;
using Bauprobe.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class LayoutSerializerTests
    {
        [Test]
        public void ToJson_PuisFromJson_GardeLesObjets()
        {
            var data = new LayoutData();
            data.items.Add(new LayoutEntry
            {
                itemId = "table",
                position = new Vector3(1f, 0f, 2f),
                rotation = Quaternion.Euler(0f, 45f, 0f),
                scale = new Vector3(2f, 2f, 2f),
            });

            var back = LayoutSerializer.FromJson(LayoutSerializer.ToJson(data));

            Assert.That(back.items, Has.Count.EqualTo(1));
            Assert.That(back.items[0].itemId, Is.EqualTo("table"));
            Assert.That(back.items[0].position, Is.EqualTo(new Vector3(1f, 0f, 2f)));
            Assert.That(back.items[0].scale, Is.EqualTo(new Vector3(2f, 2f, 2f)));
        }

        [Test]
        public void Version3_GardeSceneOffsetEtDimensions()
        {
            var data = new LayoutData
            {
                scene = "Acte 1 - scène 2",
                qrOffset = new QrOffset(1.5f, -3f, 1.2f),
            };
            data.items.Add(new LayoutEntry { itemId = "table", name = "Table", attachment = "Sol", size = new Vector3(2f, 0.8f, 1f) });

            var back = LayoutSerializer.FromJson(LayoutSerializer.ToJson(data));

            Assert.That(back.version, Is.EqualTo(LayoutData.CurrentVersion));
            Assert.That(back.scene, Is.EqualTo("Acte 1 - scène 2"));
            Assert.That(back.qrOffset.IsSameAs(data.qrOffset), Is.True);
            Assert.That(back.items[0].name, Is.EqualTo("Table"));
            Assert.That(back.items[0].attachment, Is.EqualTo("Sol"));
            Assert.That(back.items[0].size, Is.EqualTo(new Vector3(2f, 0.8f, 1f)));
        }

        [Test]
        public void AncienneSauvegarde_SansOffset_SeRelit()
        {
            const string json = "{\"version\":2,\"savedAt\":\"2026-10-01T10:00:00\",\"items\":[{\"itemId\":\"cube\"," +
                                "\"position\":{\"x\":1,\"y\":0,\"z\":2},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1}," +
                                "\"scale\":{\"x\":1,\"y\":1,\"z\":1}}]}";

            var back = LayoutSerializer.FromJson(json);

            Assert.That(back.version, Is.EqualTo(2));
            Assert.That(back.items, Has.Count.EqualTo(1));
            Assert.That(back.qrOffset, Is.Not.Null);
            Assert.That(back.qrOffset.IsSameAs(new QrOffset()), Is.True);
        }
    }
}

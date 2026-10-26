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
    }
}

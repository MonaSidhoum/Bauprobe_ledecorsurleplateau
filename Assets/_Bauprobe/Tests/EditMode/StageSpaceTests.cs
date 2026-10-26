using Bauprobe.Core;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class StageSpaceTests
    {
        [Test]
        public void WorldToStage_PuisStageToWorld_RendLaPoseDeDepart()
        {
            var go = new GameObject("StageRoot");
            go.transform.SetPositionAndRotation(new Vector3(2f, 0f, -3f), Quaternion.Euler(0f, 90f, 0f));
            var stage = go.AddComponent<StageSpace>();

            var world = new Pose(new Vector3(1f, 0.5f, 4f), Quaternion.Euler(0f, 30f, 0f));
            var back = stage.StageToWorld(stage.WorldToStage(world));

            Assert.That(Vector3.Distance(back.position, world.position), Is.LessThan(1e-4f));
            Assert.That(Quaternion.Angle(back.rotation, world.rotation), Is.LessThan(0.01f));

            Object.DestroyImmediate(go);
        }
    }
}

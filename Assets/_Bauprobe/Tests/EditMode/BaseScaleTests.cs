using Bauprobe.Decor;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    /// <summary>Taille de référence du Scale en % : celle du prefab, avec les étirements d'EXTRUDE.</summary>
    public class BaseScaleTests
    {
        [Test]
        public void Etirer_PuisScale100_GardeLaFormeEtiree()
        {
            var stage = new GameObject("StageRoot");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 1 m
            cube.transform.SetParent(stage.transform, false);
            var instance = cube.AddComponent<DecorInstance>();
            instance.SetOriginStagePosition(Vector3.zero);

            instance.ExtrudeFace(new DecorFace(0, 1), 1f);            // cube → pavé de 2 x 1 x 1
            Assert.That(Vector3.Distance(instance.BaseScale, new Vector3(2f, 1f, 1f)), Is.LessThan(1e-4f),
                "la taille de référence suit l'étirement");

            instance.SetScaleAroundOrigin(instance.BaseScale * 3f);    // Scale x3
            instance.SetScaleAroundOrigin(instance.BaseScale * 1f);    // puis 100 %

            Assert.That(Vector3.Distance(cube.transform.localScale, new Vector3(2f, 1f, 1f)), Is.LessThan(1e-4f),
                "100 % = le pavé, pas le cube de départ");

            Object.DestroyImmediate(stage);
        }
    }
}

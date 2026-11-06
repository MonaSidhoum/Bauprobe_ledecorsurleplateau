using Bauprobe.Decor;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class DecorLimitsTests
    {
        private static void AssertClose(Vector3 actual, Vector3 expected)
        {
            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(1e-4f), $"attendu {expected}, obtenu {actual}");
        }

        [Test]
        public void DecorInstance_UtiliseLaTailleDeSesModeles()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 1 m
            var instance = cube.AddComponent<DecorInstance>();

            AssertClose(instance.LocalSize, Vector3.one);

            Object.DestroyImmediate(cube);
        }

        [Test]
        public void Scale_SansLimiteDeTaille()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 1 m
            var instance = cube.AddComponent<DecorInstance>();

            // Plus de limite à 18 m, ni de limites dans la fiche du catalogue : l'échelle demandée est gardée.
            instance.SetScaleAroundOrigin(new Vector3(25f, 25f, 25f));
            AssertClose(instance.StageSize, new Vector3(25f, 25f, 25f));

            instance.SetScaleAroundOrigin(new Vector3(0.001f, 0.001f, 0.001f));
            AssertClose(cube.transform.localScale, new Vector3(0.001f, 0.001f, 0.001f));

            Object.DestroyImmediate(cube);
        }
    }
}

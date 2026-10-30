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
        public void SousLaLimite_EchelleInchangee()
        {
            // Cube de 1 m agrandi 10 fois : 10 m, c'est autorisé.
            AssertClose(DecorLimits.ClampScaleToMaxSize(Vector3.one, new Vector3(10f, 10f, 10f)), new Vector3(10f, 10f, 10f));
        }

        [Test]
        public void AuDessusDeLaLimite_RameneA18m()
        {
            // Cube de 1 m agrandi 30 fois : ramené à 18 m.
            AssertClose(DecorLimits.ClampScaleToMaxSize(Vector3.one, new Vector3(30f, 30f, 30f)), new Vector3(18f, 18f, 18f));
        }

        [Test]
        public void PlusGrandeDimension_DecideEtProportionsGardees()
        {
            // Mur de 6 m x 3 m x 0,2 m agrandi 4 fois : 24 m de long, ramené à 18 m (échelle 3), proportions gardées.
            var scale = DecorLimits.ClampScaleToMaxSize(new Vector3(6f, 3f, 0.2f), new Vector3(4f, 4f, 4f));

            AssertClose(scale, new Vector3(3f, 3f, 3f));
        }

        [Test]
        public void ModeleImporteTropGrand_ReduitAEchelle1()
        {
            // Modèle du bureau d'études de 36 m à l'échelle 1 : ramené à 18 m.
            AssertClose(DecorLimits.ClampScaleToMaxSize(new Vector3(36f, 5f, 2f), Vector3.one), new Vector3(0.5f, 0.5f, 0.5f));
        }

        [Test]
        public void DecorInstance_UtiliseLaTailleDeSesModeles()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 1 m
            var instance = cube.AddComponent<DecorInstance>();

            AssertClose(instance.LocalSize, Vector3.one);
            AssertClose(instance.ClampScaleToMaxSize(new Vector3(25f, 25f, 25f)), new Vector3(18f, 18f, 18f));

            Object.DestroyImmediate(cube);
        }
    }
}

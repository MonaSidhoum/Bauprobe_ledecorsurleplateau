using Bauprobe.Decor;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class DecorManipulationTests
    {
        private GameObject _stage;
        private GameObject _cube;
        private DecorInstance _instance;

        [SetUp]
        public void SetUp()
        {
            _stage = new GameObject("StageRoot");
            _cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 1 m, pivot au centre
            _cube.transform.SetParent(_stage.transform, false);
            _instance = _cube.AddComponent<DecorInstance>(); // sans fiche : élément au sol
            _instance.SetOriginStagePosition(Vector3.zero);   // posé sur le plateau
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_stage);
        }

        private float LowestPoint()
        {
            Assert.That(_instance.TryGetStageBounds(out var b), Is.True);
            return b.min.y;
        }

        [Test]
        public void Basculer_GardeLObjetPoseAuSol()
        {
            _instance.RotateAroundCenter(Vector3.right, 45f);

            Assert.That(LowestPoint(), Is.EqualTo(0f).Within(1e-4f), "l'arête la plus basse touche le plateau");
            Assert.That(_cube.transform.localPosition.y, Is.EqualTo(Mathf.Sqrt(0.5f)).Within(1e-4f),
                "le centre est remonté de la moitié de la diagonale");
        }

        [Test]
        public void TournerSurLuiMeme_NeChangePasLaHauteur()
        {
            _instance.RotateAroundCenter(Vector3.up, 30f);

            Assert.That(LowestPoint(), Is.EqualTo(0f).Within(1e-4f));
            Assert.That(_cube.transform.localPosition.y, Is.EqualTo(0.5f).Within(1e-4f));
        }

        [Test]
        public void SousLePlateau_EstRemonteAuSol()
        {
            _cube.transform.localPosition = new Vector3(1f, -2f, 0f);

            _instance.KeepAboveStage();

            Assert.That(LowestPoint(), Is.EqualTo(0f).Within(1e-4f));
            Assert.That(_cube.transform.localPosition.x, Is.EqualTo(1f).Within(1e-4f), "seule la hauteur change");
        }

        [Test]
        public void AuDessusDuPlateau_ResteEnLAir()
        {
            _cube.transform.localPosition = new Vector3(0f, 3f, 0f);

            _instance.KeepAboveStage();

            Assert.That(_cube.transform.localPosition.y, Is.EqualTo(3f).Within(1e-4f));
        }

        [Test]
        public void RemettreAPlat_AnnuleLaBascule_GardeLOrientationEtLeSol()
        {
            _instance.RotateAroundCenter(Vector3.up, 40f);    // tourné de 40° sur lui-même
            _instance.RotateAroundCenter(Vector3.right, 30f); // puis basculé de 30°

            _instance.LevelWithStage();

            Assert.That(Vector3.Angle(_cube.transform.up, Vector3.up), Is.LessThan(0.01f), "l'objet est de nouveau à plat");
            var forward = Vector3.ProjectOnPlane(_cube.transform.forward, Vector3.up);
            Assert.That(Vector3.SignedAngle(Vector3.forward, forward, Vector3.up), Is.EqualTo(40f).Within(0.01f),
                "l'orientation gauche / droite n'a pas changé");
            Assert.That(LowestPoint(), Is.EqualTo(0f).Within(1e-4f), "il est toujours posé au sol");
        }
    }
}

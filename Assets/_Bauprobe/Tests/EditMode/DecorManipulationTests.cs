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
        public void AlignerAuSol_PeuPenche_SeRedresse_GardeLOrientationEtLeSol()
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

        [Test]
        public void AlignerAuSol_TresPenche_SeCoucheSurLaFaceLaPlusProche()
        {
            _instance.RotateAroundCenter(Vector3.right, 70f); // basculé de 70° : plus proche d'être couché que droit

            _instance.LevelWithStage();

            // L'arrière du cube (-forward) est maintenant tourné vers le haut : il est couché sur sa face avant.
            Assert.That(Vector3.Angle(-_cube.transform.forward, Vector3.up), Is.LessThan(0.01f), "couché sur la face la plus proche");
            Assert.That(LowestPoint(), Is.EqualTo(0f).Within(1e-4f), "il est toujours posé au sol");
        }

        // ---- Étirer une face (EXTRUDE) ----

        private Bounds StageBounds()
        {
            Assert.That(_instance.TryGetStageBounds(out var b), Is.True);
            return b;
        }

        [Test]
        public void Etirer_FaceDroite_LaFaceGaucheNeBougePas()
        {
            var length = _instance.ExtrudeFace(new DecorFace(0, 1), 1f); // pousser la face +X de 1 m

            var b = StageBounds();
            Assert.That(length, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(b.min.x, Is.EqualTo(-0.5f).Within(1e-4f), "la face opposée n'a pas bougé");
            Assert.That(b.max.x, Is.EqualTo(1.5f).Within(1e-4f));
            Assert.That(b.size.y, Is.EqualTo(1f).Within(1e-4f), "les autres dimensions ne changent pas");
            Assert.That(b.min.y, Is.EqualTo(0f).Within(1e-4f), "toujours posé au sol");
        }

        [Test]
        public void Etirer_FaceDuHaut_ProlongeVersLeHaut()
        {
            _instance.ExtrudeFace(new DecorFace(1, 1), 0.5f); // une porte plus haute

            var b = StageBounds();
            Assert.That(b.min.y, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(b.max.y, Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test]
        public void Etirer_FaceDuBas_ObjetAuSol_ResteSurLeSol()
        {
            _instance.ExtrudeFace(new DecorFace(1, -1), 0.5f);

            var b = StageBounds();
            Assert.That(b.min.y, Is.EqualTo(0f).Within(1e-4f), "il ne s'enfonce pas dans le sol");
            Assert.That(b.size.y, Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test]
        public void Raccourcir_PasMoinsDe5cm()
        {
            var length = _instance.ExtrudeFace(new DecorFace(2, 1), -5f);

            Assert.That(length, Is.EqualTo(DecorLimits.MinFaceLength).Within(1e-4f));
            Assert.That(StageBounds().size.z, Is.EqualTo(DecorLimits.MinFaceLength).Within(1e-4f));
        }

        [Test]
        public void Allonger_PasPlusDe18m()
        {
            var length = _instance.ExtrudeFace(new DecorFace(0, 1), 30f);

            Assert.That(length, Is.EqualTo(DecorLimits.MaxSizeMeters).Within(1e-3f));
        }

        [Test]
        public void FaceVisee_EstLaPlusProcheDuPoint()
        {
            // Cube de 1 m posé au sol : centre à (0 ; 0,5 ; 0)
            Assert.That(_instance.TryGetFace(new Vector3(0.5f, 0.5f, 0.1f), out var right), Is.True);
            Assert.That(right, Is.EqualTo(new DecorFace(0, 1)), "face de droite (+X)");

            Assert.That(_instance.TryGetFace(new Vector3(0.1f, 1f, -0.2f), out var top), Is.True);
            Assert.That(top, Is.EqualTo(new DecorFace(1, 1)), "face du haut (+Y)");
        }

        [Test]
        public void ObjetEtire_ScaleGardeSaForme()
        {
            _instance.ExtrudeFace(new DecorFace(0, 1), 1f); // 2 m x 1 m x 1 m

            var scale = DecorLimits.ClampScaleToRange(_cube.transform.localScale * 1.5f, 0.1f, 10f);

            Assert.That(scale.x / scale.y, Is.EqualTo(2f).Within(1e-4f), "les proportions sont gardées");
        }
    }
}

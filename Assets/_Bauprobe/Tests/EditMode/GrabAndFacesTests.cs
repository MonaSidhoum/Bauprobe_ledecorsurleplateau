using Bauprobe.Decor;
using Bauprobe.Manipulation;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    /// <summary>Saisie à deux mains (grab) et choix d'une face par sa direction.</summary>
    public class GrabAndFacesTests
    {
        private static void AssertClose(Vector3 actual, Vector3 expected, string message = null)
        {
            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(1e-3f), $"{message} attendu {expected}, obtenu {actual}");
        }

        // ---- Grab à deux mains ----

        [Test]
        public void DeuxMains_GardeLaTailleActuelle_PasDeRetourEnArriere()
        {
            // Objet étiré (2 x 1 x 1) : quand on le prend sans écarter les mains, sa taille ne change pas.
            var start = new Pose(Vector3.zero, Quaternion.identity);
            GrabMath.TwoHands(start, new Vector3(2f, 1f, 1f),
                              new Vector3(-0.3f, 1f, 0.5f), new Vector3(0.3f, 1f, 0.5f),
                              new Vector3(-0.3f, 1f, 0.5f), new Vector3(0.3f, 1f, 0.5f),
                              keepLevel: true, allowScale: true, out var pose, out var scale);

            AssertClose(scale, new Vector3(2f, 1f, 1f), "taille");
            AssertClose(pose.position, Vector3.zero, "position");
        }

        [Test]
        public void DeuxMains_EcarterLesMains_AgranditEnGardantLaForme()
        {
            var start = new Pose(Vector3.zero, Quaternion.identity);
            GrabMath.TwoHands(start, new Vector3(2f, 1f, 1f),
                              new Vector3(-0.3f, 1f, 0f), new Vector3(0.3f, 1f, 0f),
                              new Vector3(-0.6f, 1f, 0f), new Vector3(0.6f, 1f, 0f),
                              keepLevel: true, allowScale: true, out _, out var scale);

            AssertClose(scale, new Vector3(4f, 2f, 2f), "deux fois plus grand, proportions gardées");
        }

        [Test]
        public void DeuxMains_UneMainPlusHaute_ObjetResteParalleleAuSol()
        {
            // Une main monte : avant, l'objet basculait ; maintenant il reste à plat.
            var start = new Pose(Vector3.zero, Quaternion.identity);
            GrabMath.TwoHands(start, Vector3.one,
                              new Vector3(-0.3f, 1f, 0f), new Vector3(0.3f, 1f, 0f),
                              new Vector3(-0.3f, 1f, 0f), new Vector3(0.3f, 1.4f, 0f),
                              keepLevel: true, allowScale: false, out var pose, out _);

            Assert.That(Vector3.Angle(pose.rotation * Vector3.up, Vector3.up), Is.LessThan(0.01f), "toujours à plat");
        }

        [Test]
        public void DeuxMains_TournerLesMains_TourneAutourDeLaVerticale()
        {
            var start = new Pose(Vector3.zero, Quaternion.identity);
            GrabMath.TwoHands(start, Vector3.one,
                              new Vector3(-0.3f, 1f, 0f), new Vector3(0.3f, 1f, 0f),
                              new Vector3(0f, 1f, 0.3f), new Vector3(0f, 1f, -0.3f),
                              keepLevel: true, allowScale: false, out var pose, out _);

            var forward = pose.rotation * Vector3.forward;
            Assert.That(Vector3.SignedAngle(Vector3.forward, forward, Vector3.up), Is.EqualTo(90f).Within(0.01f));
        }

        // ---- Grab à une main ----

        [Test]
        public void UneMain_PoignetPenche_ObjetResteParalleleAuSol()
        {
            var hand0 = new Pose(new Vector3(0f, 1f, 0f), Quaternion.identity);
            var obj = new Pose(new Vector3(0f, 1f, 2f), Quaternion.identity);
            GrabMath.OneHandOffsets(hand0, obj, keepLevel: true, out var offsetPosition, out var offsetRotation);

            // Main penchée de 45° vers le bas : l'objet ne bascule pas
            var hand = new Pose(hand0.position, Quaternion.Euler(45f, 0f, 0f));
            var pose = GrabMath.OneHand(hand, offsetPosition, offsetRotation, keepLevel: true);

            Assert.That(Vector3.Angle(pose.rotation * Vector3.up, Vector3.up), Is.LessThan(0.01f));
            AssertClose(pose.position, obj.position, "il reste devant la main");
        }

        // ---- Face tournée vers une direction ----

        [Test]
        public void FaceTowards_TrouveLaFaceDuHaut_MemeObjetTourne()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.rotation = Quaternion.Euler(0f, 30f, 90f); // couché sur le côté : son axe X est vertical
            var instance = cube.AddComponent<DecorInstance>();

            var top = instance.FaceTowards(Vector3.up);
            instance.GetFaceGeometry(top, out _, out var normal, out _, out _);
            Assert.That(Vector3.Angle(normal, Vector3.up), Is.LessThan(0.01f));

            Object.DestroyImmediate(cube);
        }

        [Test]
        public void FaceTowards_FaceTourneeVersLaPersonne()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var instance = cube.AddComponent<DecorInstance>();

            // La personne est derrière l'objet (Z négatif) : la face tournée vers elle est la face -Z
            Assert.That(instance.FaceTowards(Vector3.back), Is.EqualTo(new DecorFace(2, -1)));

            Object.DestroyImmediate(cube);
        }
    }
}

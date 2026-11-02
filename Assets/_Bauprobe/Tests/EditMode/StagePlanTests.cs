using Bauprobe.Core;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class StagePlanTests
    {
        [Test]
        public void Position_LaHauteurDevientZ()
        {
            // Unity : Y = hauteur ; plan : Z = hauteur, Y = profondeur.
            Assert.That(StagePlan.ToPlan(new Vector3(1f, 2f, 3f)), Is.EqualTo(new Vector3(1f, 3f, 2f)));
            Assert.That(StagePlan.FromPlan(StagePlan.ToPlan(new Vector3(1f, 2f, 3f))), Is.EqualTo(new Vector3(1f, 2f, 3f)));
        }

        [Test]
        public void Angle_TournerVersLaGauche_EstPositif()
        {
            // Unity tourne dans le sens des aiguilles d'une montre vu de dessus ; le plan dans l'autre sens.
            var angles = StagePlan.ToPlanAngles(Quaternion.Euler(0f, -30f, 0f));
            Assert.That(angles.z, Is.EqualTo(30f).Within(1e-3f));
            Assert.That(angles.x, Is.EqualTo(0f).Within(1e-3f));
            Assert.That(angles.y, Is.EqualTo(0f).Within(1e-3f));

            // Vérifié sur un vecteur : l'avant de l'objet (Y du plan) tourné de +90° pointe vers -X du plan.
            var front = StagePlan.ToPlan(StagePlan.FromPlanAngles(new Vector3(0f, 0f, 90f)) * StagePlan.FromPlan(Vector3.up));
            Assert.That(Vector3.Distance(front, new Vector3(-1f, 0f, 0f)), Is.LessThan(1e-4f));
        }

        [Test]
        public void Angles_AllerRetour()
        {
            var planAngles = new Vector3(10f, -20f, 135f);
            var back = StagePlan.ToPlanAngles(StagePlan.FromPlanAngles(planAngles));
            Assert.That(Vector3.Distance(back, planAngles), Is.LessThan(1e-3f));

            var rotation = Quaternion.Euler(25f, 200f, -15f);
            var again = StagePlan.FromPlanAngles(StagePlan.ToPlanAngles(rotation));
            Assert.That(Quaternion.Angle(rotation, again), Is.LessThan(0.01f));
        }

        [Test]
        public void Normalize_EntreMoins180Et180()
        {
            Assert.That(StagePlan.Normalize(270f), Is.EqualTo(-90f).Within(1e-4f));
            Assert.That(StagePlan.Normalize(-180f), Is.EqualTo(180f).Within(1e-4f));
            Assert.That(StagePlan.Normalize(360f), Is.EqualTo(0f));
        }

        [Test]
        public void OffsetNul_LePointZeroEstLeQrCode()
        {
            var pose = StagePlan.ReferenceInQrFrame(0f, 0f, 0f);
            Assert.That(pose.position, Is.EqualTo(Vector3.zero));
            Assert.That(Quaternion.Angle(pose.rotation, Quaternion.identity), Is.LessThan(1e-3f));
        }

        [Test]
        public void Offset_LeQrCodeEstAuBonEndroitDuPlan()
        {
            // QR code 2 m à droite et 3 m devant le point zéro, tourné de 90° vers la gauche.
            var reference = StagePlan.ReferenceInQrFrame(2f, 3f, 90f);

            // Le centre du QR code, vu depuis le point zéro : (2 ; 3).
            Assert.That(Vector3.Distance(QrToPlan(reference, Vector3.zero), new Vector3(2f, 3f, 0f)), Is.LessThan(1e-4f));

            // 1 m devant le QR code (son « avant ») : il pointe vers -X du plan → (1 ; 3).
            Assert.That(Vector3.Distance(QrToPlan(reference, Vector3.forward), new Vector3(1f, 3f, 0f)), Is.LessThan(1e-4f));

            // Un objet posé face au QR code (rotation nulle par rapport au QR code) a un Angle de 90° dans le plan.
            var rotationInPlan = Quaternion.Inverse(reference.rotation);
            Assert.That(StagePlan.ToPlanAngles(rotationInPlan).z, Is.EqualTo(90f).Within(1e-3f));
        }

        /// <summary>Point dans le repère du QR code (Unity) → point dans le plan du bureau d'études.</summary>
        private static Vector3 QrToPlan(Pose referenceInQr, Vector3 qrPoint) =>
            StagePlan.ToPlan(Quaternion.Inverse(referenceInQr.rotation) * (qrPoint - referenceInQr.position));
    }
}

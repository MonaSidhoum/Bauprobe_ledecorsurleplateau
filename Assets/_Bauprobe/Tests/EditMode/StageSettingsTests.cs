using Bauprobe.Core;
using NUnit.Framework;

namespace Bauprobe.Tests
{
    public class StageSettingsTests
    {
        [Test]
        public void FichierDeReglages_ExisteDansResources()
        {
            Assert.That(StageSettings.LoadDefault(), Is.Not.Null,
                "Assets/_Bauprobe/Core/Resources/StageSettings.asset est introuvable");
        }

        [Test]
        public void Sol_EstSousLeBasDuQrCode()
        {
            // Bas du QR code à 1,6 m (repère du casque), collé à 1,2 m du sol : le sol est à 0,4 m.
            Assert.That(StageSettings.FloorHeight(1.6f, 1.2f), Is.EqualTo(0.4f).Within(1e-5f));
        }

        [Test]
        public void QrAuSol_LeSolEstAuBasDuQrCode()
        {
            Assert.That(StageSettings.FloorHeight(0.3f, 0f), Is.EqualTo(0.3f).Within(1e-5f));
        }
    }
}

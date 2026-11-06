using Bauprobe.Core;
using NUnit.Framework;

namespace Bauprobe.Tests
{
    public class QrOffsetStoreTests
    {
        private QrOffset _before;

        [SetUp]
        public void SaveCurrent() => _before = QrOffsetStore.Current;

        [TearDown]
        public void RestoreCurrent() => QrOffsetStore.Set(_before);

        [Test]
        public void Set_PuisCurrent_RendLaMemeValeur()
        {
            QrOffsetStore.Set(new QrOffset(1.25f, -3f, 1.2f));
            Assert.That(QrOffsetStore.Current.IsSameAs(new QrOffset(1.25f, -3f, 1.2f)), Is.True);
        }

        [Test]
        public void Set_PrevientLesAutresModules()
        {
            QrOffset received = null;
            void OnChanged(QrOffset o) => received = o;

            QrOffsetStore.Changed += OnChanged;
            try
            {
                QrOffsetStore.Set(new QrOffset(0f, 2f, 0f));
            }
            finally
            {
                QrOffsetStore.Changed -= OnChanged;
            }

            Assert.That(received, Is.Not.Null);
            Assert.That(received.IsSameAs(new QrOffset(0f, 2f, 0f)), Is.True);
        }

        [Test]
        public void HauteurNegative_RameneeAZero()
        {
            QrOffsetStore.Set(new QrOffset { x = 0f, y = 0f, height = -1f });
            Assert.That(QrOffsetStore.Current.height, Is.EqualTo(0f));
        }

        [Test]
        public void Sol_EstSousLeBasDuQrCode()
        {
            // Bas du QR code à 1,6 m (repère du casque), collé à 1,2 m du sol : le sol est à 0,4 m.
            Assert.That(new QrOffset(0f, 0f, 1.2f).FloorHeight(1.6f), Is.EqualTo(0.4f).Within(1e-5f));
        }

        [Test]
        public void QrAuSol_LeSolEstAuBasDuQrCode()
        {
            Assert.That(new QrOffset().FloorHeight(0.3f), Is.EqualTo(0.3f).Within(1e-5f));
        }

        [Test]
        public void Clear_OffsetNul()
        {
            QrOffsetStore.Set(new QrOffset(4f, 5f, 6f));
            QrOffsetStore.Clear();
            Assert.That(QrOffsetStore.Current.IsZero, Is.True);
        }
    }
}

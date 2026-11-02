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
            QrOffsetStore.Set(new QrOffset(1.25f, -3f, 90f));
            Assert.That(QrOffsetStore.Current.IsSameAs(new QrOffset(1.25f, -3f, 90f)), Is.True);
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
        public void Clear_OffsetNul()
        {
            QrOffsetStore.Set(new QrOffset(4f, 5f, 6f));
            QrOffsetStore.Clear();
            Assert.That(QrOffsetStore.Current.IsZero, Is.True);
        }
    }
}

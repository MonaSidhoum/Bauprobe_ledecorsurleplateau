using Bauprobe.Persistence;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class LayoutCsvTests
    {
        private static LayoutData Sample()
        {
            var data = new LayoutData { scene = "Scène 1" };
            data.items.Add(new LayoutEntry
            {
                itemId = "cube",
                name = "Cube",
                attachment = "Sol",
                position = new Vector3(1f, 0.5f, 2f),         // Unity : Y = hauteur
                rotation = Quaternion.Euler(0f, -90f, 0f),    // un quart de tour vers la gauche
                scale = new Vector3(1f, 1.5f, 2f),
                size = new Vector3(2f, 3f, 1f),               // largeur, hauteur, profondeur
            });
            return data;
        }

        [Test]
        public void PremiereLigne_EstLeTitreDesColonnes()
        {
            var lines = LayoutCsv.ToCsv(Sample()).Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines, Has.Length.EqualTo(2));
            Assert.That(lines[0], Is.EqualTo(string.Join(";", LayoutCsv.Header)));
        }

        [Test]
        public void Ligne_EnRepereDuPlan_AvecVirgule()
        {
            var lines = LayoutCsv.ToCsv(Sample()).Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            // X ; Y (profondeur) ; Z (hauteur) ; Rotation ; Tilt X ; Tilt Y ; Width ; Depth ; Height ; Scale width / depth / height ;
            // Color (vide : couleur du modèle)
            Assert.That(lines[1], Is.EqualTo("Scène 1;cube;Cube;Floor;Bottom centre;1;2;0,5;90;0;0;2;1;3;1;2;1,5;"));
        }

        [Test]
        public void Couleur_EnHexadecimal()
        {
            var entry = Sample().items[0];
            Assert.That(LayoutCsv.ColorCell(entry), Is.EqualTo(""), "couleur du modèle : case vide");

            entry.hasColor = true;
            entry.color = new Color32(0xB2, 0x3A, 0x3A, 255); // rouge théâtre de la palette
            var row = LayoutCsv.Row("S", entry);
            Assert.That(row, Has.Length.EqualTo(LayoutCsv.Header.Length));
            Assert.That(row[row.Length - 1], Is.EqualTo("#B23A3A"));
        }

        [Test]
        public void Ligne_AvecPoint()
        {
            var row = LayoutCsv.Row("S", Sample().items[0], decimalComma: false);
            Assert.That(row[7], Is.EqualTo("0.5"));
        }

        [Test]
        public void ElementAuCintre_PointDeReferenceEnHaut()
        {
            var entry = Sample().items[0];
            entry.attachment = "Cintre";
            var row = LayoutCsv.Row("S", entry);
            Assert.That(row[3], Is.EqualTo("Flown"));
            Assert.That(row[4], Is.EqualTo("Top centre"));
        }

        [Test]
        public void Titres_EnAnglais_SansPointVirgule()
        {
            Assert.That(LayoutCsv.Header[0], Is.EqualTo("Scene"));
            foreach (var title in LayoutCsv.Header)
            {
                Assert.That(title, Does.Not.Contain(";"));
            }
        }

        [Test]
        public void Nombres_ArrondisAuMillimetre_SansMoinsZero()
        {
            Assert.That(LayoutCsv.Number(1.23456f, 3, false), Is.EqualTo("1.235"));
            Assert.That(LayoutCsv.Number(-0.0001f, 3, true), Is.EqualTo("0"));
            Assert.That(LayoutCsv.Number(-2.5f, 3, true), Is.EqualTo("-2,5"));
        }

        [Test]
        public void Case_AvecPointVirgule_EntreGuillemets()
        {
            Assert.That(LayoutCsv.Escape("Salon; partie A"), Is.EqualTo("\"Salon; partie A\""));
            Assert.That(LayoutCsv.Escape("Porte \"jardin\""), Is.EqualTo("\"Porte \"\"jardin\"\"\""));
            Assert.That(LayoutCsv.Escape("Cube"), Is.EqualTo("Cube"));
        }
    }
}

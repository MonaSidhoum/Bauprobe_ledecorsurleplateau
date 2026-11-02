using Bauprobe.Manipulation;
using NUnit.Framework;
using UnityEngine;

namespace Bauprobe.Tests
{
    public class MenuDockingTests
    {
        // Menu général ouvert : 400 x 600 unités, centré sur (0, 0)
        private static readonly Rect OpenMenu = Rect.MinMaxRect(-200f, -300f, 200f, 300f);

        // Menu général replié : il ne reste que le bouton « + » (80 x 50) au centre
        private static readonly Rect CollapsedMenu = Rect.MinMaxRect(-40f, -25f, 40f, 25f);

        [Test]
        public void Ouvert_PasReplie()
        {
            Assert.That(MenuDocking.IsCollapsed(OpenMenu.height, 600f), Is.False);
        }

        [Test]
        public void SeulLeBoutonPlus_Replie()
        {
            Assert.That(MenuDocking.IsCollapsed(CollapsedMenu.height, 600f), Is.True);
        }

        [Test]
        public void Ouvert_MenuObjetADroite_AligneEnHaut()
        {
            var center = MenuDocking.DockedCenter(OpenMenu, new Vector2(300f, 400f), collapsed: false, gap: 20f);

            Assert.That(center.x - 150f, Is.EqualTo(OpenMenu.xMax + 20f).Within(1e-3f), "bord gauche du menu objet = bord droit du menu général + espace");
            Assert.That(center.y + 200f, Is.EqualTo(OpenMenu.yMax).Within(1e-3f), "les deux menus sont alignés en haut");
        }

        [Test]
        public void Replie_MenuObjetAuDessusDuBoutonPlus()
        {
            var center = MenuDocking.DockedCenter(CollapsedMenu, new Vector2(300f, 400f), collapsed: true, gap: 20f);

            Assert.That(center.x, Is.EqualTo(CollapsedMenu.center.x).Within(1e-3f), "centré au-dessus du bouton");
            Assert.That(center.y - 200f, Is.EqualTo(CollapsedMenu.yMax + 20f).Within(1e-3f), "bas du menu objet = haut du bouton + espace");
        }
    }
}

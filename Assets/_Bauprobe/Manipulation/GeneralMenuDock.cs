using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Place un panneau (menu objet, bouton « Align with floor »…) à la MAIN GAUCHE, collé au menu général :
    ///   - menu général ouvert : à sa droite, aligné en haut ;
    ///   - menu général replié (bouton − / +) : au-dessus du bouton « + ».
    ///
    /// Le menu général est trouvé tout seul sous la main gauche (objet nommé <see cref="MenuName"/>).
    /// « Replié » est détecté en regardant ce qui en est visible (voir <see cref="MenuDocking"/>) :
    /// pas besoin de modifier le code du menu général.
    /// </summary>
    public static class GeneralMenuDock
    {
        /// <summary>Nom de l'objet du menu général, pour le trouver sous la main gauche.</summary>
        public static string MenuName = "BauprobeMenu";

        static RectTransform s_menu;
        static float s_nextSearchTime;
        static bool s_warned;
        static float s_openHeight;
        static readonly List<Graphic> s_graphics = new List<Graphic>();
        static readonly Vector3[] s_corners = new Vector3[4];

        /// <summary>Indique le menu général à utiliser (au lieu de le chercher par son nom).</summary>
        public static void SetMenu(RectTransform menu)
        {
            if (menu != null && menu != s_menu)
            {
                s_menu = menu;
                s_openHeight = 0f;
            }
        }

        /// <summary>Le menu général (null s'il est introuvable).</summary>
        public static RectTransform Menu
        {
            get
            {
                // Cherché au plus toutes les 2 secondes tant qu'il est introuvable
                if (s_menu == null && Time.unscaledTime >= s_nextSearchTime)
                {
                    s_nextSearchTime = Time.unscaledTime + 2f;
                    s_menu = Find();
                    s_openHeight = 0f;
                }
                return s_menu;
            }
        }

        /// <summary>
        /// Place <paramref name="panel"/> à côté du menu général, dans le même plan que lui.
        /// Renvoie false (sans rien bouger) si le menu général est introuvable ou caché.
        /// </summary>
        /// <param name="gapMeters">Espace entre le menu général et le panneau, en mètres.</param>
        public static bool TryPlace(RectTransform panel, float gapMeters)
        {
            var menu = Menu;
            if (menu == null || panel == null || !menu.gameObject.activeInHierarchy) return false;
            if (!TryGetVisibleRect(menu, out var visible)) return false;

            // Replié ou ouvert ? (replié = il ne reste que le petit bouton « + »)
            s_openHeight = Mathf.Max(s_openHeight, visible.height, menu.rect.height);
            bool collapsed = MenuDocking.IsCollapsed(visible.height, s_openHeight);

            // Taille du panneau et espace, convertis dans les unités du menu général
            Vector3 menuScale = menu.lossyScale;
            if (Mathf.Abs(menuScale.x) < 1e-8f || Mathf.Abs(menuScale.y) < 1e-8f) return false;
            Vector3 panelScale = panel.lossyScale;
            var panelSize = new Vector2(panel.rect.width * panelScale.x / menuScale.x,
                                        panel.rect.height * panelScale.y / menuScale.y);
            float gap = gapMeters / Mathf.Abs(menuScale.y);

            Vector2 center = MenuDocking.DockedCenter(visible, panelSize, collapsed, gap);

            // La position d'un RectTransform est celle de son pivot : on corrige si le pivot n'est pas au milieu.
            Vector2 pivotOffset = (panel.pivot - new Vector2(0.5f, 0.5f)) * panelSize;
            panel.position = menu.TransformPoint(new Vector3(center.x + pivotOffset.x, center.y + pivotOffset.y, 0f));
            panel.rotation = menu.rotation;
            return true;
        }

        static RectTransform Find()
        {
            var rig = Object.FindAnyObjectByType<OVRCameraRig>();
            if (rig != null && rig.leftHandAnchor != null)
            {
                foreach (var rt in rig.leftHandAnchor.GetComponentsInChildren<RectTransform>(true))
                {
                    if (rt.name == MenuName) return rt;
                }
            }

            if (!s_warned)
            {
                s_warned = true;
                Debug.LogWarning($"[GeneralMenuDock] Menu général « {MenuName} » introuvable sous la main gauche : " +
                                 "le menu objet et le bouton « Align with floor » s'afficheront au-dessus de l'objet.");
            }
            return null;
        }

        /// <summary>
        /// Rectangle de la partie VISIBLE du menu général (dans son repère local) : on réunit les éléments
        /// affichés (images, textes) dont la transparence n'est pas nulle.
        /// </summary>
        static bool TryGetVisibleRect(RectTransform menu, out Rect visible)
        {
            visible = default;
            bool any = false;
            float xMin = 0f, xMax = 0f, yMin = 0f, yMax = 0f;

            menu.GetComponentsInChildren(false, s_graphics);
            foreach (var g in s_graphics)
            {
                if (!g.isActiveAndEnabled || g.color.a < 0.01f || g.canvasRenderer.GetInheritedAlpha() < 0.01f) continue;

                g.rectTransform.GetWorldCorners(s_corners);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = menu.InverseTransformPoint(s_corners[i]);
                    if (!any) { xMin = xMax = p.x; yMin = yMax = p.y; any = true; }
                    else
                    {
                        xMin = Mathf.Min(xMin, p.x); xMax = Mathf.Max(xMax, p.x);
                        yMin = Mathf.Min(yMin, p.y); yMax = Mathf.Max(yMax, p.y);
                    }
                }
            }

            if (!any) return false;
            visible = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            return true;
        }
    }
}

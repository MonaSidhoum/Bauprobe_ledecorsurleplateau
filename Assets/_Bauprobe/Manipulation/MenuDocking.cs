using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Calculs de placement du menu objet à côté du menu général (main gauche).
    /// Tout est exprimé dans le repère local du menu général (unités de son Canvas).
    /// Séparé d'ObjectMenu pour pouvoir être testé sans casque.
    /// </summary>
    public static class MenuDocking
    {
        /// <summary>
        /// Le menu général est-il replié ? Oui si la partie visible est beaucoup moins haute que
        /// le menu ouvert (quand il est replié, il ne reste que le petit bouton « + »).
        /// </summary>
        /// <param name="visibleHeight">Hauteur de la partie visible du menu général.</param>
        /// <param name="openHeight">Hauteur du menu général ouvert (la plus grande hauteur visible observée).</param>
        public static bool IsCollapsed(float visibleHeight, float openHeight)
        {
            return openHeight > 0f && visibleHeight < 0.4f * openHeight;
        }

        /// <summary>
        /// Centre du menu objet :
        /// - menu général ouvert : à sa droite, aligné sur son haut ;
        /// - menu général replié : au-dessus de ce qui en reste (le bouton « + »), centré.
        /// </summary>
        /// <param name="visible">Rectangle visible du menu général.</param>
        /// <param name="menuSize">Taille du menu objet, dans les mêmes unités.</param>
        /// <param name="collapsed">Menu général replié ?</param>
        /// <param name="gap">Espace entre les deux menus, dans les mêmes unités.</param>
        public static Vector2 DockedCenter(Rect visible, Vector2 menuSize, bool collapsed, float gap)
        {
            if (collapsed)
            {
                return new Vector2(visible.center.x, visible.yMax + gap + menuSize.y * 0.5f);
            }

            return new Vector2(visible.xMax + gap + menuSize.x * 0.5f, visible.yMax - menuSize.y * 0.5f);
        }
    }
}

using UnityEngine;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// « Le laser vise-t-il un des panneaux de manipulation ? » (bouton Align with floor, INFO, pavé du Scale,
    /// faces d'EXTRUDE). Quand c'est le cas, la gâchette sert à cliquer sur le panneau : elle ne valide pas
    /// la manipulation et ne sélectionne pas un objet situé derrière.
    /// </summary>
    public static class ManipulationPanels
    {
        public static bool IsPointerOverAny(Ray ray)
        {
            return LevelButton.IsPointerOver(ray) || HandPanel.IsPointerOverAnyPanel(ray);
        }
    }
}

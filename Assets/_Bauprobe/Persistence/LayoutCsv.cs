using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Bauprobe.Core;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>
    /// Écrit une implantation sous forme de tableau CSV pour le bureau d'études (s'ouvre dans Excel).
    /// Une ligne par élément de décor.
    ///
    /// Repère « plan » (voir <see cref="StagePlan"/>), dont l'origine est le point zéro du bureau d'études,
    /// AU SOL (= le point du sol sous le QR code si l'offset X / Y du QR code est nul) :
    ///   X (m) à droite (cour), Y (m) vers l'avant (lointain), Z (m) hauteur au-dessus du sol du plateau ;
    ///   la position est celle du point d'origine (bas au centre pour le sol, haut au centre pour le cintre) ;
    ///   Angle (°) = rotation autour de la verticale, sens inverse des aiguilles d'une montre vu de dessus ;
    ///   Bascule X / Y (°) = 0 tant que l'objet n'est pas penché ;
    ///   Largeur / Profondeur / Hauteur (m) = taille réelle de l'objet le long de ses propres axes
    ///   (avant rotation) ; Échelle X / Y / Z = facteur appliqué au modèle 3D le long de ces mêmes axes.
    /// </summary>
    public static class LayoutCsv
    {
        /// <summary>Séparateur des colonnes : point-virgule (ce qu'attend Excel en français).</summary>
        public const char Separator = ';';

        /// <summary>Titres des colonnes, dans l'ordre.</summary>
        public static readonly string[] Header =
        {
            "Scène", "Identifiant", "Nom", "Accroche",
            "X (m)", "Y (m)", "Z (m)",
            "Angle (°)", "Bascule X (°)", "Bascule Y (°)",
            "Largeur (m)", "Profondeur (m)", "Hauteur (m)",
            "Échelle X", "Échelle Y", "Échelle Z",
        };

        /// <summary>
        /// Texte du fichier CSV.
        /// </summary>
        /// <param name="decimalComma">
        /// Vrai : nombres avec une virgule (1,25), pour Excel en français.
        /// Faux : nombres avec un point (1.25), pour les autres logiciels.
        /// </param>
        public static string ToCsv(LayoutData data, bool decimalComma = true)
        {
            var sb = new StringBuilder();
            AppendRow(sb, Header);

            foreach (var entry in data.items)
            {
                AppendRow(sb, Row(data.scene, entry, decimalComma));
            }

            return sb.ToString();
        }

        /// <summary>Les cases d'une ligne du tableau (une par colonne de <see cref="Header"/>).</summary>
        public static string[] Row(string scene, LayoutEntry entry, bool decimalComma = true)
        {
            var position = StagePlan.ToPlan(entry.position);
            var angles = StagePlan.ToPlanAngles(entry.rotation);

            // Axes propres de l'objet : largeur = X, profondeur = Z, hauteur = Y (repère Unity).
            var size = entry.size;
            var scale = entry.scale;

            return new[]
            {
                scene ?? "", entry.itemId ?? "", entry.name ?? "", entry.attachment ?? "",
                Number(position.x, 3, decimalComma), Number(position.y, 3, decimalComma), Number(position.z, 3, decimalComma),
                Number(angles.z, 2, decimalComma), Number(angles.x, 2, decimalComma), Number(angles.y, 2, decimalComma),
                Number(Mathf.Abs(size.x), 3, decimalComma), Number(Mathf.Abs(size.z), 3, decimalComma), Number(Mathf.Abs(size.y), 3, decimalComma),
                Number(scale.x, 4, decimalComma), Number(scale.z, 4, decimalComma), Number(scale.y, 4, decimalComma),
            };
        }

        /// <summary>Nombre arrondi (au millimètre pour 3 décimales), sans zéros inutiles ni « -0 ».</summary>
        public static string Number(float value, int decimals, bool decimalComma)
        {
            var rounded = System.Math.Round((double)value, decimals, System.MidpointRounding.AwayFromZero);
            if (rounded == 0d)
            {
                rounded = 0d; // évite « -0 »
            }

            var text = rounded.ToString("0." + new string('#', decimals), CultureInfo.InvariantCulture);
            return decimalComma ? text.Replace('.', ',') : text;
        }

        /// <summary>Met une case entre guillemets si elle contient un séparateur, un guillemet ou un retour à la ligne.</summary>
        public static string Escape(string cell)
        {
            if (string.IsNullOrEmpty(cell))
            {
                return "";
            }

            if (cell.IndexOf(Separator) < 0 && cell.IndexOf('"') < 0 && cell.IndexOf('\n') < 0 && cell.IndexOf('\r') < 0)
            {
                return cell;
            }

            return "\"" + cell.Replace("\"", "\"\"") + "\"";
        }

        private static void AppendRow(StringBuilder sb, IEnumerable<string> cells)
        {
            var first = true;
            foreach (var cell in cells)
            {
                if (!first)
                {
                    sb.Append(Separator);
                }
                sb.Append(Escape(cell));
                first = false;
            }
            sb.Append("\r\n"); // fin de ligne Windows : Excel la lit partout
        }
    }
}

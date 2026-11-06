using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Bauprobe.Core;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>
    /// Écrit une implantation sous forme de tableau CSV pour le bureau d'études (s'ouvre dans Excel).
    /// Une ligne par élément de décor. Titres et textes en anglais, avec le sens de chaque valeur.
    ///
    /// Repère « plan » (voir <see cref="StagePlan"/>), dont l'origine est le point zéro du bureau d'études,
    /// AU SOL (= le point du sol sous le QR code si l'offset X / Y du QR code est nul), vu depuis la salle :
    ///   X (m) + à droite (cour), Y (m) + vers le loin (lointain), Z (m) hauteur au-dessus du sol du plateau ;
    ///   la position est celle du point d'origine (bas au centre pour le sol, haut au centre pour le cintre) ;
    ///   Rotation (°) = autour de la verticale, + = sens inverse des aiguilles d'une montre vu de dessus,
    ///   0 = l'avant du modèle (son axe Y) regarde vers le loin ;
    ///   Tilt X (°) + = le haut de l'objet penche vers la salle ; Tilt Y (°) + = le haut penche vers la droite
    ///   (0 tant que l'objet n'est pas penché) ;
    ///   Width / Depth / Height (m) = taille réelle de l'objet le long de ses propres axes (avant rotation) ;
    ///   Scale = facteur appliqué au modèle 3D le long de ces mêmes axes.
    /// </summary>
    public static class LayoutCsv
    {
        /// <summary>Séparateur des colonnes : point-virgule (ce qu'attend Excel en français).</summary>
        public const char Separator = ';';

        /// <summary>Titres des colonnes, dans l'ordre (en anglais).</summary>
        public static readonly string[] Header =
        {
            "Scene", "Item ID", "Item name", "Mounting", "Reference point",
            "X from zero (m) [+ right / - left, seen from audience]",
            "Y from zero (m) [+ far / - near]",
            "Z above floor (m)",
            "Rotation (deg) [+ counterclockwise seen from above, 0 = item front faces far]",
            "Tilt X (deg) [+ top leans near]",
            "Tilt Y (deg) [+ top leans right]",
            "Width (m)", "Depth (m)", "Height (m)",
            "Scale width", "Scale depth", "Scale height",
        };

        /// <summary>
        /// Texte du fichier CSV.
        /// </summary>
        /// <param name="decimalComma">
        /// Vrai : nombres avec une virgule (1,25), pour Excel en français.
        /// Faux : nombres avec un point (1.25), pour Excel en anglais et les autres logiciels.
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
            var flown = IsFlown(entry.attachment);

            return new[]
            {
                scene ?? "", entry.itemId ?? "", entry.name ?? "",
                flown ? "Flown" : "Floor",
                flown ? "Top centre" : "Bottom centre",
                Number(position.x, 3, decimalComma), Number(position.y, 3, decimalComma), Number(position.z, 3, decimalComma),
                Number(angles.z, 2, decimalComma), Number(angles.x, 2, decimalComma), Number(angles.y, 2, decimalComma),
                Number(Mathf.Abs(size.x), 3, decimalComma), Number(Mathf.Abs(size.z), 3, decimalComma), Number(Mathf.Abs(size.y), 3, decimalComma),
                Number(scale.x, 4, decimalComma), Number(scale.z, 4, decimalComma), Number(scale.y, 4, decimalComma),
            };
        }

        /// <summary>Vrai pour un élément accroché au cintre (« Cintre » dans la sauvegarde).</summary>
        private static bool IsFlown(string attachment) =>
            !string.IsNullOrEmpty(attachment) && attachment.Trim().ToLowerInvariant() == "cintre";

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

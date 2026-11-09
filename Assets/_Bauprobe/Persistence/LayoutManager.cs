using System.IO;
using System.Text;
using Bauprobe.Core;
using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>
    /// Sauvegarde et recharge une implantation dans un fichier JSON, sur le casque.
    /// Chaque sauvegarde écrit aussi un tableau CSV (même nom, extension .csv) pour le bureau d'études.
    /// Les méthodes Save() et Load() peuvent être branchées directement sur des boutons.
    /// </summary>
    public class LayoutManager : MonoBehaviour
    {
        [SerializeField] private DecorSpawner spawner;

        [Tooltip("Nom utilisé par Save() et Load() sans paramètre.")]
        [SerializeField] private string defaultLayoutName = "implantation";

        [Header("Tableau CSV (bureau d'études)")]
        [Tooltip("Écrit aussi un fichier .csv à chaque sauvegarde, à côté du .json.")]
        [SerializeField] private bool exportCsv = true;

        [Tooltip("Coché : nombres avec une virgule (1,25), pour Excel en français. " +
                 "Décoché : nombres avec un point (1.25).")]
        [SerializeField] private bool csvDecimalComma = true;

        /// <summary>Dossier des sauvegardes (sur le Quest : stockage interne de l'appli).</summary>
        public static string Folder => Path.Combine(Application.persistentDataPath, "Layouts");

        /// <summary>Chemin du tableau CSV d'une sauvegarde.</summary>
        public static string CsvPathFor(string layoutName) => Path.Combine(Folder, layoutName + ".csv");

        public void Save() => Save(defaultLayoutName);

        public void Load() => Load(defaultLayoutName);

        public void Save(string layoutName)
        {
            var data = LayoutSerializer.Capture(spawner.Instances, layoutName, QrOffsetStore.Current);
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathFor(layoutName), LayoutSerializer.ToJson(data));
            Debug.Log($"Implantation '{layoutName}' sauvegardée ({data.items.Count} objets) : {PathFor(layoutName)}");

            if (exportCsv)
            {
                // UTF-8 avec BOM : sinon Excel affiche mal les accents (« ScÃ¨ne »).
                File.WriteAllText(CsvPathFor(layoutName), LayoutCsv.ToCsv(data, csvDecimalComma), new UTF8Encoding(true));
                Debug.Log($"Tableau CSV écrit : {CsvPathFor(layoutName)}");
            }
        }

        public void Load(string layoutName)
        {
            var path = PathFor(layoutName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"Aucune implantation '{layoutName}' trouvée ({path}).");
                return;
            }

            var data = LayoutSerializer.FromJson(File.ReadAllText(path));

            // Les positions sont mesurées depuis le point zéro du bureau d'études : si le QR code a été posé
            // ailleurs depuis (et son offset mis à jour), le décor revient quand même au même endroit du plateau.
            var current = QrOffsetStore.Current;
            if (data.version >= 3 && !current.IsSameAs(data.qrOffset))
            {
                Debug.Log($"Implantation '{layoutName}' sauvegardée avec un autre offset du QR code ({data.qrOffset}) " +
                          $"que l'offset actuel ({current}) : le décor est replacé par rapport au point zéro, " +
                          "pas par rapport au QR code.");
            }

            spawner.Clear();
            foreach (var entry in data.items)
            {
                var definition = spawner.Catalog.Find(entry.itemId);
                if (definition == null)
                {
                    Debug.LogWarning($"Décor '{entry.itemId}' absent du catalogue : ignoré.");
                    continue;
                }

                var instance = spawner.Spawn(definition, entry.position, entry.rotation, entry.scale);

                // Anciennes sauvegardes (version 1) : la position était celle du pivot.
                if (instance != null && data.version < 2)
                {
                    instance.transform.localPosition = entry.position;
                }

                // Taille de référence du Scale en %, avec les étirements (sauvegardes version 5 et plus).
                if (instance != null && entry.baseScale != Vector3.zero)
                {
                    instance.BaseScale = entry.baseScale;
                }

                // Couleur choisie avec le bouton Color (sauvegardes version 4 et plus).
                if (instance != null && entry.hasColor)
                {
                    instance.SetColor(entry.color);
                }
            }

            Debug.Log($"Implantation '{layoutName}' chargée ({data.items.Count} objets).");
        }

        /// <summary>Supprime une sauvegarde (le .json et son tableau .csv).</summary>
        public static void Delete(string layoutName)
        {
            foreach (var file in new[] { PathFor(layoutName), CsvPathFor(layoutName) })
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private static string PathFor(string layoutName) => Path.Combine(Folder, layoutName + ".json");
    }
}

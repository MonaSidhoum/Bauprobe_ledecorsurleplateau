using System.IO;
using Bauprobe.Decor;
using UnityEngine;

namespace Bauprobe.Persistence
{
    /// <summary>
    /// Sauvegarde et recharge une implantation dans un fichier JSON, sur le casque.
    /// Les méthodes Save() et Load() peuvent être branchées directement sur des boutons.
    /// </summary>
    public class LayoutManager : MonoBehaviour
    {
        [SerializeField] private DecorSpawner spawner;

        [Tooltip("Nom utilisé par Save() et Load() sans paramètre.")]
        [SerializeField] private string defaultLayoutName = "implantation";

        /// <summary>Dossier des sauvegardes (sur le Quest : stockage interne de l'appli).</summary>
        public static string Folder => Path.Combine(Application.persistentDataPath, "Layouts");

        public void Save() => Save(defaultLayoutName);

        public void Load() => Load(defaultLayoutName);

        public void Save(string layoutName)
        {
            var data = LayoutSerializer.Capture(spawner.Instances);
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathFor(layoutName), LayoutSerializer.ToJson(data));
            Debug.Log($"Implantation '{layoutName}' sauvegardée ({data.items.Count} objets) : {PathFor(layoutName)}");
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

            spawner.Clear();
            foreach (var entry in data.items)
            {
                var definition = spawner.Catalog.Find(entry.itemId);
                if (definition == null)
                {
                    Debug.LogWarning($"Décor '{entry.itemId}' absent du catalogue : ignoré.");
                    continue;
                }

                spawner.Spawn(definition, entry.position, entry.rotation, entry.scale);
            }

            Debug.Log($"Implantation '{layoutName}' chargée ({data.items.Count} objets).");
        }

        private static string PathFor(string layoutName) => Path.Combine(Folder, layoutName + ".json");
    }
}

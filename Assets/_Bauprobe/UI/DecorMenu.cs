using Bauprobe.Decor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bauprobe.UI
{
    /// <summary>
    /// Menu qui affiche un bouton par élément du catalogue.
    /// Un clic fait apparaître l'objet devant le metteur en scène.
    ///
    /// Les autres boutons (Sauvegarder, Charger, Figer la calibration...) n'ont
    /// pas besoin de code : dans l'Inspector du bouton, section "On Click ()",
    /// on choisit directement LayoutManager.Save, StageCalibrator.Lock, etc.
    /// </summary>
    public class DecorMenu : MonoBehaviour
    {
        [SerializeField] private DecorSpawner spawner;

        [Tooltip("Bouton modèle (Button + texte TextMeshPro). Il est copié pour chaque décor.")]
        [SerializeField] private Button buttonTemplate;

        [Tooltip("Où ranger les boutons (par exemple un objet avec un Vertical Layout Group).")]
        [SerializeField] private Transform buttonContainer;

        private void Start()
        {
            BuildButtons();
        }

        private void BuildButtons()
        {
            buttonTemplate.gameObject.SetActive(false);

            foreach (var definition in spawner.Catalog.Items)
            {
                if (definition == null)
                {
                    continue;
                }

                var button = Instantiate(buttonTemplate, buttonContainer);
                button.gameObject.SetActive(true);

                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = definition.DisplayName;
                }

                var captured = definition; // copie locale pour la lambda
                button.onClick.AddListener(() => spawner.SpawnInFrontOfUser(captured));
            }
        }
    }
}

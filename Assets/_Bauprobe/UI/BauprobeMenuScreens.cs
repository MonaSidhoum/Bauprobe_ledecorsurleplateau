using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bauprobe.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bauprobe.UI
{
    /// <summary>
    /// Ajoute au menu BauprobeMenu :
    ///   1. un petit bouton − / + au-dessus du menu pour le ranger / le rouvrir ;
    ///   2. un écran "Décors" (liste d'objets paginée) à la place du menu principal quand on clique sur Importer ;
    ///   3. un écran "Sauvegarder" : « + Nouvelle sauvegarde » (avec un nom) ou écraser une sauvegarde existante ;
    ///   4. un écran "Charger" : la liste de toutes les sauvegardes, un clic recharge l'implantation.
    ///
    /// A mettre sur la RACINE du prefab BauprobeMenu (l'objet qui contient tous les boutons).
    /// Remplace MenuCollapseToggle / RangerMenu : retire-les du prefab s'ils y sont.
    ///
    /// Les boutons Importer, Sauvegarder et Charger sont repris au lancement : leur ancien onClick est remplacé
    /// par l'ouverture de l'écran correspondant. Les sauvegardes sont les fichiers du dossier de LayoutManager.
    /// Le menu principal n'est pas désactivé, seulement caché : ses scripts continuent de tourner.
    /// </summary>
    [DisallowMultipleComponent]
    public class BauprobeMenuScreens : MonoBehaviour
    {
        private enum MenuScreen { Main, Decors, Save, Load, Naming, Confirm }

        private class Row
        {
            public string label;
            public System.Action onClick;
            public System.Action onDelete;   // non nul = petite croix à droite de la ligne
            public bool accent;              // bouton à cadre (ex : « Nouvelle sauvegarde », « Valider »)
        }

        // ------------------------------------------------------------------ réglages
        [Header("Boutons du menu principal (vide = trouvés automatiquement par leur texte)")]
        [SerializeField] private Button importButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        [Header("Liens")]
        [Tooltip("Vide = cherché automatiquement sur ce menu. La liste des objets est celle de « Import Items » de BauprobeMenuLogic.")]
        [SerializeField] private BauprobeMenuLogic logic;
        [Tooltip("Vide = cherché automatiquement dans la scène (objet Managers).")]
        [SerializeField] private LayoutManager layoutManager;

        [Header("Textes")]
        [SerializeField] private string decorsTitle = "Décors";
        [SerializeField] private string saveTitle = "Sauvegarder";
        [SerializeField] private string loadTitle = "Charger";
        [SerializeField] private string namingTitle = "Nom de la sauvegarde";
        [SerializeField] private string newSaveLabel = "+ Nouvelle sauvegarde";
        [SerializeField] private string backLabel = "Retour";
        [SerializeField] private string pageLabel = "page";
        [SerializeField] private string noDecorLabel = "Aucun objet";
        [SerializeField] private string noSaveLabel = "Aucune sauvegarde";

        [Header("Sauvegardes")]
        [Tooltip("Nombre maximum de sauvegardes. Au-delà, il faut en supprimer une.")]
        [SerializeField, Min(1)] private int maxSaves = 50;
        [SerializeField, Min(5)] private int maxNameLength = 30;

        [Header("Mise en page des écrans")]
        [Tooltip("Hauteur d'une ligne, en proportion de la hauteur du menu (plus petit = plus de lignes par page).")]
        [SerializeField, Range(0.04f, 0.12f)] private float itemHeightRatio = 0.065f;
        [SerializeField, Range(0.4f, 0.95f)] private float itemWidthRatio = 0.72f;
        [Tooltip("(0, 0) = même taille que le menu principal, calculée automatiquement.")]
        [SerializeField] private Vector2 overridePanelSize = Vector2.zero;
        [Tooltip("Dessine un fond derrière les écrans (utile si le fond du menu principal est un enfant).")]
        [SerializeField] private bool drawPanelBackground = true;

        [Header("Bouton − / +")]
        [SerializeField] private bool startCollapsed = false;
        [SerializeField] private string collapseLabel = "−";   // −
        [SerializeField] private string expandLabel = "+";
        [Tooltip("(0, 0) = taille automatique d'après le menu.")]
        [SerializeField] private Vector2 toggleSize = Vector2.zero;
        [Tooltip("Coché : au-dessus du menu. Décoché : dans le coin haut droit, à l'intérieur " +
                 "(si le bouton n'est pas cliquable au casque).")]
        [SerializeField] private bool placeAboveMenu = true;
        [Tooltip("Position du bouton + quand le menu est rangé, par rapport au centre du menu. " +
                 "Z négatif = vers toi (hors du plan du menu). Unités du Canvas.")]
        [SerializeField] private Vector3 collapsedPosition = new Vector3(0f, 0f, -10f);

        [Header("Style (couleurs de la maquette)")]
        [SerializeField] private Color panelColor = new Color(0.42f, 0.00f, 0.012f, 1f);
        [SerializeField] private Color buttonColor = new Color(0.91f, 0.77f, 0.58f, 1f);
        [SerializeField] private Color buttonTextColor = new Color(0.42f, 0.00f, 0.012f, 1f);
        [Tooltip("Titre, texte « page », flèches, séparateur, texte des boutons à cadre.")]
        [SerializeField] private Color accentColor = new Color(0.91f, 0.77f, 0.58f, 1f);
        [SerializeField] private Color backFillColor = new Color(0.33f, 0.04f, 0.03f, 1f);
        [Tooltip("Idéalement une police à empattements, comme la maquette. Vide = police par défaut.")]
        [SerializeField] private TMP_FontAsset font;
        [Tooltip("Ex : RoundedRect.png. Vide = sprite arrondi généré par le script.")]
        [SerializeField] private Sprite roundedSprite;

        // ------------------------------------------------------------------ interne
        private struct SavedState
        {
            public CanvasGroup group;
            public float alpha;
            public bool interactable;
            public bool blocksRaycasts;
        }

        private readonly List<SavedState> _mainTargets = new List<SavedState>();
        private RectTransform _rootRT;
        private Canvas _canvas;
        private Graphic _ownGraphic;
        private bool _ownGraphicWasEnabled;

        private GameObject _toggleGO;
        private RectTransform _toggleRT;
        private TMP_Text _toggleLabel;

        private RectTransform _pickerRoot;
        private RectTransform _listRoot;
        private GameObject _prevArrow;
        private GameObject _nextArrow;
        private TMP_Text _pageText;

        private Sprite _rounded;
        private Sprite _triangle;

        private MenuScreen _screen = MenuScreen.Main;
        private bool _collapsed;

        // écran de liste en cours
        private string _title;
        private string _emptyText;
        private System.Func<List<Row>> _provider;
        private System.Action _back;
        private List<Row> _rows = new List<Row>();
        private int _page;
        private int _perPage = 1;
        private int _pageCount = 1;
        private bool _hasNav;
        private float _itemH;
        private float _gap;
        private float _listTop;

        // saisie du nom
        private TouchScreenKeyboard _keyboard;
        private string _typed = "";

        public bool IsCollapsed => _collapsed;

        // ================================================================== cycle de vie
        private void Awake()
        {
            _rootRT = (RectTransform)transform;
            if (logic == null) logic = GetComponentInParent<BauprobeMenuLogic>();
            if (logic == null) logic = GetComponentInChildren<BauprobeMenuLogic>(true);
            if (logic == null) Debug.LogWarning("[BauprobeMenuScreens] BauprobeMenuLogic introuvable : assigne-le dans le champ « Logic ».", this);
            _canvas = GetComponentInParent<Canvas>();
            _rounded = roundedSprite != null ? roundedSprite : MakeRoundedSprite();
            _triangle = MakeTriangleSprite();

            CollectMainTargets();
            CreateToggleButton();

            _collapsed = startCollapsed;
            ApplyVisibility();
        }

        private void Start()
        {
            StartCoroutine(PostInit());
        }

        private IEnumerator PostInit()
        {
            // Une image d'attente : les autres scripts du menu ont fini leur démarrage et la mise en page est calculée.
            yield return null;
            RepositionToggle();
            HookButtons();
            EnsureLayoutManager();
        }

        private void OnDisable()
        {
            CloseKeyboard();
        }

        private void Update()
        {
            if (_keyboard == null) return;

            var status = _keyboard.status;
            string text = _keyboard.text;

            if (status == TouchScreenKeyboard.Status.Visible)
            {
                if (text != _typed)
                {
                    _typed = text;
                    if (_screen == MenuScreen.Naming) RefreshScreen();
                }
                return;
            }

            if (status == TouchScreenKeyboard.Status.Done)
            {
                _typed = text;
                _keyboard = null;
                if (_screen == MenuScreen.Naming) FinishNaming(_typed);
                return;
            }

            // Annulé ou fermé sans valider : on reste sur l'écran, on peut rouvrir le clavier ou valider
            _keyboard = null;
            if (_screen == MenuScreen.Naming) RefreshScreen();
        }

        // ================================================================== API publique
        public void Toggle() => SetCollapsed(!_collapsed);

        public void SetCollapsed(bool collapsed)
        {
            _collapsed = collapsed;
            ApplyVisibility();
        }

        public void OpenDecors()
        {
            if (_collapsed) return;
            OpenScreen(MenuScreen.Decors, decorsTitle, noDecorLabel, BuildDecorRows, CloseToMain);
        }

        public void OpenSave()
        {
            if (_collapsed) return;
            if (!EnsureLayoutManager()) return;
            OpenScreen(MenuScreen.Save, saveTitle, noSaveLabel, BuildSaveRows, CloseToMain);
        }

        public void OpenLoad()
        {
            if (_collapsed) return;
            if (!EnsureLayoutManager()) return;
            OpenScreen(MenuScreen.Load, loadTitle, noSaveLabel, BuildLoadRows, CloseToMain);
        }

        /// <summary>Revient au menu principal.</summary>
        public void CloseToMain()
        {
            CloseKeyboard();
            _screen = MenuScreen.Main;
            ApplyVisibility();
        }

        // ================================================================== visibilité
        private void CollectMainTargets()
        {
            _ownGraphic = GetComponent<Graphic>();
            _ownGraphicWasEnabled = _ownGraphic != null && _ownGraphic.enabled;

            foreach (Transform child in transform)
            {
                var group = child.GetComponent<CanvasGroup>();
                if (group == null) group = child.gameObject.AddComponent<CanvasGroup>();

                _mainTargets.Add(new SavedState
                {
                    group = group,
                    alpha = group.alpha,
                    interactable = group.interactable,
                    blocksRaycasts = group.blocksRaycasts
                });
            }
        }

        private void ApplyVisibility()
        {
            bool showMain = !_collapsed && _screen == MenuScreen.Main;
            bool showList = !_collapsed && _screen != MenuScreen.Main;

            foreach (var s in _mainTargets)
            {
                if (s.group == null) continue;
                s.group.alpha = showMain ? s.alpha : 0f;
                s.group.interactable = showMain && s.interactable;
                s.group.blocksRaycasts = showMain && s.blocksRaycasts;
            }

            // Le fond éventuel de la racine reste affiché sur tous les écrans, et disparaît quand le menu est rangé.
            if (_ownGraphic != null)
                _ownGraphic.enabled = !_collapsed && _ownGraphicWasEnabled;

            if (_pickerRoot != null)
                _pickerRoot.gameObject.SetActive(showList);

            if (_toggleLabel != null)
                _toggleLabel.text = _collapsed ? expandLabel : collapseLabel;

            RepositionToggle();
        }

        // ================================================================== boutons du menu principal
        private void HookButtons()
        {
            if (importButton == null) importButton = FindButton("import");
            if (saveButton == null) saveButton = FindButton("sauveg", "save");
            if (loadButton == null) loadButton = FindButton("charg", "load");

            Replace(importButton, OpenDecors, "Importer");
            Replace(saveButton, OpenSave, "Sauvegarder");
            Replace(loadButton, OpenLoad, "Charger");
        }

        private void Replace(Button button, UnityAction action, string name)
        {
            if (button == null)
            {
                Debug.LogWarning("[BauprobeMenuScreens] Bouton « " + name + " » introuvable : assigne-le dans l'inspecteur.", this);
                return;
            }

            // L'ancien onClick (par ex. LayoutManager.Save) est remplacé : le clic ouvre l'écran correspondant.
            var replacement = new Button.ButtonClickedEvent();
            replacement.AddListener(action);
            button.onClick = replacement;
        }

        private Button FindButton(params string[] keywords)
        {
            foreach (var b in GetComponentsInChildren<Button>(true))
            {
                if (b == null || b.gameObject == _toggleGO) continue;
                if (_pickerRoot != null && b.transform.IsChildOf(_pickerRoot)) continue;

                var label = b.GetComponentInChildren<TMP_Text>(true);
                string haystack = (b.name + " " + (label != null ? label.text : "")).ToLowerInvariant();
                foreach (var k in keywords)
                    if (haystack.Contains(k)) return b;
            }
            return null;
        }

        // ================================================================== contenu des écrans
        private List<Row> BuildDecorRows()
        {
            var rows = new List<Row>();
            int count = logic != null && logic.importItems != null ? logic.importItems.Count : 0;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                string l = logic.importItems[i] != null ? logic.importItems[i].label : "";
                rows.Add(new Row
                {
                    label = string.IsNullOrEmpty(l) ? "Objet " + (i + 1) : l,
                    onClick = () => { if (logic != null) logic.SpawnIndex(index); CloseToMain(); }
                });
            }
            return rows;
        }

        private List<Row> BuildSaveRows()
        {
            var rows = new List<Row>();
            rows.Add(new Row { label = newSaveLabel, accent = true, onClick = StartNaming });

            foreach (var n in SavedNames())
            {
                string name = n;
                rows.Add(new Row
                {
                    label = name,
                    onClick = () => ConfirmOverwrite(name),
                    onDelete = () => ConfirmDelete(name, OpenSave)
                });
            }
            return rows;
        }

        private List<Row> BuildLoadRows()
        {
            var rows = new List<Row>();
            foreach (var n in SavedNames())
            {
                string name = n;
                rows.Add(new Row
                {
                    label = name,
                    onClick = () => { if (EnsureLayoutManager()) layoutManager.Load(name); CloseToMain(); },
                    onDelete = () => ConfirmDelete(name, OpenLoad)
                });
            }
            return rows;
        }

        // ================================================================== sauvegardes
        private bool EnsureLayoutManager()
        {
            if (layoutManager == null) layoutManager = FindFirstObjectByType<LayoutManager>();
            if (layoutManager == null)
                Debug.LogWarning("[BauprobeMenuScreens] LayoutManager introuvable dans la scène (objet Managers).", this);
            return layoutManager != null;
        }

        /// <summary>Noms des sauvegardes, de la plus récente à la plus ancienne.</summary>
        private List<string> SavedNames()
        {
            var names = new List<string>();
            string folder = LayoutManager.Folder;
            if (!Directory.Exists(folder)) return names;

            var files = new List<string>(Directory.GetFiles(folder, "*.json"));
            files.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
            foreach (var f in files) names.Add(Path.GetFileNameWithoutExtension(f));
            return names;
        }

        private static bool SaveExists(string name) =>
            File.Exists(Path.Combine(LayoutManager.Folder, name + ".json"));

        private void ConfirmOverwrite(string name)
        {
            Confirm("Écraser « " + Short(name) + " » ?", () => DoSave(name), OpenSave);
        }

        private void ConfirmDelete(string name, System.Action returnTo)
        {
            Confirm("Supprimer « " + Short(name) + " » ?", () =>
            {
                try { File.Delete(Path.Combine(LayoutManager.Folder, name + ".json")); }
                catch (System.Exception e) { Debug.LogWarning("[BauprobeMenuScreens] Suppression impossible : " + e.Message, this); }
                returnTo();
            }, returnTo);
        }

        private void DoSave(string name)
        {
            if (!EnsureLayoutManager()) return;

            if (!SaveExists(name) && SavedNames().Count >= maxSaves)
            {
                ShowMessage("Limite de " + maxSaves + " atteinte", OpenSave);
                return;
            }

            layoutManager.Save(name);
            CloseToMain();
        }

        // ------------------------------------------------------------------ nom de la sauvegarde
        private void StartNaming()
        {
            _typed = "";

            // Sans clavier système (éditeur Unity) : nom automatique
            if (!TouchScreenKeyboard.isSupported)
            {
                FinishNaming("");
                return;
            }

            OpenScreen(MenuScreen.Naming, namingTitle, "", BuildNamingRows, () => { CloseKeyboard(); OpenSave(); });
            OpenKeyboard();
        }

        private List<Row> BuildNamingRows()
        {
            return new List<Row>
            {
                new Row
                {
                    label = "Nom : " + (string.IsNullOrEmpty(_typed) ? "(toucher pour écrire)" : _typed),
                    onClick = OpenKeyboard
                },
                new Row { label = "Valider", accent = true, onClick = () => { CloseKeyboard(); FinishNaming(_typed); } },
                new Row { label = "Nom automatique", onClick = () => { CloseKeyboard(); FinishNaming(""); } }
            };
        }

        private void OpenKeyboard()
        {
            _keyboard = TouchScreenKeyboard.Open(_typed, TouchScreenKeyboardType.Default, false, false, false, false,
                "Nom", maxNameLength);
        }

        private void CloseKeyboard()
        {
            if (_keyboard != null)
            {
                _keyboard.active = false;
                _keyboard = null;
            }
        }

        private void FinishNaming(string raw)
        {
            string name = Sanitize(raw);
            if (string.IsNullOrEmpty(name)) name = AutoName();

            if (SaveExists(name)) ConfirmOverwrite(name);
            else DoSave(name);
        }

        private string Sanitize(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            foreach (char c in Path.GetInvalidFileNameChars()) raw = raw.Replace(c.ToString(), "");
            raw = raw.Trim().TrimEnd('.');
            if (raw.Length > maxNameLength) raw = raw.Substring(0, maxNameLength).Trim();
            return raw;
        }

        private string AutoName()
        {
            int i = SavedNames().Count + 1;
            while (SaveExists("Scène " + i)) i++;
            return "Scène " + i;
        }

        private static string Short(string name) => name.Length <= 16 ? name : name.Substring(0, 15) + "...";

        // ------------------------------------------------------------------ messages et confirmations
        private void Confirm(string message, System.Action onYes, System.Action onNo)
        {
            OpenScreen(MenuScreen.Confirm, message, "", () => new List<Row>
            {
                new Row { label = "Oui", accent = true, onClick = onYes },
                new Row { label = "Non", onClick = onNo }
            }, onNo);
        }

        private void ShowMessage(string message, System.Action next)
        {
            OpenScreen(MenuScreen.Confirm, message, "", () => new List<Row>
            {
                new Row { label = "OK", accent = true, onClick = next }
            }, next);
        }

        // ================================================================== bouton − / +
        private void CreateToggleButton()
        {
            _toggleGO = new GameObject("Btn_RangerMenu",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            _toggleGO.transform.SetParent(transform, false);
            _toggleGO.transform.SetAsLastSibling();
            _toggleGO.GetComponent<LayoutElement>().ignoreLayout = true;   // pas repositionné par un Layout Group

            _toggleRT = (RectTransform)_toggleGO.transform;
            _toggleRT.sizeDelta = toggleSize.x > 0f && toggleSize.y > 0f ? toggleSize : new Vector2(80f, 50f);

            var img = _toggleGO.GetComponent<Image>();
            StyleRounded(img, buttonColor, _toggleRT.sizeDelta.y * 0.2f);

            var button = _toggleGO.GetComponent<Button>();
            button.targetGraphic = img;
            StyleButtonColors(button);
            button.onClick.AddListener(Toggle);

            var label = NewRect("Texte", _toggleRT);
            Stretch(label);
            _toggleLabel = AddText(label.gameObject, collapseLabel, buttonTextColor, _toggleRT.sizeDelta.y * 0.8f, FontStyles.Bold);
        }

        private void RepositionToggle()
        {
            if (_toggleRT == null) return;

            Vector2 size, center;
            GetContentRect(out size, out center);

            float h = toggleSize.y > 0f ? toggleSize.y : Mathf.Max(0.0001f, size.y * 0.06f);
            float w = toggleSize.x > 0f ? toggleSize.x : h * 1.5f;
            float margin = h * 0.2f;

            _toggleRT.sizeDelta = new Vector2(w, h);
            _toggleRT.anchorMin = _toggleRT.anchorMax = _rootRT.pivot;

            if (_collapsed)
            {
                // Menu rangé : le "+" se place là où était le centre du menu (X, Y, Z réglables)
                _toggleRT.pivot = new Vector2(0.5f, 0.5f);
                _toggleRT.anchoredPosition3D = collapsedPosition;
            }
            else if (placeAboveMenu)
            {
                _toggleRT.pivot = new Vector2(0.5f, 0f);
                _toggleRT.anchoredPosition3D = new Vector3(center.x, center.y + size.y * 0.5f + margin, 0f);
            }
            else
            {
                _toggleRT.pivot = new Vector2(1f, 1f);
                _toggleRT.anchoredPosition3D = new Vector3(center.x + size.x * 0.5f - margin, center.y + size.y * 0.5f - margin, 0f);
            }

            _toggleRT.SetAsLastSibling();
        }

        // ================================================================== écran de liste (commun à tous les sous-menus)
        private void OpenScreen(MenuScreen screen, string title, string emptyText,
                                System.Func<List<Row>> provider, System.Action back)
        {
            _screen = screen;
            _title = title;
            _emptyText = emptyText;
            _provider = provider;
            _back = back;
            BuildScreen(provider(), 0);
            ApplyVisibility();
        }

        /// <summary>Reconstruit l'écran en cours (même page) : après une saisie de texte, par exemple.</summary>
        private void RefreshScreen()
        {
            if (_provider == null) return;
            BuildScreen(_provider(), _page);
            ApplyVisibility();
        }

        private void BuildScreen(List<Row> rows, int page)
        {
            _rows = rows ?? new List<Row>();

            if (_pickerRoot != null)
            {
                _pickerRoot.gameObject.SetActive(false);
                Destroy(_pickerRoot.gameObject);
                _pickerRoot = null;
            }

            Vector2 size, center;
            GetContentRect(out size, out center);
            if (overridePanelSize.x > 0.0001f && overridePanelSize.y > 0.0001f) size = overridePanelSize;

            float W = size.x, H = size.y;
            float pad = Mathf.Min(W, H) * 0.05f;
            _itemH = H * itemHeightRatio;
            _gap = _itemH * 0.2f;
            float titleH = Mathf.Min(_itemH * 1.8f, H * 0.12f);
            float backH = _itemH * 1.1f;
            float dividerH = _itemH * 0.3f;
            float navH = _itemH * 0.9f;

            // Combien de lignes par page : le maximum qui tient dans la hauteur du menu
            _listTop = pad + titleH + _gap;
            float bottomBase = pad + backH + _gap + dividerH + _gap;
            float listH = H - _listTop - bottomBase;
            int capacityNoNav = Mathf.Max(1, Mathf.FloorToInt((listH + _gap) / (_itemH + _gap)));

            if (_rows.Count <= capacityNoNav)
            {
                _perPage = Mathf.Max(1, _rows.Count);
                _hasNav = false;
            }
            else
            {
                float listWithNav = listH - navH - _gap;
                _perPage = Mathf.Max(1, Mathf.FloorToInt((listWithNav + _gap) / (_itemH + _gap)));
                _hasNav = true;
            }
            _pageCount = Mathf.Max(1, Mathf.CeilToInt(_rows.Count / (float)_perPage));

            // Panneau (même taille et même position que le menu principal)
            _pickerRoot = NewRect("Ecran_" + _screen, transform);
            // Sans ça, le VerticalLayoutGroup du menu range l'écran comme un bouton de plus
            // (il l'écrase et l'empile sous les autres) au lieu de le superposer au menu.
            _pickerRoot.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            _pickerRoot.anchorMin = _pickerRoot.anchorMax = _rootRT.pivot;
            _pickerRoot.pivot = new Vector2(0.5f, 0.5f);
            _pickerRoot.sizeDelta = size;
            _pickerRoot.anchoredPosition = center;

            if (drawPanelBackground)
            {
                var bg = NewRect("Fond", _pickerRoot);
                Stretch(bg);
                var bgImg = bg.gameObject.AddComponent<Image>();
                StyleRounded(bgImg, panelColor, pad * 0.6f);
                bgImg.raycastTarget = true;
            }

            // Titre
            var title = NewRect("Titre", _pickerRoot);
            SetTopCenter(title, pad, new Vector2(W - 2f * pad, titleH));
            AddText(title.gameObject, _title, accentColor, titleH * 0.85f, FontStyles.Normal);

            // Liste
            _listRoot = NewRect("Liste", _pickerRoot);
            Stretch(_listRoot);

            // Séparateur avec ses deux losanges
            var divider = NewRect("Separateur", _pickerRoot);
            SetBottomCenter(divider, pad + backH + _gap, new Vector2(W * 0.86f, dividerH));
            BuildDivider(divider, dividerH);

            // Flèches de page
            _prevArrow = _nextArrow = null;
            _pageText = null;
            if (_hasNav)
                BuildNav(_pickerRoot, W, pad + backH + _gap + dividerH + _gap, navH);

            // Bouton Retour (fond sombre, cadre crème)
            var back = NewRect("Bouton_Retour", _pickerRoot);
            SetBottomCenter(back, pad, new Vector2(W * 0.5f, backH));
            CreateOutlinedButton(back, backLabel, backH, () => _back?.Invoke());

            ShowPage(page);
            _pickerRoot.SetAsLastSibling();
            _toggleRT.SetAsLastSibling();
        }

        private void ShowPage(int page)
        {
            _page = Mathf.Clamp(page, 0, _pageCount - 1);

            foreach (Transform child in _listRoot)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            float itemW = _pickerRoot.sizeDelta.x * itemWidthRatio;
            int first = _page * _perPage;
            int last = Mathf.Min(_rows.Count, first + _perPage);

            if (_rows.Count == 0 && !string.IsNullOrEmpty(_emptyText))
            {
                var empty = NewRect("Vide", _listRoot);
                SetTopCenter(empty, _listTop, new Vector2(itemW, _itemH));
                AddText(empty.gameObject, _emptyText, accentColor, _itemH * 0.5f, FontStyles.Italic);
            }

            for (int i = first, line = 0; i < last; i++, line++)
            {
                var row = _rows[i];
                var rt = NewRect("Ligne_" + i, _listRoot);
                SetTopCenter(rt, _listTop + line * (_itemH + _gap), new Vector2(itemW, _itemH));

                float deleteW = row.onDelete != null ? _itemH : 0f;
                float space = row.onDelete != null ? _itemH * 0.15f : 0f;

                var main = NewRect("Bouton", rt);
                main.anchorMin = Vector2.zero;
                main.anchorMax = Vector2.one;
                main.offsetMin = Vector2.zero;
                main.offsetMax = new Vector2(-(deleteW + space), 0f);

                if (row.accent) CreateOutlinedButton(main, row.label, _itemH, row.onClick);
                else CreateFilledButton(main, row.label, _itemH, row.onClick);

                if (row.onDelete != null)
                {
                    var del = NewRect("Supprimer", rt);
                    del.anchorMin = new Vector2(1f, 0f);
                    del.anchorMax = new Vector2(1f, 1f);
                    del.pivot = new Vector2(1f, 0.5f);
                    del.sizeDelta = new Vector2(deleteW, 0f);
                    del.anchoredPosition = Vector2.zero;
                    CreateFilledButton(del, "×", _itemH, row.onDelete);   // ×
                }
            }

            if (_pageText != null) _pageText.text = pageLabel + " " + (_page + 1);
            if (_prevArrow != null) _prevArrow.SetActive(_page > 0);
            if (_nextArrow != null) _nextArrow.SetActive(_page < _pageCount - 1);
        }

        private void BuildNav(RectTransform parent, float W, float yFromBottom, float navH)
        {
            var nav = NewRect("Pages", parent);
            SetBottomCenter(nav, yFromBottom, new Vector2(W * 0.8f, navH));

            var textRT = NewRect("Page", nav);
            textRT.anchorMin = textRT.anchorMax = new Vector2(0.5f, 0.5f);
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = new Vector2(W * 0.3f, navH);
            textRT.anchoredPosition = Vector2.zero;
            _pageText = AddText(textRT.gameObject, pageLabel + " 1", accentColor, navH * 0.6f, FontStyles.Normal);

            _prevArrow = CreateArrow(nav, "Page_Precedente", -W * 0.22f, navH, true, () => ShowPage(_page - 1));
            _nextArrow = CreateArrow(nav, "Page_Suivante", W * 0.22f, navH, false, () => ShowPage(_page + 1));
        }

        private GameObject CreateArrow(RectTransform parent, string name, float x, float size, bool flip, UnityAction onClick)
        {
            var rt = NewRect(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(x, 0f);
            if (flip) rt.localScale = new Vector3(-1f, 1f, 1f);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = _triangle;
            img.color = accentColor;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            StyleButtonColors(btn);
            btn.onClick.AddListener(onClick);
            return rt.gameObject;
        }

        private void BuildDivider(RectTransform divider, float height)
        {
            float thickness = Mathf.Max(0.0001f, height * 0.18f);

            var line = NewRect("Ligne", divider);
            line.anchorMin = new Vector2(0f, 0.5f);
            line.anchorMax = new Vector2(1f, 0.5f);
            line.pivot = new Vector2(0.5f, 0.5f);
            line.offsetMin = new Vector2(0f, -thickness * 0.5f);
            line.offsetMax = new Vector2(0f, thickness * 0.5f);
            var lineImg = line.gameObject.AddComponent<Image>();
            lineImg.color = accentColor;
            lineImg.raycastTarget = false;

            for (int side = 0; side < 2; side++)
            {
                var diamond = NewRect(side == 0 ? "Losange_G" : "Losange_D", divider);
                diamond.anchorMin = diamond.anchorMax = new Vector2(side == 0 ? 0f : 1f, 0.5f);
                diamond.pivot = new Vector2(0.5f, 0.5f);
                diamond.sizeDelta = new Vector2(height * 0.7f, height * 0.7f);
                diamond.anchoredPosition = Vector2.zero;
                diamond.localEulerAngles = new Vector3(0f, 0f, 45f);
                var img = diamond.gameObject.AddComponent<Image>();
                img.color = accentColor;
                img.raycastTarget = false;
            }
        }

        // ------------------------------------------------------------------ boutons génériques
        /// <summary>Bouton crème plein, texte bordeaux.</summary>
        private void CreateFilledButton(RectTransform rt, string label, float height, System.Action onClick)
        {
            var img = rt.gameObject.AddComponent<Image>();
            StyleRounded(img, buttonColor, height * 0.12f);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            StyleButtonColors(btn);
            btn.onClick.AddListener(() => onClick?.Invoke());

            var text = NewRect("Texte", rt);
            Stretch(text);
            text.offsetMin = new Vector2(height * 0.2f, 0f);
            text.offsetMax = new Vector2(-height * 0.2f, 0f);
            AddText(text.gameObject, label, buttonTextColor, height * 0.5f, FontStyles.Normal);
        }

        /// <summary>Bouton à cadre crème, fond sombre, texte crème (style du bouton Retour).</summary>
        private void CreateOutlinedButton(RectTransform rt, string label, float height, System.Action onClick)
        {
            var outer = rt.gameObject.AddComponent<Image>();
            StyleRounded(outer, accentColor, height * 0.15f);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = outer;
            StyleButtonColors(btn);
            btn.onClick.AddListener(() => onClick?.Invoke());

            var inner = NewRect("Interieur", rt);
            Stretch(inner);
            float inset = height * 0.12f;
            inner.offsetMin = new Vector2(inset, inset);
            inner.offsetMax = new Vector2(-inset, -inset);
            var innerImg = inner.gameObject.AddComponent<Image>();
            StyleRounded(innerImg, backFillColor, height * 0.1f);
            innerImg.raycastTarget = false;

            AddText(inner.gameObject, label, accentColor, height * 0.5f, FontStyles.Normal);
        }

        // ================================================================== mesures
        /// <summary>Rectangle occupé par le menu principal, dans l'espace de la racine du menu.</summary>
        private void GetContentRect(out Vector2 size, out Vector2 center)
        {
            bool has = false;
            Bounds total = default;
            var corners = new Vector3[4];

            foreach (var s in _mainTargets)
            {
                if (s.group == null) continue;
                foreach (var rt in s.group.GetComponentsInChildren<RectTransform>(false))
                {
                    rt.GetWorldCorners(corners);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 p = _rootRT.InverseTransformPoint(corners[i]);
                        p.z = 0f;
                        if (!has) { total = new Bounds(p, Vector3.zero); has = true; }
                        else total.Encapsulate(p);
                    }
                }
            }

            if (has && total.size.x > 0.0001f && total.size.y > 0.0001f)
            {
                size = new Vector2(total.size.x, total.size.y);
                center = new Vector2(total.center.x, total.center.y);
            }
            else
            {
                Rect r = _rootRT.rect;
                size = r.size;
                center = r.center;
            }
        }

        // ================================================================== petits outils UI
        private RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetTopCenter(RectTransform rt, float yFromTop, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(0f, -yFromTop);
        }

        private static void SetBottomCenter(RectTransform rt, float yFromBottom, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(0f, yFromBottom);
        }

        private TMP_Text AddText(GameObject target, string text, Color color, float maxSize, FontStyles style)
        {
            var tmp = target.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = Mathf.Max(0.01f, maxSize);
            tmp.fontSizeMin = Mathf.Max(0.005f, maxSize * 0.3f);
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.raycastTarget = false;
            if (font != null) tmp.font = font;
            return tmp;
        }

        private void StyleRounded(Image img, Color color, float cornerUnits)
        {
            img.sprite = _rounded;
            img.type = Image.Type.Sliced;
            img.color = color;
            float refPpu = _canvas != null ? _canvas.referencePixelsPerUnit : 100f;
            img.pixelsPerUnitMultiplier = Mathf.Max(0.01f, 16f * refPpu / (100f * Mathf.Max(cornerUnits, 0.0001f)));
        }

        private static void StyleButtonColors(Button button)
        {
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        // ================================================================== sprites générés (aucun asset à importer)
        private static Sprite MakeRoundedSprite(int size = 64, int radius = 16)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;
                    float cx = Mathf.Clamp(px, radius, size - radius);
                    float cy = Mathf.Clamp(py, radius, size - radius);
                    float dist = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    byte alpha = (byte)(Mathf.Clamp01(radius - dist + 0.5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        /// <summary>Triangle plein qui pointe vers la droite (la flèche gauche est ce même sprite retourné).</summary>
        private static Sprite MakeTriangleSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float fx = x + 0.5f;
                    float fy = y + 0.5f;
                    float d1 = fx - 14f;                                        // bord gauche
                    float d2 = (56f - 0.6f * (fx - 14f) - fy) / 1.166f;         // bord haut
                    float d3 = (fy - 8f - 0.6f * (fx - 14f)) / 1.166f;          // bord bas
                    float d = Mathf.Min(d1, Mathf.Min(d2, d3));
                    byte alpha = (byte)(Mathf.Clamp01(d + 0.5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}

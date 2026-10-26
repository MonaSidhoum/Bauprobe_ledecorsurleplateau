using System.Collections;
using System.Collections.Generic;
using Bauprobe.Decor;
using Meta.XR.MRUtilityKit;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bauprobe.UI
{
    /// <summary>
    /// Logique du menu Bauprobe. Le menu lui-même (fond, boutons, textes) est un vrai objet
    /// de la Hierarchy, généré par Bauprobe > Générer le menu. Ce script ne dessine rien :
    /// il fait seulement marcher les boutons.
    ///
    /// Fonctions QR gardées du menu d'origine :
    ///   - RequestPermission : permission Scene (obligatoire pour détecter un QR code)
    ///   - ToggleTracking    : active / coupe la détection du QR code
    ///   - affichage de l'état (permission, suivi, nombre de QR détectés)
    /// </summary>
    public class BauprobeMenuLogic : MonoBehaviour
    {
        [Header("Textes d'état (assignés par le générateur)")]
        public TMP_Text permissionStatus;
        public TMP_Text trackingLabel;
        public TMP_Text trackingStatus;
        public TMP_Text detectedStatus;

        [Header("Importer")]
        public GameObject importList;
        [Tooltip("Crée les objets sur le plateau. Le bouton n°i de la liste importe le décor n°i du catalogue du DecorSpawner.")]
        public DecorSpawner spawner;

        [Header("QR code")]
        public string scenePermission = "com.oculus.permission.USE_SCENE";
        public bool trackingInitiallyOn = true;

        [Header("Menu d'origine (auto-détecté, à assigner si introuvable)")]
        public Button originalRequestButton;
        public Button originalTrackingButton;
        public Toggle originalTrackingToggle;
        public bool hideOriginalPanel = true;

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private RectTransform _container;
        private bool _trackingOn;
        private float _nextRefresh;
        private string _lastPayload = "";
        private readonly HashSet<MRUKTrackable> _tracked = new HashSet<MRUKTrackable>();
        private MRUK.MRUKSettings _hookedSettings;

        // ------------------------------------------------------------------ cycle de vie
        private IEnumerator Start()
        {
            _container = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) { Debug.LogError("[BauprobeMenu] Pas de Canvas parent.", this); yield break; }

            _canvas = canvas.rootCanvas;
            _canvasRect = _canvas.GetComponent<RectTransform>();
            _trackingOn = trackingInitiallyOn;

            FindOriginalControls();
            if (hideOriginalPanel) HideOriginalPanel();
            if (importList != null) importList.SetActive(false);

            yield return null;
            FitCanvasToMenu();
            RefreshStatus();
            StartCoroutine(HookMruk());
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.25f;
            RefreshStatus();
        }

        private void OnDestroy()
        {
            if (_hookedSettings != null)
            {
                _hookedSettings.TrackableAdded.RemoveListener(OnTrackableAdded);
                _hookedSettings.TrackableRemoved.RemoveListener(OnTrackableRemoved);
            }
        }

        // ------------------------------------------------------------------ menu d'origine
        private void FindOriginalControls()
        {
            foreach (var b in _canvas.GetComponentsInChildren<Button>(true))
            {
                if (b.transform.IsChildOf(transform)) continue;
                string key = (b.name + " " + LabelOf(b.transform)).ToLowerInvariant();
                if (originalRequestButton == null && (key.Contains("permission") || key.Contains("request")))
                    originalRequestButton = b;
                else if (originalTrackingButton == null && (key.Contains("track") || key.Contains("qr") || key.Contains("toggle")))
                    originalTrackingButton = b;
            }
            foreach (var t in _canvas.GetComponentsInChildren<Toggle>(true))
            {
                if (t.transform.IsChildOf(transform)) continue;
                string key = (t.name + " " + LabelOf(t.transform)).ToLowerInvariant();
                if (originalTrackingToggle == null && (key.Contains("track") || key.Contains("qr") || key.Contains("detect")))
                    originalTrackingToggle = t;
            }

            if (originalRequestButton == null)
                Debug.LogWarning("[BauprobeMenu] Bouton d'origine 'permissions' introuvable : permission demandée directement. Clic droit sur le composant > 'Lister les contrôles du menu d'origine'.", this);
            if (originalTrackingButton == null && originalTrackingToggle == null)
                Debug.LogWarning("[BauprobeMenu] Contrôle d'origine du suivi QR introuvable : assigne-le dans l'inspecteur.", this);
        }

        private void HideOriginalPanel()
        {
            foreach (var l in _canvas.GetComponents<LayoutGroup>()) l.enabled = false;
            foreach (var f in _canvas.GetComponents<ContentSizeFitter>()) f.enabled = false;
            foreach (var g in _canvas.GetComponents<Graphic>()) g.enabled = false;

            for (int i = 0; i < _canvas.transform.childCount; i++)
            {
                var child = _canvas.transform.GetChild(i);
                if (child == transform) continue;
                var group = child.GetComponent<CanvasGroup>();
                if (group == null) group = child.gameObject.AddComponent<CanvasGroup>();
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        [ContextMenu("Lister les contrôles du menu d'origine")]
        private void ListOriginalControls()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            foreach (var b in canvas.rootCanvas.GetComponentsInChildren<Button>(true))
                if (!b.transform.IsChildOf(transform)) Debug.Log($"[BauprobeMenu] Button : {PathOf(b.transform)} | '{LabelOf(b.transform)}'");
            foreach (var t in canvas.rootCanvas.GetComponentsInChildren<Toggle>(true))
                if (!t.transform.IsChildOf(transform)) Debug.Log($"[BauprobeMenu] Toggle : {PathOf(t.transform)} | '{LabelOf(t.transform)}'");
        }

        private void FitCanvasToMenu()
        {
            if (_container == null || _canvasRect == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
            _canvasRect.sizeDelta = _container.sizeDelta;
        }

        // ------------------------------------------------------------------ boutons (appelés par les onClick)
        public void RequestPermission()
        {
            if (originalRequestButton != null) originalRequestButton.onClick.Invoke();
            else
            {
    #if UNITY_ANDROID && !UNITY_EDITOR
                UnityEngine.Android.Permission.RequestUserPermission(scenePermission);
    #else
                Debug.Log("[BauprobeMenu] Demande de permission ignorée hors casque.");
    #endif
            }
        }

        public void ToggleTracking()
        {
            if (originalTrackingToggle != null) originalTrackingToggle.isOn = !originalTrackingToggle.isOn;
            else if (originalTrackingButton != null) { originalTrackingButton.onClick.Invoke(); _trackingOn = !_trackingOn; }
            else Debug.LogWarning("[BauprobeMenu] Aucun contrôle d'origine pour le suivi QR.", this);
            RefreshStatus();
        }

        public void ToggleImportList()
        {
            if (importList == null) return;
            importList.SetActive(!importList.activeSelf);
            StartCoroutine(RefitNextFrame());
        }

        private IEnumerator RefitNextFrame()
        {
            yield return null;
            FitCanvasToMenu();
        }

        /// <summary>Importe le décor n°index du catalogue (argument du onClick du bouton).</summary>
        public void SpawnIndex(int index)
        {
            if (spawner == null || spawner.Catalog == null)
            {
                Debug.LogWarning("[BauprobeMenu] Assigne 'Spawner' (un DecorSpawner avec un catalogue).", this);
                return;
            }

            var items = spawner.Catalog.Items;
            if (index < 0 || index >= items.Count)
            {
                Debug.LogWarning($"[BauprobeMenu] Pas de décor n°{index} dans le catalogue.", this);
                return;
            }

            var instance = spawner.SpawnInFrontOfUser(items[index]);
            if (instance != null) Debug.Log("[BauprobeMenu] Objet créé : " + instance.name);
        }

        /// <summary>Supprime tous les objets du plateau.</summary>
        public void DeleteAll()
        {
            if (spawner == null) return;
            int count = spawner.Instances.Count;
            spawner.Clear();
            Debug.Log("[BauprobeMenu] Supprimer tout : " + count + " objet(s) supprimé(s)");
        }

        // ------------------------------------------------------------------ état affiché
        private void RefreshStatus()
        {
            if (permissionStatus != null)
            {
    #if UNITY_ANDROID && !UNITY_EDITOR
                permissionStatus.text = UnityEngine.Android.Permission.HasUserAuthorizedPermission(scenePermission)
                    ? "Permission : accordée" : "Permission : non accordée";
    #else
                permissionStatus.text = "Permission : (hors casque)";
    #endif
            }

            bool known = originalTrackingToggle != null || originalTrackingButton != null;
            bool on = originalTrackingToggle != null ? originalTrackingToggle.isOn : _trackingOn;
            if (trackingLabel != null) trackingLabel.text = !known ? "Suivi QR" : (on ? "Suivi QR : actif" : "Suivi QR : inactif");
            if (trackingStatus != null) trackingStatus.text = !known ? "Contrôle d'origine introuvable" : (on ? "Détection en cours" : "Détection coupée");

            if (detectedStatus != null)
            {
                string text = "QR détectés : " + _tracked.Count;
                if (_tracked.Count > 0 && !string.IsNullOrEmpty(_lastPayload))
                    text += "  (" + (_lastPayload.Length <= 22 ? _lastPayload : _lastPayload.Substring(0, 22) + "...") + ")";
                detectedStatus.text = text;
            }
        }

        private IEnumerator HookMruk()
        {
            while (MRUK.Instance == null || MRUK.Instance.SceneSettings == null) yield return null;
            _hookedSettings = MRUK.Instance.SceneSettings;
            _hookedSettings.TrackableAdded.AddListener(OnTrackableAdded);
            _hookedSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
        }

        private void OnTrackableAdded(MRUKTrackable t)
        {
            if (t == null) return;
            _tracked.Add(t);
            if (!string.IsNullOrEmpty(t.MarkerPayloadString)) _lastPayload = t.MarkerPayloadString;
        }

        private void OnTrackableRemoved(MRUKTrackable t)
        {
            if (t != null) _tracked.Remove(t);
        }

        private static string LabelOf(Transform t)
        {
            var tmp = t.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) return tmp.text;
            var legacy = t.GetComponentInChildren<Text>(true);
            return legacy != null ? legacy.text : "";
        }

        private static string PathOf(Transform t)
        {
            string p = t.name;
            while (t.parent != null) { t = t.parent; p = t.name + "/" + p; }
            return p;
        }
    }
}

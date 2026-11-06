using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bauprobe.Manipulation
{
    /// <summary>
    /// Surbrillance de l'objet sélectionné : un voile bleu qui pulse doucement, posé par-dessus l'objet,
    /// pour voir tout de suite quel objet on a choisi avec la gâchette.
    ///
    /// L'objet sélectionné est celui du menu objet (ObjectMenu.Target) ou, pendant Déplacer / Scale / Rotation,
    /// celui qu'on manipule. Pas de surbrillance en EXTRUDE : la face visée / choisie est déjà colorée.
    /// Plus de surbrillance dès qu'on ferme le menu (croix) ou qu'on supprime l'objet.
    ///
    /// Créé par le code au lancement : rien à ajouter dans la scène ni dans un prefab.
    /// Les matériaux de l'objet ne sont pas modifiés (la couleur choisie et la sauvegarde ne changent pas) :
    /// les modèles 3D sont simplement redessinés une deuxième fois avec le matériau
    /// Manipulation/Resources/SelectionHighlight (sa couleur règle celle du voile).
    /// </summary>
    public class SelectionHighlight : MonoBehaviour
    {
        const string MaterialPath = "SelectionHighlight";     // dans un dossier Resources
        const float PulseSpeed = 0.8f;                         // pulsations par seconde
        const float MinAlpha = 0.2f, MaxAlpha = 0.45f;         // transparence du voile, du plus léger au plus fort

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly Color DefaultColor = new Color(0.2f, 0.8f, 1f, 0.35f); // même bleu que le laser au survol

        static SelectionHighlight s_instance;

        Material _material;
        Color _color;
        InteractableObject _target;
        readonly List<MeshFilter> _meshes = new List<MeshFilter>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateAtStartup()
        {
            if (s_instance != null) return;
            var go = new GameObject("SelectionHighlight");
            DontDestroyOnLoad(go);
            s_instance = go.AddComponent<SelectionHighlight>();
        }

        void Awake()
        {
            var asset = Resources.Load<Material>(MaterialPath);
            if (asset != null)
            {
                _material = new Material(asset); // copie : on peut changer sa transparence sans toucher à l'asset
            }
            else
            {
                Debug.LogWarning("[SelectionHighlight] Matériau Resources/SelectionHighlight introuvable : " +
                                 "matériau transparent créé par le code.", this);
                _material = CreateFallbackMaterial();
            }

            _color = _material != null && _material.HasProperty(BaseColorId) ? _material.GetColor(BaseColorId) : DefaultColor;
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
            if (s_instance == this) s_instance = null;
        }

        /// <summary>L'objet sélectionné en ce moment (null s'il n'y en a pas).</summary>
        static InteractableObject CurrentSelection()
        {
            var active = InteractableObject.Active;
            if (active != null) return active;
            var menu = ObjectMenu.Instance;
            return menu != null ? menu.Target : null;
        }

        void LateUpdate()
        {
            // LateUpdate : l'objet a déjà bougé pendant cette image, le voile est dessiné à sa place exacte.
            var target = CurrentSelection();
            if (target != _target)
            {
                _target = target;
                _meshes.Clear();
                if (_target != null) _target.GetComponentsInChildren(_meshes);
            }

            if (_target == null || _material == null) return;
            if (_target.CurrentMode == InteractableObject.Mode.Extrude) return;

            // Pulsation douce, pour bien distinguer l'objet sélectionné des autres.
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * PulseSpeed * 2f * Mathf.PI);
            var color = _color;
            color.a = Mathf.Lerp(MinAlpha, MaxAlpha, pulse);
            _material.SetColor(BaseColorId, color);

            foreach (var filter in _meshes)
            {
                if (filter == null || !filter.gameObject.activeInHierarchy) continue;
                var mesh = filter.sharedMesh;
                if (mesh == null) continue;
                var meshRenderer = filter.GetComponent<MeshRenderer>();
                if (meshRenderer == null || !meshRenderer.enabled) continue;

                var renderParams = new RenderParams(_material)
                {
                    layer = filter.gameObject.layer,
                    shadowCastingMode = ShadowCastingMode.Off,
                    receiveShadows = false,
                };
                Matrix4x4 matrix = filter.transform.localToWorldMatrix;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    Graphics.RenderMesh(renderParams, mesh, sub, matrix);
                }
            }
        }

        /// <summary>Au cas où l'asset manque : URP Unlit, transparent, sans écriture de profondeur.</summary>
        static Material CreateFallbackMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return null;

            var mat = new Material(shader) { name = "SelectionHighlight (code)" };
            mat.SetFloat("_Surface", 1f);   // Transparent
            mat.SetFloat("_Blend", 0f);     // Alpha
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Cull", (float)CullMode.Back);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetColor(BaseColorId, DefaultColor);
            return mat;
        }
    }
}

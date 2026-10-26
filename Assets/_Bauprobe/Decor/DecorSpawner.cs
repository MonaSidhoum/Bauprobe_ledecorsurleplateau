using System;
using System.Collections.Generic;
using Bauprobe.Core;
using UnityEngine;

namespace Bauprobe.Decor
{
    /// <summary>
    /// Fait apparaître et disparaître les objets de décor sur le plateau.
    /// Les objets sont toujours créés comme enfants du StageSpace.
    /// </summary>
    public class DecorSpawner : MonoBehaviour
    {
        [SerializeField] private StageSpace stage;
        [SerializeField] private DecorCatalog catalog;

        [Tooltip("Distance (en mètres) devant le metteur en scène où apparaît un nouvel objet.")]
        [SerializeField, Min(0f)] private float spawnDistance = 1.5f;

        private readonly List<DecorInstance> _instances = new List<DecorInstance>();

        public DecorCatalog Catalog => catalog;
        public IReadOnlyList<DecorInstance> Instances => _instances;

        public event Action<DecorInstance> Spawned;
        public event Action<DecorInstance> Removed;

        /// <summary>
        /// Fait apparaître l'objet devant la personne qui porte le casque, posé sur le plateau
        /// (le bas de l'objet touche le sol, quel que soit l'endroit de son pivot).
        /// </summary>
        public DecorInstance SpawnInFrontOfUser(DecorItemDefinition definition)
        {
            var head = Camera.main != null ? Camera.main.transform : transform;

            var forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();

            var position = head.position + forward * spawnDistance;
            position.y = stage.transform.position.y; // posé au niveau du plateau

            // L'objet est tourné vers la personne.
            var worldPose = new Pose(position, Quaternion.LookRotation(-forward, Vector3.up));
            var stagePose = stage.WorldToStage(worldPose);
            var instance = Spawn(definition, stagePose.position, stagePose.rotation, definition.Prefab.transform.localScale);
            if (instance != null)
            {
                PlaceOnFloor(instance.transform);
            }
            return instance;
        }

        /// <summary>Fait apparaître un objet à une position donnée dans le repère du plateau.</summary>
        public DecorInstance Spawn(DecorItemDefinition definition, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            if (definition == null || definition.Prefab == null)
            {
                Debug.LogWarning("Impossible de créer le décor : fiche ou prefab manquant.");
                return null;
            }

            var go = Instantiate(definition.Prefab, stage.transform);
            go.name = definition.DisplayName;
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = localScale;

            var instance = go.GetComponent<DecorInstance>();
            if (instance == null)
            {
                instance = go.AddComponent<DecorInstance>();
            }
            instance.Initialize(definition);
            instance.Destroyed += OnInstanceDestroyed;

            _instances.Add(instance);
            Spawned?.Invoke(instance);
            return instance;
        }

        public void Remove(DecorInstance instance)
        {
            if (instance == null || !_instances.Contains(instance))
            {
                return;
            }

            // La suite (retrait de la liste, événement Removed) se fait dans OnInstanceDestroyed.
            Destroy(instance.gameObject);
        }

        /// <summary>Retire tous les objets du plateau.</summary>
        public void Clear()
        {
            // On retire tout de suite de la liste : Destroy() n'agit qu'à la fin de la frame,
            // et un chargement d'implantation peut recréer des objets juste après.
            foreach (var instance in _instances)
            {
                if (instance == null)
                {
                    continue;
                }
                instance.Destroyed -= OnInstanceDestroyed;
                Removed?.Invoke(instance);
                Destroy(instance.gameObject);
            }
            _instances.Clear();
        }

        // Appelé quand un objet est détruit, que ce soit par le spawner ou ailleurs
        // (ex. bouton « Supprimer » du menu de l'objet).
        private void OnInstanceDestroyed(DecorInstance instance)
        {
            if (_instances.Remove(instance))
            {
                Removed?.Invoke(instance);
            }
        }

        // Remonte (ou descend) l'objet pour que le bas de ses modèles 3D touche le plateau.
        private void PlaceOnFloor(Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            t.position += Vector3.up * (stage.transform.position.y - bounds.min.y);
        }
    }
}

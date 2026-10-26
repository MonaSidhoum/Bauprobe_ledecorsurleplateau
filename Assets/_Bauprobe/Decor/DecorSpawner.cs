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

        /// <summary>Fait apparaître l'objet au sol, devant la personne qui porte le casque.</summary>
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
            return Spawn(definition, stagePose.position, stagePose.rotation, Vector3.one);
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

            _instances.Add(instance);
            Spawned?.Invoke(instance);
            return instance;
        }

        public void Remove(DecorInstance instance)
        {
            if (instance == null || !_instances.Remove(instance))
            {
                return;
            }

            Removed?.Invoke(instance);
            Destroy(instance.gameObject);
        }

        /// <summary>Retire tous les objets du plateau.</summary>
        public void Clear()
        {
            for (var i = _instances.Count - 1; i >= 0; i--)
            {
                Remove(_instances[i]);
            }
        }
    }
}

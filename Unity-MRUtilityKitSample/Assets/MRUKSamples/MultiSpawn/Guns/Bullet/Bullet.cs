// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using MRUtilityKitSample.NavMesh;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.MRUtilityKitSamples.FindMultiSpawn
{
    /// <summary>
    /// A bullet projectile that moves forward and handles collisions with targets and surfaces.
    /// </summary>
    [MetaCodeSample("MRUKSample-FindMultiSpawn")]
    public class Bullet : MonoBehaviour
    {
        /// <summary>
        /// The speed at which the bullet travels in units per second.
        /// </summary>
        public float Speed = 30f;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private LayerMask collisionLayers = ~0; // All layers by default
        [SerializeField] private GameObject vfxTargetHit;
        [SerializeField] private GameObject vfxSurfaceHit;
        [SerializeField] private GameObject missScoreLabel;
        private float _distanceTraveled;
        private TrailRenderer _trailRenderer;
        private MultiSpawnMenu _multiSpawnMenu;
        [Header("Audio")]
        /// <summary>
        /// The audio source used to play gun sound effects.
        /// </summary>
        [HideInInspector] public AudioSource GunAudioSource;
        [SerializeField] private AudioClip _wallHitSound;
        [SerializeField] private AudioClip[] _targetHitSound;
        [SerializeField] private float _volume = 1f;

        private void Start()
        {
            _trailRenderer = GetComponent<TrailRenderer>();

            // Add self-destruct component
            var destroyer = gameObject.AddComponent<DestroyAfterSeconds>();
            destroyer.Seconds = 3f; // Destroy after 3 seconds if no collision occurs
            _multiSpawnMenu = FindAnyObjectByType<MultiSpawnMenu>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Move the bullet forward
            var moveDistance = Speed * Time.deltaTime;
            var movement = transform.forward * moveDistance;

            // Check for collision before moving
            var hit = CheckForCollision(movement, out var hitInfo);

            if (hit)
            {
                // Handle collision
                HandleCollision(hitInfo);
                return; // Stop updating after collision
            }

            // Move forward if no collision
            transform.position += movement;
            _distanceTraveled += moveDistance;

            // Check if maximum distance has been reached
            if (_distanceTraveled >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private bool CheckForCollision(Vector3 movement, out RaycastHit hitInfo)
        {
            // Cast a ray in the direction of movement
            var hit = Physics.Raycast(transform.position, movement.normalized, out hitInfo, movement.magnitude);
            return hit;
        }

        private void HandleCollision(RaycastHit hitInfo)
        {
            // Position the bullet at the impact point
            transform.position = hitInfo.point;
            if (hitInfo.collider.gameObject.GetComponentInParent<Target>() && hitInfo.collider.gameObject ==
                hitInfo.collider.GetComponentInParent<Target>().Reticle.gameObject)
            {
                hitInfo.collider.gameObject.GetComponentInParent<Target>().GetDestroyedByBullet();
                Instantiate(vfxTargetHit, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                GunAudioSource.PlayOneShot(_targetHitSound[Random.Range(0, _targetHitSound.Length)]);
            }
            else
            {
                var instancedBulletMark = Instantiate(vfxSurfaceHit, hitInfo.point + hitInfo.normal * .01f,
                    Quaternion.LookRotation(hitInfo.normal));
                AudioSource.PlayClipAtPoint(_wallHitSound, hitInfo.point, _volume);
                if (hitInfo.transform.GetComponent<Button>())
                {
                    hitInfo.transform.GetComponent<Button>().onClick.Invoke();
                    instancedBulletMark.transform.parent = hitInfo.transform;
                }
                else
                {
                    if (_multiSpawnMenu && _multiSpawnMenu.isTimerRunning)
                    {
                        Instantiate(missScoreLabel, hitInfo.point + hitInfo.normal * .01f,
                            Quaternion.LookRotation(hitInfo.point - _multiSpawnMenu.Camera.transform.position),
                            _multiSpawnMenu.transform);
                        _multiSpawnMenu.Timer += .5f;
                    }
                }
            }

            // Stop the trail from growing
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
            }

            // Disable movement
            enabled = false;

            // Destroy after a short delay to allow the trail to be visible
            Destroy(gameObject, _trailRenderer != null ? _trailRenderer.time : 0.1f);
        }
    }
}

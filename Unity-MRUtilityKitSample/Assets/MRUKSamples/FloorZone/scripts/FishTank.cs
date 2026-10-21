// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Events;

namespace MRUtilityKitSample.FindFloorZone
{
    /// <summary>
    /// Manages the fish tank scaling and pond fitting based on room dimensions and spawn positions.
    /// Automatically scales to room size and fits to the largest available pond area.
    /// </summary>
    [MetaCodeSample("MRUK-FindFloorZone")]
    public class FishTank : MonoBehaviour
    {
        private const float ROOM_SCALING_DELAY = 0.3f;
        private const float POND_FITTING_DELAY = 1f;
        private const float ANIMATION_DURATION = 1f;
        private const int ANIMATION_FRAME_RATE = 60;
        [SerializeField] private FindSpawnPositions[] _findSpawnPositions;
        [SerializeField] private AnimationCurve _disappearingAnimationCurve;
        public UnityEvent OnPondSizeFit;

        private Vector3 _roomSize;
        private Vector3 _roomCenter;
        private Fish _fish;

        private Transform _thisTransform;

        private MRUKRoom _cachedRoom;

        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            _fish = FindAnyObjectByType<Fish>();
            _thisTransform = transform;
            _cachedRoom = MRUK.Instance.GetCurrentRoom();
        }

        private void OnDestroy()
        {
            // Cancel any running async operations
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        public async void ScaleFishTankToRoom()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            await ScaleFishTankToRoomAsync(_cancellationTokenSource.Token);
        }

        private async Task ScaleFishTankToRoomAsync(CancellationToken cancellationToken)
        {
            // Wait for initial delay
            await Task.Delay((int)(ROOM_SCALING_DELAY * 1000), cancellationToken);

            if (_cachedRoom == null)
            {
                return;
            }

            var roomBounds = _cachedRoom.GetRoomBounds();
            _roomCenter = new Vector3(roomBounds.center.x, roomBounds.min.y, roomBounds.center.z);
            _roomSize = new Vector3(roomBounds.size.x, 1, roomBounds.size.z);

            var startPosition = _thisTransform.position;
            var startScale = _thisTransform.localScale;

            await AnimateTransformAsync(startPosition, _roomCenter, startScale, _roomSize, ANIMATION_DURATION,
                cancellationToken);

            // Continue with pond management after animation completes
            if (!cancellationToken.IsCancellationRequested)
            {
                KeepOnlyBiggestPonds();
            }
        }

        public void StartSpawningPonds()
        {
            foreach (var spawner in _findSpawnPositions)
            {
                spawner.StartSpawn(MRUK.Instance.GetCurrentRoom());
            }
        }

        public void EnableObject(GameObject objectToEnable)
        {
            objectToEnable.SetActive(true);
        }

        public void KeepOnlyBiggestPonds()
        {
            if (_findSpawnPositions.Length == 0)
            {
                return;
            }

            var allCylinders = GetAllCylinders();
            if (allCylinders.Count == 0)
            {
                return;
            }

            var clusters = CreateClusters(allCylinders, 0.3f);
            var biggestCluster = GetBiggestCluster(clusters);

            if (biggestCluster.Count == 0)
            {
                return;
            }

            CalculateSwimBounds(biggestCluster);
            DestroyObjectsNotInCluster(allCylinders, biggestCluster);

            OnPondSizeFit.Invoke();
            FitFishTankToBounds(_fish.newSwimBounds);
        }

        private List<GameObject> GetAllCylinders()
        {
            var allCylinders = new List<GameObject>();

            foreach (var spawnPositions in _findSpawnPositions)
            {
                allCylinders.AddRange(spawnPositions.SpawnedObjects);
            }

            return allCylinders;
        }

        private List<List<GameObject>> CreateClusters(List<GameObject> cylinders, float overlapThreshold)
        {
            var clusters = new List<List<GameObject>>();
            var visited = new bool[cylinders.Count];

            for (var i = 0; i < cylinders.Count; i++)
            {
                if (visited[i])
                {
                    continue;
                }

                var cluster = new List<GameObject>();
                CreateClusterRecursive(cylinders, visited, i, cluster, overlapThreshold);

                if (cluster.Count > 0)
                {
                    clusters.Add(cluster);
                }
            }

            return clusters;
        }

        private void CreateClusterRecursive(List<GameObject> cylinders, bool[] visited, int currentIndex,
            List<GameObject> cluster, float overlapThreshold)
        {
            visited[currentIndex] = true;
            cluster.Add(cylinders[currentIndex]);

            var currentBounds = cylinders[currentIndex].GetComponentInChildren<MeshRenderer>().bounds;

            for (var i = 0; i < cylinders.Count; i++)
            {
                if (visited[i])
                {
                    continue;
                }

                var otherBounds = cylinders[i].GetComponentInChildren<MeshRenderer>().bounds;

                if (CalculateOverlapPercentage(currentBounds, otherBounds) >= overlapThreshold)
                {
                    CreateClusterRecursive(cylinders, visited, i, cluster, overlapThreshold);
                }
            }
        }

        private float CalculateOverlapPercentage(Bounds bounds1, Bounds bounds2)
        {
            var intersection = GetBoundsIntersection(bounds1, bounds2);

            if (intersection.size.x <= 0 || intersection.size.y <= 0 || intersection.size.z <= 0)
            {
                return 0f;
            }

            var intersectionVolume = intersection.size.x * intersection.size.y * intersection.size.z;
            var bounds1Volume = bounds1.size.x * bounds1.size.y * bounds1.size.z;
            var bounds2Volume = bounds2.size.x * bounds2.size.y * bounds2.size.z;

            var smallerVolume = Mathf.Min(bounds1Volume, bounds2Volume);

            return smallerVolume > 0 ? intersectionVolume / smallerVolume : 0f;
        }

        private Bounds GetBoundsIntersection(Bounds bounds1, Bounds bounds2)
        {
            var min1 = bounds1.min;
            var max1 = bounds1.max;
            var min2 = bounds2.min;
            var max2 = bounds2.max;

            var intersectionMin = Vector3.Max(min1, min2);
            var intersectionMax = Vector3.Min(max1, max2);

            var center = (intersectionMin + intersectionMax) * 0.5f;
            var size = Vector3.Max(Vector3.zero, intersectionMax - intersectionMin);

            return new Bounds(center, size);
        }

        private List<GameObject> GetBiggestCluster(List<List<GameObject>> clusters)
        {
            if (clusters.Count == 0)
            {
                return new List<GameObject>();
            }

            var biggestCluster = clusters[0];
            var biggestVolume = CalculateClusterVolume(biggestCluster);

            for (var i = 1; i < clusters.Count; i++)
            {
                var clusterVolume = CalculateClusterVolume(clusters[i]);
                if (clusterVolume > biggestVolume)
                {
                    biggestCluster = clusters[i];
                    biggestVolume = clusterVolume;
                }
            }

            return biggestCluster;
        }

        private float CalculateClusterVolume(List<GameObject> cluster)
        {
            if (cluster.Count == 0)
            {
                return 0f;
            }

            var combinedBounds = cluster[0].GetComponentInChildren<MeshRenderer>().bounds;

            for (var i = 1; i < cluster.Count; i++)
            {
                combinedBounds.Encapsulate(cluster[i].GetComponentInChildren<MeshRenderer>().bounds);
            }

            return combinedBounds.size.x * combinedBounds.size.y * combinedBounds.size.z;
        }

        private void CalculateSwimBounds(List<GameObject> biggestCluster)
        {
            _fish.newSwimBounds = biggestCluster[0].GetComponentInChildren<MeshRenderer>().bounds;

            foreach (var cylinder in biggestCluster)
            {
                _fish.newSwimBounds.Encapsulate(cylinder.GetComponentInChildren<MeshRenderer>().bounds);
            }

            _fish.CenterSwimRangeStart = _fish.newSwimBounds.center;
            _fish.CenterSwimRange = _fish.newSwimBounds.center;
            _fish.SwimRangeStart = Mathf.Min(_fish.newSwimBounds.size.x, _fish.newSwimBounds.size.z);
            _fish.SwimRange = Mathf.Min(_fish.newSwimBounds.size.x, _fish.newSwimBounds.size.z);
        }

        private void DestroyObjectsNotInCluster(List<GameObject> allCylinders, List<GameObject> biggestCluster)
        {
            foreach (var cylinder in allCylinders)
            {
                if (!biggestCluster.Contains(cylinder))
                {
                    Destroy(cylinder);
                }
            }
        }

        public async void FitFishTankToBounds(Bounds bounds)
        {
            try
            {
                await FitFishTankToBoundsAsync(bounds, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when component is destroyed
            }
        }

        private async Task FitFishTankToBoundsAsync(Bounds bounds, CancellationToken cancellationToken)
        {
            await Task.Delay((int)(POND_FITTING_DELAY * 1000), cancellationToken);

            var center = new Vector3(bounds.center.x, _cachedRoom.GetRoomBounds().min.y, bounds.center.z);
            var fishTankNewSize = new Vector3(bounds.size.x, 1, bounds.size.z);

            var startPosition = _thisTransform.position;
            var startScale = _thisTransform.localScale;

            await AnimateTransformAsync(startPosition, center, startScale, fishTankNewSize, ANIMATION_DURATION,
                cancellationToken);
        }

        private async Task AnimateTransformAsync(Vector3 startPosition, Vector3 targetPosition, Vector3 startScale,
            Vector3 targetScale, float duration, CancellationToken cancellationToken)
        {
            var elapsedTime = 0f;
            var frameDelayMs = 1000 / ANIMATION_FRAME_RATE;

            while (elapsedTime < duration && !cancellationToken.IsCancellationRequested)
            {
                var t = elapsedTime / duration;

                // Apply smooth animation curve
                t = Mathf.SmoothStep(0f, 1f, t);

                _thisTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                _thisTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

                elapsedTime += frameDelayMs / 1000f;
                await Task.Delay(frameDelayMs, cancellationToken);
            }

            // Ensure final values are set exactly
            if (!cancellationToken.IsCancellationRequested)
            {
                _thisTransform.position = targetPosition;
                _thisTransform.localScale = targetScale;
            }
        }
    }
}

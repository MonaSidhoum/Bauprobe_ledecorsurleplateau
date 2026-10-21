// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using Meta.XR.MRUtilityKit;
using NUnit.Framework;
using Meta.XR.MRUtilityKit.Tests;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Meta.XR.MRUtilityKitSamples.Tests
{
    public class TestsExamples : MRUKTestBase
    {
        private FindSpawnPositions _fsp;
        private GameObject _testPrefab;

        public override IEnumerator SetUp()
        {
            yield return base.SetUp();
            RoomTearDown = (roomName, room) => { _fsp.ClearSpawnedPrefabs(); };

            _fsp = new GameObject("findSpawnPositions", typeof(FindSpawnPositions)).GetComponent<FindSpawnPositions>();
            _fsp.SpawnOnStart = MRUK.RoomFilter.None;
            _testPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Packages/com.meta.xr.mrutilitykit/Tests/PlayMode/Prefabs/TestPrefab.prefab");
            _fsp.SpawnObject = _testPrefab;
        }

        public override IEnumerator TearDown()
        {
            if (_fsp != null)
            {
                _fsp.ClearSpawnedPrefabs();
                Object.DestroyImmediate(_fsp.gameObject);
                _fsp = null;
            }

            yield return base.TearDown();
        }

        [UnityTest]
        [Timeout(500000)]
        public IEnumerator SpawnAllObjects()
        {
            yield return RunTestOnAllScenes(ExecuteSpawnTest);
        }

        /// <summary>
        /// Example test that uses a custom MRUKTestsSettings created on the fly.
        /// Demonstrates how to run tests with specific settings without modifying the project's settings asset.
        /// </summary>
        [UnityTest]
        [Timeout(500000)]
        public IEnumerator SpawnAllObjectsWithCustomSettings()
        {
            // Create a temporary custom settings instance using the factory method (no asset file created)
            var customSettings = ScriptableObject.CreateInstance<MRUKTestsSettings>();

            // Further customize the settings for this specific test
            customSettings.SceneSettings.RoomIndex = 0; // Start with first room
            customSettings.SceneSettings.SeatWidth = 0.7f; // Custom seat width for this test
            customSettings.SceneSettings.LoadSceneOnStartup = true;
            customSettings.SceneSettings.DataSource = MRUK.SceneDataSource.Prefab;
            customSettings.SceneSettings.RoomPrefabs = new[]
            {
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Packages/com.meta.xr.mrutilitykit/Core/Rooms/Prefabs/Bedroom/Bedroom00.prefab")
            };

            customSettings.SceneSettings.SceneJsons = new[]
            {
                AssetDatabase.LoadAssetAtPath<TextAsset>(
                    "Packages/com.meta.xr.mrutilitykit/Core/Rooms/JSON/MeshOffice1")
            };
            Assert.AreEqual(customSettings.SceneSettings.RoomPrefabs.Length, 1);
            Debug.Log(
                $"Running test with custom settings: DataSource={customSettings.SceneSettings.DataSource}, SeatWidth={customSettings.SceneSettings.SeatWidth}");

            // Run the test with our custom settings - this will re-initialize MRUK
            yield return RunTestOnAllScenes(ExecuteSpawnTest, customSettings);

            // Cleanup the temporary settings instance
            Object.DestroyImmediate(customSettings);

            Debug.Log("SpawnAllObjectsWithCustomSettings completed successfully");
        }

        private IEnumerator ExecuteSpawnTest(MRUKRoom room)
        {
            Assert.IsNotNull(room, "Room should not be null");
            Assert.IsNotNull(_fsp, "FindSpawnPositions component should be initialized");
            Assert.IsNotNull(_testPrefab, "Test prefab should be loaded");

            _fsp.Labels = MRUKAnchor.SceneLabels.WALL_FACE;
            _fsp.SpawnLocations = FindSpawnPositions.SpawnLocation.VerticalSurfaces;
            _fsp.SpawnAmount = 15;

            _fsp.StartSpawn();

            yield return new WaitForSeconds(1f);
            Assert.AreEqual(_fsp.SpawnAmount, _fsp.SpawnedObjects.Count,
                $"Expected {_fsp.SpawnAmount} spawned objects, but got {_fsp.SpawnedObjects.Count} in room: {room.name}");

            _fsp.ClearSpawnedPrefabs();
            Assert.AreEqual(0, _fsp.SpawnedObjects.Count, "All spawned objects should be cleared");
        }
    }
}

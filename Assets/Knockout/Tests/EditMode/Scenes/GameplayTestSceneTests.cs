using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Cinemachine;

namespace Knockout.Tests.EditMode.Scenes
{
    /// <summary>
    /// Tests for GameplayTest scene structure and configuration.
    /// NOTE: Run Tools > Knockout > Generate GameplayTest Scene to create the scene.
    /// </summary>
    public class GameplayTestSceneTests
    {
        private const string ScenePath = "Assets/Knockout/Scenes/GameplayTest.unity";

        [Test]
        public void GameplayTestScene_Exists()
        {
            // Arrange & Act
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            // Assert
            Assert.IsNotNull(scene,
                "GameplayTest scene should exist. Run Tools > Knockout > Generate GameplayTest Scene if missing.");
        }

        [Test]
        public void GameplayTestScene_ContainsRequiredObjects()
        {
            // Arrange
            var originalScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Act & Assert
            Assert.IsNotNull(GameObject.Find("Ground"), "Scene should contain Ground plane");
            Assert.IsNotNull(GameObject.Find("PlayerSpawnPoint"), "Scene should contain PlayerSpawnPoint");
            Assert.IsNotNull(GameObject.Find("AISpawnPoint"), "Scene should contain AISpawnPoint");

            // Cleanup - restore original scene
            if (!string.IsNullOrEmpty(originalScene.path))
            {
                EditorSceneManager.OpenScene(originalScene.path, OpenSceneMode.Single);
            }
        }

        [Test]
        public void GameplayTestScene_SpawnPointsAreFacingEachOther()
        {
            // Arrange
            var originalScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            // Act
            GameObject playerSpawn = GameObject.Find("PlayerSpawnPoint");
            GameObject aiSpawn = GameObject.Find("AISpawnPoint");

            // Assert
            Assert.IsNotNull(playerSpawn, "PlayerSpawnPoint should exist");
            Assert.IsNotNull(aiSpawn, "AISpawnPoint should exist");

            // PlayerSpawnPoint at (-5, 0, 0) facing right (90 degrees)
            Assert.AreEqual(-5f, playerSpawn.transform.position.x, 0.1f, "PlayerSpawnPoint X position");
            Assert.AreEqual(90f, playerSpawn.transform.eulerAngles.y, 1f, "PlayerSpawnPoint rotation");

            // AISpawnPoint at (5, 0, 0) facing left (-90 degrees)
            Assert.AreEqual(5f, aiSpawn.transform.position.x, 0.1f, "AISpawnPoint X position");
            Assert.AreEqual(270f, aiSpawn.transform.eulerAngles.y, 1f, "AISpawnPoint rotation (270 = -90)");

            // Cleanup - restore original scene
            if (!string.IsNullOrEmpty(originalScene.path))
            {
                EditorSceneManager.OpenScene(originalScene.path, OpenSceneMode.Single);
            }
        }
    }
}

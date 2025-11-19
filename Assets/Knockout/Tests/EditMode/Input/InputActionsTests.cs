using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Knockout.Tests.EditMode.Input
{
    /// <summary>
    /// Tests for Input Actions asset configuration.
    /// NOTE: Run Tools > Knockout > Generate Input Actions to create the asset.
    /// Then manually generate the C# class in Inspector before running these tests.
    /// </summary>
    public class InputActionsTests
    {
        private const string InputActionsPath = "Assets/Knockout/Scripts/Input/KnockoutInputActions.inputactions";

        [Test]
        public void InputActions_Asset_Exists()
        {
            // Arrange & Act
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);

            // Assert
            Assert.IsNotNull(inputActions,
                "Input Actions asset should exist. Run Tools > Knockout > Generate Input Actions if missing.");
        }

        [Test]
        public void InputActions_HasGameplayActionMap()
        {
            // Arrange
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Assert.IsNotNull(inputActions, "Input Actions asset must exist");

            // Act
            var gameplayMap = inputActions.FindActionMap("Gameplay");

            // Assert
            Assert.IsNotNull(gameplayMap, "Gameplay action map should exist");
            Assert.IsNotNull(gameplayMap.FindAction("Movement"), "Movement action should exist");
            Assert.IsNotNull(gameplayMap.FindAction("Jab"), "Jab action should exist");
            Assert.IsNotNull(gameplayMap.FindAction("Hook"), "Hook action should exist");
            Assert.IsNotNull(gameplayMap.FindAction("Uppercut"), "Uppercut action should exist");
            Assert.IsNotNull(gameplayMap.FindAction("Block"), "Block action should exist");
        }

        [Test]
        public void InputActions_HasUIActionMap()
        {
            // Arrange
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            Assert.IsNotNull(inputActions, "Input Actions asset must exist");

            // Act
            var uiMap = inputActions.FindActionMap("UI");

            // Assert
            Assert.IsNotNull(uiMap, "UI action map should exist");
            Assert.IsNotNull(uiMap.FindAction("Pause"), "Pause action should exist");
        }
    }
}

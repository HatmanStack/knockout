using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Knockout.Tests.EditMode.Input
{
    /// <summary>
    /// Tests for Input Actions asset configuration.
    /// NOTE: These tests will fail until the Input Actions asset is created in Unity Editor.
    /// See Assets/Knockout/Scripts/Input/INPUT_SYSTEM_SETUP.md for setup instructions.
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
                "Input Actions asset should exist. See INPUT_SYSTEM_SETUP.md for creation instructions.");
        }

        [Test]
        [Ignore("Test will pass once Input Actions asset is created in Unity Editor")]
        public void InputActions_ContainsGameplayActionMap()
        {
            // This test validates the generated C# class exists
            // It's currently ignored because the asset must be created in Unity Editor first

            // TODO: Uncomment once KnockoutInputActions.cs is generated
            /*
            // Arrange
            var inputActions = new KnockoutInputActions();

            // Act
            var gameplayMap = inputActions.Gameplay;

            // Assert
            Assert.IsNotNull(gameplayMap, "Gameplay action map should exist");
            Assert.IsNotNull(gameplayMap.Movement, "Movement action should exist");
            Assert.IsNotNull(gameplayMap.Jab, "Jab action should exist");
            Assert.IsNotNull(gameplayMap.Hook, "Hook action should exist");
            Assert.IsNotNull(gameplayMap.Uppercut, "Uppercut action should exist");
            Assert.IsNotNull(gameplayMap.Block, "Block action should exist");
            */
        }

        [Test]
        [Ignore("Test will pass once Input Actions asset is created in Unity Editor")]
        public void InputActions_ContainsUIActionMap()
        {
            // This test validates the generated C# class exists
            // It's currently ignored because the asset must be created in Unity Editor first

            // TODO: Uncomment once KnockoutInputActions.cs is generated
            /*
            // Arrange
            var inputActions = new KnockoutInputActions();

            // Act
            var uiMap = inputActions.UI;

            // Assert
            Assert.IsNotNull(uiMap, "UI action map should exist");
            Assert.IsNotNull(uiMap.Pause, "Pause action should exist");
            */
        }
    }
}

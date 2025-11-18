using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// Play mode tests for CharacterInput component.
    /// NOTE: Full input tests require KnockoutInputActions asset to be generated.
    /// Basic tests verify component structure and enable/disable functionality.
    /// </summary>
    public class CharacterInputTests
    {
        private GameObject _testCharacter;
        private CharacterInput _input;

        [SetUp]
        public void SetUp()
        {
            _testCharacter = new GameObject("TestCharacter");
            _input = _testCharacter.AddComponent<CharacterInput>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testCharacter != null)
            {
                Object.Destroy(_testCharacter);
            }
        }

        [UnityTest]
        public IEnumerator CharacterInput_StartsWithInputEnabled()
        {
            yield return null; // Wait for Start()

            // Assert
            Assert.IsTrue(_input.IsInputEnabled);
        }

        [UnityTest]
        public IEnumerator CharacterInput_DisableInput_SetsInputEnabledFalse()
        {
            yield return null; // Wait for Start()

            // Act
            _input.DisableInput();

            // Assert
            Assert.IsFalse(_input.IsInputEnabled);
        }

        [UnityTest]
        public IEnumerator CharacterInput_EnableInput_SetsInputEnabledTrue()
        {
            yield return null; // Wait for Start()

            // Arrange
            _input.DisableInput();
            Assert.IsFalse(_input.IsInputEnabled);

            // Act
            _input.EnableInput();

            // Assert
            Assert.IsTrue(_input.IsInputEnabled);
        }

        [UnityTest]
        public IEnumerator CharacterInput_DisableInput_ClearsMovementInput()
        {
            yield return null; // Wait for Start()

            // Arrange
            bool movementCleared = false;
            Vector2 lastMovement = Vector2.one;

            _input.OnMoveInput += (movement) =>
            {
                lastMovement = movement;
                if (movement == Vector2.zero)
                {
                    movementCleared = true;
                }
            };

            // Act
            _input.DisableInput();

            // Assert
            Assert.IsTrue(movementCleared, "Movement input should be cleared to zero");
            Assert.AreEqual(Vector2.zero, lastMovement);
        }

        // NOTE: The following tests require Input Actions asset to be created
        // They are marked as Ignore until the asset is generated

        /*
        [UnityTest]
        [Ignore("Requires KnockoutInputActions asset")]
        public IEnumerator CharacterInput_JabPressed_RaisesEvent()
        {
            yield return null;

            bool jabPressed = false;
            _input.OnJabPressed += () => jabPressed = true;

            // Simulate jab input (requires Input System test helpers)
            // SimulateInput(_input.InputActions.Gameplay.Jab);

            yield return null;

            Assert.IsTrue(jabPressed);
        }

        [UnityTest]
        [Ignore("Requires KnockoutInputActions asset")]
        public IEnumerator CharacterInput_BlockPressed_RaisesEvent()
        {
            yield return null;

            bool blockPressed = false;
            _input.OnBlockPressed += () => blockPressed = true;

            // Simulate block input
            // SimulateInput(_input.InputActions.Gameplay.Block, started: true);

            yield return null;

            Assert.IsTrue(blockPressed);
        }

        [UnityTest]
        [Ignore("Requires KnockoutInputActions asset")]
        public IEnumerator CharacterInput_BlockReleased_RaisesEvent()
        {
            yield return null;

            bool blockReleased = false;
            _input.OnBlockReleased += () => blockReleased = true;

            // Simulate block release
            // SimulateInput(_input.InputActions.Gameplay.Block, canceled: true);

            yield return null;

            Assert.IsTrue(blockReleased);
        }

        [UnityTest]
        [Ignore("Requires KnockoutInputActions asset")]
        public IEnumerator CharacterInput_WhenDisabled_DoesNotRaiseEvents()
        {
            yield return null;

            _input.DisableInput();

            bool jabPressed = false;
            _input.OnJabPressed += () => jabPressed = true;

            // Simulate jab input
            // SimulateInput(_input.InputActions.Gameplay.Jab);

            yield return null;

            Assert.IsFalse(jabPressed, "Input events should not fire when disabled");
        }
        */
    }
}

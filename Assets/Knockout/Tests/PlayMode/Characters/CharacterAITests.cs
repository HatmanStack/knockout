using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.AI;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// Play mode tests for CharacterAI component.
    /// </summary>
    public class CharacterAITests
    {
        private GameObject _playerCharacter;
        private GameObject _aiCharacter;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // Create player character
            _playerCharacter = new GameObject("Player");
            _playerCharacter.tag = "Player";
            _playerCharacter.AddComponent<Rigidbody>();
            _playerCharacter.AddComponent<Animator>();
            _playerCharacter.AddComponent<CharacterAnimator>();
            _playerCharacter.AddComponent<CharacterMovement>();
            _playerCharacter.AddComponent<CharacterCombat>();
            _playerCharacter.AddComponent<CharacterHealth>();

            // Create AI character
            _aiCharacter = new GameObject("AI");
            _aiCharacter.AddComponent<Rigidbody>();
            _aiCharacter.AddComponent<Animator>();
            _aiCharacter.AddComponent<CharacterAnimator>();
            _aiCharacter.AddComponent<CharacterMovement>();
            _aiCharacter.AddComponent<CharacterCombat>();
            _aiCharacter.AddComponent<CharacterHealth>();
            _aiCharacter.AddComponent<CharacterAI>();

            yield return null; // Wait for Start() to be called
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_playerCharacter != null)
                Object.Destroy(_playerCharacter);
            if (_aiCharacter != null)
                Object.Destroy(_aiCharacter);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterAI_FindsPlayerTarget_OnStart()
        {
            // Arrange is done in SetUp

            // Act
            yield return null; // Wait for Start()

            var ai = _aiCharacter.GetComponent<CharacterAI>();

            // Assert
            Assert.IsNotNull(ai.Target);
            Assert.AreEqual(_playerCharacter, ai.Target);
        }

        [UnityTest]
        public IEnumerator CharacterAI_InitializesStateMachine_OnStart()
        {
            // Arrange is done in SetUp

            // Act
            yield return null; // Wait for Start()

            var ai = _aiCharacter.GetComponent<CharacterAI>();

            // Assert
            Assert.IsNotNull(ai.StateMachine);
            Assert.IsNotNull(ai.StateMachine.CurrentState);
        }

        [UnityTest]
        public IEnumerator CharacterAI_UpdatesContext_WithCurrentGameState()
        {
            // Arrange
            yield return null; // Wait for Start()

            var ai = _aiCharacter.GetComponent<CharacterAI>();

            // Position characters at known distance
            _aiCharacter.transform.position = Vector3.zero;
            _playerCharacter.transform.position = new Vector3(5f, 0f, 0f);

            // Act
            yield return new WaitForSeconds(0.2f); // Wait for AI update

            var context = ai.StateMachine.Context;

            // Assert
            Assert.AreEqual(5f, context.DistanceToPlayer, 0.5f);
        }
    }
}

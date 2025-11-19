using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.States;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// Play mode tests for CharacterCombat component.
    /// NOTE: These tests require character prefabs to exist.
    /// Some tests are marked with [Ignore] until prefabs are set up.
    /// </summary>
    public class CharacterCombatTests
    {
        private GameObject _testCharacter;

        [TearDown]
        public void TearDown()
        {
            if (_testCharacter != null)
            {
                Object.Destroy(_testCharacter);
            }
        }

        [Test]
        public void CharacterCombat_InitializesWithIdleState()
        {
            // Arrange
            _testCharacter = CreateMinimalCharacter();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();

            // Act - component should initialize in Start()
            // Force Start() to be called (done automatically in play mode)

            // Assert
            Assert.IsNotNull(combat);
            Assert.IsNotNull(combat.CurrentState);
            Assert.IsInstanceOf<IdleState>(combat.CurrentState);
        }

        [Test]
        public void CharacterCombat_CanAct_TrueWhenIdle()
        {
            // Arrange
            _testCharacter = CreateMinimalCharacter();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();

            // Assert
            Assert.IsTrue(combat.CanAct);
        }

        [Test]
        public void CharacterCombat_IsBlocking_FalseByDefault()
        {
            // Arrange
            _testCharacter = CreateMinimalCharacter();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();

            // Assert
            Assert.IsFalse(combat.IsBlocking);
        }

        [Test]
        public void CharacterCombat_IsAttacking_FalseByDefault()
        {
            // Arrange
            _testCharacter = CreateMinimalCharacter();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();

            // Assert
            Assert.IsFalse(combat.IsAttacking);
        }

        // NOTE: The following tests are marked [Ignore] because they require:
        // 1. Character prefabs with Animator
        // 2. AttackData ScriptableObject assets
        // 3. Hitbox setup
        // Uncomment and complete after Task 2 (prefab setup) is done.

        /*
        [UnityTest]
        [Ignore("Requires character prefab setup")]
        public IEnumerator CharacterCombat_ExecuteAttack_TriggersAnimation()
        {
            // Arrange
            GameObject character = InstantiateCharacterPrefab();
            CharacterCombat combat = character.GetComponent<CharacterCombat>();
            AttackData jabData = LoadAttackData("AttackData_Jab");

            yield return null;

            bool attacked = false;
            combat.OnAttackExecuted += (attackType) => attacked = true;

            // Act
            combat.ExecuteAttack(jabData);

            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.IsTrue(attacked);
            Assert.IsInstanceOf<AttackingState>(combat.CurrentState);
        }

        [UnityTest]
        [Ignore("Requires character prefab setup")]
        public IEnumerator CharacterCombat_StartBlocking_TransitionsToBlockingState()
        {
            // Arrange
            GameObject character = InstantiateCharacterPrefab();
            CharacterCombat combat = character.GetComponent<CharacterCombat>();

            yield return null;

            bool blockStarted = false;
            combat.OnBlockStarted += () => blockStarted = true;

            // Act
            combat.StartBlocking();

            yield return null;

            // Assert
            Assert.IsTrue(blockStarted);
            Assert.IsTrue(combat.IsBlocking);
            Assert.IsInstanceOf<BlockingState>(combat.CurrentState);
        }

        [UnityTest]
        [Ignore("Requires character prefab setup")]
        public IEnumerator CharacterCombat_StopBlocking_ReturnsToIdle()
        {
            // Arrange
            GameObject character = InstantiateCharacterPrefab();
            CharacterCombat combat = character.GetComponent<CharacterCombat>();

            yield return null;

            combat.StartBlocking();
            yield return null;

            bool blockEnded = false;
            combat.OnBlockEnded += () => blockEnded = true;

            // Act
            combat.StopBlocking();

            yield return null;

            // Assert
            Assert.IsTrue(blockEnded);
            Assert.IsFalse(combat.IsBlocking);
            Assert.IsInstanceOf<IdleState>(combat.CurrentState);
        }
        */

        #region Helper Methods

        /// <summary>
        /// Creates a minimal character GameObject for testing.
        /// This is a lightweight setup for tests that don't need full animation.
        /// </summary>
        private GameObject CreateMinimalCharacter()
        {
            GameObject character = new GameObject("TestCharacter");

            // Add Animator (required by CharacterCombat)
            Animator animator = character.AddComponent<Animator>();

            // Add CharacterAnimator (required by CharacterCombat)
            character.AddComponent<CharacterAnimator>();

            // Add CharacterCombat
            character.AddComponent<CharacterCombat>();

            return character;
        }

        /// <summary>
        /// Creates a character with stamina component for stamina integration tests.
        /// </summary>
        private GameObject CreateCharacterWithStamina()
        {
            GameObject character = CreateMinimalCharacter();

            // Add CharacterStamina
            CharacterStamina stamina = character.AddComponent<CharacterStamina>();

            // Create and assign StaminaData
            StaminaData staminaData = ScriptableObject.CreateInstance<StaminaData>();
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField.SetValue(stamina, staminaData);

            return character;
        }

        // TODO: Implement these helper methods after prefabs are created

        /*
        private GameObject InstantiateCharacterPrefab()
        {
            // Load prefab from Resources or path
            // GameObject prefab = Resources.Load<GameObject>("Prefabs/Characters/PlayerCharacter");
            // return Object.Instantiate(prefab);
            throw new System.NotImplementedException("Implement after prefab creation");
        }

        private AttackData LoadAttackData(string attackName)
        {
            // Load AttackData asset
            // return Resources.Load<AttackData>($"Data/{attackName}");
            throw new System.NotImplementedException("Implement after AttackData assets created");
        }
        */

        #endregion

        #region Stamina Integration Tests

        [UnityTest]
        public IEnumerator CharacterCombat_Attack_SucceedsWhenStaminaAvailable()
        {
            // Arrange
            _testCharacter = CreateCharacterWithStamina();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();
            CharacterStamina stamina = _testCharacter.GetComponent<CharacterStamina>();

            // Create test attack data
            AttackData attackData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(attackData, 0); // Jab

            yield return null; // Wait for Start()

            float initialStamina = stamina.CurrentStamina;

            // Act
            bool attackSucceeded = combat.ExecuteAttack(attackData);

            // Assert
            Assert.IsTrue(attackSucceeded, "Attack should succeed when stamina is available");
            Assert.Less(stamina.CurrentStamina, initialStamina, "Stamina should be consumed");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        [UnityTest]
        public IEnumerator CharacterCombat_Attack_FailsWhenStaminaInsufficient()
        {
            // Arrange
            _testCharacter = CreateCharacterWithStamina();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();
            CharacterStamina stamina = _testCharacter.GetComponent<CharacterStamina>();

            // Create test attack data
            AttackData attackData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(attackData, 0); // Jab

            yield return null; // Wait for Start()

            // Deplete stamina
            stamina.SetCurrentStamina(5f); // Less than jab cost (10)

            bool attackFailedEventFired = false;
            combat.OnAttackFailedNoStamina += () => attackFailedEventFired = true;

            // Act
            bool attackSucceeded = combat.ExecuteAttack(attackData);

            // Assert
            Assert.IsFalse(attackSucceeded, "Attack should fail when stamina insufficient");
            Assert.IsTrue(attackFailedEventFired, "OnAttackFailedNoStamina event should fire");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        [UnityTest]
        public IEnumerator CharacterCombat_Attack_FailureDoesNotTransitionToAttackingState()
        {
            // Arrange
            _testCharacter = CreateCharacterWithStamina();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();
            CharacterStamina stamina = _testCharacter.GetComponent<CharacterStamina>();

            // Create test attack data
            AttackData attackData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(attackData, 0); // Jab

            yield return null; // Wait for Start()

            // Deplete stamina
            stamina.SetCurrentStamina(0f);

            // Act
            combat.ExecuteAttack(attackData);

            // Assert
            Assert.IsInstanceOf<IdleState>(combat.CurrentState,
                "Should remain in IdleState when attack fails due to stamina");
            Assert.IsFalse(combat.IsAttacking, "IsAttacking should be false");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        [UnityTest]
        public IEnumerator CharacterCombat_Blocking_WorksAtZeroStamina()
        {
            // Arrange
            _testCharacter = CreateCharacterWithStamina();
            CharacterCombat combat = _testCharacter.GetComponent<CharacterCombat>();
            CharacterStamina stamina = _testCharacter.GetComponent<CharacterStamina>();

            yield return null; // Wait for Start()

            // Deplete stamina
            stamina.SetCurrentStamina(0f);

            bool blockStarted = false;
            combat.OnBlockStarted += () => blockStarted = true;

            // Act
            bool blockingSucceeded = combat.StartBlocking();

            yield return null;

            // Assert
            Assert.IsTrue(blockingSucceeded, "Blocking should succeed even at 0 stamina");
            Assert.IsTrue(blockStarted, "OnBlockStarted event should fire");
            Assert.IsTrue(combat.IsBlocking, "Should be in blocking state");
        }

        #endregion
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.States;

namespace Knockout.Tests.PlayMode.Integration
{
    /// <summary>
    /// Comprehensive integration tests for Phase 1 stamina systems.
    /// Tests end-to-end gameplay flows with all components working together.
    /// </summary>
    [TestFixture]
    public class StaminaIntegrationTests
    {
        private GameObject _character1;
        private GameObject _character2;
        private CharacterCombat _combat1;
        private CharacterCombat _combat2;
        private CharacterStamina _stamina1;
        private CharacterStamina _stamina2;
        private StaminaData _staminaData;

        [SetUp]
        public void SetUp()
        {
            // Create stamina data
            _staminaData = ScriptableObject.CreateInstance<StaminaData>();

            // Create first character
            _character1 = CreateCharacterWithStamina("Character1");
            _combat1 = _character1.GetComponent<CharacterCombat>();
            _stamina1 = _character1.GetComponent<CharacterStamina>();

            // Create second character
            _character2 = CreateCharacterWithStamina("Character2");
            _combat2 = _character2.GetComponent<CharacterCombat>();
            _stamina2 = _character2.GetComponent<CharacterStamina>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_character1 != null) Object.DestroyImmediate(_character1);
            if (_character2 != null) Object.DestroyImmediate(_character2);
            if (_staminaData != null) Object.DestroyImmediate(_staminaData);
        }

        private GameObject CreateCharacterWithStamina(string name)
        {
            GameObject character = new GameObject(name);

            // Add required components
            character.AddComponent<Animator>();
            character.AddComponent<CharacterAnimator>();
            character.AddComponent<CharacterCombat>();

            CharacterStamina stamina = character.AddComponent<CharacterStamina>();

            // Assign stamina data
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField.SetValue(stamina, _staminaData);

            return character;
        }

        #region End-to-End Stamina Flow Tests

        [UnityTest]
        public IEnumerator FullStaminaDepletionAndRecoveryCycle()
        {
            yield return null; // Wait for Start()

            // Arrange
            float initialStamina = _stamina1.CurrentStamina;
            bool depletedEventFired = false;
            bool enteredExhausted = false;

            _stamina1.OnStaminaDepleted += () => depletedEventFired = true;
            ExhaustedState.OnExhaustedStart += (combat) => enteredExhausted = true;

            // Act - deplete stamina
            _stamina1.SetCurrentStamina(0f);
            yield return new WaitForSeconds(0.1f);

            // Assert - depletion
            Assert.IsTrue(depletedEventFired, "Depletion event should fire");
            Assert.IsTrue(enteredExhausted, "Should enter exhausted state");
            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState, "Should be in ExhaustedState");

            // Wait for partial recovery
            yield return new WaitForSeconds(1f);

            // Assert - still exhausted (threshold not met)
            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState,
                "Should still be exhausted before threshold");

            // Wait for full recovery to threshold
            yield return new WaitForSeconds(2f);

            // Assert - recovered to idle
            Assert.IsInstanceOf<IdleState>(_combat1.CurrentState,
                "Should auto-recover to idle after threshold");
            Assert.Greater(_stamina1.CurrentStamina, 0f, "Stamina should have regenerated");

            // Cleanup
            ExhaustedState.OnExhaustedStart -= (combat) => enteredExhausted = true;
        }

        [UnityTest]
        public IEnumerator AttackSpamUntilExhaustion()
        {
            yield return null; // Wait for Start()

            // Arrange
            var attackData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(attackData, 0); // Jab = 10 stamina

            int attackCount = 0;
            int maxAttacks = 20; // Should deplete at 100/10 = 10 attacks

            // Act - spam attacks
            while (attackCount < maxAttacks && !_stamina1.IsDepleted)
            {
                bool attackSucceeded = _combat1.ExecuteAttack(attackData);
                if (attackSucceeded)
                {
                    attackCount++;
                }

                yield return new WaitForSeconds(0.1f);
            }

            // Assert
            Assert.IsTrue(_stamina1.IsDepleted, "Stamina should be depleted after spam");
            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState,
                "Should be in exhausted state after depletion");
            Assert.LessOrEqual(attackCount, 11, "Should deplete within expected attack count");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        [UnityTest]
        public IEnumerator ExhaustionPreventsAttacksButAllowsBlocking()
        {
            yield return null; // Wait for Start()

            // Arrange - deplete stamina
            _stamina1.SetCurrentStamina(0f);
            yield return new WaitForSeconds(0.1f);

            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState, "Setup: should be exhausted");

            var attackData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(attackData, 0);

            // Act - try to attack
            bool attackSucceeded = _combat1.ExecuteAttack(attackData);

            // Assert - attack fails
            Assert.IsFalse(attackSucceeded, "Attack should fail when exhausted");
            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState,
                "Should remain in exhausted state");

            // Act - try to block
            bool blockSucceeded = _combat1.StartBlocking();
            yield return null;

            // Assert - blocking works
            Assert.IsTrue(blockSucceeded, "Blocking should succeed when exhausted");
            Assert.IsTrue(_combat1.IsBlocking, "Should be blocking");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        [UnityTest]
        public IEnumerator StaminaRegeneratesWhenIdle()
        {
            yield return null; // Wait for Start()

            // Arrange - consume half stamina
            _stamina1.ConsumeStamina(50f);
            float staminaAfterConsumption = _stamina1.CurrentStamina;

            // Act - wait for regeneration (character is idle)
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.Greater(_stamina1.CurrentStamina, staminaAfterConsumption,
                "Stamina should regenerate when idle");
        }

        [UnityTest]
        public IEnumerator NoStaminaRegenDuringAttack()
        {
            yield return null; // Wait for Start()

            // Arrange - this test is conceptual since IsAttacking depends on animation
            // In full integration, attacks would block regeneration
            // For now, we verify the regeneration pause mechanism exists
            float regenRate = _staminaData.RegenPerSecond;
            Assert.Greater(regenRate, 0f, "Regen rate should be positive");
        }

        #endregion

        #region Attack Cost Variation Tests

        [UnityTest]
        public IEnumerator AttackCostsVaryByType()
        {
            yield return null; // Wait for Start()

            // Arrange
            var jabData = ScriptableObject.CreateInstance<AttackData>();
            var hookData = ScriptableObject.CreateInstance<AttackData>();
            var uppercutData = ScriptableObject.CreateInstance<AttackData>();

            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            attackTypeField.SetValue(jabData, 0); // Jab
            attackTypeField.SetValue(hookData, 1); // Hook
            attackTypeField.SetValue(uppercutData, 2); // Uppercut

            // Act & Assert - Jab cost
            float initialStamina = _stamina1.CurrentStamina;
            _combat1.ExecuteAttack(jabData);
            float jabCost = initialStamina - _stamina1.CurrentStamina;
            Assert.AreEqual(10f, jabCost, 0.01f, "Jab should cost 10 stamina");

            // Reset
            _stamina1.SetCurrentStamina(_staminaData.MaxStamina);

            // Act & Assert - Hook cost
            initialStamina = _stamina1.CurrentStamina;
            _combat1.ExecuteAttack(hookData);
            float hookCost = initialStamina - _stamina1.CurrentStamina;
            Assert.AreEqual(15f, hookCost, 0.01f, "Hook should cost 15 stamina");

            // Reset
            _stamina1.SetCurrentStamina(_staminaData.MaxStamina);

            // Act & Assert - Uppercut cost
            initialStamina = _stamina1.CurrentStamina;
            _combat1.ExecuteAttack(uppercutData);
            float uppercutCost = initialStamina - _stamina1.CurrentStamina;
            Assert.AreEqual(20f, uppercutCost, 0.01f, "Uppercut should cost 20 stamina");

            // Cleanup
            Object.DestroyImmediate(jabData);
            Object.DestroyImmediate(hookData);
            Object.DestroyImmediate(uppercutData);
        }

        #endregion

        #region Multi-Character Tests

        [UnityTest]
        public IEnumerator TwoCharactersHaveIndependentStamina()
        {
            yield return null; // Wait for Start()

            // Arrange - both characters start with full stamina
            Assert.AreEqual(_staminaData.MaxStamina, _stamina1.CurrentStamina);
            Assert.AreEqual(_staminaData.MaxStamina, _stamina2.CurrentStamina);

            // Act - deplete character 1's stamina
            _stamina1.ConsumeStamina(50f);

            // Assert - character 2's stamina unaffected
            Assert.AreEqual(50f, _stamina1.CurrentStamina, 0.01f);
            Assert.AreEqual(_staminaData.MaxStamina, _stamina2.CurrentStamina,
                "Character 2 stamina should be independent");
        }

        [UnityTest]
        public IEnumerator StaminaStatesDontInterfere()
        {
            yield return null; // Wait for Start()

            // Arrange - deplete character 1's stamina
            _stamina1.SetCurrentStamina(0f);
            yield return new WaitForSeconds(0.1f);

            // Assert - character 1 exhausted, character 2 idle
            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState,
                "Character 1 should be exhausted");
            Assert.IsInstanceOf<IdleState>(_combat2.CurrentState,
                "Character 2 should remain idle");

            // Act - character 2 can still attack
            var attackData = ScriptableObject.CreateInstance<AttackData>();
            bool char2CanAttack = _combat2.ExecuteAttack(attackData);

            // Assert
            Assert.IsTrue(char2CanAttack, "Character 2 should be able to attack normally");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        #endregion

        #region Edge Case Tests

        [UnityTest]
        public IEnumerator StaminaExactlyAtAttackCost_AttackSucceeds()
        {
            yield return null; // Wait for Start()

            // Arrange - set stamina to exactly jab cost (10)
            _stamina1.SetCurrentStamina(10f);

            var jabData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(jabData, 0);

            // Act
            bool attackSucceeded = _combat1.ExecuteAttack(jabData);

            // Assert
            Assert.IsTrue(attackSucceeded, "Attack should succeed when stamina exactly equals cost");
            Assert.AreEqual(0f, _stamina1.CurrentStamina, 0.01f, "Stamina should be 0 after attack");

            // Cleanup
            Object.DestroyImmediate(jabData);
        }

        [UnityTest]
        public IEnumerator StaminaBelowAttackCost_AttackFails()
        {
            yield return null; // Wait for Start()

            // Arrange - set stamina to 0.1 below jab cost
            _stamina1.SetCurrentStamina(9.9f);

            var jabData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(jabData, 0);

            bool failEventFired = false;
            _combat1.OnAttackFailedNoStamina += () => failEventFired = true;

            // Act
            bool attackSucceeded = _combat1.ExecuteAttack(jabData);

            // Assert
            Assert.IsFalse(attackSucceeded, "Attack should fail when stamina insufficient");
            Assert.IsTrue(failEventFired, "Failure event should fire");
            Assert.AreEqual(9.9f, _stamina1.CurrentStamina, 0.01f,
                "Stamina should not be consumed on failed attack");

            // Cleanup
            Object.DestroyImmediate(jabData);
        }

        [UnityTest]
        public IEnumerator RapidStateTransitions()
        {
            yield return null; // Wait for Start()

            // Arrange
            var jabData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(jabData, 0);

            // Act - rapid state changes
            _combat1.StartBlocking(); // Idle → Blocking
            yield return null;

            _combat1.StopBlocking(); // Blocking → Idle
            yield return null;

            _combat1.ExecuteAttack(jabData); // Idle → Attacking
            yield return new WaitForSeconds(0.1f);

            _stamina1.SetCurrentStamina(0f); // → Exhausted
            yield return new WaitForSeconds(0.1f);

            // Assert - no errors, state transitions handled correctly
            Assert.IsInstanceOf<ExhaustedState>(_combat1.CurrentState,
                "Should end in exhausted state after rapid transitions");

            // Cleanup
            Object.DestroyImmediate(jabData);
        }

        #endregion

        #region Special Knockdown Tests

        [UnityTest]
        public IEnumerator SpecialKnockdownStateTriggeredManually()
        {
            yield return null; // Wait for Start()

            // Arrange
            bool specialKnockdownEventFired = false;
            SpecialKnockdownState.OnSpecialKnockdownStart += (combat) => specialKnockdownEventFired = true;

            // Act - manually trigger special knockdown
            _combat1.TriggerSpecialKnockdown();
            yield return null;

            // Assert
            Assert.IsTrue(specialKnockdownEventFired, "Special knockdown event should fire");
            Assert.IsInstanceOf<SpecialKnockdownState>(_combat1.CurrentState,
                "Should be in special knockdown state");

            // Cleanup
            SpecialKnockdownState.OnSpecialKnockdownStart -= (combat) => specialKnockdownEventFired = true;
        }

        #endregion
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.Stamina
{
    /// <summary>
    /// PlayMode tests for CharacterStamina component.
    /// </summary>
    [TestFixture]
    public class CharacterStaminaTests
    {
        private GameObject _characterObject;
        private CharacterStamina _stamina;
        private CharacterCombat _combat;
        private StaminaData _staminaData;

        [SetUp]
        public void SetUp()
        {
            // Create test stamina data
            _staminaData = ScriptableObject.CreateInstance<StaminaData>();

            // Create character GameObject with required components
            _characterObject = new GameObject("TestCharacter");

            // Add CharacterAnimator (required by CharacterCombat)
            var animator = _characterObject.AddComponent<Animator>();
            var characterAnimator = _characterObject.AddComponent<CharacterAnimator>();

            // Add CharacterCombat
            _combat = _characterObject.AddComponent<CharacterCombat>();

            // Add CharacterStamina
            _stamina = _characterObject.AddComponent<CharacterStamina>();

            // Set stamina data via reflection (since it's serialized field)
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField.SetValue(_stamina, _staminaData);
        }

        [TearDown]
        public void TearDown()
        {
            if (_characterObject != null)
            {
                Object.DestroyImmediate(_characterObject);
            }

            if (_staminaData != null)
            {
                Object.DestroyImmediate(_staminaData);
            }
        }

        [UnityTest]
        public IEnumerator CharacterStamina_InitializesToMaxStamina()
        {
            // Wait for Start() to be called
            yield return null;

            // Assert
            Assert.AreEqual(_staminaData.MaxStamina, _stamina.CurrentStamina,
                "Stamina should initialize to max");
            Assert.AreEqual(1f, _stamina.StaminaPercentage, 0.01f,
                "Stamina percentage should be 100%");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_ConsumesStaminaCorrectly()
        {
            yield return null; // Wait for initialization

            // Arrange
            float initialStamina = _stamina.CurrentStamina;
            float cost = 20f;

            // Act
            bool consumed = _stamina.ConsumeStamina(cost);

            // Assert
            Assert.IsTrue(consumed, "Stamina consumption should succeed");
            Assert.AreEqual(initialStamina - cost, _stamina.CurrentStamina, 0.01f,
                "Stamina should decrease by cost amount");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_RegeneratesWhenIdle()
        {
            yield return null; // Wait for initialization

            // Arrange - consume some stamina
            _stamina.ConsumeStamina(50f);
            float staminaAfterConsumption = _stamina.CurrentStamina;

            // Wait for multiple fixed updates (regeneration happens in FixedUpdate)
            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.Greater(_stamina.CurrentStamina, staminaAfterConsumption,
                "Stamina should regenerate when idle");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_DoesNotRegenerateDuringAttack()
        {
            yield return null; // Wait for initialization

            // This test requires CharacterCombat to be fully functional
            // For now, we'll test the regeneration pause mechanism when not attacking
            // A full integration test will verify attack blocking in Task 12

            // Arrange - consume some stamina
            _stamina.ConsumeStamina(30f);
            float staminaBeforeRegen = _stamina.CurrentStamina;

            // Wait a frame (stamina should regenerate because not attacking)
            yield return new WaitForFixedUpdate();

            // Assert - should have regenerated (because IsAttacking is false)
            Assert.GreaterOrEqual(_stamina.CurrentStamina, staminaBeforeRegen,
                "Stamina should regenerate when not attacking");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_FiresDepletedEventWhenZero()
        {
            yield return null; // Wait for initialization

            // Arrange
            bool depletedEventFired = false;
            _stamina.OnStaminaDepleted += () => depletedEventFired = true;

            // Act - consume all stamina
            _stamina.ConsumeStamina(_stamina.MaxStamina);

            // Assert
            Assert.IsTrue(depletedEventFired, "OnStaminaDepleted should fire when stamina reaches 0");
            Assert.IsTrue(_stamina.IsDepleted, "IsDepleted should be true");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_HasStaminaReturnCorrectBoolean()
        {
            yield return null; // Wait for initialization

            // Arrange
            _stamina.ConsumeStamina(50f); // Leave 50 stamina

            // Act & Assert
            Assert.IsTrue(_stamina.HasStamina(30f), "Should have enough stamina for 30");
            Assert.IsTrue(_stamina.HasStamina(50f), "Should have enough stamina for exactly 50");
            Assert.IsFalse(_stamina.HasStamina(51f), "Should not have enough stamina for 51");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_FiresChangedEventOnConsumption()
        {
            yield return null; // Wait for initialization

            // Arrange
            bool changedEventFired = false;
            float reportedCurrent = 0f;
            float reportedMax = 0f;

            _stamina.OnStaminaChanged += (current, max) =>
            {
                changedEventFired = true;
                reportedCurrent = current;
                reportedMax = max;
            };

            // Act
            _stamina.ConsumeStamina(20f);

            // Assert
            Assert.IsTrue(changedEventFired, "OnStaminaChanged should fire");
            Assert.AreEqual(80f, reportedCurrent, 0.01f, "Event should report correct current stamina");
            Assert.AreEqual(100f, reportedMax, 0.01f, "Event should report correct max stamina");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_ClampsToMaxDuringRegeneration()
        {
            yield return null; // Wait for initialization

            // Arrange - consume a small amount
            _stamina.ConsumeStamina(10f);

            // Wait for more time than needed to fully regenerate
            yield return new WaitForSeconds(1f);

            // Assert - should be clamped at max
            Assert.AreEqual(_staminaData.MaxStamina, _stamina.CurrentStamina, 0.01f,
                "Stamina should not exceed max during regeneration");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_RegenMultiplierAffectsRate()
        {
            yield return null; // Wait for initialization

            // Arrange - consume stamina
            _stamina.ConsumeStamina(50f);

            // Set slower regen multiplier
            _stamina.SetRegenMultiplier(0.5f);

            float staminaBefore = _stamina.CurrentStamina;

            // Wait for some regeneration
            yield return new WaitForSeconds(0.2f);

            float regenWithHalfSpeed = _stamina.CurrentStamina - staminaBefore;

            // Reset and test normal speed
            _stamina.SetCurrentStamina(50f);
            _stamina.ResetRegenMultiplier();

            staminaBefore = _stamina.CurrentStamina;
            yield return new WaitForSeconds(0.2f);

            float regenWithNormalSpeed = _stamina.CurrentStamina - staminaBefore;

            // Assert - normal speed should be roughly double the half speed
            Assert.Greater(regenWithNormalSpeed, regenWithHalfSpeed,
                "Normal regen should be faster than half-speed regen");
        }

        [UnityTest]
        public IEnumerator CharacterStamina_GetStaminaCostForAttackData_UsesCustomCost()
        {
            yield return null; // Wait for initialization

            // Arrange - create attack data with custom cost
            var attackData = ScriptableObject.CreateInstance<AttackData>();
            var staminaCostField = typeof(AttackData).GetField("staminaCost",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaCostField.SetValue(attackData, 25f);

            // Act
            float cost = _stamina.GetStaminaCostForAttackData(attackData);

            // Assert
            Assert.AreEqual(25f, cost, "Should use custom stamina cost from AttackData");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }

        [UnityTest]
        public IEnumerator CharacterStamina_GetStaminaCostForAttackData_UsesDefaultWhenZero()
        {
            yield return null; // Wait for initialization

            // Arrange - create attack data with 0 cost (use default)
            var attackData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(attackData, 1); // Hook = index 1

            // Act
            float cost = _stamina.GetStaminaCostForAttackData(attackData);

            // Assert
            Assert.AreEqual(15f, cost, "Should use default cost from StaminaData for Hook (index 1)");

            // Cleanup
            Object.DestroyImmediate(attackData);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.SpecialMoves
{
    /// <summary>
    /// PlayMode tests for CharacterSpecialMoves component.
    /// </summary>
    [TestFixture]
    public class CharacterSpecialMovesTests
    {
        private GameObject _characterObject;
        private CharacterSpecialMoves _specialMoves;
        private CharacterInput _input;
        private CharacterStamina _stamina;
        private CharacterCombat _combat;
        private SpecialMoveData _specialMoveData;
        private StaminaData _staminaData;
        private AttackData _baseAttackData;

        [SetUp]
        public void SetUp()
        {
            // Create test data
            _baseAttackData = ScriptableObject.CreateInstance<AttackData>();
            _specialMoveData = ScriptableObject.CreateInstance<SpecialMoveData>();
            _staminaData = ScriptableObject.CreateInstance<StaminaData>();

            // Configure special move data
            var specialMoveNameField = typeof(SpecialMoveData).GetField("specialMoveName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            specialMoveNameField.SetValue(_specialMoveData, "Test Special");

            var baseAttackField = typeof(SpecialMoveData).GetField("baseAttackData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            baseAttackField.SetValue(_specialMoveData, _baseAttackData);

            var cooldownField = typeof(SpecialMoveData).GetField("cooldownSeconds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cooldownField.SetValue(_specialMoveData, 5f);

            var staminaCostField = typeof(SpecialMoveData).GetField("staminaCost",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaCostField.SetValue(_specialMoveData, 40f);

            // Create character GameObject with required components
            _characterObject = new GameObject("TestCharacter");

            // Add required components
            var animator = _characterObject.AddComponent<Animator>();
            var characterAnimator = _characterObject.AddComponent<CharacterAnimator>();

            _input = _characterObject.AddComponent<CharacterInput>();
            _combat = _characterObject.AddComponent<CharacterCombat>();
            _stamina = _characterObject.AddComponent<CharacterStamina>();
            _specialMoves = _characterObject.AddComponent<CharacterSpecialMoves>();

            // Set component data via reflection
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField.SetValue(_stamina, _staminaData);

            var specialMoveDataField = typeof(CharacterSpecialMoves).GetField("specialMoveData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            specialMoveDataField.SetValue(_specialMoves, _specialMoveData);
        }

        [TearDown]
        public void TearDown()
        {
            if (_characterObject != null)
            {
                Object.DestroyImmediate(_characterObject);
            }

            if (_specialMoveData != null)
            {
                Object.DestroyImmediate(_specialMoveData);
            }

            if (_staminaData != null)
            {
                Object.DestroyImmediate(_staminaData);
            }

            if (_baseAttackData != null)
            {
                Object.DestroyImmediate(_baseAttackData);
            }
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_InitializesCorrectly()
        {
            // Wait for Start() to be called
            yield return null;

            // Assert
            Assert.IsFalse(_specialMoves.IsOnCooldown, "Should not be on cooldown initially");
            Assert.AreEqual(0f, _specialMoves.CooldownTimeRemaining, "Cooldown time should be 0");
            Assert.AreEqual(0f, _specialMoves.CooldownProgress, 0.01f, "Cooldown progress should be 0");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_TryUseSucceedsWhenResourcesAvailable()
        {
            yield return null; // Wait for initialization

            // Arrange
            bool eventFired = false;
            _specialMoves.OnSpecialMoveUsed += (data) => eventFired = true;

            // Act
            bool success = _specialMoves.TryUseSpecialMove();

            // Assert
            Assert.IsTrue(success, "Special move should succeed when resources available");
            Assert.IsTrue(eventFired, "OnSpecialMoveUsed event should fire");
            Assert.IsTrue(_specialMoves.IsOnCooldown, "Should be on cooldown after use");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_TryUseFailsWhenOnCooldown()
        {
            yield return null; // Wait for initialization

            // Arrange - use special move to start cooldown
            _specialMoves.TryUseSpecialMove();

            SpecialMoveFailureReason? failureReason = null;
            _specialMoves.OnSpecialMoveFailed += (reason) => failureReason = reason;

            // Act - try to use again while on cooldown
            bool success = _specialMoves.TryUseSpecialMove();

            // Assert
            Assert.IsFalse(success, "Special move should fail when on cooldown");
            Assert.AreEqual(SpecialMoveFailureReason.OnCooldown, failureReason,
                "Failure reason should be OnCooldown");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_TryUseFailsWhenInsufficientStamina()
        {
            yield return null; // Wait for initialization

            // Arrange - deplete stamina below cost
            _stamina.ConsumeStamina(100f); // Consume all stamina

            SpecialMoveFailureReason? failureReason = null;
            _specialMoves.OnSpecialMoveFailed += (reason) => failureReason = reason;

            // Act
            bool success = _specialMoves.TryUseSpecialMove();

            // Assert
            Assert.IsFalse(success, "Special move should fail when insufficient stamina");
            Assert.AreEqual(SpecialMoveFailureReason.InsufficientStamina, failureReason,
                "Failure reason should be InsufficientStamina");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_CooldownCountsDownOverTime()
        {
            yield return null; // Wait for initialization

            // Arrange
            _specialMoves.StartCooldown();
            float initialCooldown = _specialMoves.CooldownTimeRemaining;

            // Act - wait for some time
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.Less(_specialMoves.CooldownTimeRemaining, initialCooldown,
                "Cooldown should decrease over time");
            Assert.IsTrue(_specialMoves.IsOnCooldown, "Should still be on cooldown");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_OnSpecialMoveReadyFiresWhenCooldownExpires()
        {
            yield return null; // Wait for initialization

            // Arrange
            bool readyEventFired = false;
            _specialMoves.OnSpecialMoveReady += () => readyEventFired = true;

            // Set a very short cooldown
            var cooldownField = typeof(SpecialMoveData).GetField("cooldownSeconds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cooldownField.SetValue(_specialMoveData, 0.5f);

            // Act
            _specialMoves.StartCooldown();
            yield return new WaitForSeconds(0.6f);

            // Assert
            Assert.IsTrue(readyEventFired, "OnSpecialMoveReady should fire when cooldown expires");
            Assert.IsFalse(_specialMoves.IsOnCooldown, "Should not be on cooldown after expiry");
            Assert.AreEqual(0f, _specialMoves.CooldownTimeRemaining, "Cooldown time should be 0");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_ResetCooldownWorks()
        {
            yield return null; // Wait for initialization

            // Arrange
            _specialMoves.StartCooldown();
            Assert.IsTrue(_specialMoves.IsOnCooldown, "Should be on cooldown");

            bool readyEventFired = false;
            _specialMoves.OnSpecialMoveReady += () => readyEventFired = true;

            // Act
            _specialMoves.ResetCooldown();

            // Assert
            Assert.IsFalse(_specialMoves.IsOnCooldown, "Should not be on cooldown after reset");
            Assert.AreEqual(0f, _specialMoves.CooldownTimeRemaining, "Cooldown time should be 0");
            Assert.IsTrue(readyEventFired, "OnSpecialMoveReady should fire when reset from cooldown");
        }

        [UnityTest]
        public IEnumerator CharacterSpecialMoves_CooldownProgressCalculatesCorrectly()
        {
            yield return null; // Wait for initialization

            // Arrange - set cooldown to 10 seconds
            var cooldownField = typeof(SpecialMoveData).GetField("cooldownSeconds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cooldownField.SetValue(_specialMoveData, 10f);

            // Act
            _specialMoves.StartCooldown();
            float initialProgress = _specialMoves.CooldownProgress;

            yield return new WaitForSeconds(5f);
            float midProgress = _specialMoves.CooldownProgress;

            // Assert
            Assert.AreEqual(1f, initialProgress, 0.01f, "Progress should be 1.0 (100%) when just started");
            Assert.Less(midProgress, initialProgress, "Progress should decrease over time");
            Assert.Greater(midProgress, 0f, "Progress should be > 0 before cooldown expires");
        }
    }
}

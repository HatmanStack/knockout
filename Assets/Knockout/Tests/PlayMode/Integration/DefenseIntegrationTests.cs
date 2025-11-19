using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat;
using Knockout.Combat.States;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Integration
{
    /// <summary>
    /// Comprehensive integration tests for defense systems (dodge + parry).
    /// Tests end-to-end scenarios with all systems working together.
    /// </summary>
    [TestFixture]
    public class DefenseIntegrationTests
    {
        private GameObject _defender;
        private GameObject _attacker;
        private CharacterHealth _defenderHealth;
        private CharacterDodge _defenderDodge;
        private CharacterParry _defenderParry;
        private CombatStateMachine _defenderStateMachine;
        private CharacterCombat _defenderCombat;
        private CharacterCombat _attackerCombat;
        private DodgeData _dodgeData;
        private ParryData _parryData;

        [SetUp]
        public void Setup()
        {
            // Create defender
            _defender = new GameObject("Defender");
            _defenderCombat = _defender.AddComponent<CharacterCombat>();
            _defenderHealth = _defender.AddComponent<CharacterHealth>();
            _defenderDodge = _defender.AddComponent<CharacterDodge>();
            _defenderParry = _defender.AddComponent<CharacterParry>();
            _defender.AddComponent<CharacterAnimator>();
            _defender.AddComponent<CharacterInput>();
            _defender.AddComponent<Rigidbody>();

            // Create attacker
            _attacker = new GameObject("Attacker");
            _attackerCombat = _attacker.AddComponent<CharacterCombat>();
            _attacker.AddComponent<CharacterAnimator>();

            // Create test data
            _dodgeData = ScriptableObject.CreateInstance<DodgeData>();
            _parryData = ScriptableObject.CreateInstance<ParryData>();

            var stats = ScriptableObject.CreateInstance<CharacterStats>();

            // Set up components
            SetPrivateField(_defenderHealth, "characterStats", stats);
            SetPrivateField(_defenderDodge, "dodgeData", _dodgeData);
            SetPrivateField(_defenderParry, "parryData", _parryData);

            // Initialize state machine
            _defenderStateMachine = new CombatStateMachine();
            _defenderStateMachine.Initialize(_defenderCombat, new IdleState());

            SetPrivateField(_defenderHealth, "_combatStateMachine", _defenderStateMachine);
            SetPrivateField(_defenderDodge, "combatStateMachine", _defenderStateMachine);
            SetPrivateField(_defenderParry, "combatStateMachine", _defenderStateMachine);
            SetPrivateField(_defenderHealth, "_characterParry", _defenderParry);

            // Initialize components
            _defenderDodge.Initialize();
            _defenderParry.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_defender);
            Object.Destroy(_attacker);
            Object.Destroy(_dodgeData);
            Object.Destroy(_parryData);
        }

        #region Integration Scenarios

        [UnityTest]
        public IEnumerator Integration_DodgeThroughAttack_WithIFrames()
        {
            // Arrange
            float initialHealth = _defenderHealth.CurrentHealth;

            // Act - trigger dodge
            _defenderDodge.TryDodge(DodgeDirection.Left);

            // Advance to i-frame window
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Hit during i-frames
            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            Assert.AreEqual(initialHealth, _defenderHealth.CurrentHealth,
                "Should take no damage when dodging through attack with i-frames");
        }

        [UnityTest]
        public IEnumerator Integration_DodgeRecovery_CanBeHit()
        {
            // Arrange
            float initialHealth = _defenderHealth.CurrentHealth;

            // Act - trigger dodge
            _defenderDodge.TryDodge(DodgeDirection.Right);

            // Advance past i-frame window
            for (int i = 0; i < 15; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Hit during recovery
            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            Assert.Less(_defenderHealth.CurrentHealth, initialHealth,
                "Should take damage during dodge recovery");
        }

        [UnityTest]
        public IEnumerator Integration_ParryNegatesDamageAndStaggrsAttacker()
        {
            // Arrange
            float initialHealth = _defenderHealth.CurrentHealth;

            // Create attacker state machine
            var attackerStateMachine = new CombatStateMachine();
            attackerStateMachine.Initialize(_attackerCombat, new IdleState());

            var attackerStateMachineField = _attackerCombat.GetType().GetField("_stateMachine",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackerStateMachineField.SetValue(_attackerCombat, attackerStateMachine);

            // Simulate block press
            SimulateBlockPress(_defenderParry);
            yield return new WaitForFixedUpdate();

            // Act - attack lands (within parry window)
            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            Assert.AreEqual(initialHealth, _defenderHealth.CurrentHealth,
                "Should take no damage when parry succeeds");
            Assert.IsInstanceOf<ParryStaggerState>(attackerStateMachine.CurrentState,
                "Attacker should be in ParryStaggerState");
        }

        [UnityTest]
        public IEnumerator Integration_ParryCounterWindow_AllowsCounterAttack()
        {
            // Arrange
            SimulateBlockPress(_defenderParry);
            yield return new WaitForFixedUpdate();

            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            // Act - check counter window
            yield return null;

            // Assert
            Assert.IsTrue(_defenderParry.IsInCounterWindow,
                "Counter window should be open after successful parry");
        }

        [UnityTest]
        public IEnumerator Integration_BlockVsParry_Differentiation()
        {
            // Arrange
            float initialHealth = _defenderHealth.CurrentHealth;

            // Transition to blocking state (hold block, not parry timing)
            _defenderStateMachine.ChangeState(new BlockingState());

            // Act - hit lands (normal block, not parry)
            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            float expectedHealth = initialHealth - (20f * 0.25f); // 75% reduction
            Assert.AreEqual(expectedHealth, _defenderHealth.CurrentHealth, 0.1f,
                "Normal block should reduce damage by 75%");
        }

        [UnityTest]
        public IEnumerator Integration_DodgeCooldown_PreventSpam()
        {
            // Arrange
            _defenderDodge.TryDodge(DodgeDirection.Left);

            // Wait for dodge to complete
            yield return new WaitForSeconds(0.5f);
            _defenderStateMachine.ChangeState(new IdleState());

            // Act - try to dodge again immediately
            bool result = _defenderDodge.TryDodge(DodgeDirection.Right);

            // Assert
            Assert.IsFalse(result, "Should not allow dodge spam (cooldown)");
        }

        [UnityTest]
        public IEnumerator Integration_ParryCooldown_PreventSpam()
        {
            // Arrange - successful parry
            SimulateBlockPress(_defenderParry);
            yield return new WaitForFixedUpdate();

            var hitData = CreateHitData(20f);
            _defenderParry.TryParry(hitData, _attackerCombat);

            // Act - try to parry again immediately
            SimulateBlockPress(_defenderParry);
            yield return new WaitForFixedUpdate();

            bool result = _defenderParry.TryParry(hitData, _attackerCombat);

            // Assert
            Assert.IsFalse(result, "Should not allow parry spam (cooldown)");
        }

        #endregion

        #region Edge Cases

        [UnityTest]
        public IEnumerator EdgeCase_DodgeWithZeroStamina_WorksCorrectly()
        {
            // Arrange - add stamina component and deplete it
            var stamina = _defender.AddComponent<CharacterStamina>();
            var staminaData = ScriptableObject.CreateInstance<StaminaData>();
            SetPrivateField(stamina, "staminaData", staminaData);
            stamina.Initialize();

            // Deplete stamina
            for (int i = 0; i < 10; i++)
            {
                stamina.ConsumeStamina(10f);
            }

            // Act - try to dodge (should work, dodge is stamina-free)
            bool result = _defenderDodge.TryDodge(DodgeDirection.Left);

            yield return null;

            // Assert
            Assert.IsTrue(result, "Dodge should work even with zero stamina (defensive action)");

            Object.Destroy(staminaData);
        }

        [UnityTest]
        public IEnumerator EdgeCase_ParryWhileExhausted_WorksCorrectly()
        {
            // Arrange - transition to exhausted state
            _defenderStateMachine.ChangeState(new ExhaustedState());

            SimulateBlockPress(_defenderParry);
            yield return new WaitForFixedUpdate();

            // Act - try to parry
            var hitData = CreateHitData(20f);
            bool result = _defenderParry.TryParry(hitData, _attackerCombat);

            // Assert
            Assert.IsTrue(result, "Parry should work while exhausted (defensive action)");
        }

        [UnityTest]
        public IEnumerator EdgeCase_DodgeDuringHitRecovery_Blocked()
        {
            // Arrange - transition to hit stunned state
            _defenderStateMachine.ChangeState(new HitStunnedState());

            yield return null;

            // Act - try to dodge
            bool result = _defenderDodge.TryDodge(DodgeDirection.Left);

            // Assert
            Assert.IsFalse(result, "Should not allow dodge during hit recovery");
        }

        [UnityTest]
        public IEnumerator EdgeCase_DefensePriority_DodgeBeforeParryBeforeBlock()
        {
            // Arrange - set up scenario where dodge i-frames are active
            _defenderDodge.TryDodge(DodgeDirection.Left);

            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Also simulate parry timing (should be ignored, dodge has priority)
            SimulateBlockPress(_defenderParry);

            float initialHealth = _defenderHealth.CurrentHealth;

            // Act - hit lands
            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            Assert.AreEqual(initialHealth, _defenderHealth.CurrentHealth,
                "Dodge i-frames should take priority over parry");
        }

        [UnityTest]
        public IEnumerator EdgeCase_MultipleHitsDuringDodge_FirstIgnoredRestConnect()
        {
            // Arrange
            float initialHealth = _defenderHealth.CurrentHealth;

            _defenderDodge.TryDodge(DodgeDirection.Left);

            // Hit during i-frames
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            var hitData1 = CreateHitData(10f);
            _defenderHealth.TakeDamage(hitData1);

            // Advance to recovery
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            // Hit during recovery
            var hitData2 = CreateHitData(10f);
            _defenderHealth.TakeDamage(hitData2);

            yield return null;

            // Assert
            Assert.AreEqual(initialHealth - 10f, _defenderHealth.CurrentHealth,
                "First hit ignored (i-frames), second hit connects (recovery)");
        }

        [Test]
        public void EdgeCase_DodgeDirection_CorrectMovementVector()
        {
            // Arrange & Act - test each dodge direction
            _defenderDodge.TryDodge(DodgeDirection.Left);
            var leftState = _defenderStateMachine.CurrentState as DodgingState;

            // Assert
            Assert.IsNotNull(leftState);
            Assert.AreEqual(DodgeDirection.Left, leftState.CurrentDirection);
        }

        [UnityTest]
        public IEnumerator EdgeCase_ParryTimingTooEarly_NormalBlock()
        {
            // Arrange - simulate block press too early
            SimulateBlockPress(_defenderParry);

            // Wait past parry window
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float initialHealth = _defenderHealth.CurrentHealth;

            // Transition to blocking (normal block)
            _defenderStateMachine.ChangeState(new BlockingState());

            // Act - hit lands
            var hitData = CreateHitData(20f);
            _defenderHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            float expectedHealth = initialHealth - (20f * 0.25f); // Normal block reduction
            Assert.AreEqual(expectedHealth, _defenderHealth.CurrentHealth, 0.1f,
                "Late block should result in normal block, not parry");
        }

        [UnityTest]
        public IEnumerator EdgeCase_CounterWindowExpiry_ClosesAfterDuration()
        {
            // Arrange - successful parry
            SimulateBlockPress(_defenderParry);
            yield return new WaitForFixedUpdate();

            var hitData = CreateHitData(20f);
            _defenderParry.TryParry(hitData, _attackerCombat);

            // Act - wait for counter window to expire
            yield return new WaitForSeconds(0.6f);

            // Assert
            Assert.IsFalse(_defenderParry.IsInCounterWindow,
                "Counter window should close after duration");
        }

        #endregion

        #region Helper Methods

        private HitData CreateHitData(float damage)
        {
            return new HitData(
                attacker: _attacker,
                damage: damage,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "TestAttack"
            );
        }

        private void SimulateBlockPress(CharacterParry parry)
        {
            var method = typeof(CharacterParry).GetMethod("OnBlockPressedInput",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(parry, null);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        #endregion
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Integration
{
    /// <summary>
    /// Comprehensive integration tests for the combo system.
    /// Tests natural chains, predefined sequences, damage scaling, interruption, and round resets.
    /// </summary>
    public class ComboIntegrationTests
    {
        private GameObject _attacker;
        private GameObject _defender;
        private CharacterComboTracker _attackerComboTracker;
        private CharacterCombat _attackerCombat;
        private CharacterCombat _defenderCombat;
        private ComboChainData _comboChainData;
        private ComboSequenceData _jabJabHookSequence;

        [SetUp]
        public void SetUp()
        {
            // Create attacker
            _attacker = new GameObject("Attacker");
            _attackerCombat = _attacker.AddComponent<CharacterCombat>();
            _attackerComboTracker = _attacker.AddComponent<CharacterComboTracker>();

            // Create defender
            _defender = new GameObject("Defender");
            _defenderCombat = _defender.AddComponent<CharacterCombat>();

            // Create combo chain data
            _comboChainData = ScriptableObject.CreateInstance<ComboChainData>();

            // Create test sequence (Jab-Jab-Hook)
            _jabJabHookSequence = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Assign using reflection
            var comboChainDataField = typeof(CharacterComboTracker).GetField("comboChainData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            comboChainDataField?.SetValue(_attackerComboTracker, _comboChainData);

            var comboSequencesField = typeof(CharacterComboTracker).GetField("comboSequences",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            comboSequencesField?.SetValue(_attackerComboTracker, new ComboSequenceData[] { _jabJabHookSequence });

            // Initialize combo tracker
            _attackerComboTracker.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (_attacker != null) Object.DestroyImmediate(_attacker);
            if (_defender != null) Object.DestroyImmediate(_defender);
            if (_comboChainData != null) Object.DestroyImmediate(_comboChainData);
            if (_jabJabHookSequence != null) Object.DestroyImmediate(_jabJabHookSequence);
        }

        [UnityTest]
        public IEnumerator Integration_NaturalChains_AllAttackTypes()
        {
            // Test: Jab-Jab combo
            _attackerComboTracker.RegisterHitLanded(0, 10f); // Jab
            yield return new WaitForFixedUpdate();
            _attackerComboTracker.RegisterHitLanded(0, 10f); // Jab
            Assert.AreEqual(2, _attackerComboTracker.ComboCount, "Jab-Jab chain should work");

            // Reset
            _attackerComboTracker.ResetCombo();
            yield return new WaitForFixedUpdate();

            // Test: Hook-Uppercut combo
            _attackerComboTracker.RegisterHitLanded(1, 15f); // Hook
            yield return new WaitForFixedUpdate();
            _attackerComboTracker.RegisterHitLanded(2, 20f); // Uppercut
            Assert.AreEqual(2, _attackerComboTracker.ComboCount, "Hook-Uppercut chain should work");

            // Reset
            _attackerComboTracker.ResetCombo();
            yield return new WaitForFixedUpdate();

            // Test: Mixed attack types
            _attackerComboTracker.RegisterHitLanded(0, 10f); // Jab
            yield return new WaitForFixedUpdate();
            _attackerComboTracker.RegisterHitLanded(1, 15f); // Hook
            yield return new WaitForFixedUpdate();
            _attackerComboTracker.RegisterHitLanded(2, 20f); // Uppercut
            Assert.AreEqual(3, _attackerComboTracker.ComboCount, "Mixed attack chain should work");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_DamageScaling_CorrectMultipliers()
        {
            // Act - Land 4 hits and check each multiplier
            float mult1 = _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(1.0f, mult1, 0.001f, "1st hit: 100% damage");

            yield return new WaitForFixedUpdate();

            float mult2 = _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(0.75f, mult2, 0.001f, "2nd hit: 75% damage");

            yield return new WaitForFixedUpdate();

            float mult3 = _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(0.5f, mult3, 0.001f, "3rd hit: 50% damage");

            yield return new WaitForFixedUpdate();

            float mult4 = _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(0.5f, mult4, 0.001f, "4th hit: 50% damage (floor)");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_ComboTimeout_ResetsAfterInactivity()
        {
            // Start combo
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(1, _attackerComboTracker.ComboCount);

            // Wait for global timeout (90 frames at 60fps = 1.5 seconds)
            // Add extra time to be safe
            yield return new WaitForSeconds(2f);

            // Combo should have timed out
            Assert.AreEqual(0, _attackerComboTracker.ComboCount, "Combo should timeout after inactivity");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_ComboInterruption_GettingHit()
        {
            // Start combo
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(2, _attackerComboTracker.ComboCount);

            bool comboBroken = false;
            _attackerComboTracker.OnComboBroken += (count) => comboBroken = true;

            // Attacker gets hit (call HandleHitTaken via reflection)
            var hitData = new HitData(
                attacker: _defender,
                damage: 10f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Jab"
            );

            var method = typeof(CharacterComboTracker).GetMethod("HandleHitTaken",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_attackerComboTracker, new object[] { hitData });

            // Assert
            Assert.IsTrue(comboBroken, "Getting hit should break combo");
            Assert.AreEqual(0, _attackerComboTracker.ComboCount, "Combo count should be 0");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_ComboReset_ClearsState()
        {
            // Start combo
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            _attackerComboTracker.RegisterHitLanded(1, 15f);
            Assert.AreEqual(3, _attackerComboTracker.ComboCount);

            // Reset
            _attackerComboTracker.ResetCombo();

            // Assert
            Assert.AreEqual(0, _attackerComboTracker.ComboCount, "Combo count should be 0");
            Assert.IsFalse(_attackerComboTracker.IsInCombo, "Should not be in combo");
            Assert.AreEqual(0, _attackerComboTracker.CurrentSequence.Count, "Sequence history should be empty");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_Events_FireCorrectly()
        {
            // Track events
            bool comboStarted = false;
            int hitCount = 0;
            bool comboEnded = false;

            _attackerComboTracker.OnComboStarted += () => comboStarted = true;
            _attackerComboTracker.OnComboHitLanded += (hitNum, damage) => hitCount++;
            _attackerComboTracker.OnComboEnded += (count, damage) => comboEnded = true;

            // Start combo
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.IsTrue(comboStarted, "OnComboStarted should fire");
            Assert.AreEqual(1, hitCount, "OnComboHitLanded should fire");

            yield return new WaitForFixedUpdate();

            _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(2, hitCount, "Second hit event should fire");

            // Reset combo
            _attackerComboTracker.ResetCombo();
            Assert.IsTrue(comboEnded, "OnComboEnded should fire on reset");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_SequenceHistory_TrackedCorrectly()
        {
            // Land attacks and check sequence
            _attackerComboTracker.RegisterHitLanded(0, 10f); // Jab
            Assert.AreEqual(1, _attackerComboTracker.CurrentSequence.Count);
            Assert.AreEqual(0, _attackerComboTracker.CurrentSequence[0]);

            yield return new WaitForFixedUpdate();

            _attackerComboTracker.RegisterHitLanded(1, 15f); // Hook
            Assert.AreEqual(2, _attackerComboTracker.CurrentSequence.Count);
            Assert.AreEqual(1, _attackerComboTracker.CurrentSequence[1]);

            yield return new WaitForFixedUpdate();

            _attackerComboTracker.RegisterHitLanded(2, 20f); // Uppercut
            Assert.AreEqual(3, _attackerComboTracker.CurrentSequence.Count);
            Assert.AreEqual(2, _attackerComboTracker.CurrentSequence[2]);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Integration_BreakCombo_PublicMethod()
        {
            // Start combo
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            _attackerComboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(2, _attackerComboTracker.ComboCount);

            bool comboBroken = false;
            _attackerComboTracker.OnComboBroken += (count) => comboBroken = true;

            // Break combo via public method
            _attackerComboTracker.BreakCombo();

            // Assert
            Assert.IsTrue(comboBroken, "OnComboBroken should fire");
            Assert.AreEqual(0, _attackerComboTracker.ComboCount, "Combo should be broken");

            yield return null;
        }

        [Test]
        public void Integration_CounterWindow_TrackedCorrectly()
        {
            // Initial state
            Assert.IsFalse(_attackerComboTracker.IsInCounterWindow, "Should not be in counter window initially");

            // Open counter window (call HandleCounterWindowOpened via reflection)
            var openMethod = typeof(CharacterComboTracker).GetMethod("HandleCounterWindowOpened",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            openMethod?.Invoke(_attackerComboTracker, null);

            // Assert
            Assert.IsTrue(_attackerComboTracker.IsInCounterWindow, "Should be in counter window");

            // Close counter window
            var closeMethod = typeof(CharacterComboTracker).GetMethod("HandleCounterWindowClosed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            closeMethod?.Invoke(_attackerComboTracker, null);

            // Assert
            Assert.IsFalse(_attackerComboTracker.IsInCounterWindow, "Counter window should be closed");
        }
    }
}

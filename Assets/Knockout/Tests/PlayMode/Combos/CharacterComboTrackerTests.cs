using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Combos
{
    /// <summary>
    /// PlayMode tests for CharacterComboTracker component.
    /// Tests combo tracking, damage scaling, and sequence detection.
    /// </summary>
    public class CharacterComboTrackerTests
    {
        private GameObject _characterObject;
        private CharacterComboTracker _comboTracker;
        private CharacterCombat _characterCombat;
        private CharacterHealth _characterHealth;
        private ComboChainData _comboChainData;
        private ComboSequenceData _testSequence;

        [SetUp]
        public void SetUp()
        {
            // Create character GameObject
            _characterObject = new GameObject("TestCharacter");

            // Add required components
            _characterCombat = _characterObject.AddComponent<CharacterCombat>();
            _characterHealth = _characterObject.AddComponent<CharacterHealth>();
            _comboTracker = _characterObject.AddComponent<CharacterComboTracker>();

            // Create test combo chain data
            _comboChainData = ScriptableObject.CreateInstance<ComboChainData>();

            // Create test sequence (Jab-Jab-Hook)
            _testSequence = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Assign data using reflection (since fields are private)
            var comboChainDataField = typeof(CharacterComboTracker).GetField("comboChainData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            comboChainDataField?.SetValue(_comboTracker, _comboChainData);

            var comboSequencesField = typeof(CharacterComboTracker).GetField("comboSequences",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            comboSequencesField?.SetValue(_comboTracker, new ComboSequenceData[] { _testSequence });

            // Initialize combo tracker
            _comboTracker.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (_characterObject != null)
            {
                Object.DestroyImmediate(_characterObject);
            }

            if (_comboChainData != null)
            {
                Object.DestroyImmediate(_comboChainData);
            }

            if (_testSequence != null)
            {
                Object.DestroyImmediate(_testSequence);
            }
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_LandingFirstHit_StartsCombo()
        {
            // Arrange
            bool comboStarted = false;
            _comboTracker.OnComboStarted += () => comboStarted = true;

            // Act
            _comboTracker.RegisterHitLanded(0, 10f); // Jab

            // Assert
            Assert.IsTrue(comboStarted, "OnComboStarted should fire on first hit");
            Assert.AreEqual(1, _comboTracker.ComboCount, "Combo count should be 1");
            Assert.IsTrue(_comboTracker.IsInCombo, "Should be in combo");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_LandingConsecutiveHits_IncrementsCombo()
        {
            // Arrange
            int hitCount = 0;
            _comboTracker.OnComboHitLanded += (hitNum, damage) => hitCount++;

            // Act - Land 3 hits quickly (within chain window)
            _comboTracker.RegisterHitLanded(0, 10f); // Jab
            yield return new WaitForFixedUpdate(); // Wait 1 frame
            _comboTracker.RegisterHitLanded(0, 10f); // Jab
            yield return new WaitForFixedUpdate();
            _comboTracker.RegisterHitLanded(1, 15f); // Hook

            // Assert
            Assert.AreEqual(3, _comboTracker.ComboCount, "Combo count should be 3");
            Assert.AreEqual(3, hitCount, "Should have fired 3 hit events");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_FirstHit_DealFullDamage()
        {
            // Act
            float multiplier = _comboTracker.RegisterHitLanded(0, 10f);

            // Assert
            Assert.AreEqual(1.0f, multiplier, 0.001f, "First hit should have 100% damage (1.0x multiplier)");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_SecondHit_Deals75PercentDamage()
        {
            // Arrange - Land first hit
            _comboTracker.RegisterHitLanded(0, 10f);
            yield return new WaitForFixedUpdate();

            // Act - Land second hit
            float multiplier = _comboTracker.RegisterHitLanded(0, 10f);

            // Assert
            Assert.AreEqual(0.75f, multiplier, 0.001f, "Second hit should have 75% damage (0.75x multiplier)");
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_ThirdHit_Deals50PercentDamage()
        {
            // Arrange - Land first two hits
            _comboTracker.RegisterHitLanded(0, 10f);
            yield return new WaitForFixedUpdate();
            _comboTracker.RegisterHitLanded(0, 10f);
            yield return new WaitForFixedUpdate();

            // Act - Land third hit
            float multiplier = _comboTracker.RegisterHitLanded(0, 10f);

            // Assert
            Assert.AreEqual(0.5f, multiplier, 0.001f, "Third hit should have 50% damage (0.5x multiplier)");
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_GettingHit_BreaksCombo()
        {
            // Arrange - Start a combo
            _comboTracker.RegisterHitLanded(0, 10f);
            yield return new WaitForFixedUpdate();
            _comboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(2, _comboTracker.ComboCount, "Setup: Should have 2-hit combo");

            bool comboBroken = false;
            _comboTracker.OnComboBroken += (count) => comboBroken = true;

            // Act - Take a hit (simulated)
            var hitData = new HitData(
                attacker: new GameObject("Opponent"),
                damage: 10f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Jab"
            );

            // Manually invoke the private HandleHitTaken method using reflection
            var method = typeof(CharacterComboTracker).GetMethod("HandleHitTaken",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(_comboTracker, new object[] { hitData });

            // Assert
            Assert.IsTrue(comboBroken, "OnComboBroken should fire");
            Assert.AreEqual(0, _comboTracker.ComboCount, "Combo should be broken (count = 0)");
            Assert.IsFalse(_comboTracker.IsInCombo, "Should not be in combo");

            // Cleanup
            Object.DestroyImmediate(hitData.Attacker);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_ResetCombo_ClearsState()
        {
            // Arrange - Start a combo
            _comboTracker.RegisterHitLanded(0, 10f);
            _comboTracker.RegisterHitLanded(0, 10f);
            Assert.AreEqual(2, _comboTracker.ComboCount);

            // Act
            _comboTracker.ResetCombo();

            // Assert
            Assert.AreEqual(0, _comboTracker.ComboCount, "Combo count should be 0");
            Assert.IsFalse(_comboTracker.IsInCombo, "Should not be in combo");

            yield return null;
        }

        [UnityTest]
        public IEnumerator CharacterComboTracker_OnComboHitLanded_FiresWithCorrectHitNumber()
        {
            // Arrange
            int lastHitNumber = 0;
            _comboTracker.OnComboHitLanded += (hitNum, damage) => lastHitNumber = hitNum;

            // Act
            _comboTracker.RegisterHitLanded(0, 10f); // Hit 1
            Assert.AreEqual(1, lastHitNumber);

            yield return new WaitForFixedUpdate();

            _comboTracker.RegisterHitLanded(0, 10f); // Hit 2
            Assert.AreEqual(2, lastHitNumber);

            yield return new WaitForFixedUpdate();

            _comboTracker.RegisterHitLanded(1, 15f); // Hit 3
            Assert.AreEqual(3, lastHitNumber);

            yield return null;
        }

        [Test]
        public void CharacterComboTracker_GetCurrentDamageMultiplier_ReturnsCorrectValue()
        {
            // Arrange - Land first hit
            _comboTracker.RegisterHitLanded(0, 10f);

            // Act - Land second hit and check multiplier BEFORE registering
            // (This tests the GetCurrentDamageMultiplier method directly)
            float multiplierBeforeSecondHit = _comboTracker.GetCurrentDamageMultiplier();

            // Assert - Since combo count is 1, multiplier for next hit (2nd) should be 0.75
            // Wait, actually GetCurrentDamageMultiplier returns multiplier for CURRENT combo position
            // After first hit, combo count = 1, so GetCurrentDamageMultiplier should return scale for hit 1 = 1.0
            // Let's test it correctly:
            Assert.AreEqual(1.0f, multiplierBeforeSecondHit, 0.001f, "After 1 hit, current multiplier should be for position 1 (100%)");
        }

        [Test]
        public void CharacterComboTracker_IsInCombo_ReflectsState()
        {
            // Arrange
            Assert.IsFalse(_comboTracker.IsInCombo, "Should not be in combo initially");

            // Act - Start combo
            _comboTracker.RegisterHitLanded(0, 10f);

            // Assert
            Assert.IsTrue(_comboTracker.IsInCombo, "Should be in combo after first hit");

            // Act - Reset combo
            _comboTracker.ResetCombo();

            // Assert
            Assert.IsFalse(_comboTracker.IsInCombo, "Should not be in combo after reset");
        }
    }
}

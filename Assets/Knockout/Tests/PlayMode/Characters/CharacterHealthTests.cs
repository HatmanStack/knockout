using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Characters
{
    /// <summary>
    /// Play mode tests for CharacterHealth component.
    /// </summary>
    public class CharacterHealthTests
    {
        private GameObject _testCharacter;
        private CharacterHealth _health;
        private CharacterStats _testStats;

        [SetUp]
        public void SetUp()
        {
            // Create test character
            _testCharacter = CreateMinimalCharacter();
            _health = _testCharacter.GetComponent<CharacterHealth>();

            // Create test stats
            _testStats = ScriptableObject.CreateInstance<CharacterStats>();
            _testStats.SetMaxHealth(100f);
            _testStats.SetDamageTakenMultiplier(1.0f);

            // Assign stats via reflection (since it's a serialized field)
            var statsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statsField?.SetValue(_health, _testStats);
        }

        [TearDown]
        public void TearDown()
        {
            if (_testCharacter != null)
            {
                Object.Destroy(_testCharacter);
            }

            if (_testStats != null)
            {
                Object.DestroyImmediate(_testStats);
            }
        }

        [UnityTest]
        public IEnumerator CharacterHealth_InitializesToMaxHealth()
        {
            // Wait for Start() to be called
            yield return null;

            // Assert
            Assert.AreEqual(100f, _health.CurrentHealth);
            Assert.AreEqual(100f, _health.MaxHealth);
            Assert.AreEqual(1.0f, _health.HealthPercentage);
            Assert.IsFalse(_health.IsDead);
        }

        [UnityTest]
        public IEnumerator TakeDamage_WithoutBlocking_ReducesHealth()
        {
            yield return null; // Wait for Start()

            // Arrange
            HitData hit = new HitData(
                attacker: new GameObject("Attacker"),
                damage: 20f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Test"
            );

            // Act
            _health.TakeDamage(hit);

            // Assert
            Assert.AreEqual(80f, _health.CurrentHealth);
            Assert.AreEqual(0.8f, _health.HealthPercentage, 0.01f);

            // Cleanup
            Object.Destroy(hit.Attacker);
        }

        [UnityTest]
        public IEnumerator TakeDamage_WithBlocking_ReducesDamageBy75Percent()
        {
            yield return null; // Wait for Start()

            // Arrange
            var combat = _testCharacter.GetComponent<CharacterCombat>();
            combat.StartBlocking();

            HitData hit = new HitData(
                attacker: new GameObject("Attacker"),
                damage: 20f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Test"
            );

            // Act
            _health.TakeDamage(hit);

            // Assert
            // 20 * 0.25 = 5 damage when blocking
            Assert.AreEqual(95f, _health.CurrentHealth);

            // Cleanup
            Object.Destroy(hit.Attacker);
        }

        [UnityTest]
        public IEnumerator TakeDamage_HealthReachesZero_TriggersDeath()
        {
            yield return null; // Wait for Start()

            // Arrange
            bool deathEventFired = false;
            _health.OnDeath += () => deathEventFired = true;

            HitData hit = new HitData(
                attacker: new GameObject("Attacker"),
                damage: 150f, // More than max health
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 2,
                attackName: "Knockout"
            );

            // Act
            _health.TakeDamage(hit);

            // Assert
            Assert.AreEqual(0f, _health.CurrentHealth);
            Assert.IsTrue(_health.IsDead);
            Assert.IsTrue(deathEventFired);

            // Cleanup
            Object.Destroy(hit.Attacker);
        }

        [UnityTest]
        public IEnumerator TakeDamage_WhenDead_IgnoresFurtherDamage()
        {
            yield return null; // Wait for Start()

            // Arrange - kill character
            HitData lethalHit = new HitData(
                attacker: new GameObject("Attacker"),
                damage: 150f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 2,
                attackName: "Knockout"
            );

            _health.TakeDamage(lethalHit);
            Assert.IsTrue(_health.IsDead);

            // Act - try to damage again
            HitData secondHit = new HitData(
                attacker: new GameObject("Attacker2"),
                damage: 10f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Test"
            );

            _health.TakeDamage(secondHit);

            // Assert - health should still be 0
            Assert.AreEqual(0f, _health.CurrentHealth);

            // Cleanup
            Object.Destroy(lethalHit.Attacker);
            Object.Destroy(secondHit.Attacker);
        }

        [UnityTest]
        public IEnumerator Heal_IncreasesHealth()
        {
            yield return null; // Wait for Start()

            // Arrange - take some damage
            HitData hit = new HitData(
                attacker: new GameObject("Attacker"),
                damage: 30f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Test"
            );

            _health.TakeDamage(hit);
            Assert.AreEqual(70f, _health.CurrentHealth);

            // Act
            _health.Heal(20f);

            // Assert
            Assert.AreEqual(90f, _health.CurrentHealth);

            // Cleanup
            Object.Destroy(hit.Attacker);
        }

        [UnityTest]
        public IEnumerator Heal_CannotExceedMaxHealth()
        {
            yield return null; // Wait for Start()

            // Act
            _health.Heal(50f); // Already at max, try to heal more

            // Assert
            Assert.AreEqual(100f, _health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator OnHealthChanged_FiresWhenDamageTaken()
        {
            yield return null; // Wait for Start()

            // Arrange
            bool eventFired = false;
            float reportedCurrent = 0f;
            float reportedMax = 0f;

            _health.OnHealthChanged += (current, max) =>
            {
                eventFired = true;
                reportedCurrent = current;
                reportedMax = max;
            };

            HitData hit = new HitData(
                attacker: new GameObject("Attacker"),
                damage: 25f,
                knockback: 0f,
                hitPoint: Vector3.zero,
                hitDirection: Vector3.forward,
                hitType: 0,
                attackName: "Test"
            );

            // Act
            _health.TakeDamage(hit);

            // Assert
            Assert.IsTrue(eventFired);
            Assert.AreEqual(75f, reportedCurrent);
            Assert.AreEqual(100f, reportedMax);

            // Cleanup
            Object.Destroy(hit.Attacker);
        }

        #region Helper Methods

        private GameObject CreateMinimalCharacter()
        {
            GameObject character = new GameObject("TestCharacter");

            // Add Animator (required)
            character.AddComponent<Animator>();

            // Add CharacterAnimator (required by CharacterCombat)
            character.AddComponent<CharacterAnimator>();

            // Add CharacterCombat (required by CharacterHealth)
            character.AddComponent<CharacterCombat>();

            // Add CharacterHealth
            character.AddComponent<CharacterHealth>();

            return character;
        }

        #endregion
    }
}

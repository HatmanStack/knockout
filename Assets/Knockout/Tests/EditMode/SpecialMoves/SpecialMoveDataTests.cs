using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;
using System.Reflection;

namespace Knockout.Tests.EditMode.SpecialMoves
{
    /// <summary>
    /// Tests for SpecialMoveData ScriptableObject validation and data integrity.
    /// </summary>
    [TestFixture]
    public class SpecialMoveDataTests
    {
        [Test]
        public void SpecialMoveData_CreateInstance_Succeeds()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual("Special Move", data.SpecialMoveName);
        }

        [Test]
        public void SpecialMoveData_DefaultValues_MatchDesignSpec()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Assert
            Assert.AreEqual(2.0f, data.DamageMultiplier, "Default damage multiplier should be 2.0x");
            Assert.AreEqual(2.5f, data.KnockbackMultiplier, "Default knockback multiplier should be 2.5x");
            Assert.AreEqual(45f, data.CooldownSeconds, "Default cooldown should be 45 seconds");
            Assert.AreEqual(40f, data.StaminaCost, "Default stamina cost should be 40");
            Assert.IsTrue(data.TriggersSpecialKnockdown, "Should trigger special knockdown by default");
            Assert.AreEqual(3.0f, data.SpecialKnockdownDuration, "Special knockdown duration should be 3.0s");
        }

        [Test]
        public void SpecialMoveData_OnValidate_ClampsDamageMultiplierToRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Set damage multiplier above max (3.0)
            var damageMultField = typeof(SpecialMoveData).GetField("damageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            damageMultField.SetValue(data, 5.0f);

            // Act
            var onValidateMethod = typeof(SpecialMoveData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(3.0f, data.DamageMultiplier, "Damage multiplier should be clamped to 3.0");
        }

        [Test]
        public void SpecialMoveData_OnValidate_ClampsDamageMultiplierToMinimum()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Set damage multiplier below min (1.0)
            var damageMultField = typeof(SpecialMoveData).GetField("damageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            damageMultField.SetValue(data, 0.5f);

            // Act
            var onValidateMethod = typeof(SpecialMoveData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(1.0f, data.DamageMultiplier, "Damage multiplier should be clamped to 1.0");
        }

        [Test]
        public void SpecialMoveData_OnValidate_ClampsKnockbackMultiplierToRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Set knockback multiplier above max (3.0)
            var knockbackMultField = typeof(SpecialMoveData).GetField("knockbackMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            knockbackMultField.SetValue(data, 4.5f);

            // Act
            var onValidateMethod = typeof(SpecialMoveData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(3.0f, data.KnockbackMultiplier, "Knockback multiplier should be clamped to 3.0");
        }

        [Test]
        public void SpecialMoveData_OnValidate_EnsuresCooldownPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Set negative cooldown
            var cooldownField = typeof(SpecialMoveData).GetField("cooldownSeconds", BindingFlags.NonPublic | BindingFlags.Instance);
            cooldownField.SetValue(data, -10f);

            // Act
            var onValidateMethod = typeof(SpecialMoveData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.Greater(data.CooldownSeconds, 0f, "Cooldown should be positive");
        }

        [Test]
        public void SpecialMoveData_OnValidate_EnsuresStaminaCostPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Set negative stamina cost
            var staminaField = typeof(SpecialMoveData).GetField("staminaCost", BindingFlags.NonPublic | BindingFlags.Instance);
            staminaField.SetValue(data, -20f);

            // Act
            var onValidateMethod = typeof(SpecialMoveData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.Greater(data.StaminaCost, 0f, "Stamina cost should be positive");
        }

        [Test]
        public void SpecialMoveData_AnimationTrigger_UsesBaseAttackWhenEmpty()
        {
            // Arrange
            var baseAttack = ScriptableObject.CreateInstance<AttackData>();
            var baseAttackTriggerField = typeof(AttackData).GetField("animationTrigger", BindingFlags.NonPublic | BindingFlags.Instance);
            baseAttackTriggerField.SetValue(baseAttack, "Hook");

            var specialMove = ScriptableObject.CreateInstance<SpecialMoveData>();
            var baseAttackField = typeof(SpecialMoveData).GetField("baseAttackData", BindingFlags.NonPublic | BindingFlags.Instance);
            baseAttackField.SetValue(specialMove, baseAttack);

            var animTriggerField = typeof(SpecialMoveData).GetField("animationTrigger", BindingFlags.NonPublic | BindingFlags.Instance);
            animTriggerField.SetValue(specialMove, "");

            // Act
            string trigger = specialMove.AnimationTrigger;

            // Assert
            Assert.AreEqual("Hook", trigger, "Should use base attack trigger when special trigger is empty");
        }

        [Test]
        public void SpecialMoveData_AnimationTrigger_UsesCustomWhenProvided()
        {
            // Arrange
            var baseAttack = ScriptableObject.CreateInstance<AttackData>();
            var baseAttackTriggerField = typeof(AttackData).GetField("animationTrigger", BindingFlags.NonPublic | BindingFlags.Instance);
            baseAttackTriggerField.SetValue(baseAttack, "Hook");

            var specialMove = ScriptableObject.CreateInstance<SpecialMoveData>();
            var baseAttackField = typeof(SpecialMoveData).GetField("baseAttackData", BindingFlags.NonPublic | BindingFlags.Instance);
            baseAttackField.SetValue(specialMove, baseAttack);

            var animTriggerField = typeof(SpecialMoveData).GetField("animationTrigger", BindingFlags.NonPublic | BindingFlags.Instance);
            animTriggerField.SetValue(specialMove, "Haymaker");

            // Act
            string trigger = specialMove.AnimationTrigger;

            // Assert
            Assert.AreEqual("Haymaker", trigger, "Should use custom trigger when provided");
        }

        [Test]
        public void SpecialMoveData_OnValidate_ClampsKnockdownDurationToNonNegative()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Set negative knockdown duration
            var durationField = typeof(SpecialMoveData).GetField("specialKnockdownDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            durationField.SetValue(data, -2.0f);

            // Act
            var onValidateMethod = typeof(SpecialMoveData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.SpecialKnockdownDuration, 0f, "Knockdown duration should be non-negative");
        }
    }
}

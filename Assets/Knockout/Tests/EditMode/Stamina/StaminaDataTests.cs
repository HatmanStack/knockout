using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;
using System.Reflection;

namespace Knockout.Tests.EditMode.Stamina
{
    /// <summary>
    /// Tests for StaminaData ScriptableObject validation and data integrity.
    /// </summary>
    [TestFixture]
    public class StaminaDataTests
    {
        [Test]
        public void StaminaData_CreateInstance_Succeeds()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual(100f, data.MaxStamina);
            Assert.AreEqual(25f, data.RegenPerSecond);
        }

        [Test]
        public void StaminaData_DefaultValues_MatchDesignSpec()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Assert
            Assert.AreEqual(100f, data.MaxStamina, "Max stamina should be 100");
            Assert.AreEqual(25f, data.RegenPerSecond, "Regen rate should be 25/sec");
            Assert.AreEqual(10f, data.GetAttackCost(0), "Jab cost should be 10");
            Assert.AreEqual(15f, data.GetAttackCost(1), "Hook cost should be 15");
            Assert.AreEqual(20f, data.GetAttackCost(2), "Uppercut cost should be 20");
            Assert.AreEqual(40f, data.SpecialMoveCost, "Special move cost should be 40");
            Assert.AreEqual(2f, data.ExhaustionDuration, "Exhaustion duration should be 2s");
            Assert.AreEqual(0.5f, data.ExhaustionRegenMultiplier, "Exhaustion regen multiplier should be 0.5");
            Assert.AreEqual(25f, data.ExhaustionRecoveryThreshold, "Exhaustion recovery threshold should be 25");
        }

        [Test]
        public void StaminaData_OnValidate_ClampsNegativeRegenRate()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Use reflection to set negative value and trigger OnValidate
            var regenField = typeof(StaminaData).GetField("regenPerSecond", BindingFlags.NonPublic | BindingFlags.Instance);
            regenField.SetValue(data, -10f);

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0f, data.RegenPerSecond, "Negative regen rate should be clamped to 0");
        }

        [Test]
        public void StaminaData_OnValidate_ClampsNegativeMaxStamina()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Use reflection to set negative value
            var maxStaminaField = typeof(StaminaData).GetField("maxStamina", BindingFlags.NonPublic | BindingFlags.Instance);
            maxStaminaField.SetValue(data, -50f);

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0f, data.MaxStamina, "Negative max stamina should be clamped to 0");
        }

        [Test]
        public void StaminaData_OnValidate_ClampsAttackCostsToMaxStamina()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Set max stamina to 50
            var maxStaminaField = typeof(StaminaData).GetField("maxStamina", BindingFlags.NonPublic | BindingFlags.Instance);
            maxStaminaField.SetValue(data, 50f);

            // Set attack cost higher than max
            var attackCostsField = typeof(StaminaData).GetField("attackCosts", BindingFlags.NonPublic | BindingFlags.Instance);
            attackCostsField.SetValue(data, new float[] { 10f, 80f, 20f });

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.LessOrEqual(data.GetAttackCost(1), 50f, "Attack cost should be clamped to max stamina");
        }

        [Test]
        public void StaminaData_OnValidate_ClampsNegativeAttackCosts()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Set negative attack cost
            var attackCostsField = typeof(StaminaData).GetField("attackCosts", BindingFlags.NonPublic | BindingFlags.Instance);
            attackCostsField.SetValue(data, new float[] { -5f, 15f, 20f });

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.GetAttackCost(0), 0f, "Negative attack cost should be clamped to 0");
        }

        [Test]
        public void StaminaData_OnValidate_FixesInvalidAttackCostsArray()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Set invalid array (wrong size)
            var attackCostsField = typeof(StaminaData).GetField("attackCosts", BindingFlags.NonPublic | BindingFlags.Instance);
            attackCostsField.SetValue(data, new float[] { 10f });

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(3, data.AttackCosts.Length, "Attack costs array should be fixed to length 3");
        }

        [Test]
        public void StaminaData_GetAttackCost_ReturnsZeroForInvalidIndex()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Act & Assert
            Assert.AreEqual(0f, data.GetAttackCost(-1), "Negative index should return 0");
            Assert.AreEqual(0f, data.GetAttackCost(10), "Out of bounds index should return 0");
        }

        [Test]
        public void StaminaData_BoundaryCondition_ZeroMaxStamina()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Set max stamina to 0
            var maxStaminaField = typeof(StaminaData).GetField("maxStamina", BindingFlags.NonPublic | BindingFlags.Instance);
            maxStaminaField.SetValue(data, 0f);

            // Set attack costs
            var attackCostsField = typeof(StaminaData).GetField("attackCosts", BindingFlags.NonPublic | BindingFlags.Instance);
            attackCostsField.SetValue(data, new float[] { 10f, 15f, 20f });

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0f, data.MaxStamina);
            // Attack costs should be clamped to 0 when max stamina is 0
            Assert.AreEqual(0f, data.GetAttackCost(0));
            Assert.AreEqual(0f, data.GetAttackCost(1));
            Assert.AreEqual(0f, data.GetAttackCost(2));
        }

        [Test]
        public void StaminaData_BoundaryCondition_ZeroRegenRate()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Set regen rate to 0
            var regenField = typeof(StaminaData).GetField("regenPerSecond", BindingFlags.NonPublic | BindingFlags.Instance);
            regenField.SetValue(data, 0f);

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0f, data.RegenPerSecond, "Zero regen rate is valid");
        }

        [Test]
        public void StaminaData_OnValidate_ClampsRecoveryThresholdToValidPercentage()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<StaminaData>();

            // Set threshold above 100%
            var thresholdField = typeof(StaminaData).GetField("exhaustionRecoveryThreshold", BindingFlags.NonPublic | BindingFlags.Instance);
            thresholdField.SetValue(data, 150f);

            // Act
            var onValidateMethod = typeof(StaminaData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.LessOrEqual(data.ExhaustionRecoveryThreshold, 100f, "Recovery threshold should be clamped to 100");
        }
    }
}

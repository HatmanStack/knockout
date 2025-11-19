using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;
using System.Reflection;

namespace Knockout.Tests.EditMode.Characters
{
    /// <summary>
    /// Tests for AttackData ScriptableObject validation and properties.
    /// </summary>
    [TestFixture]
    public class AttackDataTests
    {
        [Test]
        public void AttackData_CreateInstance_Succeeds()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<AttackData>();

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void AttackData_DefaultStaminaCost_IsZero()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<AttackData>();

            // Assert
            Assert.AreEqual(0f, data.StaminaCost, "Default stamina cost should be 0 (use StaminaData default)");
        }

        [Test]
        public void AttackData_OnValidate_ClampsNegativeStaminaCost()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<AttackData>();

            // Use reflection to set negative value
            var staminaCostField = typeof(AttackData).GetField("staminaCost", BindingFlags.NonPublic | BindingFlags.Instance);
            staminaCostField.SetValue(data, -10f);

            // Act
            var onValidateMethod = typeof(AttackData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0f, data.StaminaCost, "Negative stamina cost should be clamped to 0");
        }

        [Test]
        public void AttackData_StaminaCostProperty_ReturnsCorrectValue()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<AttackData>();

            // Use reflection to set value
            var staminaCostField = typeof(AttackData).GetField("staminaCost", BindingFlags.NonPublic | BindingFlags.Instance);
            staminaCostField.SetValue(data, 15f);

            // Act & Assert
            Assert.AreEqual(15f, data.StaminaCost);
        }
    }
}

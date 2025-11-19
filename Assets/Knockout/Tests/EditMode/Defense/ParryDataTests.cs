using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;
using System.Reflection;

namespace Knockout.Tests.EditMode.Defense
{
    /// <summary>
    /// Tests for ParryData ScriptableObject validation and data integrity.
    /// </summary>
    [TestFixture]
    public class ParryDataTests
    {
        private const int TARGET_FRAME_RATE = 60;
        private const float FIXED_DELTA_TIME = 1f / TARGET_FRAME_RATE;

        [Test]
        public void ParryData_CreateInstance_Succeeds()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void ParryData_DefaultValues_MatchDesignSpec()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Assert
            Assert.AreEqual(6, data.ParryWindowFrames, "Parry window should be 6 frames");
            Assert.AreEqual(12, data.ParrySuccessDurationFrames, "Parry success duration should be 12 frames");
            Assert.AreEqual(18, data.ParryCooldownFrames, "Cooldown should be 18 frames");
            Assert.AreEqual(0.5f, data.AttackerStaggerDuration, "Attacker stagger should be 0.5s");
            Assert.AreEqual(0.5f, data.CounterWindowDuration, "Counter window should be 0.5s");
        }

        [Test]
        public void ParryData_FrameToSecondConversion_Correct()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Act
            float parryWindow = data.ParryWindow;
            float parrySuccessDuration = data.ParrySuccessDuration;
            float cooldownDuration = data.CooldownDuration;

            // Assert
            Assert.AreEqual(6 * FIXED_DELTA_TIME, parryWindow, 0.001f, "Parry window conversion incorrect");
            Assert.AreEqual(12 * FIXED_DELTA_TIME, parrySuccessDuration, 0.001f, "Success duration conversion incorrect");
            Assert.AreEqual(18 * FIXED_DELTA_TIME, cooldownDuration, 0.001f, "Cooldown duration conversion incorrect");
        }

        [Test]
        public void ParryData_OnValidate_ClampsParryWindowToPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set zero or negative window
            var windowField = typeof(ParryData).GetField("parryWindowFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            windowField.SetValue(data, 0);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.ParryWindowFrames, 1, "Parry window should be at least 1 frame");
        }

        [Test]
        public void ParryData_OnValidate_ClampsSuccessDurationToPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set zero or negative duration
            var durationField = typeof(ParryData).GetField("parrySuccessDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            durationField.SetValue(data, 0);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.ParrySuccessDurationFrames, 1, "Success duration should be at least 1 frame");
        }

        [Test]
        public void ParryData_OnValidate_ClampsCooldownToNonNegative()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set negative cooldown
            var cooldownField = typeof(ParryData).GetField("parryCooldownFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            cooldownField.SetValue(data, -5);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.ParryCooldownFrames, 0, "Cooldown should be non-negative");
        }

        [Test]
        public void ParryData_OnValidate_ClampsStaggerDurationToPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set zero or negative stagger
            var staggerField = typeof(ParryData).GetField("attackerStaggerDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            staggerField.SetValue(data, 0f);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.AttackerStaggerDuration, 0.1f, "Stagger duration should be at least 0.1s");
        }

        [Test]
        public void ParryData_OnValidate_CounterWindowNotExceedStagger()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set counter window larger than stagger
            var staggerField = typeof(ParryData).GetField("attackerStaggerDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            staggerField.SetValue(data, 0.5f);

            var counterField = typeof(ParryData).GetField("counterWindowDuration", BindingFlags.NonPublic | BindingFlags.Instance);
            counterField.SetValue(data, 1.0f);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.LessOrEqual(data.CounterWindowDuration, data.AttackerStaggerDuration,
                "Counter window should not exceed stagger duration");
        }

        [Test]
        public void ParryData_BoundaryCondition_MinimumParryWindow()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set minimum parry window
            var windowField = typeof(ParryData).GetField("parryWindowFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            windowField.SetValue(data, 1);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(1, data.ParryWindowFrames, "Minimum parry window is 1 frame");
        }

        [Test]
        public void ParryData_BoundaryCondition_ZeroCooldown()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ParryData>();

            // Set zero cooldown
            var cooldownField = typeof(ParryData).GetField("parryCooldownFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            cooldownField.SetValue(data, 0);

            // Act
            var onValidateMethod = typeof(ParryData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0, data.ParryCooldownFrames, "Zero cooldown is valid (no spam prevention)");
        }
    }
}

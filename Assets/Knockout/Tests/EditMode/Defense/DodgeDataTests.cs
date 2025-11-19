using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;
using System.Reflection;

namespace Knockout.Tests.EditMode.Defense
{
    /// <summary>
    /// Tests for DodgeData ScriptableObject validation and data integrity.
    /// </summary>
    [TestFixture]
    public class DodgeDataTests
    {
        private const int TARGET_FRAME_RATE = 60;
        private const float FIXED_DELTA_TIME = 1f / TARGET_FRAME_RATE;

        [Test]
        public void DodgeData_CreateInstance_Succeeds()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Assert
            Assert.IsNotNull(data);
        }

        [Test]
        public void DodgeData_DefaultValues_MatchDesignSpec()
        {
            // Arrange & Act
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Assert
            Assert.AreEqual(18, data.DodgeDurationFrames, "Dodge duration should be 18 frames");
            Assert.AreEqual(2, data.IFrameStartFrame, "i-frame start should be 2 frames");
            Assert.AreEqual(8, data.IFrameDurationFrames, "i-frame duration should be 8 frames");
            Assert.AreEqual(1.5f, data.DodgeDistance, "Dodge distance should be 1.5 units");
            Assert.AreEqual(12, data.CooldownFrames, "Cooldown should be 12 frames");
        }

        [Test]
        public void DodgeData_FrameToSecondConversion_Correct()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Act
            float dodgeDuration = data.DodgeDuration;
            float iFrameStartTime = data.IFrameStartTime;
            float iFrameDuration = data.IFrameDuration;
            float cooldownDuration = data.CooldownDuration;

            // Assert
            Assert.AreEqual(18 * FIXED_DELTA_TIME, dodgeDuration, 0.001f, "Dodge duration conversion incorrect");
            Assert.AreEqual(2 * FIXED_DELTA_TIME, iFrameStartTime, 0.001f, "i-frame start time conversion incorrect");
            Assert.AreEqual(8 * FIXED_DELTA_TIME, iFrameDuration, 0.001f, "i-frame duration conversion incorrect");
            Assert.AreEqual(12 * FIXED_DELTA_TIME, cooldownDuration, 0.001f, "Cooldown duration conversion incorrect");
        }

        [Test]
        public void DodgeData_IFrameEndFrame_CalculatedCorrectly()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Act
            int iFrameEndFrame = data.IFrameEndFrame;

            // Assert
            Assert.AreEqual(10, iFrameEndFrame, "i-frame end should be start (2) + duration (8) = 10");
        }

        [Test]
        public void DodgeData_OnValidate_ClampsIFrameStartToValidRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set i-frame start beyond dodge duration
            var iFrameStartField = typeof(DodgeData).GetField("iFrameStartFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            iFrameStartField.SetValue(data, 20);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.LessOrEqual(data.IFrameStartFrame, data.DodgeDurationFrames - 1, "i-frame start should be clamped within dodge duration");
        }

        [Test]
        public void DodgeData_OnValidate_PreventsIFrameDurationExceedingTotalDuration()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set dodge duration to 10 frames
            var durationField = typeof(DodgeData).GetField("dodgeDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            durationField.SetValue(data, 10);

            // Set i-frame start to 2 and duration to 20 (would exceed total)
            var iFrameStartField = typeof(DodgeData).GetField("iFrameStartFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            iFrameStartField.SetValue(data, 2);

            var iFrameDurationField = typeof(DodgeData).GetField("iFrameDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            iFrameDurationField.SetValue(data, 20);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            int maxValidDuration = data.DodgeDurationFrames - data.IFrameStartFrame;
            Assert.LessOrEqual(data.IFrameDurationFrames, maxValidDuration, "i-frame duration should fit within dodge duration");
        }

        [Test]
        public void DodgeData_OnValidate_ClampsDodgeDurationToPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set negative or zero duration
            var durationField = typeof(DodgeData).GetField("dodgeDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            durationField.SetValue(data, 0);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.DodgeDurationFrames, 1, "Dodge duration should be at least 1 frame");
        }

        [Test]
        public void DodgeData_OnValidate_ClampsCooldownToNonNegative()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set negative cooldown
            var cooldownField = typeof(DodgeData).GetField("cooldownFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            cooldownField.SetValue(data, -5);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.CooldownFrames, 0, "Cooldown should be non-negative");
        }

        [Test]
        public void DodgeData_OnValidate_ClampsDodgeDistanceToPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set zero or negative distance
            var distanceField = typeof(DodgeData).GetField("dodgeDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            distanceField.SetValue(data, 0f);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.DodgeDistance, 0.1f, "Dodge distance should be at least 0.1");
        }

        [Test]
        public void DodgeData_OnValidate_ClampsSpeedMultiplierToPositive()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set zero or negative multiplier
            var multiplierField = typeof(DodgeData).GetField("dodgeSpeedMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            multiplierField.SetValue(data, 0f);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.GreaterOrEqual(data.DodgeSpeedMultiplier, 0.1f, "Speed multiplier should be at least 0.1");
        }

        [Test]
        public void DodgeData_BoundaryCondition_IFrameAtDodgeEnd()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set i-frame to start at last frame
            var durationField = typeof(DodgeData).GetField("dodgeDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            durationField.SetValue(data, 10);

            var iFrameStartField = typeof(DodgeData).GetField("iFrameStartFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            iFrameStartField.SetValue(data, 9);

            var iFrameDurationField = typeof(DodgeData).GetField("iFrameDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            iFrameDurationField.SetValue(data, 1);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(9, data.IFrameStartFrame, "i-frame can start at second-to-last frame");
            Assert.AreEqual(1, data.IFrameDurationFrames, "i-frame duration should be 1 frame");
            Assert.AreEqual(10, data.IFrameEndFrame, "i-frame should end at dodge end");
        }

        [Test]
        public void DodgeData_BoundaryCondition_ZeroIFrameDuration()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<DodgeData>();

            // Set i-frame duration to 0
            var iFrameDurationField = typeof(DodgeData).GetField("iFrameDurationFrames", BindingFlags.NonPublic | BindingFlags.Instance);
            iFrameDurationField.SetValue(data, 0);

            // Act
            var onValidateMethod = typeof(DodgeData).GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(data, null);

            // Assert
            Assert.AreEqual(0, data.IFrameDurationFrames, "Zero i-frame duration is valid (no invincibility)");
        }
    }
}

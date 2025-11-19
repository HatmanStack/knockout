using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;

namespace Knockout.Tests.EditMode.Combos
{
    /// <summary>
    /// EditMode tests for ComboSequenceData ScriptableObject.
    /// Tests validation and property calculations.
    /// </summary>
    public class ComboSequenceDataTests
    {
        [Test]
        public void ComboSequenceData_Creation_Succeeds()
        {
            // Act
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Assert
            Assert.IsNotNull(data);
            Assert.AreEqual("Combo Sequence", data.SequenceName);
        }

        [Test]
        public void ComboSequenceData_DefaultSequence_HasMinimumTwoAttacks()
        {
            // Act
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Assert
            Assert.IsNotNull(data.AttackSequence);
            Assert.GreaterOrEqual(data.AttackSequence.Length, 2, "Sequence must have at least 2 attacks");
        }

        [Test]
        public void ComboSequenceData_SequenceLength_CalculatedCorrectly()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Act
            int length = data.SequenceLength;

            // Assert
            Assert.AreEqual(data.AttackSequence.Length, length);
        }

        [Test]
        public void ComboSequenceData_TimingWindowSeconds_ConvertedFromFrames()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();
            // Assume default timing window is 15 frames

            // Act
            float seconds = data.TimingWindowSeconds;

            // Assert
            float expectedSeconds = data.TimingWindowFrames / 60f;
            Assert.AreEqual(expectedSeconds, seconds, 0.001f);
        }

        [Test]
        public void ComboSequenceData_DamageBonusMultiplier_ClampedToRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Act & Assert - Check initial value is in range
            Assert.GreaterOrEqual(data.DamageBonusMultiplier, 1.0f);
            Assert.LessOrEqual(data.DamageBonusMultiplier, 3.0f);
        }

        [Test]
        public void ComboSequenceData_KnockbackMultiplier_ClampedToRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Act & Assert
            Assert.GreaterOrEqual(data.KnockbackMultiplier, 1.0f);
            Assert.LessOrEqual(data.KnockbackMultiplier, 3.0f);
        }

        [Test]
        public void ComboSequenceData_AttackIndices_ValidRange()
        {
            // Arrange
            var data = ScriptableObject.CreateInstance<ComboSequenceData>();

            // Act & Assert - All attack indices should be 0-2 (Jab, Hook, Uppercut)
            foreach (int attackType in data.AttackSequence)
            {
                Assert.GreaterOrEqual(attackType, 0);
                Assert.LessOrEqual(attackType, 2);
            }
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using Knockout.Characters.Data;
using System.Reflection;

namespace Knockout.Tests.EditMode.Scoring
{
    /// <summary>
    /// Tests for ScoringWeights ScriptableObject validation and data integrity.
    /// </summary>
    [TestFixture]
    public class ScoringWeightsTests
    {
        [Test]
        public void ScoringWeights_CreateInstance_Succeeds()
        {
            // Arrange & Act
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Assert
            Assert.IsNotNull(weights);
        }

        [Test]
        public void ScoringWeights_DefaultValues_MatchDesignSpec()
        {
            // Arrange & Act
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Assert - Offensive weights
            Assert.AreEqual(1f, weights.CleanHitPoints, "Clean hit points should be 1");
            Assert.AreEqual(0.5f, weights.ComboHitBonus, "Combo hit bonus should be 0.5");
            Assert.AreEqual(5f, weights.ComboSequencePoints, "Combo sequence points should be 5");
            Assert.AreEqual(8f, weights.SpecialMovePoints, "Special move points should be 8");
            Assert.AreEqual(10f, weights.KnockdownPoints, "Knockdown points should be 10");
            Assert.AreEqual(0.1f, weights.DamageDealtWeight, "Damage dealt weight should be 0.1");

            // Assert - Defensive weights
            Assert.AreEqual(0.5f, weights.BlockPoints, "Block points should be 0.5");
            Assert.AreEqual(2f, weights.ParryPoints, "Parry points should be 2");
            Assert.AreEqual(1f, weights.DodgePoints, "Dodge points should be 1");

            // Assert - Control weights
            Assert.AreEqual(0.1f, weights.AggressionPointsPerSecond, "Aggression points should be 0.1/sec");
            Assert.AreEqual(0f, weights.RingControlBonus, "Ring control bonus should be 0 (optional)");

            // Assert - Penalties
            Assert.AreEqual(3f, weights.ExhaustionPenalty, "Exhaustion penalty should be 3");
            Assert.AreEqual(0f, weights.MissedAttackPenalty, "Missed attack penalty should be 0 (optional)");
        }

        [Test]
        public void ScoringWeights_OnValidate_ClampsNegativeCleanHitPoints()
        {
            // Arrange
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Use reflection to set negative value
            var cleanHitField = typeof(ScoringWeights).GetField("cleanHitPoints",
                BindingFlags.NonPublic | BindingFlags.Instance);
            cleanHitField.SetValue(weights, -5f);

            // Act
            var onValidateMethod = typeof(ScoringWeights).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(weights, null);

            // Assert
            Assert.AreEqual(0f, weights.CleanHitPoints, "Negative clean hit points should be clamped to 0");
        }

        [Test]
        public void ScoringWeights_OnValidate_ClampsNegativeComboSequencePoints()
        {
            // Arrange
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Use reflection to set negative value
            var comboSeqField = typeof(ScoringWeights).GetField("comboSequencePoints",
                BindingFlags.NonPublic | BindingFlags.Instance);
            comboSeqField.SetValue(weights, -10f);

            // Act
            var onValidateMethod = typeof(ScoringWeights).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(weights, null);

            // Assert
            Assert.AreEqual(0f, weights.ComboSequencePoints, "Negative combo sequence points should be clamped to 0");
        }

        [Test]
        public void ScoringWeights_OnValidate_ClampsNegativeParryPoints()
        {
            // Arrange
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Use reflection to set negative value
            var parryField = typeof(ScoringWeights).GetField("parryPoints",
                BindingFlags.NonPublic | BindingFlags.Instance);
            parryField.SetValue(weights, -2f);

            // Act
            var onValidateMethod = typeof(ScoringWeights).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(weights, null);

            // Assert
            Assert.AreEqual(0f, weights.ParryPoints, "Negative parry points should be clamped to 0");
        }

        [Test]
        public void ScoringWeights_OnValidate_ClampsNegativeExhaustionPenalty()
        {
            // Arrange
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Use reflection to set negative value
            var exhaustionField = typeof(ScoringWeights).GetField("exhaustionPenalty",
                BindingFlags.NonPublic | BindingFlags.Instance);
            exhaustionField.SetValue(weights, -3f);

            // Act
            var onValidateMethod = typeof(ScoringWeights).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(weights, null);

            // Assert
            Assert.AreEqual(0f, weights.ExhaustionPenalty, "Negative exhaustion penalty should be clamped to 0");
        }

        [Test]
        public void ScoringWeights_OnValidate_ClampsNegativeAggressionPoints()
        {
            // Arrange
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Use reflection to set negative value
            var aggressionField = typeof(ScoringWeights).GetField("aggressionPointsPerSecond",
                BindingFlags.NonPublic | BindingFlags.Instance);
            aggressionField.SetValue(weights, -0.5f);

            // Act
            var onValidateMethod = typeof(ScoringWeights).GetMethod("OnValidate",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onValidateMethod.Invoke(weights, null);

            // Assert
            Assert.AreEqual(0f, weights.AggressionPointsPerSecond,
                "Negative aggression points should be clamped to 0");
        }

        [Test]
        public void ScoringWeights_AllWeights_NonNegativeByDefault()
        {
            // Arrange & Act
            var weights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Assert
            Assert.GreaterOrEqual(weights.CleanHitPoints, 0f);
            Assert.GreaterOrEqual(weights.ComboHitBonus, 0f);
            Assert.GreaterOrEqual(weights.ComboSequencePoints, 0f);
            Assert.GreaterOrEqual(weights.SpecialMovePoints, 0f);
            Assert.GreaterOrEqual(weights.KnockdownPoints, 0f);
            Assert.GreaterOrEqual(weights.DamageDealtWeight, 0f);
            Assert.GreaterOrEqual(weights.BlockPoints, 0f);
            Assert.GreaterOrEqual(weights.ParryPoints, 0f);
            Assert.GreaterOrEqual(weights.DodgePoints, 0f);
            Assert.GreaterOrEqual(weights.AggressionPointsPerSecond, 0f);
            Assert.GreaterOrEqual(weights.RingControlBonus, 0f);
            Assert.GreaterOrEqual(weights.ExhaustionPenalty, 0f);
            Assert.GreaterOrEqual(weights.MissedAttackPenalty, 0f);
        }
    }
}

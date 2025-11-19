using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.Scoring
{
    /// <summary>
    /// PlayMode tests for CharacterScoring component.
    /// </summary>
    [TestFixture]
    public class CharacterScoringTests
    {
        private GameObject _characterObject;
        private CharacterScoring _scoring;
        private ScoringWeights _scoringWeights;

        [SetUp]
        public void SetUp()
        {
            // Create test scoring weights
            _scoringWeights = ScriptableObject.CreateInstance<ScoringWeights>();

            // Create character GameObject
            _characterObject = new GameObject("TestCharacter");

            // Add CharacterScoring
            _scoring = _characterObject.AddComponent<CharacterScoring>();

            // Set scoring weights via reflection
            var weightsField = typeof(CharacterScoring).GetField("scoringWeights",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            weightsField.SetValue(_scoring, _scoringWeights);
        }

        [TearDown]
        public void TearDown()
        {
            if (_characterObject != null)
            {
                Object.DestroyImmediate(_characterObject);
            }

            if (_scoringWeights != null)
            {
                Object.DestroyImmediate(_scoringWeights);
            }
        }

        [UnityTest]
        public IEnumerator CharacterScoring_InitializesCorrectly()
        {
            // Wait for Start()
            yield return null;

            // Assert
            Assert.AreEqual(0f, _scoring.TotalScore, "Initial score should be 0");
            Assert.AreEqual(0, _scoring.CleanHitsLanded, "Initial hits should be 0");
            Assert.AreEqual(0f, _scoring.TotalDamageDealt, "Initial damage should be 0");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_RecordHitLanded_IncreasesScore()
        {
            yield return null; // Wait for initialization

            // Arrange
            float eventFiredScore = -1f;
            _scoring.OnScoreChanged += (score) => eventFiredScore = score;

            // Act
            _scoring.RecordHitLanded(10f);

            // Assert
            Assert.AreEqual(1, _scoring.CleanHitsLanded, "Hits landed should increment");
            Assert.AreEqual(10f, _scoring.TotalDamageDealt, "Damage should accumulate");
            Assert.Greater(_scoring.TotalScore, 0f, "Score should increase");
            Assert.Greater(eventFiredScore, 0f, "OnScoreChanged event should fire");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_RecordKnockdown_AddsKnockdownPoints()
        {
            yield return null;

            // Arrange
            float initialScore = _scoring.TotalScore;

            // Act
            _scoring.RecordKnockdownInflicted();

            // Assert
            Assert.AreEqual(1, _scoring.KnockdownsInflicted, "Knockdown count should increment");
            Assert.Greater(_scoring.TotalScore, initialScore, "Score should increase from knockdown");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_RecordBlockSuccessful_AddsBlockPoints()
        {
            yield return null;

            // Arrange
            float initialScore = _scoring.TotalScore;

            // Act
            _scoring.RecordBlockSuccessful();

            // Assert
            Assert.AreEqual(1, _scoring.BlocksSuccessful, "Block count should increment");
            Assert.Greater(_scoring.TotalScore, initialScore, "Score should increase from block");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_RecordCombo_AddsComboPoints()
        {
            yield return null;

            // Arrange
            float initialScore = _scoring.TotalScore;

            // Act
            _scoring.RecordComboCompleted(3); // 3-hit combo

            // Assert
            Assert.AreEqual(1, _scoring.CombosCompleted, "Combo count should increment");
            Assert.Greater(_scoring.TotalScore, initialScore, "Score should increase from combo");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_RecordCombo_IgnoresLessThan2Hits()
        {
            yield return null;

            // Act
            _scoring.RecordComboCompleted(1); // Single hit, not a combo

            // Assert
            Assert.AreEqual(0, _scoring.CombosCompleted, "Single hit should not count as combo");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_CalculateTotalScore_AppliesWeightsCorrectly()
        {
            yield return null;

            // Arrange - set known weights
            var cleanHitField = typeof(ScoringWeights).GetField("cleanHitPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cleanHitField.SetValue(_scoringWeights, 2f); // 2 points per hit

            var knockdownField = typeof(ScoringWeights).GetField("knockdownPoints",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            knockdownField.SetValue(_scoringWeights, 10f); // 10 points per knockdown

            // Act
            _scoring.RecordHitLanded(0f); // 1 hit = 2 points
            _scoring.RecordKnockdownInflicted(); // 1 knockdown = 10 points

            float calculatedScore = _scoring.CalculateTotalScore();

            // Assert
            Assert.AreEqual(12f, calculatedScore, 0.01f, "Score should be 2 + 10 = 12");
            Assert.AreEqual(12f, _scoring.TotalScore, 0.01f, "TotalScore property should match");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_ResetScore_ClearsAllStatistics()
        {
            yield return null;

            // Arrange - accumulate some stats
            _scoring.RecordHitLanded(10f);
            _scoring.RecordKnockdownInflicted();
            _scoring.RecordBlockSuccessful();
            Assert.Greater(_scoring.TotalScore, 0f, "Score should be > 0 before reset");

            // Act
            _scoring.ResetScore();

            // Assert
            Assert.AreEqual(0f, _scoring.TotalScore, "Score should be 0 after reset");
            Assert.AreEqual(0, _scoring.CleanHitsLanded, "Hits should be 0");
            Assert.AreEqual(0f, _scoring.TotalDamageDealt, "Damage should be 0");
            Assert.AreEqual(0, _scoring.KnockdownsInflicted, "Knockdowns should be 0");
            Assert.AreEqual(0, _scoring.BlocksSuccessful, "Blocks should be 0");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_OnScoreChanged_FiresWhenScoreUpdates()
        {
            yield return null;

            // Arrange
            int eventFireCount = 0;
            float lastScore = 0f;
            _scoring.OnScoreChanged += (score) =>
            {
                eventFireCount++;
                lastScore = score;
            };

            // Act
            _scoring.RecordHitLanded(5f);
            _scoring.RecordHitLanded(5f);

            // Assert
            Assert.AreEqual(2, eventFireCount, "Event should fire twice (once per hit)");
            Assert.Greater(lastScore, 0f, "Last score should be > 0");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_MultipleActions_AccumulateCorrectly()
        {
            yield return null;

            // Act - perform multiple actions
            _scoring.RecordHitLanded(10f);
            _scoring.RecordHitLanded(15f);
            _scoring.RecordBlockSuccessful();
            _scoring.RecordKnockdownInflicted();
            _scoring.RecordComboCompleted(3);

            // Assert
            Assert.AreEqual(2, _scoring.CleanHitsLanded, "Should have 2 hits");
            Assert.AreEqual(25f, _scoring.TotalDamageDealt, "Should have 25 total damage");
            Assert.AreEqual(1, _scoring.BlocksSuccessful, "Should have 1 block");
            Assert.AreEqual(1, _scoring.KnockdownsInflicted, "Should have 1 knockdown");
            Assert.AreEqual(1, _scoring.CombosCompleted, "Should have 1 combo");
            Assert.Greater(_scoring.TotalScore, 0f, "Total score should reflect all actions");
        }

        [UnityTest]
        public IEnumerator CharacterScoring_Score_NeverNegative()
        {
            yield return null;

            // Arrange - set high exhaustion penalty
            var exhaustionField = typeof(ScoringWeights).GetField("exhaustionPenalty",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            exhaustionField.SetValue(_scoringWeights, 100f); // Very high penalty

            // Get exhaustion count field via reflection to simulate exhaustion
            var exhaustionCountField = typeof(CharacterScoring).GetField("_exhaustionCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            exhaustionCountField.SetValue(_scoring, 10); // 10 exhaustions = -1000 points

            // Act
            float score = _scoring.CalculateTotalScore();

            // Assert
            Assert.GreaterOrEqual(score, 0f, "Score should never be negative, should clamp to 0");
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Systems;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Systems
{
    public class RoundManagerTests
    {
        private GameObject _roundManagerObj;
        private RoundManager _roundManager;
        private GameObject _playerObj;
        private CharacterHealth _playerHealth;
        private GameObject _aiObj;
        private CharacterHealth _aiHealth;
        private CharacterStats _testStats;

        [SetUp]
        public void SetUp()
        {
            // Create test stats
            _testStats = ScriptableObject.CreateInstance<CharacterStats>();
            var maxHealthField = typeof(CharacterStats).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxHealthField.SetValue(_testStats, 100f);

            // Create player character
            _playerObj = new GameObject("Player");
            _playerHealth = _playerObj.AddComponent<CharacterHealth>();

            var playerStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerStatsField.SetValue(_playerHealth, _testStats);

            // Create AI character
            _aiObj = new GameObject("AI");
            _aiHealth = _aiObj.AddComponent<CharacterHealth>();

            var aiStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiStatsField.SetValue(_aiHealth, _testStats);

            // Create round manager
            _roundManagerObj = new GameObject("RoundManager");
            _roundManager = _roundManagerObj.AddComponent<RoundManager>();

            // Set character references
            var playerField = typeof(RoundManager).GetField("playerHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(_roundManager, _playerHealth);

            var aiField = typeof(RoundManager).GetField("aiHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiField.SetValue(_roundManager, _aiHealth);

            // Set countdown to be faster for testing
            var countdownField = typeof(RoundManager).GetField("countdownDuration",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countdownField.SetValue(_roundManager, 1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_roundManagerObj);
            Object.DestroyImmediate(_playerObj);
            Object.DestroyImmediate(_aiObj);
            Object.DestroyImmediate(_testStats);
        }

        [UnityTest]
        public IEnumerator RoundManager_StartsInCountdownState()
        {
            // Act
            yield return null; // Let Start() run

            // Assert
            Assert.AreEqual(RoundManager.RoundState.Countdown, _roundManager.CurrentState,
                "Round manager should start in countdown state");
        }

        [UnityTest]
        public IEnumerator RoundManager_TransitionsToFightingAfterCountdown()
        {
            // Arrange
            yield return null;

            // Act - wait for countdown to finish (1 second + buffer)
            yield return new WaitForSeconds(2f);

            // Assert
            Assert.AreEqual(RoundManager.RoundState.Fighting, _roundManager.CurrentState,
                "Should transition to fighting state after countdown");
        }

        [UnityTest]
        public IEnumerator RoundManager_DetectsPlayerWin()
        {
            // Arrange
            yield return null;
            yield return new WaitForSeconds(2f); // Wait for fighting state

            bool roundEndCalled = false;
            bool playerWon = false;

            _roundManager.OnRoundEnd += (won, playerWins, aiWins) =>
            {
                roundEndCalled = true;
                playerWon = won;
            };

            // Act - kill AI
            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _aiHealth.TakeDamage(killHit);

            yield return null;

            // Assert
            Assert.IsTrue(roundEndCalled, "Round end event should be called");
            Assert.IsTrue(playerWon, "Player should have won the round");
            Assert.AreEqual(1, _roundManager.PlayerRoundWins, "Player should have 1 round win");
        }

        [UnityTest]
        public IEnumerator RoundManager_DetectsAIWin()
        {
            // Arrange
            yield return null;
            yield return new WaitForSeconds(2f);

            bool roundEndCalled = false;
            bool playerWon = true; // Expect this to be false

            _roundManager.OnRoundEnd += (won, playerWins, aiWins) =>
            {
                roundEndCalled = true;
                playerWon = won;
            };

            // Act - kill player
            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _playerHealth.TakeDamage(killHit);

            yield return null;

            // Assert
            Assert.IsTrue(roundEndCalled, "Round end event should be called");
            Assert.IsFalse(playerWon, "AI should have won the round");
            Assert.AreEqual(1, _roundManager.AIRoundWins, "AI should have 1 round win");
        }

        [UnityTest]
        public IEnumerator RoundManager_ResetsHealthBetweenRounds()
        {
            // Arrange
            yield return null;
            yield return new WaitForSeconds(2f);

            // Damage player
            var hitData = new HitData
            {
                Damage = 50f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _playerHealth.TakeDamage(hitData);

            float healthAfterDamage = _playerHealth.CurrentHealth;
            Assert.Less(healthAfterDamage, 100f, "Player should be damaged");

            // Act - kill AI to end round
            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _aiHealth.TakeDamage(killHit);

            // Wait for round over and new round start
            yield return new WaitForSeconds(3f);

            // Assert - health should be reset
            Assert.AreEqual(100f, _playerHealth.CurrentHealth, 0.1f,
                "Player health should be reset for new round");
        }

        [UnityTest]
        public IEnumerator RoundManager_EndsMatchAfterWinningRounds()
        {
            // Arrange
            yield return null;
            yield return new WaitForSeconds(2f);

            bool matchEndCalled = false;
            bool playerWonMatch = false;

            _roundManager.OnMatchEnd += (won) =>
            {
                matchEndCalled = true;
                playerWonMatch = won;
            };

            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act - win 2 rounds (default roundsToWin = 2)
            // Round 1
            _aiHealth.TakeDamage(killHit);
            yield return new WaitForSeconds(3f);

            // Round 2
            _aiHealth.TakeDamage(killHit);
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.IsTrue(matchEndCalled, "Match end event should be called");
            Assert.IsTrue(playerWonMatch, "Player should have won the match");
            Assert.AreEqual(RoundManager.RoundState.MatchOver, _roundManager.CurrentState,
                "Should be in match over state");
            Assert.IsTrue(_roundManager.IsMatchOver, "Match should be marked as over");
        }

        [UnityTest]
        public IEnumerator RoundManager_FiresCountdownTickEvents()
        {
            // Arrange
            int tickCount = 0;
            int lastTickValue = -1;

            _roundManager.OnCountdownTick += (value) =>
            {
                tickCount++;
                lastTickValue = value;
            };

            // Act
            yield return null;
            yield return new WaitForSeconds(2f);

            // Assert
            Assert.Greater(tickCount, 0, "Countdown ticks should have fired");
            Assert.AreEqual(0, lastTickValue, "Last tick should be 0 (Fight!)");
        }

        [UnityTest]
        public IEnumerator RestartMatch_ResetsAllState()
        {
            // Arrange - play through one round
            yield return null;
            yield return new WaitForSeconds(2f);

            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _aiHealth.TakeDamage(killHit);
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(1, _roundManager.PlayerRoundWins, "Player should have 1 win");

            // Act - restart match
            _roundManager.RestartMatch();
            yield return null;

            // Assert
            Assert.AreEqual(0, _roundManager.PlayerRoundWins, "Player wins should be reset");
            Assert.AreEqual(0, _roundManager.AIRoundWins, "AI wins should be reset");
            Assert.IsFalse(_roundManager.IsMatchOver, "Match should not be over");
            Assert.AreEqual(RoundManager.RoundState.Countdown, _roundManager.CurrentState,
                "Should restart in countdown state");
        }
    }
}

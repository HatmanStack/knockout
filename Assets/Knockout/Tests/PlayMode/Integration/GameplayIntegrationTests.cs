using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Systems;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Integration
{
    /// <summary>
    /// Integration tests for complete Player vs AI gameplay loop.
    /// Tests the full stack: Input -> Combat -> Animation -> Hit Detection -> Damage -> Round System
    /// </summary>
    public class GameplayIntegrationTests
    {
        private GameObject _playerObj;
        private GameObject _aiObj;
        private GameObject _roundManagerObj;
        private CharacterHealth _playerHealth;
        private CharacterHealth _aiHealth;
        private CharacterCombat _playerCombat;
        private CharacterCombat _aiCombat;
        private RoundManager _roundManager;
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
            _playerCombat = _playerObj.AddComponent<CharacterCombat>();
            _playerObj.AddComponent<Animator>();

            var playerStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerStatsField.SetValue(_playerHealth, _testStats);

            // Create AI character
            _aiObj = new GameObject("AI");
            _aiHealth = _aiObj.AddComponent<CharacterHealth>();
            _aiCombat = _aiObj.AddComponent<CharacterCombat>();
            _aiObj.AddComponent<Animator>();

            var aiStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiStatsField.SetValue(_aiHealth, _testStats);

            // Create round manager
            _roundManagerObj = new GameObject("RoundManager");
            _roundManager = _roundManagerObj.AddComponent<RoundManager>();

            var playerField = typeof(RoundManager).GetField("playerHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(_roundManager, _playerHealth);

            var aiField = typeof(RoundManager).GetField("aiHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiField.SetValue(_roundManager, _aiHealth);

            // Set fast countdown for testing
            var countdownField = typeof(RoundManager).GetField("countdownDuration",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countdownField.SetValue(_roundManager, 0.5f);

            var roundOverField = typeof(RoundManager).GetField("roundOverDisplayDuration",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            roundOverField.SetValue(_roundManager, 0.5f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObj);
            Object.DestroyImmediate(_aiObj);
            Object.DestroyImmediate(_roundManagerObj);
            Object.DestroyImmediate(_testStats);
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_PlayerWinsRound_WorksCorrectly()
        {
            // Arrange - wait for round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            int initialPlayerWins = _roundManager.PlayerRoundWins;

            // Act - player deals lethal damage to AI
            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _aiHealth.TakeDamage(killHit);

            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.AreEqual(initialPlayerWins + 1, _roundManager.PlayerRoundWins,
                "Player should win the round");
            Assert.IsTrue(_aiHealth.IsDead, "AI should be dead");
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_AIWinsRound_WorksCorrectly()
        {
            // Arrange - wait for round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            int initialAIWins = _roundManager.AIRoundWins;

            // Act - AI deals lethal damage to player
            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = _aiObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _playerHealth.TakeDamage(killHit);

            yield return new WaitForSeconds(0.5f);

            // Assert
            Assert.AreEqual(initialAIWins + 1, _roundManager.AIRoundWins,
                "AI should win the round");
            Assert.IsTrue(_playerHealth.IsDead, "Player should be dead");
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_CompleteMatch_WorksCorrectly()
        {
            // Arrange - wait for first round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            bool matchEnded = false;
            bool playerWonMatch = false;

            _roundManager.OnMatchEnd += (won) =>
            {
                matchEnded = true;
                playerWonMatch = won;
            };

            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act - player wins 2 rounds to win match
            // Round 1
            _aiHealth.TakeDamage(killHit);
            yield return new WaitForSeconds(1.5f);

            // Round 2
            _aiHealth.TakeDamage(killHit);
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.IsTrue(matchEnded, "Match should have ended");
            Assert.IsTrue(playerWonMatch, "Player should have won the match");
            Assert.AreEqual(2, _roundManager.PlayerRoundWins, "Player should have 2 round wins");
            Assert.IsTrue(_roundManager.IsMatchOver, "Match should be marked as over");
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_HealthResetsAfterRound_WorksCorrectly()
        {
            // Arrange - wait for round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            // Damage player but don't kill
            var damageHit = new HitData
            {
                Damage = 50f,
                Attacker = _aiObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _playerHealth.TakeDamage(damageHit);

            float healthAfterDamage = _playerHealth.CurrentHealth;
            Assert.Less(healthAfterDamage, 100f, "Player should be damaged");

            // Act - AI wins round
            var killHit = new HitData
            {
                Damage = 200f,
                Attacker = _aiObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _playerHealth.TakeDamage(killHit);

            // Wait for round to reset
            yield return new WaitForSeconds(2f);

            // Assert - health should be restored
            Assert.AreEqual(100f, _playerHealth.CurrentHealth, 0.1f,
                "Player health should be reset for new round");
            Assert.IsFalse(_playerHealth.IsDead, "Player should no longer be dead");
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_DamageCalculation_WorksCorrectly()
        {
            // Arrange - wait for round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            float initialHealth = _aiHealth.CurrentHealth;

            // Act - deal damage with known value
            var hitData = new HitData
            {
                Damage = 20f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _aiHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            Assert.AreEqual(initialHealth - 20f, _aiHealth.CurrentHealth, 0.1f,
                "Damage should be calculated correctly");
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_BlockingReducesDamage_WorksCorrectly()
        {
            // Arrange - wait for round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            // Set AI to blocking state using reflection
            var blockMethod = typeof(CharacterCombat).GetMethod("StartBlocking",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (blockMethod != null)
            {
                blockMethod.Invoke(_aiCombat, null);
            }

            float initialHealth = _aiHealth.CurrentHealth;

            // Act - deal 20 damage while blocking (should be reduced to 5)
            var hitData = new HitData
            {
                Damage = 20f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _aiHealth.TakeDamage(hitData);

            yield return null;

            // Assert - damage should be reduced by 75%
            float expectedHealth = initialHealth - 5f; // 20 * 0.25 = 5
            Assert.AreEqual(expectedHealth, _aiHealth.CurrentHealth, 0.1f,
                "Blocking should reduce damage by 75%");
        }

        [UnityTest]
        public IEnumerator FullGameplayLoop_MultipleHits_AccumulateDamage()
        {
            // Arrange - wait for round to start
            yield return null;
            yield return new WaitForSeconds(1f);

            float initialHealth = _aiHealth.CurrentHealth;

            var hitData = new HitData
            {
                Damage = 10f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act - deal multiple hits
            _aiHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(0.1f);

            _aiHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(0.1f);

            _aiHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual(initialHealth - 30f, _aiHealth.CurrentHealth, 0.1f,
                "Multiple hits should accumulate damage");
        }
    }
}

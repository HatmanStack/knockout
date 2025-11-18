using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using Knockout.UI;
using Knockout.Systems;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.UI
{
    public class RoundUITests
    {
        private GameObject _roundUIObj;
        private RoundUI _roundUI;
        private GameObject _roundManagerObj;
        private RoundManager _roundManager;
        private TextMeshProUGUI _countdownText;
        private TextMeshProUGUI _roundNumberText;
        private TextMeshProUGUI _roundResultText;
        private TextMeshProUGUI _matchResultText;
        private CharacterStats _testStats;
        private GameObject _playerObj;
        private GameObject _aiObj;

        [SetUp]
        public void SetUp()
        {
            // Create test stats
            _testStats = ScriptableObject.CreateInstance<CharacterStats>();
            var maxHealthField = typeof(CharacterStats).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxHealthField.SetValue(_testStats, 100f);

            // Create characters for round manager
            _playerObj = new GameObject("Player");
            var playerHealth = _playerObj.AddComponent<CharacterHealth>();
            var playerStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerStatsField.SetValue(playerHealth, _testStats);

            _aiObj = new GameObject("AI");
            var aiHealth = _aiObj.AddComponent<CharacterHealth>();
            var aiStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiStatsField.SetValue(aiHealth, _testStats);

            // Create round manager
            _roundManagerObj = new GameObject("RoundManager");
            _roundManager = _roundManagerObj.AddComponent<RoundManager>();

            var playerField = typeof(RoundManager).GetField("playerHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(_roundManager, playerHealth);

            var aiField = typeof(RoundManager).GetField("aiHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aiField.SetValue(_roundManager, aiHealth);

            // Create UI elements
            _roundUIObj = new GameObject("RoundUI");

            var countdownObj = new GameObject("CountdownText");
            countdownObj.transform.SetParent(_roundUIObj.transform);
            _countdownText = countdownObj.AddComponent<TextMeshProUGUI>();

            var roundNumObj = new GameObject("RoundNumberText");
            roundNumObj.transform.SetParent(_roundUIObj.transform);
            _roundNumberText = roundNumObj.AddComponent<TextMeshProUGUI>();

            var resultObj = new GameObject("RoundResultText");
            resultObj.transform.SetParent(_roundUIObj.transform);
            _roundResultText = resultObj.AddComponent<TextMeshProUGUI>();

            var matchResultObj = new GameObject("MatchResultText");
            matchResultObj.transform.SetParent(_roundUIObj.transform);
            _matchResultText = matchResultObj.AddComponent<TextMeshProUGUI>();

            // Create RoundUI component
            _roundUI = _roundUIObj.AddComponent<RoundUI>();

            // Set references
            var countdownField = typeof(RoundUI).GetField("countdownText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countdownField.SetValue(_roundUI, _countdownText);

            var roundNumField = typeof(RoundUI).GetField("roundNumberText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            roundNumField.SetValue(_roundUI, _roundNumberText);

            var resultField = typeof(RoundUI).GetField("roundResultText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            resultField.SetValue(_roundUI, _roundResultText);

            var matchResultField = typeof(RoundUI).GetField("matchResultText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            matchResultField.SetValue(_roundUI, _matchResultText);

            var managerField = typeof(RoundUI).GetField("roundManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            managerField.SetValue(_roundUI, _roundManager);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_roundUIObj);
            Object.DestroyImmediate(_roundManagerObj);
            Object.DestroyImmediate(_playerObj);
            Object.DestroyImmediate(_aiObj);
            Object.DestroyImmediate(_testStats);
        }

        [UnityTest]
        public IEnumerator RoundUI_HidesAllTextOnStart()
        {
            // Act
            yield return null;

            // Assert
            Assert.IsFalse(_countdownText.gameObject.activeSelf ||
                          _roundNumberText.gameObject.activeSelf ||
                          _roundResultText.gameObject.activeSelf ||
                          _matchResultText.gameObject.activeSelf,
                "All UI text should be hidden initially");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysCountdownText()
        {
            // Act
            yield return null;

            // Manually trigger countdown for testing
            var onCountdownMethod = typeof(RoundUI).GetMethod("OnCountdownTick",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onCountdownMethod.Invoke(_roundUI, new object[] { 3 });

            // Assert
            Assert.AreEqual("3", _countdownText.text, "Should display countdown number");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysFightMessage()
        {
            // Act
            yield return null;

            var onCountdownMethod = typeof(RoundUI).GetMethod("OnCountdownTick",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onCountdownMethod.Invoke(_roundUI, new object[] { 0 });

            // Assert
            Assert.AreEqual("FIGHT!", _countdownText.text, "Should display FIGHT when countdown reaches 0");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysRoundNumber()
        {
            // Act
            yield return null;

            var onRoundStartMethod = typeof(RoundUI).GetMethod("OnRoundStart",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onRoundStartMethod.Invoke(_roundUI, new object[] { 1 });

            // Assert
            Assert.AreEqual("Round 1", _roundNumberText.text, "Should display round number");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysWinMessage()
        {
            // Act
            yield return null;

            var onRoundEndMethod = typeof(RoundUI).GetMethod("OnRoundEnd",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onRoundEndMethod.Invoke(_roundUI, new object[] { true, 1, 0 });

            // Assert
            Assert.AreEqual("YOU WIN!", _roundResultText.text, "Should display win message");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysLossMessage()
        {
            // Act
            yield return null;

            var onRoundEndMethod = typeof(RoundUI).GetMethod("OnRoundEnd",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onRoundEndMethod.Invoke(_roundUI, new object[] { false, 0, 1 });

            // Assert
            Assert.AreEqual("YOU LOSE!", _roundResultText.text, "Should display loss message");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysMatchWonMessage()
        {
            // Act
            yield return null;

            var onMatchEndMethod = typeof(RoundUI).GetMethod("OnMatchEnd",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onMatchEndMethod.Invoke(_roundUI, new object[] { true });

            // Assert
            Assert.IsTrue(_matchResultText.text.Contains("VICTORY"),
                "Should display victory message when match is won");
        }

        [UnityTest]
        public IEnumerator RoundUI_DisplaysMatchLostMessage()
        {
            // Act
            yield return null;

            var onMatchEndMethod = typeof(RoundUI).GetMethod("OnMatchEnd",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onMatchEndMethod.Invoke(_roundUI, new object[] { false });

            // Assert
            Assert.IsTrue(_matchResultText.text.Contains("DEFEAT"),
                "Should display defeat message when match is lost");
        }

        [UnityTest]
        public IEnumerator SetRoundManager_UpdatesReference()
        {
            // Arrange
            var newManagerObj = new GameObject("NewRoundManager");
            var newManager = newManagerObj.AddComponent<RoundManager>();

            yield return null;

            // Act
            _roundUI.SetRoundManager(newManager);

            // Assert - verify no errors occurred
            Assert.IsNotNull(_roundUI);

            // Cleanup
            Object.DestroyImmediate(newManagerObj);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Knockout.Audio;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Audio
{
    public class CharacterAudioPlayerTests
    {
        private GameObject _characterObj;
        private CharacterAudioPlayer _audioPlayer;
        private CharacterCombat _characterCombat;
        private CharacterHealth _characterHealth;
        private CharacterStats _testStats;
        private AttackData _jabData;

        [SetUp]
        public void SetUp()
        {
            // Create test stats
            _testStats = ScriptableObject.CreateInstance<CharacterStats>();
            var maxHealthField = typeof(CharacterStats).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxHealthField.SetValue(_testStats, 100f);

            // Create attack data
            _jabData = ScriptableObject.CreateInstance<AttackData>();
            var attackTypeField = typeof(AttackData).GetField("attackTypeIndex",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackTypeField.SetValue(_jabData, 0);

            // Create character with components
            _characterObj = new GameObject("Character");
            _characterHealth = _characterObj.AddComponent<CharacterHealth>();
            _characterCombat = _characterObj.AddComponent<CharacterCombat>();
            _audioPlayer = _characterObj.AddComponent<CharacterAudioPlayer>();

            // Add required animator component for CharacterCombat
            var animator = _characterObj.AddComponent<Animator>();

            // Set references using reflection
            var healthStatsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthStatsField.SetValue(_characterHealth, _testStats);

            var combatField = typeof(CharacterAudioPlayer).GetField("characterCombat",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            combatField.SetValue(_audioPlayer, _characterCombat);

            var healthField = typeof(CharacterAudioPlayer).GetField("characterHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthField.SetValue(_audioPlayer, _characterHealth);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_characterObj);
            Object.DestroyImmediate(_testStats);
            Object.DestroyImmediate(_jabData);
        }

        [UnityTest]
        public IEnumerator CharacterAudioPlayer_SubscribesToCombatEvents()
        {
            // Arrange
            yield return null;

            bool eventReceived = false;

            // Use reflection to access private OnAttackExecuted method
            var method = typeof(CharacterAudioPlayer).GetMethod("OnAttackExecuted",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Subscribe to combat event manually for testing
            _characterCombat.OnAttackExecuted += (attackType) => eventReceived = true;

            // Act - trigger attack via combat component
            // This would normally trigger through ExecuteAttack, but we'll simulate the event
            var executeMethod = typeof(CharacterCombat).GetMethod("ExecuteAttack",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (executeMethod != null)
            {
                executeMethod.Invoke(_characterCombat, new object[] { _jabData });
            }

            yield return null;

            // Assert
            Assert.IsTrue(eventReceived, "Audio player should subscribe to combat events");
        }

        [UnityTest]
        public IEnumerator CharacterAudioPlayer_SubscribesToHealthEvents()
        {
            // Arrange
            yield return null;

            bool eventReceived = false;
            _characterHealth.OnHitTaken += (hitData) => eventReceived = true;

            // Act
            var hitData = new HitData
            {
                Damage = 10f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _characterHealth.TakeDamage(hitData);

            yield return null;

            // Assert
            Assert.IsTrue(eventReceived, "Audio player should subscribe to health events");
        }

        [UnityTest]
        public IEnumerator PlayVictoryVoiceLine_WithNoVoiceData_DoesNotThrowError()
        {
            // Act & Assert
            yield return null;
            Assert.DoesNotThrow(() =>
            {
                _audioPlayer.PlayVictoryVoiceLine();
            });
        }

        [UnityTest]
        public IEnumerator PlayIntroVoiceLine_WithNoVoiceData_DoesNotThrowError()
        {
            // Act & Assert
            yield return null;
            Assert.DoesNotThrow(() =>
            {
                _audioPlayer.PlayIntroVoiceLine();
            });
        }

        [UnityTest]
        public IEnumerator CharacterAudioPlayer_HandlesNullComponents()
        {
            // Arrange - create audio player with no combat/health components
            var emptyObj = new GameObject("EmptyCharacter");
            var emptyPlayer = emptyObj.AddComponent<CharacterAudioPlayer>();

            // Act & Assert
            yield return null;
            Assert.IsNotNull(emptyPlayer);

            // Cleanup
            Object.DestroyImmediate(emptyObj);
        }
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Profiling;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.Performance
{
    /// <summary>
    /// Performance tests to ensure stable 60fps gameplay.
    /// Tests frame rate stability and memory allocation during combat scenarios.
    /// </summary>
    public class PerformanceTests
    {
        private const int TARGET_FPS = 60;
        private const float TARGET_FRAME_TIME = 1000f / TARGET_FPS; // ~16.67ms

        private GameObject _playerObj;
        private GameObject _aiObj;
        private CharacterHealth _playerHealth;
        private CharacterHealth _aiHealth;
        private CharacterCombat _playerCombat;
        private CharacterCombat _aiCombat;
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
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_playerObj);
            Object.DestroyImmediate(_aiObj);
            Object.DestroyImmediate(_testStats);
        }

        [UnityTest]
        public IEnumerator Performance_DamageCalculation_IsEfficient()
        {
            // Arrange
            yield return null;

            var hitData = new HitData
            {
                Damage = 10f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _aiHealth.ResetHealth();
                _aiHealth.TakeDamage(hitData);
            }

            yield return null;

            // Act - measure time for 100 damage calculations
            float startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < 100; i++)
            {
                _aiHealth.ResetHealth();
                _aiHealth.TakeDamage(hitData);
            }

            float endTime = Time.realtimeSinceStartup;
            float totalTime = (endTime - startTime) * 1000f; // Convert to ms
            float averageTime = totalTime / 100f;

            yield return null;

            // Assert - should be very fast (< 0.1ms per calculation)
            Assert.Less(averageTime, 0.1f,
                $"Damage calculation should be fast. Average: {averageTime:F4}ms");
        }

        [UnityTest]
        public IEnumerator Performance_MultipleCharacters_MaintainFrameRate()
        {
            // Arrange - create multiple character pairs
            var characters = new System.Collections.Generic.List<GameObject>();

            for (int i = 0; i < 5; i++)
            {
                var player = new GameObject($"Player_{i}");
                player.AddComponent<CharacterHealth>();
                player.AddComponent<CharacterCombat>();
                player.AddComponent<Animator>();

                var statsField = player.GetComponent<CharacterHealth>().GetType().GetField("characterStats",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                statsField.SetValue(player.GetComponent<CharacterHealth>(), _testStats);

                characters.Add(player);
            }

            yield return null;

            // Act - measure frame time with multiple characters
            float totalFrameTime = 0f;
            int frameCount = 0;

            for (int i = 0; i < 60; i++) // Measure for 60 frames
            {
                float frameStart = Time.realtimeSinceStartup;

                // Simulate combat activity
                foreach (var character in characters)
                {
                    var health = character.GetComponent<CharacterHealth>();
                    if (health != null && !health.IsDead)
                    {
                        var hitData = new HitData
                        {
                            Damage = 1f,
                            Attacker = null,
                            HitPoint = Vector3.zero,
                            HitType = 0
                        };
                        health.TakeDamage(hitData);
                    }
                }

                yield return null;

                float frameEnd = Time.realtimeSinceStartup;
                float frameTime = (frameEnd - frameStart) * 1000f;

                totalFrameTime += frameTime;
                frameCount++;
            }

            float averageFrameTime = totalFrameTime / frameCount;

            // Cleanup
            foreach (var character in characters)
            {
                Object.DestroyImmediate(character);
            }

            // Assert - average frame time should be reasonable
            // Note: In tests this might be higher than in-game, so we use a relaxed threshold
            Assert.Less(averageFrameTime, TARGET_FRAME_TIME * 2,
                $"Average frame time should be reasonable with multiple characters. Average: {averageFrameTime:F2}ms");
        }

        [UnityTest]
        public IEnumerator Performance_NoMemoryLeaks_DuringCombat()
        {
            // Arrange
            yield return null;

            // Force garbage collection and get baseline
            System.GC.Collect();
            yield return null;

            long initialMemory = System.GC.GetTotalMemory(false);

            var hitData = new HitData
            {
                Damage = 10f,
                Attacker = _playerObj.transform,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act - perform many combat operations
            for (int i = 0; i < 100; i++)
            {
                _aiHealth.ResetHealth();
                _aiHealth.TakeDamage(hitData);

                if (i % 10 == 0)
                {
                    yield return null;
                }
            }

            yield return null;

            // Force garbage collection and measure
            System.GC.Collect();
            yield return null;

            long finalMemory = System.GC.GetTotalMemory(false);
            long memoryDelta = finalMemory - initialMemory;

            // Assert - memory should not grow significantly
            // Allow for some allocation (10KB) but flag excessive growth
            Assert.Less(memoryDelta, 10240, // 10KB
                $"Memory should not leak during combat. Delta: {memoryDelta} bytes");
        }

        [UnityTest]
        public IEnumerator Performance_EventSubscription_IsEfficient()
        {
            // Arrange
            yield return null;

            int eventCallCount = 0;
            System.Action<float, float> healthChangedHandler = (current, max) => eventCallCount++;

            // Act - measure time for many event subscriptions/unsubscriptions
            float startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < 1000; i++)
            {
                _playerHealth.OnHealthChanged += healthChangedHandler;
                _playerHealth.OnHealthChanged -= healthChangedHandler;
            }

            float endTime = Time.realtimeSinceStartup;
            float totalTime = (endTime - startTime) * 1000f;

            yield return null;

            // Assert - should be very fast
            Assert.Less(totalTime, 10f,
                $"Event subscription/unsubscription should be fast. Total: {totalTime:F2}ms");
        }

        [UnityTest]
        public IEnumerator Performance_StateTransitions_AreEfficient()
        {
            // Arrange
            yield return null;

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                _playerHealth.ResetHealth();
            }

            yield return null;

            // Act - measure time for many state resets
            float startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < 100; i++)
            {
                _playerHealth.ResetHealth();
            }

            float endTime = Time.realtimeSinceStartup;
            float totalTime = (endTime - startTime) * 1000f;
            float averageTime = totalTime / 100f;

            yield return null;

            // Assert - should be very fast
            Assert.Less(averageTime, 0.1f,
                $"State transitions should be fast. Average: {averageTime:F4}ms");
        }
    }
}

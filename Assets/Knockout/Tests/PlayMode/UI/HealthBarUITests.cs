using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using Knockout.UI;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Combat.HitDetection;

namespace Knockout.Tests.PlayMode.UI
{
    public class HealthBarUITests
    {
        private GameObject _healthBarObj;
        private HealthBarUI _healthBarUI;
        private GameObject _characterObj;
        private CharacterHealth _characterHealth;
        private Image _fillImage;
        private Image _backgroundImage;
        private CharacterStats _testStats;

        [SetUp]
        public void SetUp()
        {
            // Create test character stats
            _testStats = ScriptableObject.CreateInstance<CharacterStats>();
            var maxHealthField = typeof(CharacterStats).GetField("maxHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            maxHealthField.SetValue(_testStats, 100f);

            // Create character with health component
            _characterObj = new GameObject("TestCharacter");
            _characterHealth = _characterObj.AddComponent<CharacterHealth>();

            var statsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statsField.SetValue(_characterHealth, _testStats);

            // Create UI components
            _healthBarObj = new GameObject("HealthBar");
            var canvas = _healthBarObj.AddComponent<Canvas>();

            // Create fill image
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(_healthBarObj.transform);
            _fillImage = fillObj.AddComponent<Image>();
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Horizontal;

            // Create background image
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(_healthBarObj.transform);
            _backgroundImage = bgObj.AddComponent<Image>();

            // Create HealthBarUI component
            _healthBarUI = _healthBarObj.AddComponent<HealthBarUI>();

            // Set references using reflection
            var fillField = typeof(HealthBarUI).GetField("healthBarFill",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillField.SetValue(_healthBarUI, _fillImage);

            var bgField = typeof(HealthBarUI).GetField("healthBarBackground",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bgField.SetValue(_healthBarUI, _backgroundImage);

            var charHealthField = typeof(HealthBarUI).GetField("characterHealth",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            charHealthField.SetValue(_healthBarUI, _characterHealth);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_healthBarObj);
            Object.DestroyImmediate(_characterObj);
            Object.DestroyImmediate(_testStats);
        }

        [UnityTest]
        public IEnumerator HealthBar_InitializesToFullHealth()
        {
            // Act - let Start() run
            yield return null;

            // Assert
            Assert.AreEqual(1f, _fillImage.fillAmount, 0.01f,
                "Health bar should start at full");
        }

        [UnityTest]
        public IEnumerator HealthBar_UpdatesWhenDamageTaken()
        {
            // Arrange
            yield return null; // Let Start() run

            var hitData = new HitData
            {
                Damage = 50f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act
            _characterHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(1f); // Wait for animation

            // Assert - should be at 50% health
            Assert.AreEqual(0.5f, _fillImage.fillAmount, 0.1f,
                "Health bar should update to 50% after taking 50 damage");
        }

        [UnityTest]
        public IEnumerator HealthBar_ColorChangesBasedOnHealth()
        {
            // Arrange
            yield return null;

            // Get color fields
            var healthyColorField = typeof(HealthBarUI).GetField("healthyColor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var criticalColorField = typeof(HealthBarUI).GetField("criticalColor",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Color healthyColor = (Color)healthyColorField.GetValue(_healthBarUI);
            Color criticalColor = (Color)criticalColorField.GetValue(_healthBarUI);

            Color initialColor = _fillImage.color;

            // Act - take heavy damage (80 damage = 20% health remaining)
            var hitData = new HitData
            {
                Damage = 80f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            _characterHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(1f);

            Color lowHealthColor = _fillImage.color;

            // Assert
            Assert.AreNotEqual(initialColor, lowHealthColor,
                "Health bar color should change when health is low");

            // Verify color is closer to critical than healthy
            float distanceToCritical = Vector4.Distance(lowHealthColor, criticalColor);
            float distanceToHealthy = Vector4.Distance(lowHealthColor, healthyColor);
            Assert.Less(distanceToCritical, distanceToHealthy,
                "Low health color should be closer to critical color");
        }

        [UnityTest]
        public IEnumerator HealthBar_AnimatesDepletion()
        {
            // Arrange
            yield return null;

            var hitData = new HitData
            {
                Damage = 50f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act
            _characterHealth.TakeDamage(hitData);

            // Sample fill amount during animation
            yield return new WaitForSeconds(0.1f);
            float midAnimationFill = _fillImage.fillAmount;

            yield return new WaitForSeconds(1f);
            float finalFill = _fillImage.fillAmount;

            // Assert - should have animated from 1.0 to 0.5
            Assert.Greater(midAnimationFill, 0.5f,
                "Health bar should be animating (not instantly at final value)");
            Assert.Less(midAnimationFill, 1f,
                "Health bar should have started depleting");
            Assert.AreEqual(0.5f, finalFill, 0.05f,
                "Health bar should reach final value after animation");
        }

        [UnityTest]
        public IEnumerator SetCharacterHealth_UpdatesReference()
        {
            // Arrange
            yield return null;

            var newCharacter = new GameObject("NewCharacter");
            var newHealth = newCharacter.AddComponent<CharacterHealth>();

            var statsField = typeof(CharacterHealth).GetField("characterStats",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            statsField.SetValue(newHealth, _testStats);

            // Act
            _healthBarUI.SetCharacterHealth(newHealth);
            yield return null;

            // Damage new character
            var hitData = new HitData
            {
                Damage = 30f,
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };
            newHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(1f);

            // Assert - health bar should update for new character
            Assert.AreEqual(0.7f, _fillImage.fillAmount, 0.1f,
                "Health bar should track new character's health");

            // Cleanup
            Object.DestroyImmediate(newCharacter);
        }

        [UnityTest]
        public IEnumerator HealthBar_HandlesZeroHealth()
        {
            // Arrange
            yield return null;

            var hitData = new HitData
            {
                Damage = 200f, // More than max health
                Attacker = null,
                HitPoint = Vector3.zero,
                HitType = 0
            };

            // Act
            _characterHealth.TakeDamage(hitData);
            yield return new WaitForSeconds(1f);

            // Assert
            Assert.AreEqual(0f, _fillImage.fillAmount, 0.01f,
                "Health bar should show empty when health reaches zero");
        }
    }
}

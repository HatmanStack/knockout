using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Knockout.UI;
using Knockout.Characters.Components;
using Knockout.Characters.Data;

namespace Knockout.Tests.PlayMode.UI
{
    /// <summary>
    /// PlayMode tests for StaminaBarUI component.
    /// Tests contextual display, visual updates, and event subscriptions.
    /// </summary>
    public class StaminaBarUITests
    {
        private GameObject _testCharacter;
        private GameObject _uiGameObject;
        private CharacterStamina _characterStamina;
        private StaminaBarUI _staminaBarUI;
        private Image _fillImage;
        private Image _backgroundImage;
        private GameObject _rootGameObject;
        private StaminaData _testStaminaData;

        [SetUp]
        public void SetUp()
        {
            // Create test stamina data
            _testStaminaData = ScriptableObject.CreateInstance<StaminaData>();

            // Create test character with stamina component
            _testCharacter = new GameObject("TestCharacter");
            _characterStamina = _testCharacter.AddComponent<CharacterStamina>();

            // Use reflection to set stamina data (private field)
            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField?.SetValue(_characterStamina, _testStaminaData);

            // Initialize stamina
            _characterStamina.Initialize();

            // Create UI hierarchy
            _rootGameObject = new GameObject("StaminaBarRoot");
            _uiGameObject = new GameObject("StaminaBarUI");
            _uiGameObject.transform.SetParent(_rootGameObject.transform);

            // Create UI images
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(_uiGameObject.transform);
            _fillImage = fillObj.AddComponent<Image>();
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Horizontal;

            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(_uiGameObject.transform);
            _backgroundImage = bgObj.AddComponent<Image>();

            // Create StaminaBarUI component
            _staminaBarUI = _uiGameObject.AddComponent<StaminaBarUI>();

            // Set references using reflection (since fields are private SerializeField)
            SetPrivateField(_staminaBarUI, "staminaBarFill", _fillImage);
            SetPrivateField(_staminaBarUI, "staminaBarBackground", _backgroundImage);
            SetPrivateField(_staminaBarUI, "staminaBarRoot", _rootGameObject);
            SetPrivateField(_staminaBarUI, "characterStamina", _characterStamina);
            SetPrivateField(_staminaBarUI, "hideWhenFull", true);

            // Trigger Start() manually
            _staminaBarUI.SendMessage("Start");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testCharacter);
            Object.DestroyImmediate(_rootGameObject);
            Object.DestroyImmediate(_testStaminaData);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator BarHiddenWhenStaminaFull()
        {
            // Stamina starts at full
            Assert.IsFalse(_rootGameObject.activeSelf, "Stamina bar should be hidden when at full stamina");
            yield return null;
        }

        [UnityTest]
        public IEnumerator BarAppearsWhenStaminaDepletes()
        {
            // Consume some stamina
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_characterStamina, new object[] { 20f });

            // Wait a frame for UI to update
            yield return null;

            // Bar should now be visible
            Assert.IsTrue(_rootGameObject.activeSelf, "Stamina bar should appear when stamina depletes");
        }

        [UnityTest]
        public IEnumerator BarFillUpdatesOnStaminaChange()
        {
            // Consume half stamina
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_characterStamina, new object[] { 50f });

            // Wait for animation to complete
            yield return new WaitForSeconds(0.5f);

            // Fill should be approximately 50%
            Assert.AreEqual(0.5f, _fillImage.fillAmount, 0.1f, "Fill amount should match stamina percentage");
        }

        [UnityTest]
        public IEnumerator ColorChangesBasedOnStaminaPercentage()
        {
            // High stamina (>50%) - should be green
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            consumeMethod?.Invoke(_characterStamina, new object[] { 10f }); // 90% stamina
            yield return new WaitForSeconds(0.3f);

            Color highColor = _fillImage.color;
            Assert.Greater(highColor.g, 0.5f, "High stamina should have green tint");

            // Medium stamina (25-50%) - should be yellow-ish
            consumeMethod?.Invoke(_characterStamina, new object[] { 50f }); // 40% stamina
            yield return new WaitForSeconds(0.3f);

            Color mediumColor = _fillImage.color;
            Assert.Greater(mediumColor.r + mediumColor.g, mediumColor.b * 2, "Medium stamina should have yellow tint");

            // Low stamina (<25%) - should be red
            consumeMethod?.Invoke(_characterStamina, new object[] { 30f }); // 10% stamina
            yield return new WaitForSeconds(0.3f);

            Color lowColor = _fillImage.color;
            Assert.Greater(lowColor.r, lowColor.g, "Low stamina should have red tint");
        }

        [UnityTest]
        public IEnumerator SetCharacterStaminaUpdatesReference()
        {
            // Create new stamina component
            var newCharacter = new GameObject("NewCharacter");
            var newStamina = newCharacter.AddComponent<CharacterStamina>();

            var staminaDataField = typeof(CharacterStamina).GetField("staminaData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            staminaDataField?.SetValue(newStamina, _testStaminaData);
            newStamina.Initialize();

            // Set new reference
            _staminaBarUI.SetCharacterStamina(newStamina);

            yield return null;

            // Consume stamina on new character
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(newStamina, new object[] { 30f });

            yield return new WaitForSeconds(0.3f);

            // UI should update based on new character
            Assert.Less(_fillImage.fillAmount, 1f, "UI should update for new character");

            Object.DestroyImmediate(newCharacter);
        }

        [UnityTest]
        public IEnumerator DepletionFlashTriggersOnZeroStamina()
        {
            // Get original background color
            Color originalColor = _backgroundImage.color;

            // Deplete all stamina
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_characterStamina, new object[] { 100f });

            // Wait a tiny bit for flash to start
            yield return new WaitForSeconds(0.05f);

            // Background color should have changed (flash effect)
            Assert.AreNotEqual(originalColor, _backgroundImage.color, "Flash effect should change background color");

            // Wait for flash to complete
            yield return new WaitForSeconds(0.5f);

            // Color should return to original (approximately)
            Assert.AreEqual(originalColor.r, _backgroundImage.color.r, 0.1f, "Flash should return to original color");
        }
    }
}

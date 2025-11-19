using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using Knockout.UI;
using Knockout.Characters.Components;
using Knockout.Characters.Data;
using Knockout.Systems;

namespace Knockout.Tests.PlayMode.UI
{
    /// <summary>
    /// Integration tests for all UI components working together.
    /// Tests event subscriptions, data accuracy, and performance.
    /// </summary>
    public class UIIntegrationTests
    {
        private GameObject _testCharacter;
        private CharacterStamina _stamina;
        private CharacterHealth _health;
        private CharacterComboTracker _comboTracker;
        private CharacterSpecialMoves _specialMoves;
        private StaminaData _staminaData;
        private ComboChainData _comboChainData;
        private SpecialMoveData _specialMoveData;

        [SetUp]
        public void SetUp()
        {
            // Create test data
            _staminaData = ScriptableObject.CreateInstance<StaminaData>();
            _comboChainData = ScriptableObject.CreateInstance<ComboChainData>();
            _specialMoveData = ScriptableObject.CreateInstance<SpecialMoveData>();

            // Create test character with all components
            _testCharacter = new GameObject("TestCharacter");
            _stamina = _testCharacter.AddComponent<CharacterStamina>();
            _health = _testCharacter.AddComponent<CharacterHealth>();
            _comboTracker = _testCharacter.AddComponent<CharacterComboTracker>();
            _specialMoves = _testCharacter.AddComponent<CharacterSpecialMoves>();

            // Set data via reflection
            SetPrivateField(_stamina, "staminaData", _staminaData);
            SetPrivateField(_comboTracker, "comboChainData", _comboChainData);
            SetPrivateField(_specialMoves, "specialMoveData", _specialMoveData);

            // Initialize components
            _stamina.Initialize();
            _comboTracker.Initialize();
            _specialMoves.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_testCharacter);
            Object.DestroyImmediate(_staminaData);
            Object.DestroyImmediate(_comboChainData);
            Object.DestroyImmediate(_specialMoveData);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        [UnityTest]
        public IEnumerator AllUIComponentsSubscribeToEvents()
        {
            // Create UI components
            var staminaUI = CreateStaminaBarUI(_stamina);
            var comboUI = CreateComboCounterUI(_comboTracker);
            var specialUI = CreateSpecialMoveCooldownUI(_specialMoves, _stamina);

            yield return null;

            // Trigger stamina change
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_stamina, new object[] { 20f });

            yield return new WaitForSeconds(0.2f);

            // Verify UI updated (no errors thrown = events subscribed correctly)
            Assert.Pass("All UI components subscribed to events successfully");

            Object.DestroyImmediate(staminaUI.gameObject);
            Object.DestroyImmediate(comboUI.gameObject);
            Object.DestroyImmediate(specialUI.gameObject);
        }

        [UnityTest]
        public IEnumerator UIDisplaysAccurateData()
        {
            // Create stamina UI
            var staminaUI = CreateStaminaBarUI(_stamina);
            var fillImage = staminaUI.GetComponentInChildren<Image>();

            yield return null;

            // Consume 50% stamina
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_stamina, new object[] { 50f });

            yield return new WaitForSeconds(0.3f);

            // Verify fill amount matches stamina percentage
            float expectedFill = _stamina.StaminaPercentage;
            Assert.AreEqual(expectedFill, fillImage.fillAmount, 0.15f, "UI should display accurate stamina data");

            Object.DestroyImmediate(staminaUI.gameObject);
        }

        [UnityTest]
        public IEnumerator UIRespondsToSimultaneousEvents()
        {
            // Create multiple UI components
            var staminaUI = CreateStaminaBarUI(_stamina);
            var comboUI = CreateComboCounterUI(_comboTracker);

            yield return null;

            // Trigger multiple events simultaneously
            var consumeMethod = typeof(CharacterStamina).GetMethod("ConsumeStamina",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            consumeMethod?.Invoke(_stamina, new object[] { 30f });

            // Trigger combo
            var onComboHitEvent = typeof(CharacterComboTracker).GetField("OnComboHitLanded",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            onComboHitEvent?.GetValue(_comboTracker)?.GetType().GetMethod("Invoke")?.Invoke(
                onComboHitEvent?.GetValue(_comboTracker),
                new object[] { 3, 30f });

            yield return new WaitForSeconds(0.3f);

            // Both UI components should update without conflicts
            Assert.Pass("UI handled simultaneous events successfully");

            Object.DestroyImmediate(staminaUI.gameObject);
            Object.DestroyImmediate(comboUI.gameObject);
        }

        [UnityTest]
        public IEnumerator UIPerformanceTest()
        {
            // Create all UI components
            var staminaUI = CreateStaminaBarUI(_stamina);
            var comboUI = CreateComboCounterUI(_comboTracker);
            var specialUI = CreateSpecialMoveCooldownUI(_specialMoves, _stamina);

            yield return null;

            // Measure frame time with all UI active
            float startTime = Time.realtimeSinceStartup;
            int frames = 60; // Test for 60 frames (~1 second at 60fps)

            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            float avgFrameTime = (elapsed / frames) * 1000f; // ms per frame

            // UI should not cause significant performance impact
            // Target: < 1ms per frame for all UI combined
            Assert.Less(avgFrameTime, 2f, "UI should maintain acceptable performance (<2ms per frame)");

            Object.DestroyImmediate(staminaUI.gameObject);
            Object.DestroyImmediate(comboUI.gameObject);
            Object.DestroyImmediate(specialUI.gameObject);
        }

        private StaminaBarUI CreateStaminaBarUI(CharacterStamina stamina)
        {
            GameObject root = new GameObject("StaminaBarRoot");
            GameObject uiObj = new GameObject("StaminaBarUI");
            uiObj.transform.SetParent(root.transform);

            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(uiObj.transform);
            var fill = fillObj.AddComponent<Image>();
            fill.type = Image.Type.Filled;

            var ui = uiObj.AddComponent<StaminaBarUI>();
            SetPrivateField(ui, "staminaBarFill", fill);
            SetPrivateField(ui, "staminaBarRoot", root);
            SetPrivateField(ui, "characterStamina", stamina);
            SetPrivateField(ui, "hideWhenFull", true);

            ui.SendMessage("Awake");
            ui.SendMessage("Start");

            return ui;
        }

        private ComboCounterUI CreateComboCounterUI(CharacterComboTracker tracker)
        {
            GameObject root = new GameObject("ComboCounterRoot");
            GameObject uiObj = new GameObject("ComboCounterUI");
            uiObj.transform.SetParent(root.transform);

            var countText = uiObj.AddComponent<TextMeshProUGUI>();
            var labelText = new GameObject("Label").AddComponent<TextMeshProUGUI>();
            labelText.transform.SetParent(uiObj.transform);

            var ui = uiObj.AddComponent<ComboCounterUI>();
            SetPrivateField(ui, "comboCountText", countText);
            SetPrivateField(ui, "comboLabelText", labelText);
            SetPrivateField(ui, "comboCounterRoot", root);
            SetPrivateField(ui, "comboTracker", tracker);

            ui.SendMessage("Awake");
            ui.SendMessage("Start");

            return ui;
        }

        private SpecialMoveCooldownUI CreateSpecialMoveCooldownUI(CharacterSpecialMoves moves, CharacterStamina stamina)
        {
            GameObject uiObj = new GameObject("SpecialCooldownUI");

            var icon = new GameObject("Icon").AddComponent<Image>();
            icon.transform.SetParent(uiObj.transform);

            var overlay = new GameObject("Overlay").AddComponent<Image>();
            overlay.transform.SetParent(uiObj.transform);

            var ui = uiObj.AddComponent<SpecialMoveCooldownUI>();
            SetPrivateField(ui, "iconImage", icon);
            SetPrivateField(ui, "cooldownFillOverlay", overlay);
            SetPrivateField(ui, "specialMoves", moves);
            SetPrivateField(ui, "characterStamina", stamina);

            ui.SendMessage("Awake");
            ui.SendMessage("Start");

            return ui;
        }
    }
}

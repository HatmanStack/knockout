using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Knockout.Characters.Components;
using Knockout.AI;

namespace Knockout.UI
{
    /// <summary>
    /// Training mode settings UI for toggles and controls.
    /// Allows unlimited resources, damage display, AI behavior selection, and position reset.
    /// </summary>
    public class TrainingModeUI : MonoBehaviour
    {
        [Header("Character References")]
        [SerializeField] [Tooltip("Player character")]
        private GameObject playerCharacter;

        [SerializeField] [Tooltip("Dummy/opponent character")]
        private GameObject dummyCharacter;

        [Header("UI References")]
        [SerializeField] [Tooltip("Toggle for unlimited stamina")]
        private Toggle unlimitedStaminaToggle;

        [SerializeField] [Tooltip("Toggle for unlimited health")]
        private Toggle unlimitedHealthToggle;

        [SerializeField] [Tooltip("Toggle for damage numbers display")]
        private Toggle showDamageNumbersToggle;

        [SerializeField] [Tooltip("Dropdown or text for dummy AI behavior")]
        private TMP_Dropdown dummyBehaviorDropdown;

        [SerializeField] [Tooltip("Button to reset positions")]
        private Button resetButton;

        [SerializeField] [Tooltip("Root panel for training settings (can be toggled)")]
        private GameObject settingsPanelRoot;

        [SerializeField] [Tooltip("Toggle key to show/hide settings panel")]
        private KeyCode togglePanelKey = KeyCode.Tab;

        [Header("Damage Numbers Reference")]
        [SerializeField] [Tooltip("Damage numbers UI component (optional)")]
        private DamageNumbersUI damageNumbersUI;

        // Component references
        private CharacterStamina _playerStamina;
        private CharacterStamina _dummyStamina;
        private CharacterHealth _playerHealth;
        private CharacterHealth _dummyHealth;
        private DummyAI _dummyAI;

        // Training state
        private bool _unlimitedStamina;
        private bool _unlimitedHealth;
        private Vector3 _playerStartPosition;
        private Quaternion _playerStartRotation;
        private Vector3 _dummyStartPosition;
        private Quaternion _dummyStartRotation;

        private void Awake()
        {
            ValidateReferences();
            CacheComponentReferences();
            StoreStartingPositions();

            // Hide settings panel initially
            if (settingsPanelRoot != null)
            {
                settingsPanelRoot.SetActive(false);
            }
        }

        private void Start()
        {
            SetupUIEvents();

            // Initialize damage numbers toggle
            if (showDamageNumbersToggle != null && damageNumbersUI != null)
            {
                showDamageNumbersToggle.isOn = false;
                damageNumbersUI.SetEnabled(false);
            }
        }

        private void Update()
        {
            // Handle settings panel toggle
            if (Input.GetKeyDown(togglePanelKey) && settingsPanelRoot != null)
            {
                settingsPanelRoot.SetActive(!settingsPanelRoot.activeSelf);
            }

            // Apply unlimited resources
            ApplyUnlimitedResources();
        }

        private void OnDestroy()
        {
            // Unsubscribe from UI events
            if (unlimitedStaminaToggle != null)
            {
                unlimitedStaminaToggle.onValueChanged.RemoveListener(OnUnlimitedStaminaToggled);
            }

            if (unlimitedHealthToggle != null)
            {
                unlimitedHealthToggle.onValueChanged.RemoveListener(OnUnlimitedHealthToggled);
            }

            if (showDamageNumbersToggle != null)
            {
                showDamageNumbersToggle.onValueChanged.RemoveListener(OnShowDamageNumbersToggled);
            }

            if (dummyBehaviorDropdown != null)
            {
                dummyBehaviorDropdown.onValueChanged.RemoveListener(OnDummyBehaviorChanged);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(OnResetButtonClicked);
            }
        }

        private void ValidateReferences()
        {
            if (playerCharacter == null)
            {
                Debug.LogWarning("TrainingModeUI: Player character reference is missing!", this);
            }

            if (dummyCharacter == null)
            {
                Debug.LogWarning("TrainingModeUI: Dummy character reference is missing!", this);
            }

            if (settingsPanelRoot == null)
            {
                Debug.LogWarning("TrainingModeUI: Settings panel root is missing!", this);
            }
        }

        private void CacheComponentReferences()
        {
            if (playerCharacter != null)
            {
                _playerStamina = playerCharacter.GetComponent<CharacterStamina>();
                _playerHealth = playerCharacter.GetComponent<CharacterHealth>();
            }

            if (dummyCharacter != null)
            {
                _dummyStamina = dummyCharacter.GetComponent<CharacterStamina>();
                _dummyHealth = dummyCharacter.GetComponent<CharacterHealth>();
                _dummyAI = dummyCharacter.GetComponent<DummyAI>();
            }
        }

        private void StoreStartingPositions()
        {
            if (playerCharacter != null)
            {
                _playerStartPosition = playerCharacter.transform.position;
                _playerStartRotation = playerCharacter.transform.rotation;
            }

            if (dummyCharacter != null)
            {
                _dummyStartPosition = dummyCharacter.transform.position;
                _dummyStartRotation = dummyCharacter.transform.rotation;
            }
        }

        private void SetupUIEvents()
        {
            if (unlimitedStaminaToggle != null)
            {
                unlimitedStaminaToggle.onValueChanged.AddListener(OnUnlimitedStaminaToggled);
            }

            if (unlimitedHealthToggle != null)
            {
                unlimitedHealthToggle.onValueChanged.AddListener(OnUnlimitedHealthToggled);
            }

            if (showDamageNumbersToggle != null)
            {
                showDamageNumbersToggle.onValueChanged.AddListener(OnShowDamageNumbersToggled);
            }

            if (dummyBehaviorDropdown != null)
            {
                dummyBehaviorDropdown.onValueChanged.AddListener(OnDummyBehaviorChanged);

                // Populate dropdown options
                dummyBehaviorDropdown.ClearOptions();
                dummyBehaviorDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Passive",
                    "Blocking",
                    "Dodging",
                    "Counter"
                });
                dummyBehaviorDropdown.value = 0; // Default to Passive
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetButtonClicked);
            }
        }

        /// <summary>
        /// Called when unlimited stamina toggle changes.
        /// </summary>
        private void OnUnlimitedStaminaToggled(bool isOn)
        {
            _unlimitedStamina = isOn;
        }

        /// <summary>
        /// Called when unlimited health toggle changes.
        /// </summary>
        private void OnUnlimitedHealthToggled(bool isOn)
        {
            _unlimitedHealth = isOn;
        }

        /// <summary>
        /// Called when show damage numbers toggle changes.
        /// </summary>
        private void OnShowDamageNumbersToggled(bool isOn)
        {
            if (damageNumbersUI != null)
            {
                damageNumbersUI.SetEnabled(isOn);
            }
        }

        /// <summary>
        /// Called when dummy behavior dropdown changes.
        /// </summary>
        private void OnDummyBehaviorChanged(int behaviorIndex)
        {
            if (_dummyAI == null)
            {
                Debug.LogWarning("TrainingModeUI: Dummy AI component not found!");
                return;
            }

            DummyAI.DummyBehavior behavior = (DummyAI.DummyBehavior)behaviorIndex;
            _dummyAI.SetBehavior(behavior);
        }

        /// <summary>
        /// Called when reset button is clicked.
        /// </summary>
        private void OnResetButtonClicked()
        {
            ResetCharacters();
        }

        /// <summary>
        /// Applies unlimited resources (stamina and health).
        /// </summary>
        private void ApplyUnlimitedResources()
        {
            // Unlimited stamina
            if (_unlimitedStamina)
            {
                if (_playerStamina != null)
                {
                    // Restore stamina to max (use reflection to call private/public method)
                    var restoreMethod = typeof(CharacterStamina).GetMethod("RestoreStamina",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (restoreMethod != null)
                    {
                        restoreMethod.Invoke(_playerStamina, new object[] { 1000f });
                    }
                }

                if (_dummyStamina != null)
                {
                    var restoreMethod = typeof(CharacterStamina).GetMethod("RestoreStamina",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (restoreMethod != null)
                    {
                        restoreMethod.Invoke(_dummyStamina, new object[] { 1000f });
                    }
                }
            }

            // Unlimited health
            if (_unlimitedHealth)
            {
                if (_playerHealth != null && _playerHealth.CurrentHealth < _playerHealth.MaxHealth)
                {
                    _playerHealth.ResetHealth();
                }

                if (_dummyHealth != null && _dummyHealth.CurrentHealth < _dummyHealth.MaxHealth)
                {
                    _dummyHealth.ResetHealth();
                }
            }
        }

        /// <summary>
        /// Resets characters to starting positions and states.
        /// </summary>
        private void ResetCharacters()
        {
            // Reset player
            if (playerCharacter != null)
            {
                playerCharacter.transform.position = _playerStartPosition;
                playerCharacter.transform.rotation = _playerStartRotation;

                if (_playerHealth != null)
                {
                    _playerHealth.ResetHealth();
                }

                if (_playerStamina != null)
                {
                    // Reset stamina via reflection
                    var field = typeof(CharacterStamina).GetField("_currentStamina",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(_playerStamina, _playerStamina.MaxStamina);
                    }
                }

                // Reset combo if exists
                var comboTracker = playerCharacter.GetComponent<CharacterComboTracker>();
                if (comboTracker != null)
                {
                    comboTracker.ResetCombo();
                }
            }

            // Reset dummy
            if (dummyCharacter != null)
            {
                dummyCharacter.transform.position = _dummyStartPosition;
                dummyCharacter.transform.rotation = _dummyStartRotation;

                if (_dummyHealth != null)
                {
                    _dummyHealth.ResetHealth();
                }

                if (_dummyStamina != null)
                {
                    var field = typeof(CharacterStamina).GetField("_currentStamina",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(_dummyStamina, _dummyStamina.MaxStamina);
                    }
                }

                // Reset combo if exists
                var comboTracker = dummyCharacter.GetComponent<CharacterComboTracker>();
                if (comboTracker != null)
                {
                    comboTracker.ResetCombo();
                }
            }

            Debug.Log("Training Mode: Characters reset to starting positions");
        }

        /// <summary>
        /// Public method to set character references at runtime.
        /// </summary>
        public void SetCharacters(GameObject player, GameObject dummy)
        {
            playerCharacter = player;
            dummyCharacter = dummy;

            CacheComponentReferences();
            StoreStartingPositions();
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (playerCharacter == null || dummyCharacter == null)
            {
                Debug.LogWarning("TrainingModeUI: Character references should be assigned!", this);
            }
        }
        #endif
    }
}

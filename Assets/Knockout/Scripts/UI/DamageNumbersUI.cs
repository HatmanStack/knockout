using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Knockout.Characters.Components;

namespace Knockout.UI
{
    /// <summary>
    /// Displays floating damage numbers on hits (training mode feature).
    /// Shows damage values with color coding and float-up animation.
    /// </summary>
    public class DamageNumbersUI : MonoBehaviour
    {
        [Header("Damage Number Prefab")]
        [SerializeField] [Tooltip("Prefab for damage number (TextMeshProUGUI)")]
        private GameObject damageNumberPrefab;

        [Header("Character References")]
        [SerializeField] [Tooltip("Player character combat component")]
        private CharacterCombat playerCombat;

        [SerializeField] [Tooltip("Opponent character combat component")]
        private CharacterCombat opponentCombat;

        [Header("Spawn Settings")]
        [SerializeField] [Tooltip("Vertical offset from hit position")]
        [Range(0f, 2f)]
        private float spawnHeightOffset = 1f;

        [SerializeField] [Tooltip("Random horizontal spread")]
        [Range(0f, 1f)]
        private float randomSpread = 0.3f;

        [Header("Animation Settings")]
        [SerializeField] [Tooltip("Float up speed (units per second)")]
        [Range(0.5f, 5f)]
        private float floatSpeed = 2f;

        [SerializeField] [Tooltip("Animation duration (seconds)")]
        [Range(0.5f, 3f)]
        private float animationDuration = 1.5f;

        [SerializeField] [Tooltip("Scale on spawn (punch effect)")]
        [Range(1f, 3f)]
        private float spawnScale = 1.5f;

        [SerializeField] [Tooltip("Scale animation duration")]
        [Range(0.1f, 0.5f)]
        private float scaleAnimationDuration = 0.2f;

        [Header("Color Settings")]
        [SerializeField] [Tooltip("Color for normal hits")]
        private Color normalHitColor = Color.white;

        [SerializeField] [Tooltip("Color for combo hits")]
        private Color comboHitColor = Color.yellow;

        [SerializeField] [Tooltip("Color for special move hits")]
        private Color specialMoveColor = Color.red;

        [SerializeField] [Tooltip("Color for critical/heavy hits")]
        private Color criticalHitColor = new Color(1f, 0.5f, 0f); // Orange

        [Header("Pooling Settings")]
        [SerializeField] [Tooltip("Initial pool size")]
        [Range(5, 50)]
        private int poolSize = 20;

        // Internal state
        private bool _isEnabled = false;
        private Queue<GameObject> _damageNumberPool;
        private List<GameObject> _activeDamageNumbers;
        private Canvas _worldCanvas;

        private void Awake()
        {
            ValidateReferences();
            SetupPool();
            SetupWorldCanvas();

            _activeDamageNumbers = new List<GameObject>();
        }

        private void Start()
        {
            // Don't subscribe to events until enabled
            // Events will be subscribed when SetEnabled(true) is called
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();

            // Clean up pool
            if (_damageNumberPool != null)
            {
                foreach (var obj in _damageNumberPool)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            // Clean up active numbers
            if (_activeDamageNumbers != null)
            {
                foreach (var obj in _activeDamageNumbers)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            if (_worldCanvas != null)
            {
                Destroy(_worldCanvas.gameObject);
            }
        }

        private void ValidateReferences()
        {
            if (damageNumberPrefab == null)
            {
                Debug.LogWarning("DamageNumbersUI: Damage number prefab is missing!", this);
            }
        }

        private void SetupPool()
        {
            _damageNumberPool = new Queue<GameObject>();

            if (damageNumberPrefab == null)
            {
                return;
            }

            // Pre-create pool objects
            for (int i = 0; i < poolSize; i++)
            {
                GameObject numberObj = CreateDamageNumberObject();
                numberObj.SetActive(false);
                _damageNumberPool.Enqueue(numberObj);
            }
        }

        private void SetupWorldCanvas()
        {
            // Create world space canvas for damage numbers
            GameObject canvasObj = new GameObject("DamageNumbers_WorldCanvas");
            _worldCanvas = canvasObj.AddComponent<Canvas>();
            _worldCanvas.renderMode = RenderMode.WorldSpace;

            // Add canvas scaler for consistent sizing
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            // Find main camera for billboard effect
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                _worldCanvas.worldCamera = mainCam;
            }

            canvasObj.transform.SetParent(transform);
        }

        private GameObject CreateDamageNumberObject()
        {
            if (damageNumberPrefab != null)
            {
                GameObject numberObj = Instantiate(damageNumberPrefab, _worldCanvas.transform);
                return numberObj;
            }
            else
            {
                // Fallback: create simple text object
                GameObject numberObj = new GameObject("DamageNumber");
                numberObj.transform.SetParent(_worldCanvas.transform);
                TextMeshProUGUI text = numberObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 36;
                text.alignment = TextAlignmentOptions.Center;
                text.enableWordWrapping = false;
                return numberObj;
            }
        }

        /// <summary>
        /// Subscribes to combat events.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (playerCombat != null)
            {
                playerCombat.OnHitLanded += OnPlayerHitLanded;
            }

            if (opponentCombat != null)
            {
                opponentCombat.OnHitLanded += OnOpponentHitLanded;
            }
        }

        /// <summary>
        /// Unsubscribes from combat events.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (playerCombat != null)
            {
                playerCombat.OnHitLanded -= OnPlayerHitLanded;
            }

            if (opponentCombat != null)
            {
                opponentCombat.OnHitLanded -= OnOpponentHitLanded;
            }
        }

        /// <summary>
        /// Called when player lands a hit.
        /// </summary>
        private void OnPlayerHitLanded(CharacterHealth targetHealth, float damage, Vector3 hitPosition)
        {
            SpawnDamageNumber(damage, hitPosition, DamageType.Normal);
        }

        /// <summary>
        /// Called when opponent lands a hit.
        /// </summary>
        private void OnOpponentHitLanded(CharacterHealth targetHealth, float damage, Vector3 hitPosition)
        {
            SpawnDamageNumber(damage, hitPosition, DamageType.Normal);
        }

        /// <summary>
        /// Spawns a damage number at the specified position.
        /// </summary>
        private void SpawnDamageNumber(float damage, Vector3 worldPosition, DamageType damageType)
        {
            if (!_isEnabled)
            {
                return;
            }

            // Get damage number from pool
            GameObject numberObj = GetDamageNumberFromPool();
            if (numberObj == null)
            {
                return;
            }

            // Position with offset and random spread
            Vector3 spawnPosition = worldPosition + Vector3.up * spawnHeightOffset;
            spawnPosition += new Vector3(
                Random.Range(-randomSpread, randomSpread),
                0f,
                Random.Range(-randomSpread, randomSpread)
            );

            numberObj.transform.position = spawnPosition;
            numberObj.transform.rotation = Quaternion.identity;

            // Setup text
            TextMeshProUGUI textComponent = numberObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = Mathf.RoundToInt(damage).ToString();
                textComponent.color = GetColorForDamageType(damageType);
                textComponent.alpha = 1f;
            }

            // Activate and animate
            numberObj.SetActive(true);
            _activeDamageNumbers.Add(numberObj);

            StartCoroutine(AnimateDamageNumber(numberObj));
        }

        /// <summary>
        /// Gets a damage number from the pool.
        /// </summary>
        private GameObject GetDamageNumberFromPool()
        {
            if (_damageNumberPool.Count > 0)
            {
                return _damageNumberPool.Dequeue();
            }
            else
            {
                // Pool exhausted, create new one
                return CreateDamageNumberObject();
            }
        }

        /// <summary>
        /// Returns a damage number to the pool.
        /// </summary>
        private void ReturnDamageNumberToPool(GameObject numberObj)
        {
            if (numberObj == null)
            {
                return;
            }

            numberObj.SetActive(false);
            _activeDamageNumbers.Remove(numberObj);
            _damageNumberPool.Enqueue(numberObj);
        }

        /// <summary>
        /// Animates a damage number (float up and fade).
        /// </summary>
        private IEnumerator AnimateDamageNumber(GameObject numberObj)
        {
            TextMeshProUGUI textComponent = numberObj.GetComponent<TextMeshProUGUI>();
            Vector3 startPosition = numberObj.transform.position;
            Vector3 endPosition = startPosition + Vector3.up * (floatSpeed * animationDuration);
            Color startColor = textComponent != null ? textComponent.color : Color.white;

            float elapsed = 0f;

            // Scale punch animation
            if (textComponent != null)
            {
                float scaleElapsed = 0f;
                while (scaleElapsed < scaleAnimationDuration)
                {
                    scaleElapsed += Time.deltaTime;
                    float t = scaleElapsed / scaleAnimationDuration;
                    float scale = Mathf.Lerp(spawnScale, 1f, t);
                    numberObj.transform.localScale = Vector3.one * scale;
                    yield return null;
                }
                numberObj.transform.localScale = Vector3.one;
            }

            // Float and fade
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;

                // Move up
                numberObj.transform.position = Vector3.Lerp(startPosition, endPosition, t);

                // Fade out
                if (textComponent != null)
                {
                    Color color = startColor;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    textComponent.color = color;
                }

                // Billboard effect (face camera)
                if (Camera.main != null)
                {
                    numberObj.transform.rotation = Quaternion.LookRotation(
                        numberObj.transform.position - Camera.main.transform.position
                    );
                }

                yield return null;
            }

            // Return to pool
            ReturnDamageNumberToPool(numberObj);
        }

        /// <summary>
        /// Gets color for damage type.
        /// </summary>
        private Color GetColorForDamageType(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Combo:
                    return comboHitColor;
                case DamageType.SpecialMove:
                    return specialMoveColor;
                case DamageType.Critical:
                    return criticalHitColor;
                default:
                    return normalHitColor;
            }
        }

        /// <summary>
        /// Enables or disables damage number display.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled)
            {
                return;
            }

            _isEnabled = enabled;

            if (_isEnabled)
            {
                SubscribeToEvents();
            }
            else
            {
                UnsubscribeFromEvents();

                // Clear all active damage numbers
                foreach (var numberObj in _activeDamageNumbers.ToArray())
                {
                    ReturnDamageNumberToPool(numberObj);
                }
            }
        }

        /// <summary>
        /// Sets combat component references at runtime.
        /// </summary>
        public void SetCombatReferences(CharacterCombat player, CharacterCombat opponent)
        {
            UnsubscribeFromEvents();

            playerCombat = player;
            opponentCombat = opponent;

            if (_isEnabled)
            {
                SubscribeToEvents();
            }
        }

        /// <summary>
        /// Damage type enum for color coding.
        /// </summary>
        public enum DamageType
        {
            Normal,
            Combo,
            SpecialMove,
            Critical
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (damageNumberPrefab == null)
            {
                Debug.LogWarning("DamageNumbersUI: Damage number prefab should be assigned!", this);
            }
        }
        #endif
    }
}

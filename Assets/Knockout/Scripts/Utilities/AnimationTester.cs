using UnityEngine;
using Knockout.Characters.Components;

namespace Knockout.Utilities
{
    /// <summary>
    /// Debug utility for testing character animations via keyboard input.
    /// Attach to character prefab during development to manually test animations.
    ///
    /// Controls:
    /// - WASD: Movement
    /// - 1: Jab
    /// - 2: Hook
    /// - 3: Uppercut
    /// - LeftShift: Block
    /// - 4: Light Hit Reaction
    /// - 5: Medium Hit Reaction
    /// - 6: Heavy Hit Reaction
    /// - 7: Knockdown
    /// - 8: Knockout
    /// </summary>
    public class AnimationTester : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("Character animator component (auto-assigned if not set)")]
        private CharacterAnimator characterAnimator;

        private Vector2 _moveInput;

        #region Unity Lifecycle

        private void Start()
        {
            if (characterAnimator == null)
            {
                characterAnimator = GetComponent<CharacterAnimator>();
            }

            if (characterAnimator == null)
            {
                Debug.LogError("[AnimationTester] CharacterAnimator component not found! " +
                    "Make sure this script is attached to a GameObject with a CharacterAnimator component.", this);
            }
        }

        private void Update()
        {
            if (characterAnimator == null) return;

            HandleMovementInput();
            HandleAttackInput();
            HandleDefenseInput();
            HandleReactionInput();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 450));

            GUILayout.Label("Animation Tester Controls", GUI.skin.box);

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("<b>Movement:</b>", GUI.skin.label);
            GUILayout.Label("  WASD - Move");
            GUILayout.Space(5);

            GUILayout.Label("<b>Attacks:</b>", GUI.skin.label);
            GUILayout.Label("  1 - Jab");
            GUILayout.Label("  2 - Hook");
            GUILayout.Label("  3 - Uppercut");
            GUILayout.Space(5);

            GUILayout.Label("<b>Defense:</b>", GUI.skin.label);
            GUILayout.Label("  LeftShift - Block (hold)");
            GUILayout.Space(5);

            GUILayout.Label("<b>Hit Reactions:</b>", GUI.skin.label);
            GUILayout.Label("  4 - Light Hit");
            GUILayout.Label("  5 - Medium Hit");
            GUILayout.Label("  6 - Heavy Hit");
            GUILayout.Label("  7 - Knockdown");
            GUILayout.Label("  8 - Knockout");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion

        #region Input Handling

        private void HandleMovementInput()
        {
            _moveInput.x = 0f;
            _moveInput.y = 0f;

            if (Input.GetKey(KeyCode.D)) _moveInput.x = 1f;
            else if (Input.GetKey(KeyCode.A)) _moveInput.x = -1f;

            if (Input.GetKey(KeyCode.W)) _moveInput.y = 1f;
            else if (Input.GetKey(KeyCode.S)) _moveInput.y = -1f;

            float speed = _moveInput.magnitude;
            characterAnimator.SetMovement(_moveInput, speed);
        }

        private void HandleAttackInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[AnimationTester] Testing Jab animation");
                characterAnimator.TriggerJab();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("[AnimationTester] Testing Hook animation");
                characterAnimator.TriggerHook();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("[AnimationTester] Testing Uppercut animation");
                characterAnimator.TriggerUppercut();
            }
        }

        private void HandleDefenseInput()
        {
            bool blocking = Input.GetKey(KeyCode.LeftShift);
            characterAnimator.SetBlocking(blocking);
        }

        private void HandleReactionInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("[AnimationTester] Testing Light Hit Reaction");
                characterAnimator.TriggerHitReaction(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Debug.Log("[AnimationTester] Testing Medium Hit Reaction");
                characterAnimator.TriggerHitReaction(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Debug.Log("[AnimationTester] Testing Heavy Hit Reaction");
                characterAnimator.TriggerHitReaction(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Debug.Log("[AnimationTester] Testing Knockdown");
                characterAnimator.TriggerKnockdown();
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                Debug.Log("[AnimationTester] Testing Knockout");
                characterAnimator.TriggerKnockout();
            }
        }

        #endregion
    }
}

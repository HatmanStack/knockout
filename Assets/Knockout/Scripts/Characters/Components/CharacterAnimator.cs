using System;
using UnityEngine;

namespace Knockout.Characters.Components
{
    /// <summary>
    /// Manages character animations and provides a clean API for controlling the Animator.
    /// This component wraps Unity's Animator and exposes methods for locomotion, attacks,
    /// defense, and hit reactions. It also receives Animation Events and raises C# events
    /// for other systems to subscribe to.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        /// <summary>
        /// Centralized animator parameter names to avoid string typos.
        /// </summary>
        public static class AnimatorParams
        {
            // Locomotion parameters
            public const string MoveSpeed = "MoveSpeed";
            public const string MoveDirectionX = "MoveDirectionX";
            public const string MoveDirectionY = "MoveDirectionY";

            // Attack parameters
            public const string AttackTrigger = "AttackTrigger";
            public const string AttackType = "AttackType";
            public const string IsBlocking = "IsBlocking";
            public const string UpperBodyWeight = "UpperBodyWeight";

            // Hit reaction parameters
            public const string HitReaction = "HitReaction";
            public const string HitType = "HitType";
            public const string KnockedDown = "KnockedDown";
            public const string KnockedOut = "KnockedOut";
            public const string OverrideWeight = "OverrideWeight";
        }

        #region Events

        // Attack animation events
        public event Action OnAttackStart;
        public event Action OnHitboxActivate;
        public event Action OnHitboxDeactivate;
        public event Action OnAttackRecoveryStart;
        public event Action OnAttackEnd;

        // Hit reaction events
        public event Action OnHitReactionEnd;
        public event Action OnKnockedDownComplete;
        public event Action OnGetUpComplete;

        #endregion

        #region Private Fields

        private Animator _animator;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError($"[{gameObject.name}] CharacterAnimator requires an Animator component!", this);
            }
        }

        private void OnValidate()
        {
            // Cache animator reference in editor for validation
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        #endregion

        #region Locomotion Methods

        /// <summary>
        /// Sets the movement parameters for locomotion animation.
        /// </summary>
        /// <param name="direction">Movement direction (-1 to 1 on X and Y axes)</param>
        /// <param name="speed">Movement speed (0 to 1)</param>
        public void SetMovement(Vector2 direction, float speed)
        {
            if (_animator == null) return;

            _animator.SetFloat(AnimatorParams.MoveDirectionX, direction.x);
            _animator.SetFloat(AnimatorParams.MoveDirectionY, direction.y);
            _animator.SetFloat(AnimatorParams.MoveSpeed, speed);
        }

        #endregion

        #region Attack Methods

        /// <summary>
        /// Triggers an attack animation.
        /// </summary>
        /// <param name="attackType">0 = Jab, 1 = Hook, 2 = Uppercut</param>
        public void TriggerAttack(int attackType)
        {
            if (_animator == null) return;

            _animator.SetInteger(AnimatorParams.AttackType, attackType);
            _animator.SetTrigger(AnimatorParams.AttackTrigger);
        }

        /// <summary>
        /// Triggers a jab attack animation.
        /// </summary>
        public void TriggerJab()
        {
            TriggerAttack(0);
        }

        /// <summary>
        /// Triggers a hook attack animation.
        /// </summary>
        public void TriggerHook()
        {
            TriggerAttack(1);
        }

        /// <summary>
        /// Triggers an uppercut attack animation.
        /// </summary>
        public void TriggerUppercut()
        {
            TriggerAttack(2);
        }

        #endregion

        #region Defense Methods

        /// <summary>
        /// Sets the blocking state.
        /// </summary>
        /// <param name="isBlocking">True to start blocking, false to stop</param>
        public void SetBlocking(bool isBlocking)
        {
            if (_animator == null) return;

            _animator.SetBool(AnimatorParams.IsBlocking, isBlocking);
        }

        #endregion

        #region Hit Reaction Methods

        /// <summary>
        /// Triggers a hit reaction animation.
        /// </summary>
        /// <param name="hitType">0 = Light, 1 = Medium, 2 = Heavy</param>
        public void TriggerHitReaction(int hitType)
        {
            if (_animator == null) return;

            _animator.SetInteger(AnimatorParams.HitType, hitType);
            _animator.SetTrigger(AnimatorParams.HitReaction);
            SetOverrideLayerWeight(1.0f);
        }

        /// <summary>
        /// Triggers a knockdown animation.
        /// </summary>
        public void TriggerKnockdown()
        {
            if (_animator == null) return;

            _animator.SetBool(AnimatorParams.KnockedDown, true);
            SetOverrideLayerWeight(1.0f);
        }

        /// <summary>
        /// Triggers a knockout animation.
        /// </summary>
        public void TriggerKnockout()
        {
            if (_animator == null) return;

            _animator.SetBool(AnimatorParams.KnockedOut, true);
            SetOverrideLayerWeight(1.0f);
        }

        #endregion

        #region Layer Weight Control

        /// <summary>
        /// Sets the weight of the Upper Body animation layer.
        /// </summary>
        /// <param name="weight">Weight value (0 to 1)</param>
        public void SetUpperBodyLayerWeight(float weight)
        {
            if (_animator == null) return;

            _animator.SetFloat(AnimatorParams.UpperBodyWeight, weight);
            _animator.SetLayerWeight(1, weight); // Layer index 1 is UpperBody
        }

        /// <summary>
        /// Sets the weight of the Full Body Override animation layer.
        /// </summary>
        /// <param name="weight">Weight value (0 to 1)</param>
        public void SetOverrideLayerWeight(float weight)
        {
            if (_animator == null) return;

            _animator.SetFloat(AnimatorParams.OverrideWeight, weight);
            _animator.SetLayerWeight(2, weight); // Layer index 2 is FullBodyOverride
        }

        #endregion

        #region Animation Event Receivers

        // These methods are called by Unity Animation Events
        // They must be public for Unity to find them

        /// <summary>
        /// Animation Event: Called at the start of an attack animation.
        /// </summary>
        public void AnimEvent_OnAttackStart()
        {
            OnAttackStart?.Invoke();
        }

        /// <summary>
        /// Animation Event: Called when the hitbox should become active.
        /// </summary>
        public void AnimEvent_OnHitboxActivate()
        {
            OnHitboxActivate?.Invoke();
        }

        /// <summary>
        /// Animation Event: Called when the hitbox should become inactive.
        /// </summary>
        public void AnimEvent_OnHitboxDeactivate()
        {
            OnHitboxDeactivate?.Invoke();
        }

        /// <summary>
        /// Animation Event: Called when attack recovery phase starts.
        /// </summary>
        public void AnimEvent_OnAttackRecoveryStart()
        {
            OnAttackRecoveryStart?.Invoke();
        }

        /// <summary>
        /// Animation Event: Called when an attack animation completes.
        /// </summary>
        public void AnimEvent_OnAttackEnd()
        {
            OnAttackEnd?.Invoke();
            SetUpperBodyLayerWeight(0f); // Reset upper body layer
        }

        /// <summary>
        /// Animation Event: Called when a hit reaction animation completes.
        /// </summary>
        public void AnimEvent_OnHitReactionEnd()
        {
            OnHitReactionEnd?.Invoke();
            SetOverrideLayerWeight(0f); // Reset override layer
        }

        /// <summary>
        /// Animation Event: Called when a knockdown animation completes.
        /// </summary>
        public void AnimEvent_OnKnockedDownComplete()
        {
            OnKnockedDownComplete?.Invoke();
        }

        /// <summary>
        /// Animation Event: Called when get-up animation completes.
        /// </summary>
        public void AnimEvent_OnGetUpComplete()
        {
            OnGetUpComplete?.Invoke();

            if (_animator != null)
            {
                _animator.SetBool(AnimatorParams.KnockedDown, false);
            }

            SetOverrideLayerWeight(0f); // Reset override layer
        }

        #endregion
    }
}

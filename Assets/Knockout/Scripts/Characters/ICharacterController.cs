using UnityEngine;

namespace Knockout.Characters
{
    /// <summary>
    /// Interface for character control systems.
    /// Allows swapping between different control mechanisms (player input, AI, physics-based).
    ///
    /// Future Use Cases:
    /// - Physics-based AI control (Facebook Research paper implementation)
    /// - Network-controlled characters (multiplayer)
    /// - Replay system (recorded input playback)
    /// - Training/tutorial system (scripted movements)
    ///
    /// Implementation Examples:
    /// - PlayerController: Uses CharacterInput for user input
    /// - AIController: Uses CharacterAI for decision-making
    /// - PhysicsController: Uses physics simulation for control (future)
    /// - NetworkController: Receives input from network (future)
    /// </summary>
    public interface ICharacterController
    {
        /// <summary>
        /// Gets whether the controller is currently active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Gets the character's transform.
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Initializes the character controller.
        /// Called once when the character is spawned or scene starts.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Updates the character control logic.
        /// Called every frame when controller is active.
        /// </summary>
        void UpdateControl();

        /// <summary>
        /// Fixed update for physics-based control.
        /// Called at fixed timestep when controller is active.
        /// </summary>
        void FixedUpdateControl();

        /// <summary>
        /// Enables the character controller.
        /// Starts processing input or AI decisions.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables the character controller.
        /// Stops processing input or AI decisions.
        /// </summary>
        void Disable();

        /// <summary>
        /// Executes a movement command.
        /// </summary>
        /// <param name="direction">Movement direction (normalized Vector2)</param>
        void Move(Vector2 direction);

        /// <summary>
        /// Executes a jab attack.
        /// </summary>
        /// <returns>True if attack was executed successfully</returns>
        bool ExecuteJab();

        /// <summary>
        /// Executes a hook attack.
        /// </summary>
        /// <returns>True if attack was executed successfully</returns>
        bool ExecuteHook();

        /// <summary>
        /// Executes an uppercut attack.
        /// </summary>
        /// <returns>True if attack was executed successfully</returns>
        bool ExecuteUppercut();

        /// <summary>
        /// Starts blocking.
        /// </summary>
        void StartBlock();

        /// <summary>
        /// Stops blocking.
        /// </summary>
        void StopBlock();

        /// <summary>
        /// Resets the character to initial state.
        /// Used for round resets.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Example implementation notes for future physics-based controller:
    ///
    /// public class PhysicsBasedController : MonoBehaviour, ICharacterController
    /// {
    ///     // Physics simulation references
    ///     private ArticulationBody[] _joints;
    ///     private PDController[] _pdControllers;
    ///
    ///     // Target pose from animation
    ///     private Pose _targetPose;
    ///
    ///     public void FixedUpdateControl()
    ///     {
    ///         // Compute torques using PD controllers
    ///         for (int i = 0; i < _joints.Length; i++)
    ///         {
    ///             Vector3 torque = _pdControllers[i].ComputeTorque(
    ///                 _joints[i].transform.rotation,
    ///                 _targetPose.rotations[i]
    ///             );
    ///             _joints[i].AddTorque(torque);
    ///         }
    ///     }
    ///
    ///     public bool ExecuteJab()
    ///     {
    ///         // Set target pose to jab animation pose
    ///         _targetPose = GetAnimationPose("Jab");
    ///         return true;
    ///     }
    /// }
    ///
    /// See Facebook Research paper for full physics-based control implementation:
    /// https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/
    /// </summary>
}

using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
namespace Knockout.Editor
{
    /// <summary>
    /// Editor script to automatically generate the Input Actions asset for Phase 1.
    /// Run this via Tools > Knockout > Generate Input Actions menu item.
    /// </summary>
    public static class InputActionsGenerator
    {
        [MenuItem("Tools/Knockout/Generate Input Actions")]
        public static void GenerateInputActions()
        {
            Debug.Log("[InputActionsGenerator] Creating Input Actions asset...");

            string assetPath = "Assets/Knockout/Scripts/Input/KnockoutInputActions.inputactions";

            // Create new InputActionAsset
            var inputActions = ScriptableObject.CreateInstance<InputActionAsset>();

            // Create Gameplay Action Map
            var gameplayMap = inputActions.AddActionMap("Gameplay");

            // Movement Action (Vector2)
            var movementAction = gameplayMap.AddAction("Movement", InputActionType.Value);
            movementAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            movementAction.AddBinding("<Gamepad>/leftStick");

            // Jab Action (Button)
            var jabAction = gameplayMap.AddAction("Jab", InputActionType.Button);
            jabAction.AddBinding("<Mouse>/leftButton");
            jabAction.AddBinding("<Gamepad>/buttonWest"); // X on Xbox, Square on PS

            // Hook Action (Button)
            var hookAction = gameplayMap.AddAction("Hook", InputActionType.Button);
            hookAction.AddBinding("<Mouse>/rightButton");
            hookAction.AddBinding("<Gamepad>/buttonSouth"); // A on Xbox, X on PS

            // Uppercut Action (Button)
            var uppercutAction = gameplayMap.AddAction("Uppercut", InputActionType.Button);
            uppercutAction.AddBinding("<Mouse>/middleButton");
            uppercutAction.AddBinding("<Gamepad>/buttonNorth"); // Y on Xbox, Triangle on PS

            // Block Action (Button)
            var blockAction = gameplayMap.AddAction("Block", InputActionType.Button);
            blockAction.AddBinding("<Keyboard>/leftShift");
            blockAction.AddBinding("<Gamepad>/buttonEast"); // B on Xbox, Circle on PS

            // Create UI Action Map
            var uiMap = inputActions.AddActionMap("UI");

            // Pause Action (Button)
            var pauseAction = uiMap.AddAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");

            // Save the asset
            AssetDatabase.CreateAsset(inputActions, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[InputActionsGenerator] Input Actions asset created: {assetPath}");

            // Generate C# wrapper class
            Debug.Log("[InputActionsGenerator] Generating C# wrapper class...");
            Debug.Log("[InputActionsGenerator] NOTE: You must manually generate the C# class:");
            Debug.Log("  1. Select KnockoutInputActions.inputactions in Project window");
            Debug.Log("  2. In Inspector, check 'Generate C# Class'");
            Debug.Log("  3. Set path to: Assets/Knockout/Scripts/Input/KnockoutInputActions.cs");
            Debug.Log("  4. Click 'Apply'");

            // Select the asset in the project window
            Selection.activeObject = inputActions;
            EditorGUIUtility.PingObject(inputActions);

            AssetDatabase.Refresh();

            Debug.Log("[InputActionsGenerator] Input Actions asset generation complete!");
        }
    }
}
#endif

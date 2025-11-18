using UnityEngine;
using UnityEditor;
using Knockout.Characters.Components;

#if UNITY_EDITOR
namespace Knockout.Editor
{
    /// <summary>
    /// Editor script to configure Phase 4 AI assets.
    /// Run this via Tools > Knockout > Setup Phase 4 AI menu item.
    /// </summary>
    public static class Phase4Setup
    {
        [MenuItem("Tools/Knockout/Setup Phase 4 AI")]
        public static void SetupPhase4AI()
        {
            Debug.Log("[Phase4Setup] Configuring AI character prefab...");

            ConfigureAICharacterPrefab();

            Debug.Log("[Phase4Setup] Phase 4 AI setup complete!");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Configures the AICharacter prefab with CharacterAI component.
        /// Removes CharacterInput if present from earlier phases.
        /// </summary>
        private static void ConfigureAICharacterPrefab()
        {
            string aiPrefabPath = "Assets/Knockout/Prefabs/Characters/AICharacter.prefab";

            // Load the AI character prefab
            GameObject aiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aiPrefabPath);
            if (aiPrefab == null)
            {
                Debug.LogError($"[Phase4Setup] Could not find AICharacter prefab at {aiPrefabPath}");
                Debug.LogError("[Phase4Setup] Make sure Phase 1 has been completed and prefabs exist.");
                return;
            }

            // Instantiate prefab for modification
            GameObject aiInstance = PrefabUtility.InstantiatePrefab(aiPrefab) as GameObject;
            if (aiInstance == null)
            {
                Debug.LogError("[Phase4Setup] Failed to instantiate AICharacter prefab");
                return;
            }

            // Remove CharacterInput component if it exists
            CharacterInput existingInput = aiInstance.GetComponent<CharacterInput>();
            if (existingInput != null)
            {
                Debug.Log("[Phase4Setup] Removing CharacterInput component from AICharacter");
                Object.DestroyImmediate(existingInput);
            }

            // Add CharacterAI component if not already present
            CharacterAI aiComponent = aiInstance.GetComponent<CharacterAI>();
            if (aiComponent == null)
            {
                Debug.Log("[Phase4Setup] Adding CharacterAI component to AICharacter");
                aiComponent = aiInstance.AddComponent<CharacterAI>();

                // Configure AI parameters using SerializedObject
                SerializedObject aiSO = new SerializedObject(aiComponent);

                // Set default AI parameters
                aiSO.FindProperty("reactionTime").floatValue = 0.1f;
                aiSO.FindProperty("attackAccuracy").floatValue = 0.7f;
                aiSO.FindProperty("aggressionLevel").floatValue = 0.5f;

                // Target player will be found automatically at runtime
                aiSO.FindProperty("targetPlayer").objectReferenceValue = null;

                aiSO.ApplyModifiedProperties();

                Debug.Log("[Phase4Setup] CharacterAI component configured with default parameters:");
                Debug.Log("  - Reaction Time: 0.1s");
                Debug.Log("  - Attack Accuracy: 70%");
                Debug.Log("  - Aggression Level: Medium (0.5)");
            }
            else
            {
                Debug.Log("[Phase4Setup] CharacterAI component already exists on AICharacter");
            }

            // Apply changes to prefab
            PrefabUtility.ApplyPrefabInstance(aiInstance, InteractionMode.AutomatedAction);

            // Clean up instance
            Object.DestroyImmediate(aiInstance);

            Debug.Log($"[Phase4Setup] AICharacter prefab configured successfully at {aiPrefabPath}");
        }

        /// <summary>
        /// Verifies that all required components exist on the AI character.
        /// </summary>
        [MenuItem("Tools/Knockout/Verify Phase 4 Setup")]
        public static void VerifyPhase4Setup()
        {
            Debug.Log("[Phase4Setup] Verifying Phase 4 AI setup...");

            string aiPrefabPath = "Assets/Knockout/Prefabs/Characters/AICharacter.prefab";
            GameObject aiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(aiPrefabPath);

            if (aiPrefab == null)
            {
                Debug.LogError($"[Phase4Setup] FAILED: AICharacter prefab not found at {aiPrefabPath}");
                return;
            }

            // Check for required components
            bool hasCharacterAI = aiPrefab.GetComponent<CharacterAI>() != null;
            bool hasCharacterCombat = aiPrefab.GetComponent<CharacterCombat>() != null;
            bool hasCharacterMovement = aiPrefab.GetComponent<CharacterMovement>() != null;
            bool hasCharacterHealth = aiPrefab.GetComponent<CharacterHealth>() != null;
            bool hasCharacterAnimator = aiPrefab.GetComponent<CharacterAnimator>() != null;
            bool hasCharacterInput = aiPrefab.GetComponent<CharacterInput>() != null;

            Debug.Log("[Phase4Setup] Component verification:");
            Debug.Log($"  CharacterAI: {(hasCharacterAI ? "✓ PRESENT" : "✗ MISSING")}");
            Debug.Log($"  CharacterCombat: {(hasCharacterCombat ? "✓ PRESENT" : "✗ MISSING")}");
            Debug.Log($"  CharacterMovement: {(hasCharacterMovement ? "✓ PRESENT" : "✗ MISSING")}");
            Debug.Log($"  CharacterHealth: {(hasCharacterHealth ? "✓ PRESENT" : "✗ MISSING")}");
            Debug.Log($"  CharacterAnimator: {(hasCharacterAnimator ? "✓ PRESENT" : "✗ MISSING")}");
            Debug.Log($"  CharacterInput: {(hasCharacterInput ? "⚠ SHOULD BE REMOVED" : "✓ CORRECTLY REMOVED")}");

            bool setupCorrect = hasCharacterAI && hasCharacterCombat && hasCharacterMovement &&
                                hasCharacterHealth && hasCharacterAnimator && !hasCharacterInput;

            if (setupCorrect)
            {
                Debug.Log("[Phase4Setup] ✓ Phase 4 setup verification PASSED");
            }
            else
            {
                Debug.LogWarning("[Phase4Setup] ⚠ Phase 4 setup verification FAILED - check missing components above");
            }
        }
    }
}
#endif

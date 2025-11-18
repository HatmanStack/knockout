using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Knockout.Editor
{
    /// <summary>
    /// Editor utility to help optimize the game for 60fps performance.
    /// Provides automated optimization tools and performance guidelines.
    /// </summary>
    public class PerformanceOptimizationHelper : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showOptimizationResults;
        private List<string> _optimizationMessages = new List<string>();

        [MenuItem("Knockout/Performance Optimization Helper")]
        public static void ShowWindow()
        {
            var window = GetWindow<PerformanceOptimizationHelper>("Performance Optimizer");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            DrawOptimizationButtons();
            DrawPerformanceGuidelines();

            if (_showOptimizationResults)
            {
                DrawOptimizationResults();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Performance Optimization Helper", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool helps optimize the game for stable 60fps performance. " +
                "Click the buttons below to run automated optimizations or review manual optimization guidelines.",
                MessageType.Info
            );
            EditorGUILayout.Space();
        }

        private void DrawOptimizationButtons()
        {
            EditorGUILayout.LabelField("Automated Optimizations", EditorStyles.boldLabel);

            if (GUILayout.Button("Optimize Animator Settings", GUILayout.Height(30)))
            {
                OptimizeAnimators();
            }

            if (GUILayout.Button("Check Physics Settings", GUILayout.Height(30)))
            {
                CheckPhysicsSettings();
            }

            if (GUILayout.Button("Verify URP Settings", GUILayout.Height(30)))
            {
                VerifyURPSettings();
            }

            if (GUILayout.Button("Run All Optimizations", GUILayout.Height(30)))
            {
                RunAllOptimizations();
            }

            EditorGUILayout.Space();
        }

        private void DrawPerformanceGuidelines()
        {
            EditorGUILayout.LabelField("Manual Optimization Guidelines", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Animation Optimization:\n" +
                "• Set Animator Culling Mode to 'Cull Update Transforms' for characters\n" +
                "• Disable Animator components when characters are off-screen\n" +
                "• Use Animation Compression to reduce memory usage",
                MessageType.None
            );

            EditorGUILayout.HelpBox(
                "Physics Optimization:\n" +
                "• Use 'Discrete' collision detection for most objects\n" +
                "• Use 'Continuous' only for fast-moving objects\n" +
                "• Minimize active Rigidbody components\n" +
                "• Use appropriate layer collision matrix settings",
                MessageType.None
            );

            EditorGUILayout.HelpBox(
                "Rendering Optimization:\n" +
                "• Verify URP settings: Shadow Quality = Medium\n" +
                "• Use texture compression (DXT5/ASTC) for all textures\n" +
                "• Minimize draw calls with batching\n" +
                "• Use occlusion culling if applicable",
                MessageType.None
            );

            EditorGUILayout.HelpBox(
                "Code Optimization:\n" +
                "• Avoid GetComponent calls in Update/FixedUpdate loops\n" +
                "• Cache component references in Awake/Start\n" +
                "• Use object pooling for frequently instantiated objects\n" +
                "• Minimize garbage allocation in hot paths",
                MessageType.None
            );

            EditorGUILayout.Space();
        }

        private void DrawOptimizationResults()
        {
            EditorGUILayout.LabelField("Optimization Results", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            foreach (var message in _optimizationMessages)
            {
                EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void OptimizeAnimators()
        {
            _optimizationMessages.Clear();
            _showOptimizationResults = true;

            var animators = FindObjectsOfType<Animator>();
            int optimizedCount = 0;

            foreach (var animator in animators)
            {
                // Set culling mode to optimize performance
                if (animator.cullingMode != AnimatorCullingMode.CullUpdateTransforms)
                {
                    Undo.RecordObject(animator, "Optimize Animator");
                    animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    EditorUtility.SetDirty(animator);
                    optimizedCount++;
                }
            }

            _optimizationMessages.Add($"Optimized {optimizedCount} Animator components");
            _optimizationMessages.Add($"Set Culling Mode to 'Cull Update Transforms' for better performance");

            if (optimizedCount == 0)
            {
                _optimizationMessages.Add("All Animators already optimized!");
            }

            EditorUtility.DisplayDialog("Animator Optimization",
                $"Optimized {optimizedCount} Animator components.", "OK");
        }

        private void CheckPhysicsSettings()
        {
            _optimizationMessages.Clear();
            _showOptimizationResults = true;

            var rigidbodies = FindObjectsOfType<Rigidbody>();
            int discreteCount = 0;
            int continuousCount = 0;

            foreach (var rb in rigidbodies)
            {
                if (rb.collisionDetectionMode == CollisionDetectionMode.Discrete)
                {
                    discreteCount++;
                }
                else
                {
                    continuousCount++;
                }
            }

            _optimizationMessages.Add($"Physics Check Results:");
            _optimizationMessages.Add($"  • {rigidbodies.Length} total Rigidbody components");
            _optimizationMessages.Add($"  • {discreteCount} using Discrete collision (optimized)");
            _optimizationMessages.Add($"  • {continuousCount} using Continuous collision");
            _optimizationMessages.Add("");
            _optimizationMessages.Add($"Recommendation: Use Discrete for most objects, Continuous only for fast-moving objects");

            EditorUtility.DisplayDialog("Physics Check",
                $"Found {rigidbodies.Length} Rigidbody components.\n" +
                $"Discrete: {discreteCount}, Continuous: {continuousCount}", "OK");
        }

        private void VerifyURPSettings()
        {
            _optimizationMessages.Clear();
            _showOptimizationResults = true;

            // Note: This is a basic check. In a real project, you'd inspect the URP asset directly
            _optimizationMessages.Add("URP Settings Verification:");
            _optimizationMessages.Add("");
            _optimizationMessages.Add("Recommended Settings for 60fps:");
            _optimizationMessages.Add("  • Shadow Quality: Medium");
            _optimizationMessages.Add("  • Shadow Distance: 50-100");
            _optimizationMessages.Add("  • Cascade Count: 2");
            _optimizationMessages.Add("  • Anti-aliasing: FXAA or disabled");
            _optimizationMessages.Add("  • HDR: Disabled (unless required)");
            _optimizationMessages.Add("");
            _optimizationMessages.Add("Please verify these settings manually in your URP Asset.");

            EditorUtility.DisplayDialog("URP Settings",
                "Please verify URP settings manually in your Universal Render Pipeline Asset.\n" +
                "See the optimization messages for recommended settings.", "OK");
        }

        private void RunAllOptimizations()
        {
            _optimizationMessages.Clear();
            _showOptimizationResults = true;

            _optimizationMessages.Add("Running All Optimizations...");
            _optimizationMessages.Add("");

            OptimizeAnimators();
            CheckPhysicsSettings();
            VerifyURPSettings();

            _optimizationMessages.Add("");
            _optimizationMessages.Add("All automated optimizations complete!");

            EditorUtility.DisplayDialog("All Optimizations Complete",
                "Automated optimizations have been applied.\n" +
                "Review the optimization results below for details.", "OK");
        }
    }
}

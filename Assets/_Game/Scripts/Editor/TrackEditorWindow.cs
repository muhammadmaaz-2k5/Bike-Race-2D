using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DirtDash.Track;

namespace DirtDash.Editor
{
    /// <summary>
    /// Custom Track Editor Window for Dirt Dash.
    /// Allows designers to inspect points, snap to grid, insert ramps, hills, loops,
    /// and save generated point sequences directly to LevelData ScriptableObjects.
    /// </summary>
    public class TrackEditorWindow : EditorWindow
    {
        private LevelData targetLevelData;
        private bool snapToGrid = true;
        private float gridSize = 1.0f;
        private Vector2 scrollPos;

        [MenuItem("DirtDash/Track Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<TrackEditorWindow>("Dirt Dash Track Editor");
            window.minSize = new Vector2(360, 480);
        }

        private void OnGUI()
        {
            GUILayout.Label("DIRT DASH - TRACK EDITOR", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            targetLevelData = (LevelData)EditorGUILayout.ObjectField("Active Level Data", targetLevelData, typeof(LevelData), false);

            if (targetLevelData == null)
            {
                EditorGUILayout.HelpBox("Assign a LevelData asset to view and edit track coordinates.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Grid & Editing Options", EditorStyles.boldLabel);
            snapToGrid = EditorGUILayout.Toggle("Snap to Grid", snapToGrid);
            if (snapToGrid)
            {
                gridSize = EditorGUILayout.FloatField("Grid Step", gridSize);
            }

            EditorGUILayout.Space();
            GUILayout.Label("Preset Segment Generators", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            Vector2 lastPoint = targetLevelData.trackPoints.Count > 0 ? targetLevelData.trackPoints[^1] : Vector2.zero;

            if (GUILayout.Button("+ Flat Road"))
            {
                Undo.RecordObject(targetLevelData, "Add Flat Track");
                targetLevelData.trackPoints.AddRange(TrackGenerators.Straight(lastPoint, 20f));
                EditorUtility.SetDirty(targetLevelData);
            }

            if (GUILayout.Button("+ Hill"))
            {
                Undo.RecordObject(targetLevelData, "Add Hill");
                targetLevelData.trackPoints.AddRange(TrackGenerators.SmoothHill(lastPoint, 24f, 4f));
                EditorUtility.SetDirty(targetLevelData);
            }

            if (GUILayout.Button("+ Ramp"))
            {
                Undo.RecordObject(targetLevelData, "Add Ramp");
                targetLevelData.trackPoints.AddRange(TrackGenerators.Ramp(lastPoint, 15f, 4.5f));
                EditorUtility.SetDirty(targetLevelData);
            }

            if (GUILayout.Button("+ Loop-the-Loop"))
            {
                Undo.RecordObject(targetLevelData, "Add Loop");
                targetLevelData.trackPoints.AddRange(TrackGenerators.LoopTheLoop(lastPoint, 5f));
                EditorUtility.SetDirty(targetLevelData);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label($"Track Points ({targetLevelData.trackPoints.Count})", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(180));
            for (int i = 0; i < targetLevelData.trackPoints.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                targetLevelData.trackPoints[i] = EditorGUILayout.Vector2Field($"Pt {i}", targetLevelData.trackPoints[i]);
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(targetLevelData, "Remove Point");
                    targetLevelData.trackPoints.RemoveAt(i);
                    EditorUtility.SetDirty(targetLevelData);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Update Finish Position to End of Track", GUILayout.Height(30)))
            {
                if (targetLevelData.trackPoints.Count > 0)
                {
                    Undo.RecordObject(targetLevelData, "Update Finish Line");
                    targetLevelData.finishPosition = targetLevelData.trackPoints[^1];
                    EditorUtility.SetDirty(targetLevelData);
                }
            }

            if (GUILayout.Button("Save LevelData Asset", GUILayout.Height(35)))
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"[Dirt Dash] Saved LevelData {targetLevelData.name} successfully.");
            }
        }
    }
}

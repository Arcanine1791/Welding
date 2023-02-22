using UnityEditor;
using UnityEngine;
using MeshTracker;

// Mesh Tracker Particles - Editor.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    [CustomEditor(typeof(MeshTracker_Particles))]
    [CanEditMultipleObjects]
    public class MeshTracker_Particles_Editor : MeshTracker_EditorUtilities
    {
        private MeshTracker_Particles m;

        private void OnEnable()
        {
            m = (MeshTracker_Particles)target;
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            serializedObject.Update();

            PS(5);

            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;

            PpDrawProperty("CustomTrack", "Custom Track");
            if (m.CustomTrack) EditorGUILayout.HelpBox("This will instantiate new track prefab on every particle collision. Takes more performance and memory allocation!", MessageType.Warning);

            PS(5);

            GUILayout.BeginVertical("Box");

            if (m.CustomTrack)
            {
                PpDrawProperty("TrackPrefab", "Track Prefab");
                PpDrawProperty("TrackLifeTime", "Track LifeTime","Lifetime for track destroy (in seconds)");
            }
            else
            {
                PpDrawProperty("TrackSize", "Track Size");
                PpDrawProperty("TrackGraphic", "Track Graphic");
                PpDrawProperty("AdditionalBrush", "Additional Brush", "Additional custom brush for more details");
            }
            GUILayout.EndVertical();
        }
    }
}
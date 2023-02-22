using UnityEngine;
using UnityEditor;
using MeshTracker;

// Mesh Tracker Procedural Plane [PP] - Editor.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    [CustomEditor(typeof(MeshTracker_ProceduralPlane))]
    [CanEditMultipleObjects]
    public class MeshTracker_PP_Editor : MeshTracker_EditorUtilities
    {
        private MeshTracker_ProceduralPlane asp;

        private void OnEnable()
        {
            asp = (MeshTracker_ProceduralPlane)target;
        }

        public override void OnInspectorGUI()
        {
            PS(5);
            PpDrawProperty("UpdateMeshEveryFrame", "Update Every Frame");
            if (!asp.UpdateMeshEveryFrame)
            {
                if (PB("Generate Plane")) asp.GenerateMesh();
            }

            GUI.color = Color.green;
            PpDrawProperty("GenerateMeshColliderAfterStart", "Generate Collider After Start");
            GUI.color = Color.white;

            PV();

            PpDrawProperty("Plane_length", "Length");
            PpDrawProperty("Plane_width", "Width");

            PS(6);

            PpDrawProperty("Plane_resX", "Resolution");

            PVE();

            serializedObject.Update();
        }
    }
}
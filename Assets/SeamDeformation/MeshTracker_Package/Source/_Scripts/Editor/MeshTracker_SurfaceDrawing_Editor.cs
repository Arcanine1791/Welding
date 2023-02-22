using UnityEngine;
using UnityEditor;
using MeshTracker;

// Mesh Tracker Surface Drawing - Editor.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    [CustomEditor(typeof(MeshTracker_SurfaceDrawing))]
    [CanEditMultipleObjects]
    public class MeshTracker_SurfaceDrawing_Editor : MeshTracker_EditorUtilities
    {
        private MeshTracker_SurfaceDrawing msd;

        private Vector2 scroll;
        private float indx;
        private bool useSlider = true;

        private void OnEnable()
        {
            msd = (MeshTracker_SurfaceDrawing)target;
            indx = msd.mmt_SelectedTrackGraphicIndex;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Surface Drawing is compatible with MeshTracker_Object GPU-Based system only!", MessageType.Warning);
            PS();
            PL("Track Graphics Palette", true);
            PV();
            PpDrawProperty("mmt_TrackGraphics", "Track Graphics", "", true);

            scroll = GUILayout.BeginScrollView(scroll);
            PH();
            for (int i = 0; i < msd.mmt_TrackGraphics.Count; i++)
            {
                if (i == msd.mmt_SelectedTrackGraphicIndex)
                    PV();
                GUILayout.Label(new GUIContent(i.ToString(), msd.mmt_TrackGraphics[i]), GUILayout.Width(50), GUILayout.Height(50));
                if (i == msd.mmt_SelectedTrackGraphicIndex)
                    PVE();
            }
            PHE();
            GUILayout.EndScrollView();
            if (msd.mmt_TrackGraphics.Count > 0)
            {
                if (PB("Remove Last"))
                    msd.mmt_TrackGraphics.RemoveAt(msd.mmt_TrackGraphics.Count - 1);
            }
            if (useSlider && !Application.isPlaying)
            {
                indx = GUILayout.HorizontalSlider(indx, 0, msd.mmt_TrackGraphics.Count - 1);
                if (msd.mmt_SelectedTrackGraphicIndex != (int)indx)
                    msd.mmt_SelectedTrackGraphicIndex = Mathf.RoundToInt(indx);
            }
            PS(20);
            useSlider = GUILayout.Toggle(useSlider, "Use Smooth Slider");

            PpDrawProperty("mmt_SelectedTrackGraphicIndex", "Selected Index", "Index of the track list above");
            PVE();

            PV();
            PpDrawProperty("mmt_RandomizeTrackDrawing", "Randomize Track Drawing");
            if (msd.mmt_RandomizeTrackDrawing)
            {
                PS();
                PL("          From index " + msd.mmt_RandomizeIndex.x.ToString());
                PS(5);
                msd.mmt_RandomizeIndex.x = GUILayout.HorizontalSlider(msd.mmt_RandomizeIndex.x, 0, msd.mmt_TrackGraphics.Count - 1);
                PS(20);
                PL("          To index " + msd.mmt_RandomizeIndex.y.ToString());
                PS(5);
                msd.mmt_RandomizeIndex.y = GUILayout.HorizontalSlider(msd.mmt_RandomizeIndex.y, 0, msd.mmt_TrackGraphics.Count - 1);
                PS(20);
                msd.mmt_RandomizeIndex.x = Mathf.RoundToInt(msd.mmt_RandomizeIndex.x);
                msd.mmt_RandomizeIndex.y = Mathf.RoundToInt(msd.mmt_RandomizeIndex.y);
            }
            PVE();

            PS(5);

            PL("Input & Platform", true);
            PV();
            PpDrawProperty("mmt_CamTarget", "Camera Target","Main Camera Target that will represent the 'Raycast Origin'");
            PpDrawProperty("mmt_MobilePlatform", "Mobile Platform");
            if (msd.mmt_MobilePlatform == false)
                PpDrawProperty("mmt_InputKey", "Input Key");
            PVE();

            PS(5);

            PL("Track Parameters", true);
            PV();
            PV();
            PpDrawProperty("mmt_VisualBrush", "Visual Brush (Optional)");
            if (msd.mmt_VisualBrush != null)
            {
                PpDrawProperty("mmt_VisualBrushSizeMultiplier", "Visual Brush Size Multiplier");
                PpDrawProperty("mmt_VisualBrushYOffset", "Visual Brush Y Offset");
            }
            PVE();
            PS();
            PV();
            PpDrawProperty("mmt_TrackSize", "Track Size");
            PpDrawProperty("mmt_TrackStrength", "Track Opacity");
            PpDrawProperty("mmt_TrackHeight", "Track Height");
            PVE();
            PS(5);
            PV();
            PpDrawProperty("mmt_LoadTracksIntoLayoutGroup", "Load Tracks Into Layout Group","If enabled, you will be able to visualize assigned tracks into UI images group");
            if (msd.mmt_LoadTracksIntoLayoutGroup)
            {
                PpDrawProperty("mmt_LayoutGroupParent", "Layout Group Parent");
                PpDrawProperty("mmt_ImageSize", "Image Size");
                PS(5);
                PpDrawProperty("mmt_AdditionalOnClickEvent", "Additional On Click Event", "Additional event if any generated track image button is pressed");
            }
            PVE();
            PVE();

            PS(5);

            PL("Conditions", true);
            PV();
            PpDrawProperty("mmt_AllowedLayers", "Allowed Layers");
            PpDrawProperty("mmt_AllObjectsAllowed", "All Objects Allowed");
            if (!msd.mmt_AllObjectsAllowed)
                PpDrawProperty("mmt_AllowedWithTag", "Allowed Object With Tag");
            PVE();
            PS();

            serializedObject.Update();
        }
    }
}
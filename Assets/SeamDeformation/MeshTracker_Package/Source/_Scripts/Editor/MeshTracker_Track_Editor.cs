using UnityEngine;
using UnityEditor;
using MeshTracker;

// Mesh Tracker Track - Editor.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    [CustomEditor(typeof(MeshTracker_Track))]
    [CanEditMultipleObjects]
    public class MeshTracker_Track_Editor : MeshTracker_EditorUtilities
    {
        private MeshTracker_Track m;

        private static MeshTracker_Track.Mt_TrackLayer CopyMeshTrackLayerStorage;

        private void OnEnable()
        {
            m = (MeshTracker_Track)target;
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            serializedObject.Update();

            PS();

            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;

            PV();
            PpDrawProperty("mmt_TargetedToCPUBasedType", "Targeted To CPU-Based Type", "If enabled, the track will be mostly targeted to surfaces with Mesh Tracker Object - CPU-Based type surface. This is optional, the tracks will work on both surfaces either. This will just clear unecessary fields...");
            PVE();

            PS();

            PH();
            if (PB("Add Layer"))
            {
                m.TrackLayer.Add(new MeshTracker_Track.Mt_TrackLayer());
                return;
            }
            if (PB("Remove Last Layer") && m.TrackLayer.Count > 0)
            {
                string tName = m.TrackLayer[m.TrackLayer.Count - 1].mmt_TrackName;
                if (string.IsNullOrEmpty(tName)) tName = "Track Layer " + (m.TrackLayer.Count - 1).ToString();
                if (!EditorUtility.DisplayDialog("Question", "Are you sure you want to delete last layer \n'" + tName + "'?\nThere is no way back!", "Yes", "No"))
                    return;
                m.TrackLayer.RemoveAt(m.TrackLayer.Count - 1);
                return;
            }
            PHE();

            if (CopyMeshTrackLayerStorage != null)
                GUI.color = Color.white;
            else
                GUI.color = Color.gray;

            if (PB("Paste Layer"))
            {
                if (CopyMeshTrackLayerStorage != null)
                    m.TrackLayer.Add(CopyMeshTrackLayerStorage);

                CopyMeshTrackLayerStorage = null;
                return;
            }

            GUI.color = Color.white;

            if (m.TrackLayer.Count > 0)
            {
                PS(10);

                ppDrawList();
            }
            GUI.backgroundColor = Color.white;

            PS(10);

            PL("Track Effects");

            PV();
            PpDrawProperty("mmt_useEffects", "Use Track Effects", "If enabled, the system will keep checking the surface every frame");
            if (m.mmt_useEffects)
            {
                PpDrawProperty("mmt_effectQuality", "Effects Quality", "If enabled, you will be able to check the surface in specified interval");
                if(m.mmt_effectQuality == MeshTracker_Track.EffectQuality.Custom)
                {
                    PpDrawProperty("mmt_CustomTrackBuffers", "Custom Quality", "Type custom quality value. The higher number is, the more raycast passes you will get, but the more performance it will take");
                    EditorGUILayout.HelpBox("It's highly recommended to use the preset quality settings to avoid performance issues!", MessageType.Warning);
                }
            }
            PVE();

            PS(10);

            PL("Update Logic");

            PV();
            PpDrawProperty("mmt_AlwaysCheckSurface", "Always Check Surface", "If enabled, the system will keep checking the surface every frame");
            if (!m.mmt_AlwaysCheckSurface)
            {
                PpDrawProperty("mmt_UseInterval", "Use Intervals", "If enabled, you will be able to check the surface in specified interval");
                if (m.mmt_UseInterval)
                    PpDrawProperty("mmt_Interval", "Interval [In Seconds]");
            }
            PVE();
        }

        private bool foldoutEffects = false;

        /// <summary>
        /// Draw track list
        /// </summary>
        private void ppDrawList()
        {
            for (int i = 0; i < m.TrackLayer.Count; i++)
            {
                GUI.backgroundColor = m.TrackLayer[i].mmt_TrackColor;

                PV();

                SerializedProperty item = serializedObject.FindProperty("TrackLayer").GetArrayElementAtIndex(i);
                string Tname = m.TrackLayer[i].mmt_TrackName;
                if (string.IsNullOrEmpty(Tname)) Tname = "Track Layer " + i.ToString();

                PpDrawProperty(item, Tname);

                if (!item.isExpanded)
                {
                    PVE();
                    continue;
                }

                PS(5);

                EditorGUI.indentLevel += 1;

                PL("  Track Basics");
                PV();//-----------------------------1
                PV();
                if (!m.TrackLayer[i].mmt_FixObjectScaleWithTrackSize)
                    PpDrawProperty(item.FindPropertyRelative("mmt_TrackSize"), "Track Size");
                else
                {
                    PpDrawProperty(item.FindPropertyRelative("mmt_GetScaleFromRoot"), "Get Scale From Root","If enabled, the scale value will be received from this objects root transform");
                    PpDrawProperty(item.FindPropertyRelative("mmt_InternalScaleMultiplier"), "Internal Scale Multiplier","Additional scale multiplier");
                }

                PpDrawProperty(item.FindPropertyRelative("mmt_FixObjectScaleWithTrackSize"), "Objects Scale Is Track Size","If enabled, the track size will be equal to the current objects local scale");
                PVE();

                if (!m.mmt_TargetedToCPUBasedType)
                {
                    PS(3);

                    PV();
                    PpDrawProperty(item.FindPropertyRelative("mmt_TrackGraphic"), "Track Graphic", "Enter track graphic. If surface will be set to 'Script Type', this field will be ignored");
                    PpDrawProperty(item.FindPropertyRelative("mmt_TrackBrush"), "Track Brush Type", "(optional) Enter material with brush shader. Add more details to your tracks [This field is not required]");
                    PVE();

                    PS(3);

                    if (m.TrackLayer[i].mmt_TrackGraphic)
                    {
                        PV();
                        PpDrawProperty(item.FindPropertyRelative("mmt_UseSmartLayerRotation"), "Use Smart Layer Rotation", "If enabled, track graphic will rotate by the object transform move direction (Especially for Smart Layers)");
                        if (m.TrackLayer[i].mmt_UseSmartLayerRotation && m.TrackLayer[i].mmt_TrackBrush == null)
                            EditorGUILayout.HelpBox("The 'Track Brush Type' is required while using the Smart Layers (please enable and assign the field above)", MessageType.Warning);
                        if (m.TrackLayer[i].mmt_UseSmartLayerRotation == false)
                        {
                            PpDrawProperty(item.FindPropertyRelative("mmt_CopyObjectRotation"), "Copy Transform Rotation", "If enabled, track graphic will rotate by the object transform rotation X/Y/Z Axis (Depends on Rotation Type below)");
                            PpDrawProperty(item.FindPropertyRelative("mmt_GetRotationType"), "Get Rotation Axis", "In which direction axis should the rotation work?");
                        }
                        else
                        {
                            PpDrawProperty(item.FindPropertyRelative("mmt_InverseSmartLayerRotation"), "Inverse Smart Layer Rotation", "If enabled, the Y rotation of the smart track will be inversed by 180 degrees");
                            PpDrawProperty(item.FindPropertyRelative("mmt_UpwardDirection"), "Upward Direction", "Upward direction of the smart layer rotation");
                        }
                        PVE();
                    }
                }
                PS(3);
                PpDrawProperty(item.FindPropertyRelative("mmt_TrackName"), "Track Name [Editor]");
                PpDrawProperty(item.FindPropertyRelative("mmt_TrackColor"), "Track Color [Editor]");
                PVE();//-----------------------------1

                PS(10);

                PL("  Ray Settings");
                PV();//-----------------------------2
                PpDrawProperty(item.FindPropertyRelative("mmt_RayOriginIsCursor"), "Ray Origin Is Cursor", "If enabled, you will be able to create track from cursor position");
                if (!m.TrackLayer[i].mmt_RayOriginIsCursor)
                {
                    PpDrawProperty(item.FindPropertyRelative("mmt_RayDirectionWorldSpace"), "World Space", "If enabled, the ray direction will be handled in World Space. If disable, the ray direction will be handled in Local Space related to THIS object");
                    PpDrawProperty(item.FindPropertyRelative("mmt_RayDirection"), "Ray Direction", "Global ray direction (in world space)");
                    PpDrawProperty(item.FindPropertyRelative("mmt_RayOriginOffset"), "Ray Origin Offset");
                }
                else
                {
                    PpDrawProperty(item.FindPropertyRelative("mmt_CameraTarget"), "Camera Target", "Enter target camera for raycast origin");
                    PpDrawProperty(item.FindPropertyRelative("mmt_MobilePlatform"), "Mobile Platform", "If enabled, ray origin will be set to 'touch position'");
                    if (!m.TrackLayer[i].mmt_MobilePlatform)
                    {
                        PpDrawProperty(item.FindPropertyRelative("mmt_InputEvent"), "Enable Input");
                        if (m.TrackLayer[i].mmt_InputEvent)
                        {
                            EditorGUI.indentLevel++;
                            PpDrawProperty(item.FindPropertyRelative("mmt_InputKey"), "Key");
                            EditorGUI.indentLevel--;
                        }
                    }
                }

                PV();
                PpDrawProperty(item.FindPropertyRelative("mmt_RayDistance"), "Ray Distance", "");
                PVE();

                PVE();//-----------------------------2

                PS(10);

                PL("  Conditions");
                PV();//-----------------------------3
                PV();
                PpDrawProperty(item.FindPropertyRelative("mmt_AllowedLayers"), "Allowed Layers");
                PVE();

                PS(5);

                PV();
                PpDrawProperty(item.FindPropertyRelative("mmt_AllObjectsAllowed"), "All Objects Allowed", "If enabled, all objects with colliders will be allowed for this raycast");
                if (!m.TrackLayer[i].mmt_AllObjectsAllowed)
                    PpDrawProperty(item.FindPropertyRelative("mmt_AllowedWithTag"), "Allowed Tags");
                PVE();

                if (!m.TrackLayer[i].mmt_RayOriginIsCursor)
                {
                    PV();
                    PS(5);
                    PpDrawProperty(item.FindPropertyRelative("mmt_UseSpeedLimits"), "Use Speed Limits","If enabled, you will be able to use speed limits & access to the object's speed parameter");
                    if (m.TrackLayer[i].mmt_UseSpeedLimits)
                    {
                        PpDrawProperty(item.FindPropertyRelative("mmt_SpeedLimitMin"), "Minimum Speed Limit");
                        PpDrawProperty(item.FindPropertyRelative("mmt_SpeedLimitMax"), "Maximum Speed Limit");
                    }
                    PVE();

                    GUI.color = Color.grey;
                    if (m.TrackLayer[i].mmt_UseSpeedLimits)
                        PpDrawProperty("speed", "Debug: Object Speed");
                }
                GUI.color = Color.white;
                PVE();//-----------------------------3

                if (m.mmt_useEffects)
                {
                    PS(10);

                    PL("  Track Effects");
                    PV();//-----------------------------4
                    PpDrawProperty(item.FindPropertyRelative("mmt_enabledTrackEffects"), "Enable Effects");
                    if (m.TrackLayer[i].mmt_enabledTrackEffects)
                        ppDrawEffectList(m.TrackLayer[i], item);
                    PVE();//-----------------------------4
                }

                PS(10);

                PL("  Events");
                PV();//-----------------------------5
                PpDrawProperty(item.FindPropertyRelative("mmt_EnableEvents"), "Enable Events");
                if (m.TrackLayer[i].mmt_EnableEvents)
                {
                    PpDrawProperty(item.FindPropertyRelative("mmt_Event_UpdateEventEveryFrame"), "Update Events Every Frame");
                    PpDrawProperty(item.FindPropertyRelative("mmt_Event_SurfaceDetected"), "On Surface Detected");
                    PpDrawProperty(item.FindPropertyRelative("mmt_Event_SurfaceExit"), "On Surface Exit");
                }
                PVE();//-----------------------------5

                EditorGUI.indentLevel -= 1;

                PS(10);

                PH();
                if (PB("Copy"))
                {
                    MeshTracker_Track.Mt_TrackLayer t = new MeshTracker_Track.Mt_TrackLayer();
                    t.mmt_TrackName = m.TrackLayer[i].mmt_TrackName;
                    t.mmt_TrackColor = m.TrackLayer[i].mmt_TrackColor;
                    t.mmt_TrackSize = m.TrackLayer[i].mmt_TrackSize;
                    t.mmt_TrackGraphic = m.TrackLayer[i].mmt_TrackGraphic;
                    if (m.TrackLayer[i].mmt_TrackBrush) t.mmt_TrackBrush = m.TrackLayer[i].mmt_TrackBrush;
                    else t.mmt_TrackBrush = null;
                    if (m.TrackLayer[i].mmt_CameraTarget) t.mmt_CameraTarget = m.TrackLayer[i].mmt_CameraTarget;
                    else t.mmt_CameraTarget = null;
                    t.mmt_CopyObjectRotation = m.TrackLayer[i].mmt_CopyObjectRotation;
                    t.mmt_InverseSmartLayerRotation = m.TrackLayer[i].mmt_InverseSmartLayerRotation;
                    t.mmt_UseSmartLayerRotation = m.TrackLayer[i].mmt_UseSmartLayerRotation;
                    t.mmt_UpwardDirection = m.TrackLayer[i].mmt_UpwardDirection;
                    t.mmt_GetRotationType = m.TrackLayer[i].mmt_GetRotationType;
                    t.mmt_FixObjectScaleWithTrackSize = m.TrackLayer[i].mmt_FixObjectScaleWithTrackSize;
                    t.mmt_GetScaleFromRoot = m.TrackLayer[i].mmt_GetScaleFromRoot;
                    t.mmt_InternalScaleMultiplier = m.TrackLayer[i].mmt_InternalScaleMultiplier;
                    t.mmt_RayOriginIsCursor = m.TrackLayer[i].mmt_RayOriginIsCursor;
                    t.mmt_MobilePlatform = m.TrackLayer[i].mmt_MobilePlatform;
                    t.mmt_InputEvent = m.TrackLayer[i].mmt_InputEvent;
                    t.mmt_InputKey = m.TrackLayer[i].mmt_InputKey;
                    t.mmt_RayDirection = m.TrackLayer[i].mmt_RayDirection;
                    t.mmt_RayDistance = m.TrackLayer[i].mmt_RayDistance;
                    t.mmt_RayOriginOffset = m.TrackLayer[i].mmt_RayOriginOffset;
                    t.mmt_AllowedLayers = m.TrackLayer[i].mmt_AllowedLayers;
                    t.mmt_AllObjectsAllowed = m.TrackLayer[i].mmt_AllObjectsAllowed;
                    t.mmt_AllowedWithTag = m.TrackLayer[i].mmt_AllowedWithTag;
                    t.mmt_UseSpeedLimits = m.TrackLayer[i].mmt_UseSpeedLimits;
                    t.mmt_SpeedLimitMin = m.TrackLayer[i].mmt_SpeedLimitMin;
                    t.mmt_SpeedLimitMax = m.TrackLayer[i].mmt_SpeedLimitMax;
                    t.mmt_EnableEvents = m.TrackLayer[i].mmt_EnableEvents;
                    t.mmt_Event_UpdateEventEveryFrame = m.TrackLayer[i].mmt_Event_UpdateEventEveryFrame;
                    t.mmt_Event_SurfaceDetected = m.TrackLayer[i].mmt_Event_SurfaceDetected;
                    t.mmt_Event_SurfaceExit = m.TrackLayer[i].mmt_Event_SurfaceExit;

                    t.mmt_enabledTrackEffects = m.TrackLayer[i].mmt_enabledTrackEffects;
                    t.mmt_trackEffects = m.TrackLayer[i].mmt_trackEffects.Clone();

                    CopyMeshTrackLayerStorage = t;
                    return;
                }
                PS(20);
                if (PB("Duplicate"))
                {
                    MeshTracker_Track.Mt_TrackLayer t = new MeshTracker_Track.Mt_TrackLayer();
                    t.mmt_TrackName = m.TrackLayer[i].mmt_TrackName;
                    t.mmt_TrackColor = m.TrackLayer[i].mmt_TrackColor;
                    t.mmt_TrackSize = m.TrackLayer[i].mmt_TrackSize;
                    t.mmt_TrackGraphic = m.TrackLayer[i].mmt_TrackGraphic;
                    if (m.TrackLayer[i].mmt_TrackBrush) t.mmt_TrackBrush = m.TrackLayer[i].mmt_TrackBrush;
                    else t.mmt_TrackBrush = null;
                    if (m.TrackLayer[i].mmt_CameraTarget) t.mmt_CameraTarget = m.TrackLayer[i].mmt_CameraTarget;
                    else t.mmt_CameraTarget = null;
                    t.mmt_CopyObjectRotation = m.TrackLayer[i].mmt_CopyObjectRotation;
                    t.mmt_InverseSmartLayerRotation = m.TrackLayer[i].mmt_InverseSmartLayerRotation;
                    t.mmt_UseSmartLayerRotation = m.TrackLayer[i].mmt_UseSmartLayerRotation;
                    t.mmt_UpwardDirection = m.TrackLayer[i].mmt_UpwardDirection;
                    t.mmt_GetRotationType = m.TrackLayer[i].mmt_GetRotationType;
                    t.mmt_FixObjectScaleWithTrackSize = m.TrackLayer[i].mmt_FixObjectScaleWithTrackSize;
                    t.mmt_GetScaleFromRoot = m.TrackLayer[i].mmt_GetScaleFromRoot;
                    t.mmt_InternalScaleMultiplier = m.TrackLayer[i].mmt_InternalScaleMultiplier;
                    t.mmt_RayOriginIsCursor = m.TrackLayer[i].mmt_RayOriginIsCursor;
                    t.mmt_MobilePlatform = m.TrackLayer[i].mmt_MobilePlatform;
                    t.mmt_InputEvent = m.TrackLayer[i].mmt_InputEvent;
                    t.mmt_InputKey = m.TrackLayer[i].mmt_InputKey;
                    t.mmt_RayDirection = m.TrackLayer[i].mmt_RayDirection;
                    t.mmt_RayDistance = m.TrackLayer[i].mmt_RayDistance;
                    t.mmt_RayOriginOffset = m.TrackLayer[i].mmt_RayOriginOffset;
                    t.mmt_AllowedLayers = m.TrackLayer[i].mmt_AllowedLayers;
                    t.mmt_AllObjectsAllowed = m.TrackLayer[i].mmt_AllObjectsAllowed;
                    t.mmt_AllowedWithTag = m.TrackLayer[i].mmt_AllowedWithTag;
                    t.mmt_UseSpeedLimits = m.TrackLayer[i].mmt_UseSpeedLimits;
                    t.mmt_SpeedLimitMin = m.TrackLayer[i].mmt_SpeedLimitMin;
                    t.mmt_SpeedLimitMax = m.TrackLayer[i].mmt_SpeedLimitMax;
                    t.mmt_EnableEvents = m.TrackLayer[i].mmt_EnableEvents;
                    t.mmt_Event_UpdateEventEveryFrame = m.TrackLayer[i].mmt_Event_UpdateEventEveryFrame;
                    t.mmt_Event_SurfaceDetected = m.TrackLayer[i].mmt_Event_SurfaceDetected;
                    t.mmt_Event_SurfaceExit = m.TrackLayer[i].mmt_Event_SurfaceExit;

                    t.mmt_enabledTrackEffects = m.TrackLayer[i].mmt_enabledTrackEffects;
                    t.mmt_trackEffects = m.TrackLayer[i].mmt_trackEffects.Clone();

                    m.TrackLayer.Add(t);
                    return;
                }
                if (PB("Remove"))
                {
                    string tName = m.TrackLayer[i].mmt_TrackName;
                    if (string.IsNullOrEmpty(tName)) tName = "Track Layer " + i.ToString();
                    if (!EditorUtility.DisplayDialog("Question", "Are you sure you want to delete layer \n'" + tName + "'?\nThere is no way back!", "Yes", "No"))
                        return;
                    m.TrackLayer.RemoveAt(i);
                    return;
                }
                PHE();
                PpDrawProperty(item.FindPropertyRelative("mmt_Debug_ShowDebugGraphic"), "Show Debug Graphics");

                PVE();
            }
        }

        /// <summary>
        /// Draw effect track list
        /// </summary>
        private void ppDrawEffectList(MeshTracker_Track.Mt_TrackLayer t, SerializedProperty prop)
        {
            SerializedProperty item = prop.FindPropertyRelative("mmt_trackEffects");

            EditorGUI.indentLevel += 1;
            foldoutEffects = EditorGUILayout.Foldout(foldoutEffects, "Track Effects");
            if (foldoutEffects)
            {
                PS(5);

                if (!m.mmt_TargetedToCPUBasedType)
                {
                    PV();
                    PpDrawProperty(item.FindPropertyRelative("mmt_useTrackReferences"), "Use Original Track Reference", "If enabled, the track graphic and track brush will be received from the original track above. Otherwise new fields will appear");
                    if (!t.mmt_trackEffects.mmt_useTrackReferences)
                    {
                        PpDrawProperty(item.FindPropertyRelative("mmt_TrackGraphic"), "Effect Track Graphic", "New track graphic for the effect");
                        PpDrawProperty(item.FindPropertyRelative("mmt_TrackBrush"), "Effect Track Brush", "New track brush for the effect");
                    }
                    PVE();
                    PS();

                    PV();
                    PpDrawProperty(item.FindPropertyRelative("mmt_SmartTrackRotation"), "Smart Track Rotation", "If enabled, the effect track will use the Smart Track Rotation technique");
                    if (!t.mmt_trackEffects.mmt_SmartTrackRotation)
                        PpDrawProperty(item.FindPropertyRelative("mmt_LocalSpace"), "Local Space", "If enabled, the track direction will be set to objects local space, otherwise the direction will be global");
                    else
                    {
                        if (t.mmt_trackEffects.mmt_useTrackReferences && !t.mmt_TrackBrush)
                            EditorGUILayout.HelpBox("For using Smart Track Rotation, the 'Track Brush' in reference track is required!", MessageType.Error);
                        else if (!t.mmt_trackEffects.mmt_useTrackReferences && !t.mmt_trackEffects.mmt_TrackBrush)
                            EditorGUILayout.HelpBox("For using Smart Track Rotation, the 'Track Brush' is required!", MessageType.Error);
                    }
                    PVE();
                }
                else
                {
                    if (t.mmt_trackEffects.mmt_SmartTrackRotation) t.mmt_trackEffects.mmt_SmartTrackRotation = false;
                    PpDrawProperty(item.FindPropertyRelative("mmt_LocalSpace"), "Local Space", "If enabled, the track direction will be set to objects local space, otherwise the direction will be global");
                }

                PV();
                if (t.mmt_trackEffects.mmt_SmartTrackRotation || t.mmt_trackEffects.mmt_LocalSpace)
                    PpDrawProperty(item.FindPropertyRelative("mmt_Direction"), "Direction Offset", "Direction offset in DEGREES of the effect track [use mostly the Y field for rotation offset left & right]");
                else
                    PpDrawProperty(item.FindPropertyRelative("mmt_Direction"), "Direction", "Global direction of the effect track");

                PpDrawProperty(item.FindPropertyRelative("mmt_useDoubleCast"), "Use Double Effect", "If enabled, the raycasting will increase 2x, so you will be able to cast two rays at once in different directions");
                if (t.mmt_trackEffects.mmt_useDoubleCast)
                {
                    if (t.mmt_trackEffects.mmt_SmartTrackRotation || t.mmt_trackEffects.mmt_LocalSpace)
                        PpDrawProperty(item.FindPropertyRelative("mmt_DirectionDouble"), "Second Direction Offset", "Second direction offset in DEGREES of the second effect track [use mostly the Y field for rotation offset left & right]");
                    else
                        PpDrawProperty(item.FindPropertyRelative("mmt_DirectionDouble"), "Second Direction", "Global second direction of the second effect track");
                }
                PVE();

                PS(5);

                PV();
                PpDrawProperty(item.FindPropertyRelative("mmt_MotionSpeed"), "Motion Speed");
                PpDrawProperty(item.FindPropertyRelative("mmt_LinearMotion"), "Linear Motion", "If enabled, the track motion & lifetime will be linear, otherwise the track will exponentially slow down or faster up");
                PV();
                if (!t.mmt_trackEffects.mmt_LinearMotion)
                    PpDrawProperty(item.FindPropertyRelative("mmt_MotionDrag"), "Motion Drag", "Expontential value of the motion drag. The lower value is, the higher expontential value is");
                PVE();
                PVE();

                PS();

                PV();
                PpDrawProperty(item.FindPropertyRelative("mmt_adjustLifetimebyObjectSpeed"), "Adjust Life Time By Speed", "If enabled, the lifetime will adjust by the objects speed. The faster the object is, the lower lifetime value will be");
                PpDrawProperty(item.FindPropertyRelative("mmt_EffectLifetime"), t.mmt_trackEffects.mmt_adjustLifetimebyObjectSpeed ? "Lifetime Thresholder" : "Effect Lifetime", "Overall effect lifetime in SECONDS");
                PVE();

                PS();

                if (!m.mmt_TargetedToCPUBasedType)
                {
                    PV();
                    PpDrawProperty(item.FindPropertyRelative("mmt_ChangeBrushOpacity"), "Change Brush Opacity", "If enabled, the track will change the brush opacity by the overall lifetime (if there is any brush)");
                    if (t.mmt_trackEffects.mmt_ChangeBrushOpacity)
                    {
                        PpDrawProperty(item.FindPropertyRelative("mmt_StartBrushOpacity"), "Starting Opacity");
                        PpDrawProperty(item.FindPropertyRelative("mmt_EndBrushOpacity"), "Ending Opacity");
                        if (t.mmt_trackEffects.mmt_useTrackReferences && !t.mmt_TrackBrush)
                            EditorGUILayout.HelpBox("For using 'Brush Opacity' effect, the 'Track Brush' in reference track is required!", MessageType.Error);
                        else if (!t.mmt_trackEffects.mmt_useTrackReferences && !t.mmt_trackEffects.mmt_TrackBrush)
                            EditorGUILayout.HelpBox("For using 'Brush Opacity' effect, the 'Track Brush' is required!", MessageType.Error);
                    }
                    PVE();

                    PS();
                }
                else
                {
                    if (t.mmt_trackEffects.mmt_ChangeBrushOpacity) t.mmt_trackEffects.mmt_ChangeBrushOpacity = false;
                }

                PV();
                PpDrawProperty(item.FindPropertyRelative("mmt_ChangeTrackSize"), "Change Track Size", "If enabled, the track will change the track size by the overall lifetime");
                if (t.mmt_trackEffects.mmt_ChangeTrackSize)
                {
                    PpDrawProperty(item.FindPropertyRelative("mmt_StartTrackSize"), "Starting Size");
                    PpDrawProperty(item.FindPropertyRelative("mmt_EndTrackSize"), "Ending Size");
                }
                PVE();


                PS(5);
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}
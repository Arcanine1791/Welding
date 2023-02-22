using UnityEngine;
using UnityEditor;
using MeshTracker;

// Mesh Tracker Object - Editor.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    [CustomEditor(typeof(MeshTracker_Object))]
    [CanEditMultipleObjects]
    public class MeshTracker_Object_Editor : MeshTracker_EditorUtilities
    {
        public Texture2D _Editor_Banner;
        public bool enableDebug = true;

        private MeshTracker_Object m;

        private const string gpuBasedKeyword = "mmt_trackParamsGPUbased.";
        private const string cpuBasedKeyword = "mmt_trackParamsCPUbased.";

        private void OnEnable()
        {
            m = (MeshTracker_Object)target;
        }

        private void OnSceneGUI()
        {
            if (!enableDebug)
                return;
            if (m.mmt_UseGPUbasedType)
                return;

            float radius = m.mmt_trackParamsCPUbased.mmt_Radius;
            float forceDetect = m.mmt_trackParamsCPUbased.mmt_MinimumForceDetection;
            Handles.color = Color.magenta;
            Handles.DrawWireDisc(m.transform.position, Vector3.up, radius);
            Handles.color = Color.red;
            Handles.ArrowHandleCap(0, m.transform.position, Quaternion.Euler(-90, 0, 0), forceDetect, EventType.Repaint);

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.magenta;
            radius = Handles.ScaleValueHandle(radius, m.transform.position + new Vector3(radius, 0, 0), m.transform.rotation, 1, Handles.SphereHandleCap, 0.1f);
            Handles.color = Color.red;
            forceDetect = Handles.ScaleValueHandle(forceDetect, m.transform.position + new Vector3(0, forceDetect, 0), m.transform.rotation, 1, Handles.CircleHandleCap, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Radius");
                m.mmt_trackParamsCPUbased.mmt_Radius = radius;
                m.mmt_trackParamsCPUbased.mmt_MinimumForceDetection = forceDetect;
            }
        }

        public override void OnInspectorGUI()
        {
            if (target == null) return;

            PS();

            PL(_Editor_Banner);

            PV();
            PpDrawProperty("mmt_UseGPUbasedType", "GPU Based Type", "If enabled, the mesh track system will be set to 'GPU Based Type' [Shaders]. If disabled, the mesh tracker system will be set to 'CPU Based Type'. Please open documentation to read more...");
            PVE();

            switch (m.mmt_UseGPUbasedType)
            {
                //--------If using GPU TYPE
                case true:
                    {
                        PS(5);
                        EditorGUILayout.HelpBox("GPU-Based track system\n\nRequired components:\n• Mesh Renderer • Mesh Collider • Material [Shader - Mesh Tracker] •", MessageType.None);
                        PS(10);
                        PL("Canvas Settings", true);
                        PV();
                        PpDrawProperty(gpuBasedKeyword + "mmt_GenerateCustomCanvas", "Generate Custom Canvas", "If enabled, you will be able to customize starting canvas color tone without any reference");

                        if (!m.mmt_trackParamsGPUbased.mmt_GenerateCustomCanvas)
                            PpDrawProperty(gpuBasedKeyword + "mmt_Canvas", "Starting Canvas", "Starting displacement canvas [leave it empty if you dont need any starting canvas]");
                        else
                        {
                            PpDrawProperty(gpuBasedKeyword + "mmt_CustomCanvasTone", "Custom Canvas Tone", "Set the custom canvas tone. The higher value is, the brighter result will be (0 = black, 0.5 = gray, 1 = white)");
                            PH();
                            PL("Canvas Tone Result: ");
                            EditorGUILayout.ColorField(Color.white * m.mmt_trackParamsGPUbased.mmt_CustomCanvasTone);
                            PHE();
                        }
                        PpDrawProperty(gpuBasedKeyword + "mmt_CanvasQuality", "Canvas Quality", "Starting canvas quality in pixels (ratio 1:1)");
                        if (m.mmt_trackParamsGPUbased.mmt_CanvasQuality == MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x8192)
                            EditorGUILayout.HelpBox("Are you sure to use x8192 texture size? This might cause a performance drop!", MessageType.Warning);
                        if (m.mmt_trackParamsGPUbased.mmt_GenerateCustomCanvas && (m.mmt_trackParamsGPUbased.mmt_CanvasQuality == MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x8192 || m.mmt_trackParamsGPUbased.mmt_CanvasQuality == MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x4096))
                            EditorGUILayout.HelpBox("4K and 8K custom canvas qualities will cause longer application startup. Please consider your choice.", MessageType.Warning);
                        if (m.mmt_trackParamsGPUbased.mmt_SurfaceRepairShader && (m.mmt_trackParamsGPUbased.mmt_CanvasQuality == MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x8192 || m.mmt_trackParamsGPUbased.mmt_CanvasQuality == MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x4096))
                            EditorGUILayout.HelpBox("4K and 8K canvas qualities might slow down your performance while the 'Surface Repair' is enabled. 1024 or 2K are recommended as default!", MessageType.Warning);

                        PVE();
                        PS(20);
                        PL("Repair Settings", true);
                        PV();
                        PpDrawProperty(gpuBasedKeyword + "mmt_SurfaceRepairShader", "Repair Surface", "If enabled, you will be able to 'repair' deformed surface to its original shape");
                        if (m.mmt_trackParamsGPUbased.mmt_SurfaceRepairShader)
                        {
                            if (m.mmt_trackParamsGPUbased.mmt_Canvas || m.mmt_trackParamsGPUbased.mmt_GenerateCustomCanvas)
                            {
                                if (PB("Repair Surface Settings"))
                                    MeshTracker_ModifyRepairTextureWindow.Init(m);
                                PpDrawProperty(gpuBasedKeyword + "mmt_RepairBrushType", "Repair Brush Type", "(optional) Add more details to the repair process of your canvas with custom Brush Type");
                            }
                            else EditorGUILayout.HelpBox("Please enter 'Canvas' texture or swith to 'Generate Custom Canvas' to access the Repair settings.", MessageType.Error);
                        }
                        PVE();

                        if (Application.isPlaying)
                        {
                            PS();
                            PV();
                            if (PB("Save Surface Canvas"))
                            {
                                string sfp = EditorUtility.SaveFilePanel("Save Canvas", Application.dataPath, "SurfCanvas", "png");
                                if (!string.IsNullOrEmpty(sfp))
                                    m.FGPUbased_SaveCurrentCanvas(sfp);
                            }
                            PVE();
                        }
                    }
                    break;

                //--------If using CPU TYPE
                case false:
                    {
                        PS(5);
                        EditorGUILayout.HelpBox("CPU-Based track system\n\nRequired components:\n• Mesh Renderer • Mesh Collider •", MessageType.None);
                        PS();
                        PL("General Settings", true);
                        PV();
                        PpDrawProperty(cpuBasedKeyword + "mmt_MultithreadingSupported", "Multithreading Supported", "If enabled, the mesh will be ready for complex operations.");
                        if (m.mmt_trackParamsCPUbased.mmt_MultithreadingSupported)
                        {
                            PpDrawProperty(cpuBasedKeyword + "mmt_ThreadSleep", "Thread Sleep", "Overall thread sleep (in miliseconds; The lower value is, the faster thread processing will be; but more performance it may take)");
                            EditorGUILayout.HelpBox("The Mesh Tracker_Object CPU-Based system is ready for complex meshes and will create a new separated thread", MessageType.None);
                        }
                        PS(5);
                        PV();
                        PpDrawProperty(cpuBasedKeyword + "mmt_Direction", "Overall Direction", "Direction of vertices after interaction");
                        PVE();
                        PV();
                        PpDrawProperty(cpuBasedKeyword + "mmt_ExponentialDeformation", "Exponential Deform", "If enabled, the mesh will be deformed expontentially (the results will be much smoother)");
                        if (m.mmt_trackParamsCPUbased.mmt_ExponentialDeformation)
                            PpDrawProperty(cpuBasedKeyword + "mmt_InstantRadius", "Instant Radius Size", "If 'Exponential Deform' is enabled, vertices inside the 'Instant Radius' will be instantly affected This will be subtracted from the input radius");
                        PVE();
                        if (m.mmt_trackParamsCPUbased.mmt_RigidbodiesAllowed)
                        {
                            PV();
                            PpDrawProperty(cpuBasedKeyword + "mmt_AdjustTrackSizeToInputSize", "Adjust To Input Object Size", "Adjust radius size by collided object size. This will set the overall interaction radius to the input radius parameter (recommended if Allow Rigidbodies is enabled)");
                            if (!m.mmt_trackParamsCPUbased.mmt_AdjustTrackSizeToInputSize)
                                PpDrawProperty(cpuBasedKeyword + "mmt_Radius", "Interactive Radius", "Radius of vertices to be interacted");
                            PVE();
                        }
                        PVE();

                        PS(20);
                        PL("Conditions", true);
                        PV();
                        PV();
                        PpDrawProperty(cpuBasedKeyword + "mmt_RigidbodiesAllowed", "Allow Rigidbodies", "Allow Collision Enter & Collision Stay functions for Rigidbodies & other physically-based entities");
                        if (m.mmt_trackParamsCPUbased.mmt_RigidbodiesAllowed)
                        {
                            PpDrawProperty(cpuBasedKeyword + "mmt_MinimumForceDetection", "Force Detection Level", "Minimum rigidbody velocity detection [zero is default = without detection]");
                            PV();
                            PpDrawProperty(cpuBasedKeyword + "mmt_CollideWithSpecificObjects", "Collision With Specific Tag", "If enabled, collision will be occured only with included tag below...");
                            if (m.mmt_trackParamsCPUbased.mmt_CollideWithSpecificObjects)
                                PpDrawProperty(cpuBasedKeyword + "mmt_CollisionTag", "Collision Tag");
                            PVE();
                        }
                        PVE();
                        PVE();
                        if (m.mmt_trackParamsCPUbased.mmt_MultithreadingSupported == false)
                        {
                            PS(20);
                            PL("Additional Interaction Settings", true);
                            PV();
                            PpDrawProperty(cpuBasedKeyword + "mmt_CustomInteractionSpeed", "Custom Interaction Speed", "If enabled, you will be able to customize vertices speed after its interaction/ collision");
                            if (m.mmt_trackParamsCPUbased.mmt_CustomInteractionSpeed)
                            {
                                PV();
                                PpDrawProperty(cpuBasedKeyword + "mmt_InteractionSpeed", "Interaction Speed");
                                PpDrawProperty(cpuBasedKeyword + "mmt_ContinuousEffect", "Enable Continuous Effect", "If enabled, interacted vertices will keep moving deeper");
                                PVE();
                            }
                            PVE();

                            PV();
                            PpDrawProperty(cpuBasedKeyword + "mmt_RepairSurface", "Repair Mesh", "Repair mesh after some time and interval");
                            if (m.mmt_trackParamsCPUbased.mmt_RepairSurface)
                                PpDrawProperty(cpuBasedKeyword + "mmt_RepairSpeed", "Repair Speed");
                            PVE();
                        }
                        PS();
                        enableDebug = GUILayout.Toggle(enableDebug, "Enable Scene Debug");
                        PS(20);
                    }
                    break;
            }

            PS(20);
            PL("Collider Settings", true);

            Color guiColor;
            ColorUtility.TryParseHtmlString("#c7ffd6", out guiColor);
            GUI.color = guiColor;

            PV();
            PpDrawProperty("mmt_Collider_GenerateMeshCollider", "Generate Mesh Collider", "If enabled, mesh track system will generate collider with specified attributes below");
            if (m.mmt_Collider_GenerateMeshCollider)
            {
                PpDrawProperty("mmt_Collider_Convex", "Convex Mesh Collider", "If enabled, generated mesh collider will be convexed");
                PpDrawProperty("mmt_Collider_CookingOptions", "Cooking Options", "Select proper option of your need");
                PpDrawProperty("mmt_Collider_ColliderOffset", "Collider Offset", "Generated Collider Offset");
            }
            PVE();

            PS(10);

            if (target != null) serializedObject.Update();
        }
    }
}
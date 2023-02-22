using UnityEngine;
using UnityEditor;
using MeshTracker;

// Mesh Tracker RepairTextureModification - Window.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    public class MeshTracker_ModifyRepairTextureWindow : EditorWindow
    {
        public static MeshTracker_Object Sender;

        private static MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality qualitySize = MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x1024;

        private static float repairSpeed;
        private static float repairInterval;

        public static void Init(MeshTracker_Object send)
        {
            Sender = send;
            repairSpeed = Sender.mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderSpeed;
            repairInterval = Sender.mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderInterval;
            qualitySize = Sender.mmt_trackParamsGPUbased.mmt_CanvasQuality;
            MeshTracker_ModifyRepairTextureWindow w = (MeshTracker_ModifyRepairTextureWindow)GetWindow(typeof(MeshTracker_ModifyRepairTextureWindow));
            w.minSize = new Vector2(300, 315);
            w.maxSize = new Vector2(310, 325);
            w.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(15);
            GUILayout.Label("Repair Surface - Shader Settings");
            GUILayout.Space(10);
            GUILayout.Label("Presets");
            GUILayout.BeginVertical("Box");
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Very Slow"))
            {
                switch (qualitySize)
                {
                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x2048:
                        repairSpeed = 0.005f;
                        repairInterval = 0.1f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x1024:
                        repairSpeed = 0.005f;
                        repairInterval = 0.2f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x512:
                        repairSpeed = 0.005f;
                        repairInterval = 0.3f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x256:
                        repairSpeed = 0.005f;
                        repairInterval = 0.4f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x128:
                        repairSpeed = 0.005f;
                        repairInterval = 0.5f;
                        break;

                    default:
                        repairSpeed = 0.005f;
                        repairInterval = 0.1f;
                        break;
                }
            }
            if (GUILayout.Button("Slow"))
            {
                switch (qualitySize)
                {
                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x2048:
                        repairSpeed = 0.01f;
                        repairInterval = 0.1f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x1024:
                        repairSpeed = 0.01f;
                        repairInterval = 0.2f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x512:
                        repairSpeed = 0.01f;
                        repairInterval = 0.3f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x256:
                        repairSpeed = 0.01f;
                        repairInterval = 0.4f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x128:
                        repairSpeed = 0.01f;
                        repairInterval = 0.5f;
                        break;

                    default:
                        repairSpeed = 0.01f;
                        repairInterval = 0.1f;
                        break;
                }
            }
            if (GUILayout.Button("Fast"))
            {
                switch (qualitySize)
                {
                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x2048:
                        repairSpeed = 0.08f;
                        repairInterval = 0.035f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x1024:
                        repairSpeed = 0.12f;
                        repairInterval = 0.05f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x512:
                        repairSpeed = 0.15f;
                        repairInterval = 0.06f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x256:
                        repairSpeed = 0.18f;
                        repairInterval = 0.07f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x128:
                        repairSpeed = 0.25f;
                        repairInterval = 0.08f;
                        break;

                    default:
                        repairSpeed = 0.08f;
                        repairInterval = 0.035f;
                        break;
                }
            }
            if (GUILayout.Button("Very Fast"))
            {
                switch (qualitySize)
                {
                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x2048:
                        repairSpeed = 0.1f;
                        repairInterval = 0.035f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x1024:
                        repairSpeed = 0.15f;
                        repairInterval = 0.036f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x512:
                        repairSpeed = 0.18f;
                        repairInterval = 0.038f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x256:
                        repairSpeed = 0.2f;
                        repairInterval = 0.04f;
                        break;

                    case MeshTracker_Object.TrackParams_GPUbased.Mt_CanvasQuality.x128:
                        repairSpeed = 0.3f;
                        repairInterval = 0.05f;
                        break;

                    default:
                        repairSpeed = 0.1f;
                        repairInterval = 0.035f;
                        break;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Repair Speed [alpha value] " + repairSpeed);
            repairSpeed = GUILayout.HorizontalSlider(repairSpeed, 0.0001f, 0.5f);
            GUILayout.Space(14);
            repairSpeed = EditorGUILayout.FloatField(repairSpeed);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.BeginVertical("Box");
            GUILayout.Label("Repair Interval [in seconds] " + repairInterval);
            repairInterval = GUILayout.HorizontalSlider(repairInterval, 0.035f, 60);
            GUILayout.Space(14);
            repairInterval = EditorGUILayout.FloatField(repairInterval);
            GUILayout.EndVertical();

            if (GUILayout.Button("Edit & Apply"))
            {
                Sender.mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderSpeed = repairSpeed;
                Sender.mmt_trackParamsGPUbased.mmt_SurfaceRepairShaderInterval = repairInterval;

                Sender = null;
                AssetDatabase.Refresh();
                Close();
            }
        }
    }
}
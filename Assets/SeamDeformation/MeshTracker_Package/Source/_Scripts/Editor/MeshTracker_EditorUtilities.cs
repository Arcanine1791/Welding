using UnityEngine;
using UnityEditor;

// Essential editor utilities for internal purposes.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    public class MeshTracker_EditorUtilities : Editor
    {
        protected void PpDrawProperty(SerializedProperty p, string Text, string ToolTip = "", bool includeChilds = false)
        {
            try
            {
                EditorGUILayout.PropertyField(p, new GUIContent(Text, ToolTip), includeChilds, null);
                serializedObject.ApplyModifiedProperties();
            }
            catch
            {  }
        }
        protected void PpDrawProperty(string p, string Text, string ToolTip = "", bool includeChilds = false)
        {
            try
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(p), new GUIContent(Text, ToolTip), includeChilds, null);
                serializedObject.ApplyModifiedProperties();
            }
            catch
            {  }
        }

        protected void PL(string s, bool bold = false)
        {
            if(bold)
                GUILayout.Label(s, EditorStyles.boldLabel);
            else
                GUILayout.Label(s);
        }
        protected void PL(Texture2D s)
        {
            GUILayout.Label(s);
        }
        protected void PV(bool box = true)
        {
            if (!box) GUILayout.BeginVertical();
            else GUILayout.BeginVertical("Box");
        }
        protected void PVE()
        {
            GUILayout.EndVertical();
        }
        protected void PH(bool box = true)
        {
            if (!box) GUILayout.BeginHorizontal();
            else GUILayout.BeginHorizontal("Box");
        }
        protected void PHE()
        {
            GUILayout.EndHorizontal();
        }
        protected bool PB(string s)
        {
            return GUILayout.Button(s);
        }
        protected void PS(int s = 10)
        {
            GUILayout.Space(s);
        }
    }

    public class MeshTracker_MaterialEditorUtilities : ShaderGUI
    {
        protected bool PpDrawProperty(MaterialEditor matSrc, MaterialProperty[] props, string p, bool texture = false, string tooltip = "")
        {
            bool found = false;
            foreach (MaterialProperty prop in props)
            {
                if (prop.name == p)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
            MaterialProperty property = FindProperty(p, props);
            if (!texture)  matSrc.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
            else
            {
                Rect last = EditorGUILayout.GetControlRect();
                matSrc.TexturePropertyMiniThumbnail(last, property, property.displayName, "");
            }

            return true;
        }
        protected bool PpCompareProperty(MaterialEditor matSrc, string a, float b)
        {
            return MaterialEditor.GetMaterialProperty(matSrc.serializedObject.targetObjects, a).floatValue == b;
        }

        protected void PL(string s)
        {
            GUILayout.Label(s);
        }
        protected void PL(Texture2D s)
        {
            GUILayout.Label(s);
        }
        protected void PV(bool box = true)
        {
            if (!box) GUILayout.BeginVertical();
            else GUILayout.BeginVertical("Box");
        }
        protected void PVE()
        {
            GUILayout.EndVertical();
        }
        protected void PH(bool box = true)
        {
            if (!box) GUILayout.BeginHorizontal();
            else GUILayout.BeginHorizontal("Box");
        }
        protected void PHE()
        {
            GUILayout.EndHorizontal();
        }
        protected bool PB(string s)
        {
            return GUILayout.Button(s);
        }
        protected void PS(int s = 10)
        {
            GUILayout.Space(s);
        }
    }
}
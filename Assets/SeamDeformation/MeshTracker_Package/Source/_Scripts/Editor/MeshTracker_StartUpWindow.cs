using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Mesh Tracker Startup Window - Editor.
// Author: Matej Vanco, originally written in 2018, updated in 2022.

namespace MeshTrackerEditor
{
    public class MeshTracker_StartUpWindow : EditorWindow
    {
        public Texture2D Logo;

        public Texture2D Home;
        public Texture2D Web;
        public Texture2D Doc;
        public Texture2D Discord;

        public Font font;
        private GUIStyle style;

        private const string INTERNAL_TrackCreatorDownloadLink = "https://mega.nz/#!i9oGEIAa!L9X0BMm_6atSdlvyGsA-Z_6JlGWwRJhTaXltu8uYZIk";
        private const string INTERNAL_ExampleContentBUILTINDownloadLink = "https://mega.nz/file/j1Q2kCAa#7hW1vMcwqeXToHOFVJAwZOghxyutbzwomPZghZuQtHg";
        private const string INTERNAL_ExampleContentURPDownloadLink = "https://mega.nz/file/rgBwWQaB#SCbVoSFMueq-QUKd1B2a7W2oS4PnlEMxXZJR6rVSdnY";
        private const string INTERNAL_DiscordLink = "https://discord.gg/Ztr8ghQKqC";

        [MenuItem("Window/Mesh Tracker/StartUp Window")]
        public static void Init()
        {
            MeshTracker_StartUpWindow md = (MeshTracker_StartUpWindow)GetWindow(typeof(MeshTracker_StartUpWindow));
            md.maxSize = new Vector2(400, 480);
            md.minSize = new Vector2(399, 481);
            md.Show();
        }

        private void OnGUI()
        {
            style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.font = font;
            GUILayout.Label(Logo);
            style.fontSize = 13;
            style.wordWrap = true;

            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Mesh Tracker version 2.0.0\n[Realtime CPU & GPU Simulations]", style);
            GUILayout.EndVertical();

            style.alignment = TextAnchor.UpperLeft;
            GUILayout.Space(5);
            style.fontSize = 12;

            GUILayout.BeginVertical("Box");
            GUILayout.Label("<size=14><color=#87e9f0>Mesh Tracker version 2.0.0  [16/10/2022 <size=8>dd/mm/yyyy</size>]</color></size>\n\n" +
                "- Added multiple axes to the Smart Tracks rotation\n" +
                "- Updated Surface Drawing component\n" +
                "- URP support\n" +
                "- Major shader refactor\n" +
                "- Major code refactor & scene clean-up", style);
            GUILayout.Space(12);
            GUILayout.EndVertical();

            GUILayout.Label("  No idea where to start? Open Documentation for more!", style);

            GUILayout.Space(5);

            style = new GUIStyle(GUI.skin.button);
            style.imagePosition = ImagePosition.ImageAbove;

            GUILayout.BeginHorizontal("Box");
            if(GUILayout.Button(new GUIContent("Take Me Home",Home), style))
            {
                try
                {
                    EditorSceneManager.OpenScene(Application.dataPath + "/MeshTracker_Package/Examples/Scenes/Introduction.unity");
                }
                catch
                {
                    Debug.LogError("I can't take you home! Directory with example scenes couldn't be found. Required path: [" + Application.dataPath + "/MeshTracker_Package/Examples/Scenes/]");
                }
            }
            if (GUILayout.Button(new GUIContent("Creator's Webpage", Web),style))
                Application.OpenURL("https://matejvanco.com");

            if (GUILayout.Button(new GUIContent("Open Documentation", Doc), style))
            {
                try
                {
                    System.Diagnostics.Process.Start(Application.dataPath + "/MeshTracker_Package/MeshTracker_Documentation.pdf");
                }
                catch
                {
                    Debug.LogError("Documentation could not be found! Required path: "+ Application.dataPath + "/MeshTracker_Package/MeshTracker_Documentation.pdf");
                }
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button(new GUIContent(Discord)))
                Application.OpenURL(INTERNAL_DiscordLink);
            if (GUILayout.Button(new GUIContent("Download Latest Example Content [Built-In]"), style))
                Application.OpenURL(INTERNAL_ExampleContentBUILTINDownloadLink);
            if (GUILayout.Button(new GUIContent("Download Latest Example Content [URP]"), style))
                Application.OpenURL(INTERNAL_ExampleContentURPDownloadLink);
            if (GUILayout.Button(new GUIContent("Download TrackCreator [V2 - PC_Windows]"), style))
                Application.OpenURL(INTERNAL_TrackCreatorDownloadLink);
        }
    }
}
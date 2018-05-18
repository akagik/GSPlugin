using UnityEngine;
using UnityEditor;
using System.IO;

namespace GSPlugin
{
    public class GSEditorWindow : EditorWindow
    {
        public GSPluginSettings settings;
        private bool isDownloading;

        [MenuItem("Window/GSPlugin", false, 0)]
        static public void OpenWindow()
        {
            EditorWindow.GetWindow<GSEditorWindow>(false, "GSPlugin", true).Show();
        }

        void OnEnable()
        {
            string[] settingGUIDArray = AssetDatabase.FindAssets("t:GSPluginSettings");

            foreach (string guid in settingGUIDArray)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                settings = AssetDatabase.LoadAssetAtPath<GSPluginSettings>(path);
            }
        }

        void OnGUI()
        {
            settings = EditorGUILayout.ObjectField("Settings", settings, typeof(GSPluginSettings), false) as GSPluginSettings;

            if (GUILayout.Button("Download", "LargeButtonMid") && !isDownloading)
            {
                isDownloading = true;

                float progress = 0f;
                int i = 0;
                show_progress(progress, i, settings.sheets.Length);

                foreach (var ss in settings.sheets)
                {
                    string sheetId = ss.sheetId;
                    string gid = ss.gid;

                    CsvData csvData = GSLoader.LoadGS(sheetId, gid);

                    if (csvData != null)
                    {
                        string fileName = string.Format("{0}.{1}", ss.fileName, ss.isCsv ? "csv" : "asset");
                        string path = Path.Combine("Assets", Path.Combine(ss.downloadFolder, fileName));

                        if (ss.isCsv)
                        {
                            using (var s = new StreamWriter(path))
                            {
                                s.Write(csvData.ToString());
                            }
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(csvData, path);
                        }
                        progress = (float)(++i) / settings.sheets.Length;
                        Debug.Log("Write " + path);
                        show_progress(progress, i, settings.sheets.Length);
                    }
                    else
                    {
                        Debug.LogError("Fails for " + ss.ToString());
                    }
                }

                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
                isDownloading = false;
            }
        }

        private void show_progress(float progress, int i, int total) {
            EditorUtility.DisplayProgressBar("Progress", progress_msg(i, total), progress);
        }

        private string progress_msg(int i, int total) 
        {
            return string.Format("Downloading ({0}/{1})", i, total);
        }

        //void OnInspectorUpdate()
        //{
        //    Repaint();
        //}
    }
}

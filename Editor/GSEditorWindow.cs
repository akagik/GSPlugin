using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GSPlugin {
    public class GSEditorWindow : EditorWindow {
        public static string SETTINGS_KEY = "GSPlugin/settings";

        public GSPluginSettings settings;
        private bool isDownloading;
        private Vector2 scrollPosition;

        // 二重に保存しないようにするために導入
        private string savedGUID;

        [MenuItem("Window/GSPlugin", false, 0)]
        static public void OpenWindow() {
            EditorWindow.GetWindow<GSEditorWindow>(false, "GSPlugin", true).Show();
        }

        void OnEnable() {
            string guid = EditorUserSettings.GetConfigValue(SETTINGS_KEY);
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (path != "") {
                // Debug.Log("Found prefs settings GUID: " + EditorPrefs.GetString(SETTINGS_KEY));
                settings = AssetDatabase.LoadAssetAtPath<GSPluginSettings>(path);
                // string[] settingGUIDArray = AssetDatabase.FindAssets("t:GSPluginSettings");
            }
            else {
                // Debug.Log("Not Found GUID");
            }

            // foreach (string guid in settingGUIDArray) {
            //     string path = AssetDatabase.GUIDToAssetPath(guid);
            //     settings = AssetDatabase.LoadAssetAtPath<GSPluginSettings>(path);
            // }
        }

        void OnGUI() {
            settings = EditorGUILayout.ObjectField("Settings", settings, typeof(GSPluginSettings), false) as GSPluginSettings;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (settings != null) {
                // セットされている settings 情報を EditorUserSettings に保存する.
                {
                    string guid;
                    long localId;

                    if(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(settings, out guid, out localId)) {
                        if (savedGUID != guid) {
                            // Debug.Log("Save GUID(" + guid + ") at " + SETTINGS_KEY);
                            EditorPrefs.SetString(SETTINGS_KEY, guid);
                            EditorUserSettings.SetConfigValue(SETTINGS_KEY, guid);
                            savedGUID = guid;
                        }
                    }
                }

                for (int i = 0; i < settings.sheets.Length; i++) {
                    var sheet = settings.sheets[i];

                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label(sheet.targetPath);

                    if (GUILayout.Button("Download", GUILayout.Width(80)) && !isDownloading) {
                        isDownloading = true;
                        DownloadOne(sheet);
                        isDownloading = false;

                        GUIUtility.ExitGUI();
                    }

                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("DownloadAll", "LargeButtonMid") && !isDownloading) {
                    isDownloading = true;

                    var sheets = new List<GSPluginSettings.Sheet>(settings.sheets);
                    DownloadAll(sheets);
                    isDownloading = false;

                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.EndScrollView();
        }

        private void DownloadAll(List<GSPluginSettings.Sheet> sheets) {
            float progress = 0f;
            int i = 0;

            foreach (var ss in sheets) {
                show_progress(ss.targetPath, progress, i, sheets.Count);
                Download(ss);

                progress = (float)(++i) / sheets.Count;
            }
            show_progress("", progress, i, sheets.Count);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private void DownloadOne(GSPluginSettings.Sheet sheet) {
            show_progress(sheet.targetPath, 0f, 0, 1);
            Download(sheet);
            show_progress(sheet.targetPath, 1f, 1, 1);
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public void Download(GSPluginSettings.Sheet ss) {
            string sheetId = ss.sheetId;
            string gid = ss.gid;

            CsvData csvData = GSLoader.LoadGS(sheetId, gid);

            if (csvData != null) {
                if (ss.isCsv) {
                    if (!Directory.Exists(Path.Combine("Assets", ss.downloadFolder))) {
                        Debug.LogError("指定のフォルダは存在しません: " + ss.downloadFolder);
                        return;
                    }

                    using (var s = new StreamWriter(ss.targetPath)) {
                        s.Write(csvData.ToString());
                    }
                }
                else {
                    AssetDatabase.CreateAsset(csvData, ss.targetPath);
                }
                Debug.Log("Write " + ss.targetPath);
            }
            else {
                Debug.LogError("Fails for " + ss.ToString());
            }
        }

        private void show_progress(string path, float progress, int i, int total) {
            EditorUtility.DisplayProgressBar("Progress", progress_msg(path, i, total), progress);
        }

        private string progress_msg(string path, int i, int total) {
            return string.Format("Downloading {0} ({1}/{2})", path, i, total);
        }

        //void OnInspectorUpdate()
        //{
        //    Repaint();
        //}
    }
}

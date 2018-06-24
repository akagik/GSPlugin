using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GSPlugin {
    public class GSPluginSettings : ScriptableObject {
        public Sheet[] sheets;

        [MenuItem("Assets/Create/GSPluginSettings")]
        public static void Create() {
            GSPluginSettings o = ScriptableObject.CreateInstance<GSPluginSettings>();
            create<GSPluginSettings>(o);
        }

        private static void create<T>(T t) where T : UnityEngine.Object {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (path == "") {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(path), "");
            }

            string filePath = Path.Combine(path, "Settings.asset");
            AssetDatabase.CreateAsset(t, AssetDatabase.GenerateUniqueAssetPath(filePath));
            AssetDatabase.Refresh();
        }

        [Serializable]
        public class Sheet {
            public string fileName;
            public string downloadFolder;

            public string targetPath {
                get {
                    return Path.Combine("Assets", Path.Combine(downloadFolder, fileNameWithExt));
                }
            }

            public string sheetId;
            public string gid;

            public bool isCsv = true;

            public override string ToString() {
                return string.Format("SheetSettings(sheetId=\"{0}\", gid=\"{1}\")", sheetId, gid);
            }

            public string fileNameWithExt {
                get {
                    return string.Format("{0}.{1}", fileName, isCsv ? "csv" : "asset");
                }
            }
        }
    }
}

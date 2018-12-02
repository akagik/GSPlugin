using System;
using UnityEngine;
using System.Collections.Generic;

namespace GSPlugin {
    public class CsvData : ScriptableObject {
        public Row[] content;

        public int row {
            get {
                return content.Length;
            }
        }

        public int col {
            get {
                return content[0].data.Length;
            }
        }

        [Serializable]
        public class Row {
            public string[] data;

            public Row(int col) {
                data = new string[col];
            }
        }

        public string Get(int i, int j) {
            return content[i].data[j];
        }

        public void Set(int i, int j, string v) {
            content[i].data[j] = v;
        }

        public static Row[] CreateTable(int row, int col) {
            Row[] rows = new Row[row];
            for (int i = 0; i < row; i++) {
                rows[i] = new Row(col);
            }
            return rows;
        }

        public void SetFromList(List<List<string>> list) {
            int maxCol = -1;

            foreach (List<string> row in list) {
                if (row.Count > maxCol) {
                    maxCol = row.Count;
                }
            }

            content = CreateTable(list.Count, maxCol);

            for (int i = 0; i < row; i++) {
                for (int j = 0; j < col; j++) {
                    if (j < list[i].Count) {
                        Set(i, j, list[i][j]);
                    }
                    else {
                        Set(i, j, "");
                    }
                }
            }
        }

        public override string ToString() {
            string s = "";

            for (int i = 0; i < row; i++) {
                for (int j = 0; j < col; j++) {
                    string value = Get(i, j);
                    value = value.Replace("\"", "\"\"");
                    value = value.Replace("\n", "\\n");
                    s += "\"" + value + "\", ";
                }
                s = s.Substring(0, s.Length - 2);
                s += "\n";
            }

            return s;
        }
    }

}

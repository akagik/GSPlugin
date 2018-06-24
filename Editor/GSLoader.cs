using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace GSPlugin {
    // スプレッドシートをウェブから引っ張ってきて CsvData に変換する.
    // エントリポイントは LoadGS.
    public class GSLoader {
        public static readonly string WORKSHEET_LIST_URL = "https://spreadsheets.google.com/feeds/worksheets/{0}/public/full?alt=json";

        public static string ExtractGID(string url) {
            Uri uri = new Uri(url);
            string query = uri.Query;
            string gid = "";

            int firstIndex = query.IndexOf("gid=", StringComparison.OrdinalIgnoreCase) + 4;

            string leftovers = query.Substring(firstIndex);

            int len = leftovers.IndexOf("&", StringComparison.OrdinalIgnoreCase) + 1;

            if (len == 0) {
                gid = query.Substring(firstIndex);
            }
            else {
                gid = query.Substring(firstIndex, len - 1);
            }

            return gid;
        }

        /// <summary>
        /// 指定の sheetId, gid のシートを CsvData としてダウンロードする.
        /// 
        /// Google Spreadsheet URL に2つのリクエストを送る。
        /// まず一つ目は sheetId に対する複数の worksheet を持つ URL へのリクエスト。
        /// 二つ目は その sheetId の中で特定の gid を持つ URL へのリクエスト。
        /// </summary>
        public static CsvData LoadGS(string sheetId, string gid) {
            // Load spread sheet
            List<GSWorksheet> worksheets = downloadWorksheets(sheetId);

            if (worksheets == null) {
                return null;
            }

            // Load each worksheet
            foreach (GSWorksheet sheet in worksheets) {
                if (sheet.gid != gid) {
                    continue;
                }

                CsvData csv = LoadCsvData(sheet);

                if (csv != null) {
                    return csv;
                }
            }
            return null;
        }

        public static CsvData LoadCsvData(GSWorksheet sheet) {
            string title = sheet.title;
            string url = sheet.link;

            UnityWebRequest req = UnityWebRequest.Get(url);

            var op = req.SendWebRequest();

            while (!op.isDone) {
            }

            if (req.isNetworkError) {
                Debug.Log(req.error);
                return null;
            }
            else if (req.responseCode != 200) {
                return null;
            }

            string resJson = req.downloadHandler.text;

            resJson = resJson.Replace("$", "_d_");
            GSResponse response = JsonUtility.FromJson<GSResponse>(resJson);

            if (response == null || response.feed == null) {
                return null;
            }

            List<List<string>> cellList = new List<List<string>>();

            foreach (GSResponse.Entry entry in response.feed.entry) {
                var cell = entry.gs_d_cell;

                int row = cell.row;
                int col = cell.col;
                string inputValue = cell._d_t;

                while (cellList.Count < row)
                    cellList.Add(new List<string>());

                while (cellList[row - 1].Count < col)
                    cellList[row - 1].Add(string.Empty);

                cellList[row - 1][col - 1] = inputValue;
            }

            CsvData csv = ScriptableObject.CreateInstance<CsvData>();
            csv.SetFromList(cellList);
            return csv;
        }


        static List<GSWorksheet> downloadWorksheets(string sheetId) {
            string url_worksheet = string.Format(WORKSHEET_LIST_URL, sheetId);
            UnityWebRequest req = UnityWebRequest.Get(url_worksheet);

            var op = req.SendWebRequest();

            while (!op.isDone) {
            }

            if (req.isNetworkError) {
                Debug.Log(req.error);
                return null;
            }
            else if (req.responseCode != 200) {
                return null;
            }

            string workSheetText = req.downloadHandler.text;
            workSheetText = workSheetText.Replace("$", "_d_");
            GSResponse response = JsonUtility.FromJson<GSResponse>(workSheetText);

            if (response == null || response.feed == null) {
                return null;
            }

            List<GSWorksheet> worksheets = new List<GSWorksheet>();

            foreach (GSResponse.Entry entry in response.feed.entry) {
                string worksheetTitle = entry.title._d_t;

                GSWorksheet worksheet = new GSWorksheet(worksheetTitle);
                bool gid_check = false;
                bool cells_check = false;

                foreach (GSResponse.Link link in entry.link) {
                    string url = link.href;

                    if (url.Contains("/cells/")) {
                        worksheet.link = url + "?alt=json";
                        cells_check = true;
                    }

                    if (url.Contains("gid=")) {
                        worksheet.gid = ExtractGID(url);
                        gid_check = true;
                    }

                    if (cells_check && gid_check) {
                        break;
                    }
                }
                worksheets.Add(worksheet);
            }
            return worksheets;
        }

        public class GSWorksheet {
            public string title;
            public string link;
            public string gid;

            public GSWorksheet(string title) {
                this.title = title;
            }
        }
    }
}

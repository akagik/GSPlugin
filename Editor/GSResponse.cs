using System;

namespace GSPlugin {
    /// <summary>
    /// Google Spreadsheet URL からのレスポンスの json を
    /// このオブジェクトに変換する.
    /// 
    /// 2つのレスポンス両方に対応している。
    /// 
    /// !!!"$" を "_d_" としてみなしている点に注意!!!
    /// </summary>
    [Serializable]
    public class GSResponse {
        public Feed feed;

        [Serializable]
        public class Feed {
            public Title title;
            public Entry[] entry;
        }

        [Serializable]
        public class Title {
            public string type;
            public string _d_t;
        }

        [Serializable]
        public class Entry {
            public Title title;
            public Link[] link;
            public Cell gs_d_cell;
        }

        [Serializable]
        public class Link {
            public string rel;
            public string type;
            public string href;
        }

        [Serializable]
        public class Cell {
            public int row;
            public int col;
            public string inputValue;
            public string _d_t;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordSegmentation
{
    public class WordInfo
    {
        /// <summary>
        /// 文档频率
        /// </summary>
        public float Freq { get; set; }

        /// <summary>
        /// 逆文档频率
        /// </summary>
        public float IDF { get; set; }

        public int RowNumber { get; set; }
    }
}

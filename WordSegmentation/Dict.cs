using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WordSegmentation
{

    public static class Dict
    {
        private static Hashtable _trie;

        private static Dictionary<string, WordInfo> _wordExtraInfos;

        private static HashSet<string> _stopwords;

        private static float _minFreq;

        private static int _minCount;

        private static long _totalCount;

        private static readonly object syncTrieRoot = new object();

        private static readonly object syncStopwordRoot = new object();

        /// <summary>
        /// 单词查找树
        /// </summary>
        public static Hashtable Trie
        {
            get
            {
                Init();
                return _trie;
            }
        }

        /// <summary>
        /// 词语附加信息
        /// </summary>
        public static Dictionary<string, WordInfo> WordExtraInfos
        {
            get
            {
                Init();
                return _wordExtraInfos;
            }
        }

        /// <summary>
        /// 停止词
        /// </summary>
        public static HashSet<string> StopWords
        {
            get
            {
                if(_stopwords==null)
                {
                    lock (syncStopwordRoot)
                    {
                        if(_stopwords==null)
                        {
                            using (StreamReader reader=new StreamReader("stopword.txt"))
                            {
                                _stopwords = new HashSet<string>();
                                while (!reader.EndOfStream)
                                {
                                    string word = reader.ReadLine();
                                    if(!string.IsNullOrEmpty(word))
                                        _stopwords.Add(word);
                                }
                            }
                        }
                    }
                }

                return _stopwords;
            }
        }

        /// <summary>
        /// 最小文档频率
        /// </summary>
        public static float MinFreq
        {
            get
            {
                Init();
                return _minFreq;
            }
        }

        public static void Init(string dictFile = "dict.txt")
        {
            if (_trie == null)
            {
                lock (syncTrieRoot)
                {
                    if (_trie == null)
                    {
                        LoadDict(dictFile);
                    }
                }
            }
        }

        private static void LoadDict(string dictFile)
        {
            using (StreamReader reader = new StreamReader(dictFile))
            {
                _trie = new Hashtable();
                _wordExtraInfos = new Dictionary<string, WordInfo>();

                string line;
                int rn = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    Hashtable root = _trie;

                    string[] arrOfLine = line.Split(' ');
                    string word = arrOfLine[0];
                    int count = int.Parse(arrOfLine[1]);

                    //构造单词查找表
                    for (int i = 0; i < word.Length; i++)
                    {
                        string key = word.Substring(i, 1);
                        if (!root.ContainsKey(key))
                        {
                            root.Add(key, new Hashtable());
                        }
                        root = (Hashtable)root[key];
                    }
                    root[""] = "";//结束标记

                    //计算词最小出现次数
                    if (_minCount == 0 || count < _minCount)
                        _minCount = count;

                    _totalCount += count;

                    //填充单词额外信息
                    WordInfo info = new WordInfo() { Freq = count, RowNumber = rn }; //freq先设置为次数 后面要重新计算
                    _wordExtraInfos[word] = info;
                    rn++;
                }
            }

            foreach (KeyValuePair<string, WordInfo> wordExtraInfo in _wordExtraInfos)
            {
                //计算 逆文档频率（一个词出现次数越高 则越不重要）
                wordExtraInfo.Value.IDF = (float)Math.Log(_totalCount / (wordExtraInfo.Value.Freq + 1));

                //计算 文档频率
                wordExtraInfo.Value.Freq = (float)Math.Log(wordExtraInfo.Value.Freq / _totalCount);
            }

            _minFreq = (float)Math.Log((float)_minCount / _totalCount);
        }
    }
}

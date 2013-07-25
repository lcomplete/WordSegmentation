using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WordSegmentation
{
    public static class WordTool
    {
        private static Regex re_chinese = new Regex(@"([\u4E00-\u9FA5a-zA-Z0-9+#&\._]+)", RegexOptions.Compiled);
        private static Regex re_alphabet_digit = new Regex(@"(\d+\.\d+|[a-zA-Z0-9]+)", RegexOptions.Compiled);

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static IEnumerable<string> Cut(string sentence)
        {
            string[] blocks = re_chinese.Split(sentence);
            foreach (string block in blocks)
            {
                if (re_chinese.IsMatch(block))
                {
                    foreach (string word in CutBlock(block))
                    {
                        yield return word;
                    }
                }
            }
        }

        /// <summary>
        /// 对一个词块分词
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private static IEnumerable<string> CutBlock(string block)
        {
            Dictionary<int, IList<int>> dag = GetDAG(block);
            int[] route = CalcRoute(block, dag);
            int length = block.Length;
            int i = 0;
            string buffer = string.Empty;
            while (i < length)
            {
                int end = route[i];
                string word = block.Substring(i, end - i + 1);
                //不存在的单个词放入缓冲区
                if (end - i == 0 && !Dict.WordExtraInfos.ContainsKey(word))
                    buffer += word;
                else
                {
                    if (buffer.Length > 0)
                    {
                        foreach (string s in CutBuffer(buffer))
                        {
                            yield return s;
                        }

                        buffer = string.Empty;
                    }

                    yield return word;
                }

                i = end + 1;
            }

            if (buffer.Length > 0)
            {
                foreach (string s in CutBuffer(buffer))
                {
                    yield return s;
                }
            }
        }

        /// <summary>
        /// 对缓冲区的字符进行分词
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static IEnumerable<string> CutBuffer(string buffer)
        {
            if (buffer.Length == 1)
                yield return buffer;
            else
            {
                //不存在的词 使用字母和数字 拆分后返回
                string[] tmp = re_alphabet_digit.Split(buffer);
                foreach (string s in tmp)
                {
                    if (!string.IsNullOrEmpty(s))
                        yield return s;
                }
            }
        }

        /// <summary>
        /// 计算最大概率路径（使用动态规划）
        /// </summary>
        /// <param name="block"></param>
        /// <param name="dag"></param>
        /// <returns></returns>
        private static int[] CalcRoute(string block, Dictionary<int, IList<int>> dag)
        {
            int length = block.Length;
            int[] route = new int[length];
            float[] freq = new float[length + 1];

            //汉语重心经常落在后面 采用逆向最大匹配
            for (int i = length - 1; i >= 0; i--)
            {
                var candidates = (from end in dag[i]
                                  select Tuple.Create(GetFreq(block.Substring(i, end - i + 1)) + freq[end + 1], end)).ToList();
                Tuple<float, int> freqAndend = candidates.OrderByDescending(t => t.Item1).FirstOrDefault();
                freq[i] = freqAndend.Item1;
                route[i] = freqAndend.Item2;
            }

            return route;
        }

        /// <summary>
        /// 获取词语的文档频率
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private static float GetFreq(string word)
        {
            WordInfo info;
            if (Dict.WordExtraInfos.TryGetValue(word, out info))
                return info.Freq;

            return Dict.MinFreq;
        }

        /// <summary>
        /// 获取有向无环图
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private static Dictionary<int, IList<int>> GetDAG(string block)
        {
            int length = block.Length;
            Hashtable trie = Dict.Trie;
            int i = 0, end = 0;

            Dictionary<int, IList<int>> dag = new Dictionary<int, IList<int>>();
            while (i < length)
            {
                string key = block.Substring(end, 1);
                bool goNextWord;
                if (trie.ContainsKey(key))
                {
                    trie = (Hashtable)trie[key];
                    if (trie.ContainsKey(""))
                    {
                        if (!dag.ContainsKey(i))
                        {
                            dag.Add(i, new List<int>());
                        }
                        dag[i].Add(end);
                    }

                    end++;
                    goNextWord = end >= length;
                }
                else
                    goNextWord = true;

                if (goNextWord)
                {
                    end = ++i;
                    trie = Dict.Trie;
                }
            }

            for (int k = 0; k < length; k++)
            {
                if (!dag.ContainsKey(k))
                    dag[k] = new List<int>() { k };
            }

            return dag;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WordSegmentation
{
    public static class ArticleUtils
    {
        /// <summary>
        /// 计算相似度
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        /// <returns></returns>
        public static double CaclSimilar(Dictionary<int, float> vector1, Dictionary<int, float> vector2, double length1, double length2)
        {
            if (vector1.Count == 0 && vector2.Count == 0)
                return 1;
            if (vector1.Count == 0 || vector2.Count == 0)
                return 0;

            float numerator = 0;
            foreach (KeyValuePair<int, float> idAndTFIDF in vector1)
            {
                if (vector2.ContainsKey(idAndTFIDF.Key))
                    numerator += idAndTFIDF.Value * vector2[idAndTFIDF.Key];
            }

            double scale = length1 > length2 ? length2 / length1 : length1 / length2;
            return numerator * scale / (length1 * length2);
        }

        /// <summary>
        /// 计算向量长度
        /// </summary>
        /// <param name="vector1"></param>
        /// <returns></returns>
        public static double CaclVectorLength(Dictionary<int, float> vector1)
        {
            double result = 0;
            foreach (float tfidf in vector1.Values)
            {
                result += tfidf * tfidf;
            }
            return Math.Sqrt(result);
        }

        /// <summary>
        /// 获取特征向量
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static Dictionary<int, float> GetFeatureVector(string sentence)
        {
            Dictionary<int, Tuple<int, float>> wordId_countIDF = new Dictionary<int, Tuple<int, float>>();
            int totalWordCount = 0;
            foreach (string word in WordTool.Cut(sentence))
            {
                if (Dict.StopWords.Contains(word))
                    continue;

                WordInfo info;
                if (Dict.WordExtraInfos.TryGetValue(word, out info))
                {
                    Tuple<int, float> countAndIDF;
                    if (!wordId_countIDF.TryGetValue(info.RowNumber, out countAndIDF))
                        countAndIDF = Tuple.Create(0, info.IDF);
                    wordId_countIDF[info.RowNumber] = Tuple.Create(countAndIDF.Item1 + 1, countAndIDF.Item2);
                }
                totalWordCount++;
            }

            Dictionary<int, float> idAndTFIDF = new Dictionary<int, float>();
            foreach (KeyValuePair<int, Tuple<int, float>> pair in wordId_countIDF)
            {
                //计算TF-IDF值
                idAndTFIDF[pair.Key] = ((float)pair.Value.Item1 / totalWordCount) * pair.Value.Item2;
            }

            return idAndTFIDF;
        }
    }
}

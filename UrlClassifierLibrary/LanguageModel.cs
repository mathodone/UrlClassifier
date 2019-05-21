using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlClassifier
{
    public class LanguageModel
    {
        int ngram_size;
        string smoothing;
        double laplace_gamma;
        int corpus_mix;
        string corpus_model;
        List<string> vocabulary;


        public LanguageModel(int ngram_size=3, string smoothing="Laplace", double laplace_gamma=1, int corpus_mix=0, string corpus_model="Miller")
        {
            this.ngram_size = ngram_size;
            this.smoothing = smoothing;
            this.laplace_gamma = laplace_gamma;
            this.corpus_mix = corpus_mix;
            this.corpus_model = corpus_model;
        }

        public List<string> GetVocabulary()
        {
            return vocabulary;
        }

        public void CreateNGrams(string[] urls)
        {
            List<KeyValuePair<string, int>> gram_counts = new List<KeyValuePair<string, int>>();

            foreach (int val in Enumerable.Range(ngram_size - 1, 2))
            {
                List<string> val_grams = new List<string>();

                foreach (string url in urls)
                {
                    string path = string.Join("", PredictionManager.urlSplit(url.ToLower()));
                    var grams = Enumerable.Range(0, path.Length)
                                          .Where(x => x + val <= path.Length)
                                          .Select(s => path.Substring(s, val));

                    foreach (string gram in grams)
                    {
                        val_grams.Add(gram);
                    }
                }

                var gram_count = val_grams.Select(x => x).GroupBy(s => s);

                foreach (IGrouping<string, string> pair in gram_count)
                {
                    gram_counts.Add(new KeyValuePair<string, int>(pair.Key, pair.Count()));
                }
            }

            IGrouping<int, KeyValuePair<string, int>>[] rtn = gram_counts.GroupBy(s => s.Key.Length).ToArray();

            Console.WriteLine("done");
        }

        //public string[][] createNGrams(string[] terms)
        //{
        //    if (terms.Length <= this.ngram_size)
        //    {
        //        return new[] { terms };
        //    }
        //    else if (ngram_size == 1)
        //    {
        //        return terms.Select(x => new[] { x }).ToArray();
        //    }
        //    else
        //    {
        //        List<string[]> ngrams = new List<string[]>();

        //        foreach (var i in Enumerable.Range(0,terms.Length - ngram_size + 1))
        //        {
        //            ngrams.Add(new ArraySegment<string>(terms, i, ngram_size).ToArray());
        //        }

        //        return ngrams.ToArray();
        //    }
        //}
    }
}

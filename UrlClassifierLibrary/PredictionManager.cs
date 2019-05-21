using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace UrlClassifier
{
    public static class PredictionManager
    {
        // Class variables
        private static HashSet<string> stopWords = new HashSet<string>(new[] {"zh","asp","en","html","php","http","https","www","com","org","weebly",
                                             "wordpress","wp","the","id","of","miracle","ear","wellstar","aspx",
                                             "krispykreme","co","agtegra","sysco","amedisys","life","net","ny",
                                             "cdgranite","granite","cd","uk","pa","countertop","avera","fl",
                                             "au","mi","wi","sdcoffeetea","tx","yr","oh","ia","aireserv",
                                             "ceres", "il", "ga", "nc", "mn", "htm", "ne", "atlanta", "in",
                                             "trilogyhs", "nj","acuren","ma","az","agprocompanies","it","mo",
                                             "hendrickauto","deere","aspirepublicschools","or","va","ky", "page", "default",
                                             "and" });
        private static readonly char[] urlSplitChar = "/-_.#=$?:".ToArray();

        // Constructor
        static PredictionManager()
        {
            if (!File.Exists(Path_LocationCorpusInfo)) throw new FileNotFoundException("Location Classifier Corpus file not found.", $"Location Corpus: {Path_LocationCorpusInfo}");
            else if (!File.Exists(Path_LocationClassifierDictionaryValid)) throw new FileNotFoundException("Location Classifier Valid Dict not found.", $"Location Valid Dict: {Path_LocationClassifierDictionaryValid}");
            else if (!File.Exists(Path_LocationClassifierDictionaryInvalid)) throw new FileNotFoundException("Location Classifier Invalid Dict not found.", $"Location Invalid Dict: {Path_LocationClassifierDictionaryInvalid}");
            else if (!File.Exists(Path_PeopleCorpusInfo)) throw new FileNotFoundException("People Classifier Corpus file not found.", $"People Corpus: {Path_PeopleCorpusInfo}");
            else if (!File.Exists(Path_PeopleClassifierDictionaryValid)) throw new FileNotFoundException("People Classifier Valid Dict not found.", $"People Valid Dict: {Path_PeopleClassifierDictionaryValid}");
            else if (!File.Exists(Path_PeopleClassifierDictionaryInvalid)) throw new FileNotFoundException("People Classifier Invalid Dict not found.", $"People Invalid Dict: {Path_PeopleClassifierDictionaryInvalid}");
            else if (!File.Exists(Path_ServicesCorpusInfo)) throw new FileNotFoundException("Services Classifier Corpus file not found.", $"People Corpus: {Path_ServicesCorpusInfo}");
            else if (!File.Exists(Path_ServicesClassifierDictionaryValid)) throw new FileNotFoundException("Services Classifier Valid Dict not found.", $"People Valid Dict: {Path_ServicesClassifierDictionaryValid}");
            else if (!File.Exists(Path_ServicesClassifierDictionaryInvalid)) throw new FileNotFoundException("Services Classifier Invalid Dict not found.", $"People Invalid Dict: {Path_ServicesClassifierDictionaryInvalid}");
        }

        public static double ClassifyUrl(string url, ClassifyType type, string linkText = "") => urlClassify(type, url + linkText);
        private static double urlClassify(ClassifyType type, string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return 0;
            var split = urlSplit(url.ToLower());
            if (split == null || split.Length == 0) return 0;

            url = string.Join("", split);

            Dictionary<string, double> sdc0;
            Dictionary<string, double> sdc1;
            double corp0;
            double corp1;
            double sum0;
            double sum1;

            switch (type)
            {
                case ClassifyType.locations:
                    sdc0 = Loc_SDC_0;
                    sdc1 = Loc_SDC_1;
                    corp0 = Loc_CorpusInfo["C_0"];
                    corp1 = Loc_CorpusInfo["C_1"];
                    sum0 = Loc_SDC_0_Sum;
                    sum1 = Loc_SDC_1_Sum;
                    break;
                case ClassifyType.people:
                    sdc0 = Ppl_SDC_0;
                    sdc1 = Ppl_SDC_1;
                    corp0 = Ppl_CorpusInfo["C_0"];
                    corp1 = Ppl_CorpusInfo["C_1"];
                    sum0 = Ppl_SDC_0_Sum;
                    sum1 = Ppl_SDC_1_Sum;
                    break;
                case ClassifyType.services:
                    sdc0 = Srv_SDC_0;
                    sdc1 = Srv_SDC_1;
                    corp0 = Srv_CorpusInfo["C_0"];
                    corp1 = Srv_CorpusInfo["C_1"];
                    sum0 = Srv_SDC_0_Sum;
                    sum1 = Srv_SDC_1_Sum;
                    break;
                default: return 0;
            }

            Tuple<double, double> rtn = Enumerable.Range(2, 7)
                .SelectMany(i => Enumerable.Range(0, url.Length).Where(x => x + i <= url.Length).Select(s => url.Substring(s, i)))
                .GroupBy(s => s)
                .Select(group => Tuple.Create(
                    sdc0.ContainsKey(group.Key) ? sdc0[group.Key] * group.Count() : 0.0,
                    sdc1.ContainsKey(group.Key) ? sdc1[group.Key] * group.Count() : 0.0
                ))
                .Aggregate((sum, current) => Tuple.Create(
                    (sum.Item1 + current.Item1) / sum0 * corp0,
                    (sum.Item2 + current.Item2) / sum1 * corp1
                    ));
            
            double prob = rtn.Item2 / (rtn.Item1 + rtn.Item2);

            return Double.IsNaN(prob) ? 0 : prob;
        }
        public static void CreateLanguageModel(string[] urls, int ngram)
        {
            List<KeyValuePair<string, int>> gram_counts = new List<KeyValuePair<string, int>>();

            foreach (int val in Enumerable.Range(2, ngram - 1))
            {
                List<string> val_grams = new List<string>();

                foreach (string url in urls)
                {
                    string path = string.Join("", urlSplit(url.ToLower()));
                    var grams = Enumerable.Range(0, path.Length).Where(x => x + val <= path.Length).Select(s => path.Substring(s, val));
                    foreach (string gram in grams)
                    {
                        val_grams.Add(gram);
                    }
                }

                var gram_count = val_grams.Select(x => x).GroupBy(s => s);
                
                foreach (IGrouping<string,string> pair in gram_count)
                {
                    gram_counts.Add(new KeyValuePair<string,int>(pair.Key, pair.Count()));
                }
            }

            IGrouping<int, KeyValuePair<string,int>>[] rtn = gram_counts.GroupBy(s => s.Key.Length).ToArray();

            Console.WriteLine("done");
        }

        // Private functions
        public static string[] urlSplit(string url)
        {
            if (!Uri.TryCreate("https://a.com" + url.Replace(@"#", "").Replace(@"://", ""), UriKind.Absolute, out Uri uri)) return new string[0];
            return uri.AbsolutePath.Split(urlSplitChar, StringSplitOptions.RemoveEmptyEntries)
                          .Where(x => !stopWords.Contains(x)
                            && x.Length > 1
                            && !string.IsNullOrWhiteSpace(x))
                            .ToArray();
        }

        // Functions for feature selection/statistic dictionaries
        private static IEnumerable<string> nGramFeatures(string url, int nGramSize)
        {
            if (nGramSize == 0) throw new Exception("nGram size was not set");

            // split the url
            string[] concat_split = string.IsNullOrWhiteSpace(url) ? new string[0] : urlSplit(url);
            string concat = string.Join(string.Empty, concat_split);

            StringBuilder nGrams = new StringBuilder();

            for (var k = 2; k < nGramSize + 1; k++)
            {
                StringBuilder nGram = new StringBuilder();
                Queue<int> wordLengths = new Queue<int>();

                for (var i = 0; i < concat.Length - k + 1; i++)
                {
                    StringBuilder gram = new StringBuilder();

                    for (var j = 0; j < k; j++)
                    {
                        gram.Append(concat[i + j]);
                    }

                    if (i == concat.Length - k)
                    {
                        nGram.Append(gram);
                    }
                    else
                    {
                        nGram.Append(gram + " ");
                    }
                }

                if (k == nGramSize)
                {
                    nGrams.Append(nGram);
                }
                else
                {
                    nGrams.Append(nGram + " ");
                }
            }
            return nGrams.ToString().Split(' ');
        }

        #region Training Code
        private static HashSet<string> CreateCorpus(string[] urls, int nGramSize)
        {
            HashSet<string> corpus = new HashSet<string>();

            foreach (string url in urls)
            {
                IEnumerable<string> nGrams = nGramFeatures(url.ToLowerInvariant(), nGramSize);
                foreach (string gram in nGrams)
                {
                    if (gram != "")
                    {
                        corpus.Add(gram);
                    }
                }
            }
            return corpus;
        }

        // taken from https://onlinelibrary.wiley.com/doi/full/10.1111/coin.12158#coin12158-bib-0040
        public static void CreateSDC(string[] urls, int[] indicators)
        {
            Dictionary<string, double> SDC_0 = new Dictionary<string, double>();
            Dictionary<string, double> SDC_1 = new Dictionary<string, double>();

            Dictionary<string, double> CorpusInfo = new Dictionary<string, double>();

            double numUrls = urls.Length;

            // set up ngrams and terms
            IEnumerable<string[]> zipped = urls.Zip(indicators, (first, second) => new string[] { first, second.ToString() });
            HashSet<string> corpus = CreateCorpus(urls, 9);
            var positiveUrls = zipped.Where(x => x[1] == "1")
                                     .Select(x => nGramFeatures(x[0], 9));
            var negativeUrls = zipped.Where(x => x[1] == "0")
                                     .Select(x => nGramFeatures(x[0], 9));

            // store posteriors P(C_k) = #(urls in class k)/#(urls)
            CorpusInfo.Add("C_0", negativeUrls.Count() / numUrls);
            CorpusInfo.Add("C_1", positiveUrls.Count() / numUrls);

            Dictionary<string, Tuple<double, double>> relevant_1 = new Dictionary<string, Tuple<double, double>>();
            Dictionary<string, Tuple<double, double>> relevant_0 = new Dictionary<string, Tuple<double, double>>();

            // for each term, calculate:
            // the observed co-occurrence frequencies of t_i and the given class C_k
            // the expected co-occurrence frequencies of t_i and the given class C_k
            // relevance per category (this eliminates bad terms)
            // chi square for term t_i for class C_k
            foreach (string term in corpus)
            {
                // OBSERVED FREQUENCIES

                // # of urls in C_0 with t_i
                double gamma_0_0 = negativeUrls.Where(x => x.Contains(term)).Count();

                // # of urls in C_1 with t_i
                double gamma_0_1 = positiveUrls.Where(x => x.Contains(term)).Count();

                // # of urls in C_0 without t_i
                double gamma_1_0 = negativeUrls.Count() - gamma_0_0;

                // # of urls in C_1 without t_i
                double gamma_1_1 = positiveUrls.Count() - gamma_0_1;

                // EXPECTED FREQUENCIES

                // E(t_i, C_0)
                double E_0_0 = ((gamma_0_0 + gamma_1_0) * (gamma_0_0 + gamma_0_1)) / numUrls;

                // E(t_i, C_1)
                double E_0_1 = ((gamma_0_1 + gamma_1_1) * (gamma_0_0 + gamma_0_1)) / numUrls;

                // E(!t_i, C_0)
                double E_1_0 = ((gamma_0_0 + gamma_1_0) * (gamma_1_0 + gamma_1_1)) / numUrls;

                // E(!t_i, C_1)
                double E_1_1 = ((gamma_0_1 + gamma_1_1) * (gamma_1_0 + gamma_1_1)) / numUrls;

                // R(t_i, C_0)
                double R_0 = gamma_0_0 / E_0_0;

                // R(t_i, C_1)
                double R_1 = gamma_0_1 / E_0_1;

                // X^2_{t_i, C_k}
                // only have to calculate one X^2 since 2x2 contingency table
                // reference: critical value is 3.84 for 1 df, alpha = 0.05
                double chiSquare = Math.Pow(gamma_0_1 - E_0_1, 2.0f) / E_0_1 +
                                   Math.Pow(gamma_0_0 - E_0_0, 2.0f) / E_0_0 +
                                   Math.Pow(gamma_1_1 - E_1_1, 2.0f) / E_1_1 +
                                   Math.Pow(gamma_1_0 - E_1_0, 2.0f) / E_1_0;

                // R_1 means term is relevant for class 1, similar for R_0 and class 0
                if (R_1 > 1)
                {
                    // chi square weights. this is used in term goodness of fit.
                    // calculated for both classes
                    // P(R(t_i, C_K)) = R(t_i, C_k) / SUM_{k=1}^{m} R(t_i, C_k)

                    // term goodness of fit for class 1
                    // g_i = P(R(t_i, C_1)) * X^2_{t_i, C_1}
                    //double goodness_1 = chiSquareWeight_1 * chiSquare;
                    relevant_1.Add(term, new Tuple<double, double>(R_1, chiSquare));
                }
                else if (R_0 > 1)
                {
                    // term goodness of fit for class 0
                    //g_i = P(R(t_i, C_0)) * X^2_{ t_i, C_0}
                    //double goodness_0 = chiSquareWeight_0 * chiSquare;

                    relevant_0.Add(term, new Tuple<double, double>(R_0, chiSquare));
                }
                else // term is completely irrelevant, skip iteration
                {
                    continue;
                }
            }

            // sum of relevances, used in the calculation for chi square weighting
            double sumOfRelevances_1 = relevant_1.Sum(x => x.Value.Item1);
            double sumOfRelevances_0 = relevant_0.Sum(x => x.Value.Item1);

            // for each term, calculate chi square weights. this is used in term goodness of fit.
            // calculated for both classes
            // P(R(t_i, C_K)) = R(t_i, C_k) / SUM_{k=1}^{m} R(t_i, C_k)
            foreach (KeyValuePair<string, Tuple<double, double>> term in relevant_1)
            {
                double chiSquareWeight_1 = term.Value.Item1 / sumOfRelevances_1;
                double goodness_1 = chiSquareWeight_1 * term.Value.Item2;
                SDC_1.Add(term.Key, goodness_1);
            }

            foreach (KeyValuePair<string, Tuple<double, double>> term in relevant_0)
            {
                double chiSquareWeight_0 = term.Value.Item1 / sumOfRelevances_0;
                double goodness_0 = chiSquareWeight_0 * term.Value.Item2;
                SDC_0.Add(term.Key, goodness_0);
            }

            File.WriteAllText(@"C:\Users\matthew\Documents\projects\UrlClassifier\DLLs\locationsclassifierdictionaryvalid.txt", new JavaScriptSerializer().Serialize(SDC_1));
            File.WriteAllText(@"C:\Users\matthew\Documents\projects\UrlClassifier\DLLs\locationsclassifierdictionaryinvalid.txt", new JavaScriptSerializer().Serialize(SDC_0));
            File.WriteAllText(@"C:\Users\matthew\Documents\projects\UrlClassifier\DLLs\locationsclassifiercorpus.txt", new JavaScriptSerializer().Serialize(CorpusInfo));
        }
        #endregion

        // Properties
        public enum ClassifyType { locations, people, services };
        public enum PathType
        {
            LocationCorpus,
            LocationDictionaryValid,
            LocationDictionaryInvalid,
            PeopleCorpus,
            PeopleDictionaryValid,
            PeopleDictionaryInvalid,
            ServicesCorpus,
            ServicesDictionaryValid,
            ServicesDictionaryInvalid
        }

        public static Dictionary<PathType, string> Path { get; private set; } = new Dictionary<PathType, string>
        {
            { PathType.LocationCorpus, Path_LocationCorpusInfo },
            { PathType.LocationDictionaryValid, Path_LocationClassifierDictionaryValid },
            { PathType.LocationDictionaryInvalid, Path_LocationClassifierDictionaryInvalid },
            { PathType.PeopleCorpus, Path_PeopleCorpusInfo },
            { PathType.PeopleDictionaryValid, Path_PeopleClassifierDictionaryValid },
            { PathType.PeopleDictionaryInvalid, Path_PeopleClassifierDictionaryInvalid },
            { PathType.ServicesCorpus, Path_ServicesCorpusInfo },
            { PathType.ServicesDictionaryValid, Path_ServicesClassifierDictionaryValid },
            { PathType.ServicesDictionaryInvalid, Path_ServicesClassifierDictionaryInvalid }
        };

        private static SettingsLibrary.SettingsManager Settings { get; } = new SettingsLibrary.SettingsManager("urlclassifier.config");
        private static string Path_LocationClassifierDictionaryValid { get; } = Settings.GetSetting("filelocationdictionaryvalid", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\locationclassifierdictionaryvalid.txt");
        private static string Path_LocationCorpusInfo { get; } = Settings.GetSetting("filelocationcorpus", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\locationclassifiercorpus.txt");
        private static string Path_LocationClassifierDictionaryInvalid { get; } = Settings.GetSetting("filelocationdictionaryinvalid", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\locationclassifierdictionaryinvalid.txt");
        private static string Path_PeopleCorpusInfo { get; } = Settings.GetSetting("filepeoplecorpus", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\peopleclassifiercorpus.txt");
        private static string Path_PeopleClassifierDictionaryValid { get; } = Settings.GetSetting("filepeopledictionaryvalid", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\peopleclassifierdictionaryvalid.txt");
        private static string Path_PeopleClassifierDictionaryInvalid { get; } = Settings.GetSetting("filepeopledictionaryinvalid", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\peopleclassifierdictionaryinvalid.txt");
        private static string Path_ServicesCorpusInfo { get; } = Settings.GetSetting("fileservicescorpus", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\servicesclassifiercorpus.txt");
        private static string Path_ServicesClassifierDictionaryValid { get; } = Settings.GetSetting("fileservicesdictionaryvalid", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\servicesclassifierdictionaryvalid.txt");
        private static string Path_ServicesClassifierDictionaryInvalid { get; } = Settings.GetSetting("fileservicesdictionaryinvalid", SettingsLibrary.FileManager.Path[SettingsLibrary.FileManager.PathType.Base] + @"\models\servicesclassifierdictionaryinvalid.txt");

        private static readonly Dictionary<string, double> Loc_SDC_0 = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_LocationClassifierDictionaryInvalid));
        private static readonly Dictionary<string, double> Loc_SDC_1 = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_LocationClassifierDictionaryValid));
        private static readonly Dictionary<string, double> Loc_CorpusInfo = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_LocationCorpusInfo));
        private static readonly Dictionary<string, double> Ppl_SDC_0 = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_PeopleClassifierDictionaryInvalid));
        private static readonly Dictionary<string, double> Ppl_SDC_1 = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_PeopleClassifierDictionaryValid));
        private static readonly Dictionary<string, double> Ppl_CorpusInfo = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_PeopleCorpusInfo));
        private static readonly Dictionary<string, double> Srv_SDC_0 = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_ServicesClassifierDictionaryInvalid));
        private static readonly Dictionary<string, double> Srv_SDC_1 = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_ServicesClassifierDictionaryValid));
        private static readonly Dictionary<string, double> Srv_CorpusInfo = new JavaScriptSerializer().Deserialize<Dictionary<string, double>>(File.ReadAllText(Path_ServicesCorpusInfo));

        private static readonly double Loc_SDC_0_Sum = Loc_SDC_0.Values.Sum();
        private static readonly double Loc_SDC_1_Sum = Loc_SDC_1.Values.Sum();
        private static readonly double Ppl_SDC_0_Sum = Ppl_SDC_0.Values.Sum();
        private static readonly double Ppl_SDC_1_Sum = Ppl_SDC_1.Values.Sum();
        private static readonly double Srv_SDC_0_Sum = Srv_SDC_0.Values.Sum();
        private static readonly double Srv_SDC_1_Sum = Srv_SDC_1.Values.Sum();
    }
}
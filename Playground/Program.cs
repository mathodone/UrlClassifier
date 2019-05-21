using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UrlClassifier;

namespace Playground
{
    class Program6
    {
        static void Main(string[] args) => TestNGram();

        static DataTable ReadCsv(string path)
        {
            StreamReader sr = new StreamReader(path, System.Text.Encoding.UTF7);
            string[] headers = sr.ReadLine().Split(',');
            DataTable dt = new DataTable();

            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }

            while (!sr.EndOfStream)
            {
                string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                DataRow dr = dt.NewRow();

                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }

        static void TestNGram()
        {
            var test = new LanguageModel(ngram_size: 3);
            test.CreateNGrams(new string[] { "a.com/contact_us", "a.com/contactus", "a.com/contact", "a.com/juice" });

            Console.ReadLine();
        }

        static void TestUrl()
        {
            var prediction = UrlClassifier.PredictionManager.ClassifyUrl("https://www.ayasdi.com/company/contact-us/", UrlClassifier.PredictionManager.ClassifyType.locations);
            Console.ReadLine();
        }

        static void Trainmodel()
        {
            Console.WriteLine("BEGIN");
            string trainingData = "../../labeled_urls_classifier.csv";
            DataTable table = ReadCsv(trainingData);
            string[] urls = table.AsEnumerable().Select(r => r.Field<string>("url"))
                                                .ToArray();
            int[] locations = table.AsEnumerable()
                                   .Select(r => r.Field<string>("locations"))
                                   .Select(x => int.Parse(x))
                                   .ToArray();

            var training_data = urls.Zip(locations, (x, s) => new string[] {x, s.ToString() });

            var LM = new LanguageModel(ngram_size: 3);
            LM.CreateNGrams(training_data.Where(x => x[1] == "1").Select(y => y[0]).ToArray());

            //UrlClassifier.PredictionManager.CreateSDC(urls, locations);

            Console.WriteLine("Done");
        }

        static void Testsingle()
        {
            while (true)
            {
                string test = Console.ReadLine();
                double prediction = UrlClassifier.PredictionManager.ClassifyUrl(test, UrlClassifier.PredictionManager.ClassifyType.services);
                Console.WriteLine("    " + prediction);
            }
        }

        static void Testmodel()
        {
            UrlClassifier.PredictionManager.ClassifyUrl("https://example.com/ajsdklfa", UrlClassifier.PredictionManager.ClassifyType.locations);

            Random rand = new Random();
            string[] urls = Enumerable.Range(0, 78800).Select(x => "https://example.com/" + rand.Next(1000000).ToString()).ToArray();

            //string trainingData = "../../train_data.csv";
            //DataTable table = UrlClassifier.Training.DataPreparation.ReadCsv(trainingData);
            //EnumerableRowCollection<string> urls = table.AsEnumerable().Select(r => r.Field<string>("url"));

            //Console.WriteLine($"# of urls: {urls.ToArray().Length}");

            while (true)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var a = urls.Select(x => UrlClassifier.PredictionManager.ClassifyUrl(x, UrlClassifier.PredictionManager.ClassifyType.locations)).ToArray();
                sw.Stop();

                Console.WriteLine($"{sw.ElapsedMilliseconds}ms");
                //var zipped = urls.Zip(a, (x, y) => $"{x}, {y.ToString()}");
                Console.ReadLine();
            }

            //string trainingData = "../../test_loc.csv";
            //DataTable table = UrlClassifier.Training.DataPreparation.ReadCsv(trainingData);
            //string[] urls = table.AsEnumerable().Select(r => r.Field<string>("url"))
            //                                    .ToArray();
            //int[] people = table.AsEnumerable()
            //                       .Select(r => r.Field<string>("locations"))
            //                       .Select(x => int.Parse(x))
            //                       .ToArray();

            //List<double> predictions = new List<double>();

            //foreach (var url in urls)
            //{
            //    predictions.Add(UrlClassifier.PredictionManager.ClassifyUrl(url, UrlClassifier.PredictionManager.ClassifyType.locations));
            //}

            //var zipped = urls.Zip(predictions, (x,y) => new KeyValuePair<string, double>(x,y));

            //Console.WriteLine("Done");
        }
    }
}
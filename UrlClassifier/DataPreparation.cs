using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Training
{
    public static class DataPreparation
    {
        //public static void TrainModel()
        //{
        //    Console.WriteLine("BEGIN");
        //    string trainingData = "../../train_data.csv";
        //    DataTable table = ReadCsv(trainingData);
        //    string[] urls = table.AsEnumerable().Select(r => r.Field<string>("url"))
        //                                        .Take(345)
        //                                        .ToArray();

        //    int[] locations = table.AsEnumerable()
        //                              .Select(r => r.Field<string>("services"))
        //                              .Select(x => int.Parse(x))
        //                              .Take(345)
        //                              .ToArray();

        //    PredictionManager.CreateSDC(urls, locations);

        //    Console.WriteLine("Done");
        //}

        public static DataTable ReadCsv(string path)
        {
            StreamReader sr = new StreamReader(path);
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
    }
}   

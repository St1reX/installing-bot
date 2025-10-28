using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CSVFile;

namespace test1
{
    internal class DictionaryManagment
    {
        private static DictionaryManagment dictionaryInstance;
        public Dictionary<string, string> answers = new Dictionary<string, string>();

        static string projectRoot = Directory.GetParent(AppContext.BaseDirectory)
                               .Parent.Parent.Parent.FullName;
        static string directoryPath = Path.Combine(projectRoot, "csv");
        static string filePath = Path.Combine(directoryPath, "dictionary.csv");

        private DictionaryManagment()
        {
            Logger.SuccessMessage("Dictionary instance created successfully.");
            FetchDictionaryCSV();
            Console.WriteLine("=======================================================================================");
        }

        public static DictionaryManagment CreateInstance()
        {
            if (dictionaryInstance == null)
            {
                dictionaryInstance = new DictionaryManagment();
            }
            return dictionaryInstance;
        }

        void FetchDictionaryCSV()
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                    return;
                }

                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    var settings = new CSVSettings()
                    {
                        FieldDelimiter = ';'
                    };
                    var csv = new CSVReader(reader, settings);

                    foreach (var row in csv)
                    {
                        IList<string> fields = null;

                        try
                        {
                            fields = new List<string>();
                            foreach (var f in (IEnumerable<string>)row)
                                fields.Add(f);
                        }
                        catch
                        {
                            continue;
                        }

                        if (fields.Count >= 2)
                        {
                            string key = (fields[0] ?? "").Trim();
                            string value = (fields[1] ?? "").Trim();

                            if (!string.IsNullOrEmpty(key) && !answers.ContainsKey(key))
                                answers.Add(key, value);
                        }
                    }
                }

                Logger.SuccessMessage("Dictionary fetched.");
            }
            catch (IOException ioEx)
            {
                Logger.ErrorMessage($"Error occurred during file operations: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Logger.ErrorMessage($"Access to the file is denied: {unAuthEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.ErrorMessage($"Unexpected error occurred: {ex.Message}");
            }
        }

        public void SaveWordCSV(string word, string translation)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, append: true, Encoding.UTF8))
                {
                    var settings = new CSVSettings()
                    {
                        FieldDelimiter = ';'
                    };
                    var csv = new CSVWriter(writer, settings);
                    csv.WriteLine(new[] { word, translation });
                }

                Logger.SuccessMessage($"Added new word {word} -- {translation}.");
            }
            catch (IOException ioEx)
            {
                Logger.ErrorMessage($"Error occurred during writing word to file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Logger.ErrorMessage($"Access to the file is denied: {unAuthEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.ErrorMessage($"Unexpected error occurred: {ex.Message}");
            }
        }
    }
}

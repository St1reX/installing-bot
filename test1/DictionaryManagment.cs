using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test1
{
    internal class DictionaryManagment
    {
        private static DictionaryManagment dictionaryInstance;
        public Dictionary<string, string> answers = new Dictionary<string, string>();

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
            string directoryPath = Path.Combine("C:\\Users\\uryga\\Documents\\GitHub\\installing-bot", "csv");

            StreamReader reader;
            CsvReader csvReader;

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    directoryPath = Path.Combine(directoryPath, "dictionary.csv");

                    using (File.Create(directoryPath))
                    {
                        // Plik został utworzony, ale jest pusty
                    }
                }
                else
                {
                    directoryPath = Path.Combine(directoryPath, "dictionary.csv");
                    using (reader = new StreamReader(directoryPath, new System.Text.UTF8Encoding(true)))
                    {
                        using (csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            var records = csvReader.GetRecords<dynamic>();
                            string word = "";
                            string translation = "";

                            foreach (var record in records)
                            {
                                foreach (var field in record)
                                {
                                    if (field.Key == "Key")
                                    {
                                        word = field.Value;
                                    }
                                    else
                                    {
                                        translation = field.Value;
                                    }
                                }
                                answers.Add(word, translation); // Dodanie słowa i tłumaczenia do słownika
                            }
                        }
                    }

                    Logger.SuccessMessage("Dictionary fetched.");
                }
            }
            catch (IOException ioEx)
            {
                Logger.ErrorMessage($"Error ocurred during operations with file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Logger.ErrorMessage($"Access to the file is denied: {unAuthEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.ErrorMessage($"Unexpected error ocurred: {ex.Message}");
            }
        }

        public void SaveWordCSV(string word, string translation)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            tmp.Add(word, translation);

            string directoryPath = Path.Combine("C:\\Users\\uryga\\Documents\\GitHub\\installing-bot", "csv", "dictionary.csv");

            StreamWriter writer;
            CsvWriter csvWriter;
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            try
            {
                using (writer = new StreamWriter(directoryPath, true, new System.Text.UTF8Encoding(true)))
                {
                    using (csvWriter = new CsvWriter(writer, config))
                    {
                        csvWriter.WriteRecords(tmp);
                    }
                }

                Logger.SuccessMessage($"Added new word {word} -- {translation}.");
            }
            catch (IOException ioEx)
            {
                Logger.ErrorMessage($"Error occured during the writing word to file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Logger.ErrorMessage($"Access to the file is denied: {unAuthEx.Message}");
            }
            catch (Exception ex)
            {
                Logger.ErrorMessage($"Unexpected error ocurred: {ex.Message}");
            }
        }


    }
}

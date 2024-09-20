using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using System.Collections;
using CsvHelper.Configuration;

namespace test1
{
    internal class ChromeBot
    {
        User User { get; set; }
        int Interval {  get; set; }
        Dictionary<string, string> answers = new Dictionary<string, string>();
        ChromeDriver ChromeInstance { get; set; }

        public ChromeBot(User user, int interval) 
        {
            User = user;
            Interval = interval;
        }

        public void TakeTheQuiz()
        {
            try
            {
                FetchDictionaryCSV();

                LoginUser();
                    
                IWebElement startSessionButton = ChromeInstance.FindElement(By.ClassName("btn-session"));
                startSessionButton.Click();

                startSessionButton = (ChromeInstance.FindElement(By.Id("continue_session_button")).Displayed)
                    ? ChromeInstance.FindElement(By.Id("continue_session_button"))
                    : ChromeInstance.FindElement(By.Id("start_session_button"));
                startSessionButton.Click();

                IWebElement submitAnswerButton = ChromeInstance.FindElement(By.Id("check"));
                IWebElement nextWordButton = ChromeInstance.FindElement(By.Id("nextword"));
                IWebElement answerInput = ChromeInstance.FindElement(By.Id("answer"));
                string dictionaryKey = "";
                string dictionaryValue = "";


                while (!ChromeInstance.FindElement(By.Id("return_mainpage")).Displayed)
                {
                    Wait(2000);

                    if (ChromeInstance.FindElement(By.Id("new_word_form")).Displayed)
                    {
                        ChromeInstance.FindElement(By.Id("know_new")).Click();

                        ChromeInstance.FindElement(By.Id("skip")).Click();

                        continue;
                    }

                    dictionaryKey = ChromeInstance.FindElement(By.ClassName("translations")).Text;

                    if (answers.ContainsKey(dictionaryKey))
                    {
                        dictionaryValue = answers[dictionaryKey];
                        answerInput.SendKeys(dictionaryValue);
                        submitAnswerButton.Click();

                        Wait(Interval);

                        nextWordButton.Click();
                    }
                    else
                    {
                        submitAnswerButton.Click();

                        dictionaryValue = ChromeInstance.FindElement(By.Id("word")).Text;
                        answers.Add(dictionaryKey, dictionaryValue);
                        SaveWordCSV(dictionaryKey, dictionaryValue);

                        Wait(Interval);

                        nextWordButton.Click();
                    }
                }

                DisplayDictionary();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during the quiz process: " + ex.Message);
            }
            finally
            {

                Console.ReadKey();
                ChromeInstance.Quit();
            }

        }

        void CreateChromeInstance(params string[] chromeStartOption)
        {
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments(chromeStartOption);

                ChromeInstance = new ChromeDriver(options);
                ChromeInstance.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialize ChromeDriver: " + ex.Message);
                throw;
            }
        }

        void LoginUser()
        {
            try
            {
                CreateChromeInstance("--start-maximized", "--disable-search-engine-choice-screen");

                ChromeInstance.Navigate().GoToUrl("https://instaling.pl/teacher.php?page=login");

                IWebElement DOMElement;

                if (ChromeInstance.Url == "https://instaling.pl/teacher.php?page=login")
                {
                    DOMElement = ChromeInstance.FindElement(By.ClassName("fc-button-label"));
                    DOMElement.Click();

                    DOMElement = ChromeInstance.FindElement(By.Name("log_email"));
                    DOMElement.Clear();
                    DOMElement.SendKeys(User.Login);

                    DOMElement = ChromeInstance.FindElement(By.Name("log_password"));
                    DOMElement.Clear();
                    DOMElement.SendKeys(User.Password);

                    DOMElement = ChromeInstance.FindElement(By.XPath("//button[@type='submit']"));
                    DOMElement.Click();

                    if (!ChromeInstance.Url.Contains("https://instaling.pl/student/pages/mainPage.php"))
                    {
                        throw new Exception("Provided login data is incorrect. Please open the program again and provide correct and current login data.");
                    }
                }
                else if (!ChromeInstance.Url.Contains("https://instaling.pl/student/pages/mainPage.php?student_id="))
                {
                    throw new Exception("Unknown URL redirection. Process terminated.");
                }
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine("Error during login: Element not found. " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during login: " + ex.Message);
            }
        }

        void FetchDictionaryCSV()
        {
            string directoryPath = Path.Combine("C:\\Users\\uryga\\Documents\\GitHub\\installing-bot", "csv");

            StreamReader reader;
            CsvReader csvReader;
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

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
                }
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error ocurred during operations with file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Console.WriteLine($"Access to the file is denied: {unAuthEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error ocurred: {ex.Message}");
            }
        }

        void SaveWordCSV(string word, string translation)
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
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Error occured during the writing word to file: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException unAuthEx)
            {
                Console.WriteLine($"Access to the file is denied:: {unAuthEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error ocurred: {ex.Message}");
            }
        }

        void DisplayDictionary()
        {
            if (answers.Count == 0)
            {
                Console.WriteLine("Dictionary is empty");
                return;
            }
            else
            {
                foreach (var answer in answers)
                {
                    Console.WriteLine("Key: " + answer.Key + " Value: " + answer.Value);
                }
            }

        }

        void Wait(int seconds)
        {
            System.Threading.Thread.Sleep(seconds);
        }
        
    }
}

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

                Logger.InfoMessage("Session started.");

                Wait(Interval);

                startSessionButton = (ChromeInstance.FindElement(By.Id("continue_session_button")).Displayed)
                    ? ChromeInstance.FindElement(By.Id("continue_session_button"))
                    : ChromeInstance.FindElement(By.Id("start_session_button"));
                startSessionButton.Click();

                Wait(Interval);

                IWebElement submitAnswerButton = ChromeInstance.FindElement(By.Id("check"));
                IWebElement nextWordButton = ChromeInstance.FindElement(By.Id("nextword"));
                IWebElement answerInput = ChromeInstance.FindElement(By.Id("answer"));
                string dictionaryKey = "";
                string dictionaryValue = "";


                Logger.SuccessMessage("All DOM elements found.");


                while (!ChromeInstance.FindElement(By.Id("return_mainpage")).Displayed)
                {

                    if (ChromeInstance.FindElement(By.Id("new_word_form")).Displayed)
                    {
                        Logger.InfoMessage("'LEARN new word' spotted and skipped");

                        ChromeInstance.FindElement(By.Id("know_new")).Click();

                        Wait(Interval);

                        ChromeInstance.FindElement(By.Id("skip")).Click();

                        Wait(Interval);

                        continue;
                    }

                    dictionaryKey = ChromeInstance.FindElement(By.ClassName("translations")).Text;

                    if (answers.ContainsKey(dictionaryKey))
                    {
                        Logger.InfoMessage($"KNOWN word: {dictionaryKey} spotted. Trying to complete it...");

                        dictionaryValue = answers[dictionaryKey];
                        answerInput.SendKeys(dictionaryValue);

                        submitAnswerButton.Click();

                        Wait(Interval);

                        if(ChromeInstance.FindElements(By.ClassName("red")).Count > 0)
                        {
                            answers[dictionaryKey] = ChromeInstance.FindElement(By.Id("word")).Text;

                            Logger.ErrorMessage("Saved word wasn't correct answer. Word temporarily changed.");
                        }
                        else if(ChromeInstance.FindElements(By.ClassName("blue")).Count > 0)
                        {
                            answers[dictionaryKey] = "@SYNONYM@";

                            Logger.InfoMessage("Saved word was a synonym. Word changed to incorrect one in purpose.");
                        }
                        else
                        {
                            Logger.SuccessMessage("Word solved.");
                        }

                        nextWordButton.Click();
                    }
                    else
                    {
                        Logger.InfoMessage($"Unknown word: {dictionaryKey} spotted. Saving word...");

                        submitAnswerButton.Click();

                        Wait(Interval);

                        dictionaryValue = ChromeInstance.FindElement(By.Id("word")).Text;
                        answers.Add(dictionaryKey, dictionaryValue);
                        SaveWordCSV(dictionaryKey, dictionaryValue);

                        nextWordButton.Click();
                    }

                    Wait(Interval);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during the quiz process: " + ex.Message);
            }
            finally
            {
                ChromeInstance.Quit();
                Console.Clear();
                Console.WriteLine($"Session of user {User.Login} ended.");
            }

        }

        void CreateChromeInstance(params string[] chromeStartOption)
        {
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments(chromeStartOption);

                ChromeInstance = new ChromeDriver(options);
                ChromeInstance.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);

                Logger.SuccessMessage("Created chrome instance.");
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
                CreateChromeInstance("--start-maximized", "--disable-search-engine-choice-screen", "--mute-audio");

                ChromeInstance.Navigate().GoToUrl("https://instaling.pl/teacher.php?page=login");

                Wait(Interval);

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
                    else
                    {
                        Logger.SuccessMessage($"User {User.Login} logged in.");
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

                Logger.SuccessMessage($"Added new word {word} -- {translation}.");
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

        public void DisplayDictionary()
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
                    Console.WriteLine("Key: " + answer.Key + " || Value: " + answer.Value);
                }
            }

            Console.WriteLine("Press any key to exit program.");
            Console.ReadKey();

        }

        void Wait(int seconds)
        {
            System.Threading.Thread.Sleep(seconds);
        }
        
    }
}

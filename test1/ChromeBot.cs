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

                Wait(Interval);

                while (!ChromeInstance.FindElement(By.Id("return_mainpage")).Displayed)
                {
                   
                    if (ChromeInstance.FindElement(By.Id("new_word_form")).Displayed)
                    {
                        ChromeInstance.FindElement(By.Id("know_new")).Click();
                        Wait(Interval);

                        ChromeInstance.FindElement(By.Id("skip")).Click();
                        Wait(Interval);
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
                        Wait(Interval);
                    }
                    else
                    {
                        submitAnswerButton.Click();
                        Wait(Interval);

                        dictionaryValue = ChromeInstance.FindElement(By.Id("word")).Text;
                        answers.Add(dictionaryKey, dictionaryValue);

                        nextWordButton.Click();
                        Wait(Interval);
                    }
                }

                ChromeInstance.FindElement(By.Id("return_mainpage")).Click();
                DisplayDictionary();
                SaveDictionaryCSV();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred during the quiz process: " + ex.Message);
            }
            finally
            {

                Console.ReadKey();
                ChromeInstance.Quit(); // Always close the browser session
            }

        }

        void LoginUser()
        {
            try
            {
                CreateChromeInstance("--start-maximized", "--disable-search-engine-choice-screen");

                ChromeInstance.Navigate().GoToUrl("https://instaling.pl/teacher.php?page=login");
                Wait(2000);

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

        void CreateChromeInstance(params string[] chromeStartOption)
        {
            try
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments(chromeStartOption);

                ChromeInstance = new ChromeDriver(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to initialize ChromeDriver: " + ex.Message);
                throw;
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

        void SaveDictionaryCSV()
        {
            string dictionaryPath = "C:\\Users\\uryga\\Desktop\\dictionary.csv";
            StreamReader reader;
            StreamWriter writer;

            CsvWriter csvWriter;
            CsvReader csvReader;

            if (File.Exists(dictionaryPath))
            {

            }
            else
            {
                using (File.Create(dictionaryPath)) { }

                using (writer = new StreamWriter(dictionaryPath))
                using  (csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords(answers);
                }

            }
        }

        void Wait(int seconds)
        {
            System.Threading.Thread.Sleep(seconds);
        }
        
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;


namespace test1
{
    internal class ChromeBot
    {
        User User { get; set; }
        int Interval {  get; set; }
        Dictionary<string, string> Answers { get; set; }
        DictionaryManagment DictionaryInstance {  get; set; }
        ChromeDriver ChromeInstance { get; set; }

        public ChromeBot(User user, int interval, DictionaryManagment dictionaryInstance) 
        {
            DictionaryInstance = dictionaryInstance;
            Answers = dictionaryInstance.answers;

            User = user;
            Interval = interval;
        }

        public void TakeTheQuiz()
        {
            try
            {
                LoginUser();

                if (ChromeInstance.FindElements(By.TagName("h4")).Count != 0)
                {
                    Logger.InfoMessage("Todays session already completed. Skipping user...");
                    return;
                }

                IWebElement startSessionButton = ChromeInstance.FindElement(By.ClassName("btn-start-session"));
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
                bool synonym = false;


                Logger.SuccessMessage("All DOM elements found.");


                while (!ChromeInstance.FindElement(By.Id("return_mainpage")).Displayed)
                {

                    if (ChromeInstance.FindElement(By.Id("new_word_form")).Displayed)
                    {
                        Logger.InfoMessage("LEARN new word site spotted and skipped.");

                        ChromeInstance.FindElement(By.Id("know_new")).Click();

                        Wait(Interval);

                        ChromeInstance.FindElement(By.Id("skip")).Click();

                        Wait(Interval);

                        continue;
                    }

                    dictionaryKey = ChromeInstance.FindElement(By.ClassName("translation")).Text;

                    if (Answers.ContainsKey(dictionaryKey))
                    {
                        Logger.InfoMessage($"KNOWN word: {dictionaryKey} spotted. Trying to complete it...");

                        dictionaryValue = Answers[dictionaryKey];
                        answerInput.SendKeys(dictionaryValue);

                        submitAnswerButton.Click();

                        Wait(Interval);

                        if(ChromeInstance.FindElements(By.ClassName("red")).Count > 0)
                        {
                            Answers[dictionaryKey] = ChromeInstance.FindElement(By.Id("word")).Text;

                            if (synonym)
                            {
                                Logger.ErrorMessage($"Synonym temporarily changed to the word provided by installing: {Answers[dictionaryKey]}.");
                            }
                            else
                            {
                                Logger.ErrorMessage($"Provided word was incorrect. Temporarily changing word to the one provided by installing: {Answers[dictionaryKey]}.");
                            }
                        }
                        else if(ChromeInstance.FindElements(By.ClassName("blue")).Count > 0)
                        {
                            Answers[dictionaryKey] = "@SYNONYM@";

                            Logger.InfoMessage("Saved word was a synonym. Word changed to incorrect one to get correct translation from installing.");
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
                        Answers.Add(dictionaryKey, dictionaryValue);
                        DictionaryInstance.SaveWordCSV(dictionaryKey, dictionaryValue);

                        nextWordButton.Click();
                    }

                    Wait(Interval);
                }


                IWebElement showStatistics = ChromeInstance.FindElement(By.Id("grade_report_button"));
                IWebElement weekWorkDays = ChromeInstance.FindElement(By.Id("work_week_days"));
                showStatistics.Click();
                Wait(Interval);
                Console.WriteLine();
                Logger.InfoMessage($"NUMBER OF WORKING DAYS THIS WEEK: {weekWorkDays.Text}");

            }
            catch (Exception ex)
            {
                Logger.ErrorMessage("An error occurred during the quiz process: " + ex.Message);
            }
            finally
            {
                ChromeInstance.Close();
                Logger.InfoMessage($"Session of user {User.Login} ended.");
                Console.WriteLine("=======================================================================================");
            }

        }

        void CreateChromeInstance(params string[] chromeStartOption)
        {
            try
            {
                var serviceOptions = ChromeDriverService.CreateDefaultService();
                serviceOptions.SuppressInitialDiagnosticInformation = true;
                serviceOptions.EnableVerboseLogging = false;               
                serviceOptions.LogPath = "nul";


                ChromeOptions browserOptions = new ChromeOptions();
                browserOptions.AddArguments(chromeStartOption);

                ChromeInstance = new ChromeDriver(serviceOptions, browserOptions);
                ChromeInstance.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);

                Logger.SuccessMessage("Created chrome instance.");
            }
            catch (Exception ex)
            {
                Logger.ErrorMessage("Failed to initialize ChromeDriver: " + ex.Message);
                throw;
            }
        }

        void LoginUser()
        {
            try
            {
                CreateChromeInstance("--start-maximized", "--disable-search-engine-choice-screen", "--headless", "--mute-audio", "--log-level=3");

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
                Logger.ErrorMessage("Error during login: Element not found. " + ex.Message);
            }
            catch (Exception ex)
            {
                Logger.ErrorMessage("An error occurred during login: " + ex.Message);
            }
        }

        public void DisplayDictionary()
        {
            if (Answers.Count == 0)
            {
                Console.WriteLine("Dictionary is empty");
                return;
            }
            else
            {
                foreach (var answer in Answers)
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

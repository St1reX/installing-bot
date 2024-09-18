using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;


namespace test1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string login = "1PTP335786";
                string password = "mdnnb";
                Dictionary<string, string> answers = new Dictionary<string, string>();
                

                Console.WriteLine("Session with browser started.");


                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-search-engine-choice-screen");


                IWebDriver d1 = new ChromeDriver(options);
                d1.Navigate().GoToUrl("https://instaling.pl/student/pages/mainPage.php?student_id=1591074");

                System.Threading.Thread.Sleep(3000);

                if (d1.Url == "https://instaling.pl/teacher.php?page=login")
                {

                    var privacyButton = d1.FindElement(By.ClassName("fc-button-label"));
                    privacyButton.Click();

                    var loginInput = d1.FindElement(By.Name("log_email"));
                    loginInput.Clear();
                    loginInput.SendKeys(login);


                    var passwordInput = d1.FindElement(By.Name("log_password"));
                    passwordInput.Clear();
                    passwordInput.SendKeys(password);

                    var submitFormButton = d1.FindElement(By.XPath("//button[@type='submit']"));
                    submitFormButton.Click();
                }
                else if (!d1.Url.Contains("https://instaling.pl/student/pages/mainPage.php?student_id="))
                {
                    throw new Exception("Unkown url provided. Program terminated");
                }

                System.Threading.Thread.Sleep(3000);

                var startSessionButton = d1.FindElement(By.ClassName("btn-session"));
                startSessionButton.Click();

                System.Threading.Thread.Sleep(1000);

                startSessionButton = (d1.FindElement(By.Id("continue_session_button")).Displayed) ? d1.FindElement(By.Id("continue_session_button")) : d1.FindElement(By.Id("start_session_button"));
                startSessionButton.Click();


                IWebElement submitAnswerButton = d1.FindElement(By.Id("check"));
                IWebElement nextWordButton = d1.FindElement(By.Id("nextword"));
                IWebElement answerInput = d1.FindElement(By.Id("answer"));
                string dictionaryKey = "";
                string dictionaryValue = "";
                System.Threading.Thread.Sleep(1000);

                while (!d1.FindElement(By.Id("return_mainpage")).Displayed)
                {
                    
                    if(d1.FindElement(By.Id("new_word_form")).Displayed)
                    {
                        d1.FindElement(By.Id("know_new")).Click();
                        System.Threading.Thread.Sleep(1000);
                        d1.FindElement(By.Id("skip")).Click();
                        System.Threading.Thread.Sleep(1000);

                        continue;
                    }
                    dictionaryKey = d1.FindElement(By.ClassName("translations")).Text;
                    if(answers.ContainsKey(dictionaryKey))
                    {
                        dictionaryValue = answers[dictionaryKey];
                        answerInput.SendKeys(dictionaryValue);

                        submitAnswerButton.Click();

                        System.Threading.Thread.Sleep(1000);

                        nextWordButton.Click();
                        System.Threading.Thread.Sleep(1000);
                    }
                    else
                    {
                        submitAnswerButton.Click();

                        System.Threading.Thread.Sleep(1000);
                        dictionaryValue = d1.FindElement(By.Id("word")).Text;
                        answers.Add(dictionaryKey, dictionaryValue);

                        nextWordButton.Click();
                        System.Threading.Thread.Sleep(1000);
                    }
                }


                foreach(var answer in answers)
                {
                    Console.WriteLine("Key: " + answer.Key + " Value: " + answer.Value);
                }

                d1.FindElement(By.Id("return_mainpage")).Click();

                Console.WriteLine("Session with browser ended.");

                Console.ReadKey();

                d1.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

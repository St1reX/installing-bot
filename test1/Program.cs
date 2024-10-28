using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using CsvHelper;
using System.Globalization;
using System.IO;


namespace test1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            User u1 = new User();
            ChromeBot ch1 = new ChromeBot(u1, 2000); 
            ch1.TakeTheQuiz();
        }

    }
}

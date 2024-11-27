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
            DictionaryManagment dictionaryInstance = DictionaryManagment.CreateInstance();

            User Uryga = new User();
            ChromeBot UrygaSession = new ChromeBot(Uryga, 2000, dictionaryInstance);
            UrygaSession.TakeTheQuiz();

            


            Console.WriteLine("WORDS DICTIONARY (HI HITLA!)");
            UrygaSession.DisplayDictionary();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Globalization;
using System.IO;


namespace test1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            DictionaryManagment dictionaryInstance = DictionaryManagment.CreateInstance();

            User Uryga = new User("1PTP335786", "mdnnb");
            ChromeBot UrygaSession = new ChromeBot(Uryga, 2000, dictionaryInstance);

            User Wańczyk = new User("aktr18916", "rkned");
            ChromeBot WańczykSession = new ChromeBot(Wańczyk, 2000, dictionaryInstance);

            User Gocal = new User("1PTP70798", "eicrw");
            ChromeBot GocalSession = new ChromeBot(Gocal, 2000, dictionaryInstance);

            var tasks = new List<Task>
            {
                Task.Run(() => UrygaSession.TakeTheQuiz()),
                Task.Run(() => WańczykSession.TakeTheQuiz()),
                Task.Run(() => GocalSession.TakeTheQuiz())
            };

            await Task.WhenAll(tasks);

            Console.WriteLine("WORDS DICTIONARY (HI HITLA!)");
            UrygaSession.DisplayDictionary();
        }

    }
}

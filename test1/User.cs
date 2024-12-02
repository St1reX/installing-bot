using OpenQA.Selenium.DevTools.V126.FedCm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test1
{
    internal class User
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public User(string Login = null, string Password = null)
        {
            Console.Write("Enter your Installing login: ");
            this.Login = Login == null ? Console.ReadLine() : Login;
            Console.Write($"{this.Login} \n");

            Console.Write("Enter your Installing password: ");
            this.Password = Password == null ? Console.ReadLine() : Password;
            Console.Write($"{this.Password}");

        }
    }
}

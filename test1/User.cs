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

        public User()
        {
            Console.WriteLine("Enter you Installing login: ");
            Login = "";
            Console.WriteLine("Enter you Installing login: ");
            Password = "";

            
        }
    }
}

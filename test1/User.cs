using System;

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

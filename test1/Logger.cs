using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test1
{
    internal class Logger
    {
        private static readonly object _fileLock = new object();
        static string projectRoot = Directory.GetParent(AppContext.BaseDirectory)
                               .Parent.Parent.Parent.FullName;
        static string directoryPath = Path.Combine(projectRoot, "logs");

        public static void ErrorMessage(string message, string login = null)
        {
            string time = DateTime.Now.ToLongTimeString();
            string logLine = $"ERROR: {time} || {message}";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logLine);
            Console.ForegroundColor = ConsoleColor.White;

            if(login != null)
            {
                WriteToFile(logLine, login);
            }
        }

        public static void InfoMessage(string message)
        {
            string time = DateTime.Now.ToLongTimeString();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"INFO: {time} || {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void SuccessMessage(string message) 
        {
            string time = DateTime.Now.ToLongTimeString();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"SUCCESS: {time} || {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogInfoFile(string message, string login)
        {
            string time = DateTime.Now.ToLongTimeString();
            string logLine = $"INFO: {time} || {message}";

            WriteToFile(logLine, login);
        }

        public static void LogSuccessFile(string message, string login)
        {
            string time = DateTime.Now.ToLongTimeString();
            string logLine = $"SUCCESS: {time} || {message}";

            WriteToFile(logLine, login);
        }

        private static void WriteToFile(string message, string login)
        {
            EnsureLogDirectoryExists();

            string filePath = Path.Combine(directoryPath, $"{login}.log");

            lock (_fileLock)
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }

                File.AppendAllText(filePath, message + Environment.NewLine);
            }
        }

        private static void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}

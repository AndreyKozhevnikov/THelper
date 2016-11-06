using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THelper {
    public interface IMessageProcessor {
        void ConsoleWrite(string _message);
        void ConsoleWrite(string _message, ConsoleColor color);
        void ConsoleWriteLine();
        ConsoleKey ConsoleReadKey(bool intercept);
    }
    public class MessageProcessor : IMessageProcessor {
        public void ConsoleWrite(string _message) { //10
            Console.Write(_message);
        }
        public void ConsoleWrite(string _message, ConsoleColor color) {//11

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(_message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void ConsoleWriteLine() {
            Console.WriteLine();
        }

        public ConsoleKey ConsoleReadKey(bool intercept) {
            return Console.ReadKey(intercept).Key;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THelper {
    public interface IMessageProcessor {
        void ConsoleWrite(string _message);
        void ConsoleWrite(string _message, ConsoleColor color);
        void ConsoleWrite(string _message, ConsoleColor color,bool addNewLine);
        void ConsoleWriteLine();
        ConsoleKey ConsoleReadKey(bool intercept);
        void Setup();
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

        public void ConsoleWrite(string _message, ConsoleColor color, bool addNewLine) {
            this.ConsoleWrite(_message, color);
            this.ConsoleWriteLine();
        }

        public void Setup() {
            Console.OutputEncoding = Encoding.UTF8;
        }
    }
}

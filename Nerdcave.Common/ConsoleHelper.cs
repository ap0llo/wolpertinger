//Origin: http://codepaste.net/puk8ex#

//Writes colored text to the console and allows to clear previously written lines
//as long a not line break is present
 
//Sample - screenshot at http://img299.imageshack.us/img299/3931/consolex.png
 
/*

C.InfoLine("Non-colored text...");
            
C.Error("Outch, an error.");
Thread.CurrentThread.Join(1000);
C.ClearLine();
C.Warn("Ok, only a warning.");
Thread.CurrentThread.Join(1000);
C.ClearLine();
C.SuccessLine("OK.");
 
C.InfoColor = ConsoleColor.Blue;
C.InfoLine("I'm feeling blue");
 
 
*/
 
using System;
using Nerdcave.Common.Extensions;

namespace Nerdcave.Common
{


    /// <summary>
    /// Console helper class.
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// The color that is used to print out errors to the console.
        /// </summary>
        public static ConsoleColor ErrorColor = ConsoleColor.Red;

        /// <summary>
        /// The color that is used to print out warnings to the console.
        /// </summary>
        public static ConsoleColor WarningColor = ConsoleColor.Yellow;

        /// <summary>
        /// The color that is used to print out infos to the console.
        /// </summary>
        public static ConsoleColor SuccessColor = ConsoleColor.Green;

        /// <summary>
        /// The color that is used to print out infos to the console.
        /// If set to null, the current console color is used.
        /// </summary>
        public static ConsoleColor? InfoColor;



        public static void ErrorLine(string msg, params object[] args)
        {
            WriteLine(ErrorColor, msg, args);
        }

        public static void Error(string msg, params object[] args)
        {
            Write(ErrorColor, msg, args);
        }


        public static void WarnLine(string msg, params object[] args)
        {
            WriteLine(WarningColor, msg, args);
        }

        public static void Warn(string msg, params object[] args)
        {
            Write(WarningColor, msg, args);
        }


        public static void InfoLine(string msg, params object[] args)
        {
            WriteLine(InfoColor ?? Console.ForegroundColor, msg, args);
        }

        public static void Info(string msg, params object[] args)
        {
            Write(InfoColor ?? Console.ForegroundColor, msg, args);
        }


        public static void SuccessLine(string msg, params object[] args)
        {
            WriteLine(SuccessColor, msg, args);
        }

        public static void Success(string msg, params object[] args)
        {
            Write(SuccessColor, msg, args);
        }


        /// <summary>
        /// Clears the current line.
        /// </summary>
        public static void ClearLine()
        {
            var position = Console.CursorLeft;

            //overwrite with white space (backspace doesn't really clear the buffer,
            //would need a hacky combination of \b\b and single whitespace)
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write("".PadRight(position));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        public static void ClearLine(int top)
        {
            var position = Console.CursorLeft;

            //overwrite with white space (backspace doesn't really clear the buffer,
            //would need a hacky combination of \b\b and single whitespace)
            Console.SetCursorPosition(0, top);
            Console.Write("".PadRight(position));
            Console.SetCursorPosition(0, top);
        }


        public static void Write(string msg, params object[] args)
        {
            Console.Write(msg, args);
        }

        public static void WriteLine(ConsoleColor color, string msg, params object[] args)
        {
            Write(color, msg, args);
            Console.Out.WriteLine();
        }

        public static void Write(ConsoleColor color, string msg, params object[] args)
        {
            try
            {
                Console.ForegroundColor = color;
                Console.Write(msg, args);
            }
            finally
            {
                Console.ResetColor();
            }
        }


        public static void WriteLineCentered(ConsoleColor color, string msg, params object[] args)
        {
            while (msg.Length < Console.WindowWidth)
            {
                msg = " " + msg + " ";
            }
            WriteLine(color, msg, args);
        }

        public static void WriteSeparationLine(ConsoleColor color)
        {
            string line = "";
            for (int i = 0; i < Console.WindowWidth - 10; i++)
            {
                line += "-";
            }

            WriteLineCentered(color, line);
        }

        public static void WriteSpaces(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Console.Write(" ");
            }
        }

        /// <summary>
        /// Displays the message the user has to answer with yes or no
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Prompt(string message)
        {
            return Prompt(message, ConsoleColor.Gray);
        }

        /// <summary>
        /// Displays the message the user has to answer with yes or no
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool Prompt(string message, ConsoleColor color)
        {
            Write(color, message + " Y/N ");

            string input = Console.ReadLine().ToLower();

            while (input != "y" && input != "yes" && input != "n" && input != "no")
            {
                Write(color, message + " Y/N ");
                input = Console.ReadLine().ToLower();
            }

            return (input == "y" || input == "yes");
        }

        public static string GetPassword()
        {
            return GetPassword("Enter password: ");
        }

        /// <summary>
        /// Prompts the user to enter a password. The password in the console window will be marked with asterisks. After the usser pressed enter, the entire password-line will be removed from the Console window
        /// </summary>
        /// <returns>Returns the string the user enterd.</returns>
        public static string GetPassword(string message)
        {
            Console.Write("{0}: ", message);
            string pw = "";

            ConsoleKeyInfo key = Console.ReadKey(true);

            while (key.Key != ConsoleKey.Enter)
            {

                if (key.Key == ConsoleKey.Escape)
                {
                    ClearLine();
                    pw = "";
                    Console.Write("{0}: ", message);                 
                }
                // Backspace Should Not Work
                else if (key.Key != ConsoleKey.Backspace)
                {
                    pw += key.KeyChar;
                    Console.Write("*");
                }
                else if (pw.Length > 0)
                {
                    pw = pw.RemoveLastChar();
                    Console.Write("\b \b");
                }

                key = Console.ReadKey(true);

            }

            Console.Write("\n");

            return pw;

        }

    }
}

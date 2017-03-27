using System;
using AutoIt;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ButtonClicker
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.Title = "Clicking buttons so you dont have to";

            Console.WriteLine(@"   ___       _   _                  ___ _ _      _             ");
            Console.WriteLine(@"  / __\_   _| |_| |_ ___  _ __     / __\ (_) ___| | _____ _ __ ");
            Console.WriteLine(@" /__\// | | | __| __/ _ \| '_ \   / /  | | |/ __| |/ / _ \ '__|");
            Console.WriteLine(@"/ \/  \ |_| | |_| || (_) | | | | / /___| | | (__|   <  __/ |   ");
            Console.WriteLine(@"\_____/\__,_|\__|\__\___/|_| |_| \____/|_|_|\___|_|\_\___|_|   ");
            Console.WriteLine(@"                                                               ");

            Console.WriteLine("All this application does is push enter on a window with the given name");
            Console.WriteLine("Please dont blame me if you fuck things up with this");
            Console.WriteLine("Enter window title (default: Error Applying Security)");
            var windowTitle = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(windowTitle))
            {
                windowTitle = "Error Applying Security";
            }
            
            Log.Info("Starting Logging Framework");
            Log.Info("Starting mouse jiggler");

            Task.Factory.StartNew(Jiggler);

            while (true)
            {
                var pointer = Convert.ToBoolean(AutoItX.WinExists(windowTitle));

                if (pointer)
                {
                    AutoItX.WinActivate(windowTitle);
                    AutoItX.WinWaitActive(windowTitle);
                    Log.Info("I See the window");
                    AutoItX.Send("{ENTER}");
                    Log.Info("I clicked window");
                }

                Thread.Sleep(50);
            }
        }

        private static Task Jiggler()
        {
            int moveBy = 1;

            while (true)
            {
                var point = AutoItX.MouseGetPos();
                AutoItX.MouseMove(point.X + moveBy, point.Y, 0);

                Thread.Sleep(60000);
                moveBy = moveBy * -1;
            }
        }
    }
}

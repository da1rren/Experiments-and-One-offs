using System;
using System.IO;
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
            Console.WriteLine(@"   ___       _   _                  ___ _ _      _             ");
            Console.WriteLine(@"  / __\_   _| |_| |_ ___  _ __     / __\ (_) ___| | _____ _ __ ");
            Console.WriteLine(@" /__\// | | | __| __/ _ \| '_ \   / /  | | |/ __| |/ / _ \ '__|");
            Console.WriteLine(@"/ \/  \ |_| | |_| || (_) | | | | / /___| | | (__|   <  __/ |   ");
            Console.WriteLine(@"\_____/\__,_|\__|\__\___/|_| |_| \____/|_|_|\___|_|\_\___|_|   ");
            Console.WriteLine(@"                                                               ");

            if (!File.Exists("config.json"))
            {
                Console.WriteLine("Unable to find config.json exiting");
                Thread.Sleep(2000);
                return;
            }

            var settings = Settings.Get();

            Console.Title = settings.Title;
            

            Console.WriteLine("All this application does is push enter on a window with the given name");
            Console.WriteLine("Please dont blame me if you fuck things up with this");
            Console.WriteLine("Enter window title (default: Error Applying Security)");
            var windowTitle = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(windowTitle))
            {
                windowTitle = settings.DefaultWindow;
            }
            
            Log.Info("Starting Logging Framework");

            if (settings.JiggleMouse)
            {
                Log.Info("Starting mouse jiggler");
                Task.Factory.StartNew(Jiggler);
            }

            for (var i = 0; i < settings.KeysSequences.Count; i++)
            {
                Console.WriteLine($"{i}: {settings.KeysSequences[i].DisplayName}");
            }

            Console.WriteLine($"Please enter a number between 0 and {settings.KeysSequences.Count - 1}");
            var position = Convert.ToInt32(Console.ReadLine());

            var sequence = settings.KeysSequences[position];

            while (true)
            {
                var pointer = Convert.ToBoolean(AutoItX.WinExists(windowTitle));

                if (pointer)
                {
                    AutoItX.WinActivate(windowTitle);
                    AutoItX.WinWaitActive(windowTitle);
                    Log.Info("Window Focused");
                    Log.Info("Commands Executing");

                    foreach (var command in sequence.Sequence)
                    {
                        AutoItX.Send(command);
                        Thread.Sleep(15);

                    }

                    Log.Info("Commands Executed");
                }

                Thread.Sleep(50);
            }
        }

        private static Task Jiggler()
        {
            var moveBy = 1;

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

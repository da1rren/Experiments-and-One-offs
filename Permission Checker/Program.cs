using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PermissionChecker
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Console.WindowWidth = 200;

            Console.WriteLine(@"   _____                                                             .__              .__                       ");
            Console.WriteLine(@"  /  _  \_______  ____     _____ ___.__. ______   ___________  _____ |__| ______ _____|__| ____   ____   ______ ");
            Console.WriteLine(@" /  /_\  \_  __ \/ __ \   /     <   |  | \____ \_/ __ \_  __ \/     \|  |/  ___//  ___/  |/  _ \ /    \ /  ___/ ");
            Console.WriteLine(@"/    |    \  | \|  ___/  |  Y Y  \___  | |  |_> >  ___/|  | \/  Y Y  \  |\___ \ \___ \|  (  <_> )   |  \\___ \  ");
            Console.WriteLine(@"\____|__  /__|   \___  > |__|_|  / ____| |   __/ \___  >__|  |__|_|  /__/____  >____  >__|\____/|___|  /____  > ");
            Console.WriteLine(@"________\/_          \/__      \/\/    ._|__|______  \/            \/        \/     \/               \/     \/  ");
            Console.WriteLine(@"\_   _____/_ __  ____ |  | __ ____   __| _|_____   \                                                            ");
            Console.WriteLine(@" |    __)|  |  \/ ___\|  |/ // __ \ / __ |   /   __/                                                            ");
            Console.WriteLine(@" |     \ |  |  |  \___|    <\  ___// /_/ |  |   |                                                               ");
            Console.WriteLine(@" \___  / |____/ \___  >__|_ \\___  >____ |  |___|                                                               ");
            Console.WriteLine(@"     \/             \/     \/    \/     \/  <___>                                                               ");

            long total = 0;
            long fuckedPermissions = 0;
            var start = DateTime.Now;


            var root = string.Empty;
            if (Array.Exists(args, s => s == "--root"))
            {
                var index = Array.IndexOf(args, "--root");
                root = args[index + 1];
            }
            else
            {
                Console.WriteLine("Please enter the root path");
                root = Console.ReadLine();
            }


            var output = string.Empty;
            if (Array.Exists(args, s => s == "--outDir"))
            {
                var index = Array.IndexOf(args, "--outDir");
                output = args[index + 1];
            }
            else
            {
                Console.WriteLine("Output directory");
                output = Console.ReadLine();
            }

            output = Path.Combine(output, "Output.txt");

            if (!File.Exists(output))
            {
                File.Create(output);
            }

            var token = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Aborting");

                e.Cancel = true;

                if (!token.IsCancellationRequested)
                    token.Cancel();
            };

            var traverser = new FileTraverser(root, token.Token);

            Console.Clear();

            foreach (var file in traverser)
            {
                total++;

                var truncated = file;

                if (truncated.Length > 80)
                {
                    truncated = "..." + truncated.Substring(truncated.Length - 80, 80);
                }

                var timespace = DateTime.Now - start;

                Console.SetCursorPosition(0, 0);

                Console.WriteLine($"Total Files: {total}");
                Console.WriteLine($"Current File: {truncated}");
                Console.WriteLine($"Fucked permissions: {fuckedPermissions}");

                if (timespace.Minutes != 0)
                {
                    Console.WriteLine($"Files per sec: {total / timespace.TotalSeconds}");
                }

                if (traverser.ErrorResults != null)
                {
                    File.AppendAllText(output, traverser.ErrorResults.ToString());
                    fuckedPermissions++;
                    traverser.ErrorResults = null;
                }
            }

            var totalTime = DateTime.Now - start;

            File.AppendAllText(output, "Final Score" + Environment.NewLine);
            File.AppendAllText(output, "====================================================" + Environment.NewLine);
            File.AppendAllText(output, $"Total Files: {total}" + Environment.NewLine);
            File.AppendAllText(output, $"Fucked permissions: {fuckedPermissions}" + Environment.NewLine);
            File.AppendAllText(output, $"Files per second: {total / totalTime.TotalSeconds}" + Environment.NewLine);

            Console.WriteLine("Complete, press any key to exit");
            Console.ReadKey();
        }
    }
}

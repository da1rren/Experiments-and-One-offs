using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Archiver.Config;
using Archiver.Pipes;
using Archiver.Pipes.Interfaces;
using Archiver.Result;
using Autofac;

namespace Archiver
{
    internal class Program
    {
        private const string ConfigPath = "Config.json";

        private static readonly CancellationTokenSource ExitToken = new CancellationTokenSource();
        private static IContainer Container { get; set; }

        private static void Main(string[] args)
        {
            Console.WriteLine(@"      _       _______      ______  ____  ____  _____  ____   ____  ________  _______     ");
            Console.WriteLine(@"     / \     |_   __ \   .' ___  ||_   ||   _||_   _||_  _| |_  _||_   __  ||_   __ \    ");
            Console.WriteLine(@"    / _ \      | |__) | / .'   \_|  | |__| |    | |    \ \   / /    | |_ \_|  | |__) |   ");
            Console.WriteLine(@"   / ___ \     |  __ /  | |         |  __  |    | |     \ \ / /     |  _| _   |  __ /    ");
            Console.WriteLine(@" _/ /   \ \_  _| |  \ \_\ `.___.'\ _| |  | |_  _| |_     \ ' /     _| |__/ | _| |  \ \_  ");
            Console.WriteLine(@"|____| |____||____| |___|`.____ .'|____||____||_____|     \_/     |________||____| |___| ");
            Console.WriteLine(@"                                                                                         ");

            if (!File.Exists(ConfigPath))
                throw new ArgumentException("Cannot find config file");

            ConfigureDependencies();

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Aborting");

                e.Cancel = true;

                if (!ExitToken.IsCancellationRequested)
                    ExitToken.Cancel();
            };

            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var filePipe = new FilePipe(
                Container.Resolve<IConfig>(),
                Container.Resolve<IResultFactory>(),
                ExitToken.Token);

            var metadataPipe = new MetadataPipe(
                Container.Resolve<IConfig>(),
                Container.Resolve<IResultFactory>(),
                filePipe);

            var transferPipe = new TransferPipe(
                Container.Resolve<IConfig>(),
                Container.Resolve<IResultFactory>(),
                metadataPipe);

            var cancelSource = new CancellationTokenSource();

            var ui = Task.Run(() => ReportStatus(Container.Resolve<IResultFactory>(), filePipe, metadataPipe, transferPipe, cancelSource.Token));

            await Task.WhenAll(filePipe.Execute(),
                metadataPipe.Execute(),
                transferPipe.Execute());

            cancelSource.Cancel();
            await ui;

            var results = Container.Resolve<IResultFactory>();
            Console.WriteLine("Complete.  Writing index file.");
            results.WriteFile();
        }

        private static async Task ReportStatus(
            IResultFactory results,
            IPipeline<string> filePipeline,
            IPipeline<FileResult> metadataPipeline,
            IQueued transferPipe,
            CancellationToken token)
        {
            await Task.Run(() =>
            {
                var start = DateTime.Now;
                Console.CursorVisible = false;

                while (!token.IsCancellationRequested)
                {
                    var builder = new StringBuilder();

                    builder.AppendLine(string.Format("{0,-40}",$"Files Completed: {results.TotalFiles}"));
                    builder.AppendLine(string.Format("{0,-40}",$"Errors: {results.TotalErrors}"));
                    builder.AppendLine(string.Format("{0,-40}",$"Awaiting Analysis: {filePipeline.Buffer.Count}"));
                    builder.AppendLine(string.Format("{0,-40}",$"Awaiting Transfer: {transferPipe.GetWaitingTasks()}"));
                    builder.AppendLine(string.Format("{0,-40}",$"Time elapsed: {(DateTime.Now - start)}"));
                    builder.AppendLine(string.Format("{0,-40}",$"Size of transfer: {results.TotalSize / 1024 / 1024}MBs"));

                    Console.Write(builder.ToString());
                    Console.SetCursorPosition(0, Console.CursorTop - 6);
                    Thread.Sleep(25);
                }

                Console.WriteLine("Complete.");
            });
        }

        private static void ConfigureDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Config.Config>()
                .As<IConfig>()
                .OnActivating(x => x.ReplaceInstance(Config.Config.Load(ConfigPath)))
                .SingleInstance();

            builder.RegisterType<ResultFactory>()
                .As<IResultFactory>()
                .OnActivating(x => new ResultFactory(x.Context.Resolve<IConfig>()))
                .SingleInstance();

            Container = builder.Build();
        }
    }
}
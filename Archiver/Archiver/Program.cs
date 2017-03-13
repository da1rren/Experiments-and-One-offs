using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

            await Task.WhenAll(filePipe.Execute(),
                metadataPipe.Execute(),
                transferPipe.Execute(),
                ReportStatus(Container.Resolve<IResultFactory>(), filePipe, metadataPipe));

            var results = Container.Resolve<IResultFactory>();
            results.WriteFile();
        }

        private static async Task ReportStatus(
            IResultFactory results,
            IPipeline<string> filePipeline,
            IPipeline<FileResult> metadataPipeline)
        {
            await Task.Run(() =>
            {
                while (!metadataPipeline.Buffer.IsCompleted)
                {
                    Console.WriteLine($"Files: {results.TotalFiles}");
                    Console.WriteLine($"Errors: {results.TotalErrors}");
                    Console.WriteLine($"Awaiting Analysis: {filePipeline.Buffer.Count}");
                    Console.WriteLine($"Awaiting Transfer: {metadataPipeline.Buffer.Count}");
                    Console.SetCursorPosition(0, Console.CursorTop - 4);
                    Thread.Sleep(25);
                }

                Console.WriteLine("Complete.");
            });
        }

        private static void ConfigureDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Config>()
                .As<IConfig>()
                .OnActivating(x => x.ReplaceInstance(Config.Load(ConfigPath)))
                .SingleInstance();

            builder.RegisterType<ResultFactory>()
                .As<IResultFactory>()
                .OnActivating(x => new ResultFactory(x.Context.Resolve<IConfig>()))
                .SingleInstance();

            Container = builder.Build();
        }
    }
}
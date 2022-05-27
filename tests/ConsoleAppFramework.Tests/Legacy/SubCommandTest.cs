﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ConsoleAppFramework.Tests
{

    public class SubCommandTest
    {
        readonly ITestOutputHelper testOutput;

        public SubCommandTest(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        public class TwoSubCommand : ConsoleAppBase
        {
            public void Main(double d)
            {
                Context.Logger.LogInformation($"d:{d.ToString(CultureInfo.InvariantCulture)}");
            }

            [Command("run")]
            public void Run(string path, string pfx)
            {
                Context.Logger.LogInformation($"path:{path}");
                Context.Logger.LogInformation($"pfx:{pfx}");
            }

            [Command("sum")]
            public void Sum([Option(0)]int x, [Option(1)]int y)
            {
                Context.Logger.LogInformation($"x:{x}");
                Context.Logger.LogInformation($"y:{y}");
            }

            [Command("opt")]
            public void Option([Option(0)]string input, [Option("x")]int xxx, [Option("y")]int yyy)
            {
                Context.Logger.LogInformation($"input:{input}");
                Context.Logger.LogInformation($"x:{xxx}");
                Context.Logger.LogInformation($"y:{yyy}");
            }
        }

        [Fact]
        public async Task TwoSubCommandTest()
        {
            {
                var args = "-d 12345.12345".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<TwoSubCommand>(args);
                log.InfoLogShouldBe(0, "d:12345.12345");
            }
            {
                var args = "run -path foo -pfx bar".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<TwoSubCommand>(args);
                log.InfoLogShouldBe(0, "path:foo");
                log.InfoLogShouldBe(1, "pfx:bar");
            }
            {
                var args = "sum 10 20".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<TwoSubCommand>(args);
                log.InfoLogShouldBe(0, "x:10");
                log.InfoLogShouldBe(1, "y:20");
            }
            {
                var args = "opt foobarbaz -x 10 -y 20".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<TwoSubCommand>(args);
                log.InfoLogShouldBe(0, "input:foobarbaz");
                log.InfoLogShouldBe(1, "x:10");
                log.InfoLogShouldBe(2, "y:20");
            }
        }

        public class AliasCommand : ConsoleAppBase
        {
            [Command(new[] { "run", "r" })]
            public void Run(string path, string pfx)
            {
                Context.Logger.LogInformation($"path:{path}");
                Context.Logger.LogInformation($"pfx:{pfx}");
            }

            [Command(new[] { "su", "summmm" })]
            public void Sum([Option(0)]int x, [Option(1)]int y)
            {
                Context.Logger.LogInformation($"{x + y}");
            }
        }

        [Fact]
        public async Task AliasCommandTest()
        {
            {
                var collection = new[]{
                    "r -path foo -pfx bar".Split(' '),
                    "run -path foo -pfx bar".Split(' '),
                };
                foreach (var args in collection)
                {
                    var log = new LogStack();
                    await new HostBuilder()
                        .ConfigureTestLogging(testOutput, log, true)
                        .RunConsoleAppFrameworkAsync<AliasCommand>(args);
                    log.InfoLogShouldBe(0, "path:foo");
                    log.InfoLogShouldBe(1, "pfx:bar");
                }
            }
            {
                {
                    var args = "su 10 20".Split(' ');
                    var log = new LogStack();
                    await new HostBuilder()
                        .ConfigureTestLogging(testOutput, log, true)
                        .RunConsoleAppFrameworkAsync<AliasCommand>(args);
                    log.InfoLogShouldBe(0, "30");
                }
                {
                    var args = "summmm 99 100".Split(' ');
                    var log = new LogStack();
                    await new HostBuilder()
                        .ConfigureTestLogging(testOutput, log, true)
                        .RunConsoleAppFrameworkAsync<AliasCommand>(args);
                    log.InfoLogShouldBe(0, "199");
                }
            }
        }

        public class OverrideDefaultCommand : ConsoleAppBase
        {
            [Command("list")]
            public void List()
            {
                Context.Logger.LogInformation($"lst");
            }

            [Command(new[] { "help", "h" })]
            public void Help()
            {
                Context.Logger.LogInformation($"hlp");
            }
        }

        [Fact]
        public async Task OverrideDefaultCommandTest()
        {
            {
                var args = "list".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<OverrideDefaultCommand>(args);
                log.InfoLogShouldBe(0, "lst");
            }
            {
                var args = "help".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<OverrideDefaultCommand>(args);
                log.InfoLogShouldBe(0, "hlp");
            }
            {
                var args = "h".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkAsync<OverrideDefaultCommand>(args);
                log.InfoLogShouldBe(0, "hlp");
            }
        }


        [Command("foo")]
        public class SubCommandsWithRootCommand1 : ConsoleAppBase
        {
            [CommandRoot]
            public void Foo()
            {
                Context.Logger.LogInformation("foo");
            }

            [Command("bar")]
            public void FooBar()
            {
                Context.Logger.LogInformation("foo bar");
            }
        }

        [Command("asd")]
        public class SubCommandsWithRootCommand2 : ConsoleAppBase
        {
            [CommandRoot]
            public void Asd()
            {
                Context.Logger.LogInformation("asd");
            }

            [Command("fgh")]
            public void AsdFgh()
            {
                Context.Logger.LogInformation("asd fgh");
            }
        }

        [Fact]
        public async Task SubCommandsWithRootCommandTest()
        {
            {
                var args = "foo".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkSubCommandsAsync<SubCommandsWithRootCommand1, SubCommandsWithRootCommand2>(args);
                log.InfoLogShouldBe(0, "foo");
            }
            {
                var args = "foo bar".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkSubCommandsAsync<SubCommandsWithRootCommand1, SubCommandsWithRootCommand2>(args);
                log.InfoLogShouldBe(0, "foo bar");
            }
            {
                var args = "asd".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkSubCommandsAsync<SubCommandsWithRootCommand1, SubCommandsWithRootCommand2>(args);
                log.InfoLogShouldBe(0, "asd");
            }
            {
                var args = "foo bar".Split(' ');
                var log = new LogStack();
                await new HostBuilder()
                    .ConfigureTestLogging(testOutput, log, true)
                    .RunConsoleAppFrameworkSubCommandsAsync<SubCommandsWithRootCommand1, SubCommandsWithRootCommand2>(args);
                log.InfoLogShouldBe(0, "foo bar");
            }
        }

        public class NotFoundPath : ConsoleAppBase
        {
            [Command("run")]
            public void Run(string path, string pfx, string thumbnail, string output, bool allowoverwrite = false)
            {
                Context.Logger.LogInformation($"path:{path}");
                Context.Logger.LogInformation($"pfx:{pfx}");
                Context.Logger.LogInformation($"thumbnail:{thumbnail}");
                Context.Logger.LogInformation($"output:{output}");
                Context.Logger.LogInformation($"allowoverwrite:{allowoverwrite}");
            }
        }

        //[Fact]
        //public async Task NotFoundPathTest()
        //{
        //    var args = "run -path -pfx test.pfx -thumbnail 123456 -output output.csproj -allowoverwrite".Split(' ');
        //    var log = new LogStack();

        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //    {
        //        await new HostBuilder()
        //            .ConfigureTestLogging(testOutput, log, true)
        //            .RunConsoleAppFrameworkAsync<NotFoundPath>(args);
        //    });
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Drawing;
using CruiseAssistant.Preconditions;
using System.Net;
using CruiseAssistant.Services;

namespace CruiseAssistant.Modules.Standard
{
    public class Standard : MinitoriModule
    {
        private Config config;
        private CommandService commands;
        private IServiceProvider services;
        private Dictionary<ulong, bool> rotate = new Dictionary<ulong, bool>();
        private Dictionary<ulong, float> angle = new Dictionary<ulong, float>();
        private DiscordSocketClient socketClient;
        private SpreadsheetData spreadsheet;

        public Standard(CommandService _commands, IServiceProvider _services, Config _config, DiscordSocketClient _socketClient, SpreadsheetData _spreadsheet)
        {
            commands = _commands;
            services = _services;
            config = _config;
            socketClient = _socketClient;
            spreadsheet = _spreadsheet;
        }

        // This is possibly the worst access control I've made, but it works for now.
        // Definitely replace with something better in the future.
        // Preferably configurable
        // This should also be a precondition to be cleaner

        // Update: This has been made into a precondition, but the precondition needs to be fixed.
        // All it currently does is hide things from the help command.
        private bool CheckAccess()
        {
            var user = Context.User as SocketGuildUser;

            if (config.OwnerIds.Contains(user.Id))
                return true;
            else if (Context.Guild.Id == 800080948716503040)  // Pilots Trade Network
            {
                var sommelier = Context.Guild.GetRole(838520893181263872);
                var admin = Context.Guild.GetRole(800125148971663392);
                var mod = Context.Guild.GetRole(813814494563401780);

                if (user.Roles.Contains(sommelier) || user.Roles.Contains(admin) || user.Roles.Contains(mod))
                    return true;
                else
                    return false;
            }
            else if (Context.Guild.Id == 818174236480897055) // P.T.N. Test Server
                return true;
            else
                return false;
        }

        [Command("makepin")]
        [PTNSWineRunners]
        public async Task CreateNewPinnedMessage()
        {
            if (!CheckAccess())
            {
                return;
            }

            var message = await ReplyAsync(embed: await spreadsheet.GetPage());

            await Task.Delay(1000);

            config.UpdateMessageIds.Add(Context.Channel.Id, message.Id);
        }

        [Command("tally")]
        [PTNSWineRunners]
        public async Task CruiseTally()
        {
            // A lot of this is super basic because I slapped it together in a few hours with parts from past projects
            // Definitely a lot of room for improvement
            if (!CheckAccess())
            {
                return;
            }

            await ReplyAsync(embed: await spreadsheet.GetPage());
        }

        [Command("ooze")]
        [Alias("booze")]
        [PTNSWineRunners]
        public async Task CruiseStatus(string changeStatus = "")
        {
            if (!CheckAccess())
            {
                return;
            }

            if (changeStatus.ToLower() == "start")
            {
                if (config.CruiseStatus)
                {
                    await RespondAsync("The holiday is already active!");
                    return;
                }
                else
                {
                    config.CruiseStatus = true;
                    await RespondAsync("🍷 o7");
                    return;
                }
            }
            else if (changeStatus.ToLower() == "end")
            {
                if (!config.CruiseStatus)
                {
                    await RespondAsync("Nothing is running right now. Have you been sneaking some of the wine?");
                    return;
                }
                else
                {
                    config.CruiseStatus = false;
                    await RespondAsync("I hope that Sidewinder on the M pad didn't prevent you from selling that one last load.");
                    return;
                }
            }
            else
            {
                Random asdf = new Random();
                string file = "";
                string source = config.CruiseStatus ? "Yes" : "No";

                var dir = Directory.GetFiles($@"./Images/{source}/", "*.*", SearchOption.AllDirectories);

                if (dir.Count() == 0)
                {
                    return;
                }
                else if (dir.Count() == 1)
                    file = dir.FirstOrDefault();
                else if (dir.Count() > 1)
                    file = dir.OrderBy(x => asdf.Next()).FirstOrDefault();

                if (config.CruiseStatus)
                    await Context.Channel.SendFileAsync(file, "**The booze cruise has started!**");
                else
                    await Context.Channel.SendFileAsync(file, "**Has the booze cruise started?**");
            }
        }

        [Command("help")]
        public async Task HelpCommand()
        {
            Context.IsHelp = true;

            StringBuilder output = new StringBuilder();
            Dictionary<string, List<string>> modules = new Dictionary<string, List<string>>();
            //StringBuilder module = new StringBuilder();
            //var SeenModules = new List<string>();
            //int i = 0;

            output.Append("These are the commands you can use:");

            foreach (var c in commands.Commands)
            {
                //if (!SeenModules.Contains(c.Module.Name))
                //{
                //    if (i > 0)
                //        output.Append(module.ToString());

                //    module.Clear();

                //    foreach (var h in c.Module.Commands)
                //    {
                //        if ((await c.CheckPreconditionsAsync(Context, services)).IsSuccess)
                //        {
                //            module.Append($"\n**{c.Module.Name}:**");
                //            break;
                //        }
                //    }
                //    SeenModules.Add(c.Module.Name);
                //    i = 0;
                //}

                if ((await c.CheckPreconditionsAsync(Context, services)).IsSuccess)
                {
                    //if (i == 0)
                    //    module.Append(" ");
                    //else
                    //    module.Append(", ");

                    //i++;

                    if (!modules.ContainsKey(c.Module.Name))
                        modules.Add(c.Module.Name, new List<string>());

                    if (!modules[c.Module.Name].Contains(c.Name))
                        modules[c.Module.Name].Add(c.Name);

                    //module.Append($"`{c.Name}`");
                }
            }

            //if (i > 0)
            //    output.AppendLine(module.ToString());

            foreach (var kv in modules)
            {
                output.Append($"\n**{kv.Key}:** {kv.Value.Select(x => $"`{x}`").Join(", ")}");
            }

            await ReplyAsync(output.ToString());
        }
        
        [Command("mitri")]
        [PTNSWineRunners]
        public async Task SendMitri()
        {
            if (!CheckAccess())
            {
                //await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }
            
            await RespondAsync("https://www.kapwing.com/videos/60b93a3ae6672200d5727b63");
        }

        [Command("ping")]
        [Summary("A simple ping command")]
        [Priority(1000)]
        public async Task Ping()
        {
            await RespondAsync($"pong! {Context.User.Mention}");
        }

        [Command("quit", RunMode = RunMode.Async)]
        [Priority(1000)]
        [Hide]
        public async Task ShutDown()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            config.Save();

            await RespondAsync("Shutting down...");
            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.Success);
        }

        [Command("restart", RunMode = RunMode.Async)]
        [Priority(1000)]
        [Hide]
        public async Task Restart()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            config.Save();

            await RespondAsync("Restarting...");
            await File.WriteAllTextAsync("./update", Context.Channel.Id.ToString());

            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.Restart);
        }

        [Command("update", RunMode = RunMode.Async)]
        [Priority(1000)]
        [Hide]
        public async Task UpdateAndRestart()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            config.Save();

            await File.WriteAllTextAsync("./update", Context.Channel.Id.ToString());

            await RespondAsync("Pulling latest updates and restarting, please wait");
            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.RestartAndUpdate);
        }

        [Command("deadlocksim", RunMode = RunMode.Async)]
        [Priority(1000)]
        [Hide]
        public async Task DeadlockSimulation()
        {
            if (!config.OwnerIds.Contains(Context.User.Id))
            {
                await RespondAsync(":no_good::skin-tone-3: You don't have permission to run this command!");
                return;
            }

            config.Save();

            File.Create("./deadlock");

            await RespondAsync("Restarting...");
            await Context.Client.LogoutAsync();
            await Task.Delay(1000);
            Environment.Exit((int)ExitCodes.ExitCode.DeadlockEscape);
        }
    }
}

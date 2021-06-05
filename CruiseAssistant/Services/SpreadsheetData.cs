using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HtmlAgilityPack;
using System.Linq;
using System.Web;
using Discord.WebSocket;
using Discord;
using Discord.Rest;

namespace CruiseAssistant.Services
{
    public class SpreadsheetData
    {
        private Config config;
        private string oldContent;
        private DiscordSocketClient socketClient;

        public async Task Install(IServiceProvider _services)
        {
            config = _services.GetService<Config>();
            socketClient = _services.GetService<DiscordSocketClient>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                await Task.Delay(1000 * 60 * 5); // Wait 5 minutes before checking

                while (true)
                {
                    if (config.UpdateMessageIds.Count() > 0) // Don't run an automated update if it won't go anywhere
                    {
                        await GetPage();
                    }

                    await Task.Delay(1000 * 60 * 60); // Wait an hour before automatically updating again
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task UpdateMessages(Embed embed)
        {
            List<ulong> deadIds = new List<ulong>();

            foreach (var kv in config.UpdateMessageIds)
            {
                var m = await ((SocketTextChannel)socketClient.GetChannel(kv.Key)).GetMessageAsync(kv.Value) as IUserMessage;

                if (m == null)
                {
                    deadIds.Add(kv.Key);
                    continue;
                }

                await m.ModifyAsync(x => x.Embed = embed);
                await Task.Delay(1000);
            }

            foreach (var Id in deadIds)
            {
                Console.WriteLine($"Removing Channel Id {Id}");
                config.UpdateMessageIds.Remove(Id);
            }
        }

        public async Task<Embed> GetPage()
        {
            using (var client = new HttpClient())
            {
                var result = await (client.GetAsync(config.CruiseTallyLink));

                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();

                    var embed = FormatPage(content);

                    if (content != oldContent)
                    {
                        oldContent = content;

                        Task.Run(async () =>
                        {
                           await UpdateMessages(embed);
                        });
                    }

                    return embed;
                }
                else
                    throw new Exception(await result.Content.ReadAsStringAsync());
            }
        }

        public Embed FormatPage(string input)
        {
            List<List<string>> table = new List<List<string>>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(input);

            table = doc.DocumentNode.SelectSingleNode("//table")
                .Descendants("tr")
                .Skip(1)
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td").Skip(1).Select(td => HttpUtility.HtmlDecode(td.InnerText).Trim()).ToList())
                .ToList();

            StringBuilder output = new StringBuilder();

            //output.AppendLine($"{"# of Carriers".PadRight(17)}  {table[0][1].PadLeft(17)}");
            //output.AppendLine($"{"Profit per ton".PadRight(17)}  {table[1][1].PadLeft(17)}");
            //output.AppendLine($"{"Rackham Pop".PadRight(17)}  {table[2][1].PadLeft(17)}");
            //output.AppendLine($"{"Wine per capita".PadRight(17)}  {table[3][1].PadLeft(17)}");
            //output.AppendLine($"{"Wine per carrier".PadRight(17)}  {table[4][1].PadLeft(17)}");
            //output.AppendLine($"{"280t Python Loads".PadRight(17)}  {table[5][1].PadLeft(17)}");
            //output.AppendLine($"{"Wine Total".PadRight(17)}  {table[6][1].PadLeft(17)}");
            //output.AppendLine($"{"Total Profit".PadRight(17)}  {table[7][1].PadLeft(17)}");
            //output.AppendLine(new string('_', 46));
            //output.AppendLine($"{"# of Fleet Carriers that profit can buy"}  {table[8][1]}");

            output.AppendLine($"**{table[0][0]}** — {table[0][1]}");
            output.AppendLine($"**{table[1][0]}** — {table[1][1]}");
            output.AppendLine($"**{table[2][0]}** — {table[2][1]}");
            output.AppendLine($"**{table[3][0]}** — {table[3][1]}");
            output.AppendLine($"**{table[4][0]}** — {table[4][1]}");
            output.AppendLine($"**{table[5][0]}** — {table[5][1]}");
            output.AppendLine();
            output.AppendLine($"**{table[6][0]}** — {table[6][1]}");
            output.AppendLine($"**{table[7][0]}** — {table[7][1]}");
            output.AppendLine();
            output.AppendLine($"**{table[8][0]}** — {table[8][1]}");
            output.AppendLine();
            output.AppendLine("[Bringing wine? Sign up for the cruise here!](https://forms.gle/dWugae3M3i76NNVi7)");

            EmbedBuilder builder = new EmbedBuilder();

            return builder
                .WithTitle("P.T.N. Booze Cruise Tally")
                .WithImageUrl("https://cdn.discordapp.com/attachments/783783142737182724/849157248923992085/unknown.png")
                .WithDescription(output.ToString())
                .WithColor(Discord.Color.DarkMagenta)
                //.WithThumbnailUrl("https://cdn.discordapp.com/attachments/783783142737182724/849153687392878632/pythonwinecropped.png")
                //.WithThumbnailUrl("https://cdn.discordapp.com/attachments/783783142737182724/849153698365702144/haulerwinecropped.png")
                .Build();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using HtmlAgilityPack;
using System.Linq;
using System.Web;

namespace CruiseAssistant.Services
{
    public class SpreadsheetData
    {
        private Config config;

        public async Task Install(IServiceProvider _services)
        {
            config = _services.GetService<Config>();
        }

        public async Task<string> GetPage()
        {
            using (var client = new HttpClient())
            {
                var result = await (client.GetAsync(config.CruiseTallyLink));

                if (result.IsSuccessStatusCode)
                {
                    List<List<string>> table = new List<List<string>>();

                    var content = await result.Content.ReadAsStringAsync();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(content);

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

                    return output.ToString();
                }
                else
                    throw new Exception(await result.Content.ReadAsStringAsync());
            }
        }
    }
}

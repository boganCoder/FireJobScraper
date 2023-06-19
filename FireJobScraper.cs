using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FireJobScraper
{
    public static class FireJobScraper
    {
        [FunctionName("Scraper")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            HttpClient client = new HttpClient();

            string html = await client.GetStringAsync("https://mazzanet.net.au/cfa/pager-cfa-all.php");

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            var output = JsonConvert.SerializeObject(GetJobTableData(document));

            return new OkObjectResult(output);
        }

        static List<JobTable> GetJobTableData(HtmlDocument document)
        {
            var table1 = document.DocumentNode.SelectNodes("//table").First();

            var lst = new List<JobTable>();

            foreach (var row in table1.ChildNodes.Where(r => r.Name == "tr"))
            {
                var tbl1 = new JobTable();
                var columnsArray = row.ChildNodes.Where(c => c.Name == "td").ToArray();
                for (int i = 0; i < columnsArray.Length; i++)
                {
                    if (i == 0)
                        tbl1.CadCode = columnsArray[i].InnerText.Trim();
                    if (i == 1)
                        tbl1.TimeStamp = columnsArray[i].InnerText.Trim();
                    if (i == 2)
                        tbl1.Job = columnsArray[i].InnerText.Trim();
                }
                lst.Add(tbl1);
            }
            return lst;
        }

        public class JobTable
        {
            public string CadCode { get; set; }
            public string TimeStamp { get; set; }
            public string Job { get; set; }
        }
    }
}
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugCrawler
{
    public class IRCDrugReader
    {
        static int end = 46000;
        static string[] headers = {
        "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36"
    };

        private IEnumerable<int> GenerateUrl()
        {
            return Enumerable.Range(1, end);
        }

        static string DataObject(HtmlDocument dom, string xpath)
        {
            var element = dom.DocumentNode.SelectSingleNode(xpath);

            if (element != null)
            {
                return element.InnerText.Replace("\r\n", "").Trim();
            }
            else
            {
                Console.WriteLine("No element found with the specified XPath.");
                return null;
            }
        }

        private Dictionary<string, string> FetchDataFromUrl(int urlId)
        {
            var request = (HttpWebRequest)WebRequest.Create($"https://irc.fda.gov.ir/NFI/Detail/{urlId}");
            request.Headers.Add(headers[0]);
            request.Timeout = 10000;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var doc = new HtmlDocument();
                        doc.Load(response.GetResponseStream());

                        var result = new Dictionary<string, string>
                    {
                        { "id", urlId.ToString() },
                        { "general_name", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[1]/div[2]/bdo") },
                        { "certificate_owner", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[3]/div[1]/span") },
                        { "brand_owner", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[3]/div[2]/span") },
                        { "consumer_price", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[5]/div[1]/span[1]") },
                        { "unit_price", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[5]/div[2]/span[1]") },
                        { "irc", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[6]/div[2]/span") },
                        { "gtin", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[6]/div[1]/span") },
                        { "emergency_licence", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[7]/div[2]/span") },
                        { "license_expire_date", DataObject(doc, "/html/body/div[1]/div[5]/div[1]/div[1]/div/div/div[3]/div[4]/div[2]/span") },
                        { "response_code", ((int)response.StatusCode).ToString() }
                    };

                        return result;
                    }
                    else
                    {
                        var result = new Dictionary<string, string>
                    {
                        { "id", urlId.ToString() },
                        { "general_name", "" },
                        { "certificate_owner", "" },
                        { "brand_owner", "" },
                        { "consumer_price", "" },
                        { "unit_price", "" },
                        { "irc", "" },
                        { "gtin", "" },
                        { "emergency_licence", "" },
                        { "license_expire_date", "" },
                        { "response_code", ((int)response.StatusCode).ToString() }
                    };

                        return result;
                    }
                }
            }
            catch (WebException)
            {
                var result = new Dictionary<string, string>
            {
                { "id", urlId.ToString() },
                { "general_name", "" },
                { "certificate_owner", "" },
                { "brand_owner", "" },
                { "consumer_price", "" },
                { "unit_price", "" },
                { "irc", "" },
                { "gtin", "" },
                { "emergency_licence", "" },
                { "license_expire_date", "" },
                { "response_code", "Timeout" }
            };

                return result;
            }
        }


        private Dictionary<string, string> ReadAllRows()
        {
            var keyValues = new Dictionary<string, string>();
            var drugUrls = GenerateUrl().GetEnumerator();

            while (drugUrls.MoveNext())
            {
                Thread.Sleep(100);
                var row = FetchDataFromUrl(drugUrls.Current);
                keyValues.Intersect(row);
            }
            return keyValues;
        }
    }
}

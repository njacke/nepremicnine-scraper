using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NepremicnineScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            GetHtmlAsyncToCsv();
            Console.ReadLine(); //to avoid program closure
        }


        private static async void GetHtmlAsyncToCsv()
        {
            var dataTable = CreateDataTable();

            var pageUrls = new List<string>();

            var url = "https://www.nepremicnine.net/oglasi-prodaja/ljubljana-mesto/stanovanje/";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var PagesHtml = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("id", "")
                .Equals("pagination")).ToList();

            var totalPages = PagesHtml[0].Descendants("ul").FirstOrDefault().GetAttributeValue("data-pages", "");
            var totalPagesInt = Convert.ToInt32(totalPages);

            for (int i = 1; i <= totalPagesInt; i++)
            {
                var newUrl = "https://www.nepremicnine.net/oglasi-prodaja/ljubljana-mesto/stanovanje/" + i.ToString() + "/";
                pageUrls.Add(newUrl);
            }

            foreach (string pageUrl in pageUrls)
            {
                //var httpClientNew = new HttpClient();
                var htmlNew = await httpClient.GetStringAsync(pageUrl);

                var htmlDocumentNew = new HtmlDocument();
                htmlDocumentNew.LoadHtml(htmlNew);

                var EstatesHtml = htmlDocumentNew.DocumentNode.Descendants("div")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("seznam")).ToList();

                var EstateListItems = EstatesHtml[0].Descendants("div")
                    .Where(node => node.GetAttributeValue("id", "")
                    .Contains("o6")).ToList(); //always contains "o6", ljubljana ID

                Console.WriteLine(EstateListItems.Count());
                Console.WriteLine();

                foreach (var EstateListItem in EstateListItems)
                {
                    //id
                    var estateId =
                        EstateListItem.GetAttributeValue("id", "");

                    //url
                    var estateUrl =
                        EstateListItem.Descendants("a").FirstOrDefault().GetAttributeValue("href", "").Trim('\r', '\n', '\t');
                    estateUrl = "https://www.nepremicnine.net" + estateUrl;

                    //agency
                    var estateAgency =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("agencija")).FirstOrDefault().InnerText;

                    //location
                    var estateLocation =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("title")).FirstOrDefault().InnerText;

                    if (estateLocation.Contains(","))
                    {
                        estateLocation = estateLocation.Remove(estateLocation.IndexOf(","));
                    }

                    //type
                    var estateType =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("vrsta")).FirstOrDefault().InnerText;

                    //rooms
                    var estateRooms =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("tipi")).FirstOrDefault().InnerText;

                    //floor
                    var nodeAtribut =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("atribut")).FirstOrDefault();

                    string estateFloor;
                    if (nodeAtribut != null)
                    {
                        estateFloor =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("atribut")).FirstOrDefault().InnerText;

                        estateFloor = estateFloor.Substring(estateFloor.IndexOf(" ") + 1).Trim(',', ' ');
                    }

                    else
                    {
                        estateFloor = "n/a";
                    }

                    //year
                    var nodeAtributLeto =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("atribut leto")).FirstOrDefault();

                    string estateYear;
                    if (nodeAtributLeto != null)
                    {
                        estateYear =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("atribut leto")).FirstOrDefault().InnerText;

                        estateYear = estateYear.Substring(estateYear.IndexOf(" ") + 1).Trim(',', ' ');
                    }

                    else
                    {
                        estateYear = "n/a";
                    }

                    //size
                    var estateSize =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("velikost")).FirstOrDefault().InnerText;

                    estateSize = estateSize.Substring(0, estateSize.IndexOf(","));

                    //price
                    var estatePrice =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("cena")).FirstOrDefault().InnerText;

                    estatePrice = estatePrice.Substring(0, estatePrice.IndexOf(',')).Replace('.'.ToString(), String.Empty);

                    //add to table
                    dataTable.Rows.Add(new object[] { estateId, estateUrl, estateAgency, estateLocation, estateType, estateRooms, estateFloor, estateYear, estateSize, estatePrice });
                }

            }

            WriteToCsv(dataTable);
            Console.WriteLine("Data table to .csv export completed.");
        }

        private static void WriteToCsv(DataTable dataTable)
        {
            StringBuilder stringBuilder = new StringBuilder();

            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(";", columnNames));

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                stringBuilder.AppendLine(string.Join(";", fields));
            }

            File.WriteAllText("test.csv", stringBuilder.ToString());
        }

        private static DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(string));
            dataTable.Columns.Add("URL", typeof(string));
            dataTable.Columns.Add("Agency", typeof(string));
            dataTable.Columns.Add("Location", typeof(string));
            dataTable.Columns.Add("Type", typeof(string));
            dataTable.Columns.Add("Rooms", typeof(string));
            dataTable.Columns.Add("Floor", typeof(string));
            dataTable.Columns.Add("Year", typeof(string));
            dataTable.Columns.Add("Size [m2]", typeof(string));
            dataTable.Columns.Add("Price", typeof(string));

            return dataTable;
        }
    }
}
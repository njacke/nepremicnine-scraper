﻿using HtmlAgilityPack;
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
            dataTable.Columns.Add("Size", typeof(string));
            dataTable.Columns.Add("Price", typeof(string));

            return dataTable;
        }

        private static async void GetHtmlAsyncToCsv() //method has to be async
        {
            var dataTable = CreateDataTable();

            var url = "https://www.nepremicnine.net/oglasi-prodaja/ljubljana-mesto/stanovanje/1/";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);          

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var EstatesHtml = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("seznam")).ToList();

            var EstateListItems = EstatesHtml[0].Descendants("div")
                .Where(node => node.GetAttributeValue("id", "")
                .Contains("o6")).ToList(); //always contains "o6", ljubljana ID, otherwise i also retrieve some hidden adds that have same structure


            Console.WriteLine(EstateListItems.Count());
            Console.WriteLine();

            foreach (var EstateListItem in EstateListItems) //for each loop to parse data from each item
            {
                //id
                var estateId =
                    EstateListItem.GetAttributeValue("id", ""); //unique identifier

                //url
                var estateUrl =
                    EstateListItem.Descendants("a").FirstOrDefault().GetAttributeValue("href", "").Trim('\r', '\n', '\t');
                estateUrl = "https://www.nepremicnine.net/" + estateUrl;

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

                    estateFloor = estateFloor.Substring(estateFloor.IndexOf(" ") + 1).Trim(',' , ' ');                    
                }

                else
                {
                    estateFloor = "n/a";
                }

                //year
                var estateYear =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("atribut leto")).FirstOrDefault().InnerText;

                estateYear = estateYear.Substring(estateYear.IndexOf(" ") + 1).Trim(',' , ' ');

                //size
                var estateSize =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("velikost")).FirstOrDefault().InnerText;

                estateSize = estateSize.Substring(0, estateSize.IndexOf(" ")).Replace(',', '.');

                //price
                var estatePrice =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("cena")).FirstOrDefault().InnerText;

                estatePrice = estatePrice.Substring(0, estatePrice.IndexOf(',')).Replace('.', ',');

                //add to table
                dataTable.Rows.Add(new object[] { estateId, estateUrl, estateAgency, estateLocation, estateType, estateRooms, estateFloor, estateYear, estateSize, estatePrice });
            }

            WriteToCsv(dataTable);
            Console.WriteLine("Data table to .csv export completed.");
        }

        private static void WriteToCsv(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("test.csv", sb.ToString());
        }
    }
}
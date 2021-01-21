using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
            GetHtmlAsync();
            Console.ReadLine(); //to avoid program closure before we can read
        }

        private static async void GetHtmlAsync() //method has to be async
        {
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

                Console.WriteLine(estateId);

                //url
                var stateUrl =
                    EstateListItem.Descendants("a").FirstOrDefault().GetAttributeValue("href", "").Trim('\r', '\n', '\t');
                stateUrl = "https://www.nepremicnine.net/" + stateUrl;

                Console.WriteLine(stateUrl);

                //location
                var estateLocation =
                EstateListItem.Descendants("span")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("title")).FirstOrDefault().InnerText;


                if (estateLocation.Contains(","))
                {
                    Console.WriteLine(estateLocation.Remove(estateLocation.IndexOf(",")));
                }

                else
                {
                    Console.WriteLine(estateLocation);
                }

                //type
                var estateType =
                EstateListItem.Descendants("span")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("vrsta")).FirstOrDefault().InnerText;

                Console.WriteLine(estateType);

                //rooms
                var estateRooms =
                EstateListItem.Descendants("span")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("tipi")).FirstOrDefault().InnerText;

                Console.WriteLine(estateRooms);

                //floor
                var nodeAtribut =
                EstateListItem.Descendants("span")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("atribut")).FirstOrDefault();

                if (nodeAtribut != null)
                {
                    var estateFloor =
                        EstateListItem.Descendants("span")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("atribut")).FirstOrDefault().InnerText;

                    estateFloor = estateFloor.Substring(estateFloor.IndexOf(" ") + 1).Trim(',' , ' ');
                    
                    Console.WriteLine(estateFloor);
                }

                else
                {
                    Console.WriteLine("n/a");
                }

                //year
                var estateYear =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("atribut leto")).FirstOrDefault().InnerText;

                estateYear = estateYear.Substring(estateYear.IndexOf(" ") + 1).Trim(',' , ' ');
                    
                Console.WriteLine(estateYear);

                //size
                var estateSize =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("velikost")).FirstOrDefault().InnerText;

                estateSize = estateSize.Substring(0, estateSize.IndexOf(" ")).Replace(',', '.');

                Console.WriteLine(estateSize);

                //price
                var estatePrice =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("cena")).FirstOrDefault().InnerText;

                estatePrice = estatePrice.Substring(0, estatePrice.IndexOf(',')).Replace('.', ',');

                Console.WriteLine(estatePrice);

                //agency
                var estateAgency =
                    EstateListItem.Descendants("span")
                    .Where(node => node.GetAttributeValue("class", "")
                    .Equals("agencija")).FirstOrDefault().InnerText;


                Console.WriteLine(estateAgency);

                Console.WriteLine();
            }

            Console.WriteLine();

        }
    }
}
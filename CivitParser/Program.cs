


using civit_parser.library;
using Newtonsoft.Json;
using System;

namespace CivitParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (civit_parser.library.CivitParser parser = new civit_parser.library.CivitParser())
            {
                System.IO.DirectoryInfo savedir = new DirectoryInfo("d:\\onboarding\\img\\forai\\");
                HashSet<Uri> urls = new HashSet<Uri>();
                List<ImageData> data = [];
                WaitForLogon(parser);

                AddNewImagePagesFromUserPages(parser, urls);
                ExtractNewImagePagesFromCollectionPages(parser, urls);
                data.AddRange(ExtractImageData(parser, urls));
                SaveImageData(savedir, data);
            }
            return;
        }

        private static void SaveImageData(DirectoryInfo savedir, IEnumerable<ImageData> data)
        {
            List<Task> tasks = new();
            foreach (ImageData imgdata in data)
            {
                string imgFileName = System.IO.Path.Combine(savedir.FullName, imgdata.ID + ".jpg");
                string jsonFileName = System.IO.Path.Combine(savedir.FullName, imgdata.ID + ".json");
                tasks.Add(Task.Run(() => DownloadImageAsync(imgdata.ImageUrl, imgFileName)));
                WriteObjectToJsonFile(imgdata, jsonFileName);
            }
            Task.WaitAll(tasks.ToArray());
        }

        public static void WriteObjectToJsonFile(object obj, string filePath)
        {
            // Serialize the object to JSON format
            string jsonContent = JsonConvert.SerializeObject(obj, Formatting.Indented);

            // Use a StreamWriter to write the JSON content to the specified file path
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(jsonContent);
            }

            Console.WriteLine("Object successfully written to file: " + filePath);
        }

        public static async Task DownloadImageAsync(Uri imageUrl, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the image URL
                HttpResponseMessage response = await client.GetAsync(imageUrl);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the image content as a byte array
                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                // Save the image to the specified file path
                await File.WriteAllBytesAsync(filePath, imageBytes);
            }
        }

        private static IEnumerable<ImageData> ExtractImageData(civit_parser.library.CivitParser parser, HashSet<Uri> urls)
        {
            List<ImageData> data = new List<ImageData>();
            foreach (Uri uri in urls)
            {
                ImageData imgData = parser.ParseImagePage(uri);
                data.Add(imgData);
            }
            return data;
        }

        private static void ExtractNewImagePagesFromCollectionPages(civit_parser.library.CivitParser parser, HashSet<Uri> urls)
        {
            foreach (Uri page in GetCollectionPages())
            {
                Console.WriteLine("");
                Console.Write(page.ToString());
                foreach (Uri uri in parser.GetImagesFromImageCollectionPage(page))
                {
                    if (!urls.Contains(uri)) urls.Add(uri);
                    Console.Write("*");
                }
            }
        }

        private static void AddNewImagePagesFromUserPages(civit_parser.library.CivitParser parser, HashSet<Uri> urls)
        {
            foreach (Uri page in GetUserPages())
            {
                Console.WriteLine("");
                Console.Write(page.ToString());
                foreach (Uri uri in parser.GetImagesFromUserPage(page))
                {
                    if (!urls.Contains(uri)) urls.Add(uri);
                    Console.Write("*");
                }
            }
        }

        private static void WaitForLogon(civit_parser.library.CivitParser parser)
        {
            for (int i = 0; i < 45; i++)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write("*");
            }
            parser.Reset();
        }

        public static List<Uri> GetUserPages()
        {
            List<Uri> uris = new();

            uris.Add(new Uri("https://civitai.com/user/alyla"));  // small for save testing!!!

            //uris.Add(new Uri("https://civitai.com/user/haors"));
            uris.Add(new Uri("https://civitai.com/user/Nourdal"));
            uris.Add(new Uri("https://civitai.com/user/blacksnowskill"));

            uris.Add(new Uri("https://civitai.com/user/dororooo"));
            uris.Add(new Uri("https://civitai.com/user/Chimi_chan"));
            
            uris.Add(new Uri("https://civitai.com/user/martinffm"));
            
            uris.Add(new Uri("https://civitai.com/user/LadyMystra"));
            uris.Add(new Uri("https://civitai.com/user/Wolfsangel"));
            uris.Add(new Uri("https://civitai.com/user/7whitefire7"));
            uris.Add(new Uri("https://civitai.com/user/solumbragt355"));
            uris.Add(new Uri("https://civitai.com/user/polkaDot"));
            uris.Add(new Uri("https://civitai.com/user/Saitama_4_real/"));
            uris.Add(new Uri("https://civitai.com/user/Bancin/"));
            uris.Add(new Uri("https://civitai.com/user/IwantAichan/"));
            uris.Add(new Uri("https://civitai.com/user/hornystoryteller/"));
            uris.Add(new Uri("https://civitai.com/user/montana_fox/"));
            uris.Add(new Uri("https://civitai.com/user/knod7579"));
            uris.Add(new Uri("https://civitai.com/user/cayden_cailean"));
            uris.Add(new Uri("https://civitai.com/user/JackSimian"));
            uris.Add(new Uri("https://civitai.com/user/kavch"));
            uris.Add(new Uri("https://civitai.com/user/s1medieval"));
            uris.Add(new Uri("https://civitai.com/user/UwuSite"));
            uris.Add(new Uri("https://civitai.com/user/greenbot"));
            uris.Add(new Uri("https://civitai.com/user/bolero537"));
            uris.Add(new Uri("https://civitai.com/user/VelvetS"));
            uris.Add(new Uri("https://civitai.com/user/greenbot1"));
            uris.Add(new Uri("https://civitai.com/user/psoft"));
            uris.Add(new Uri("https://civitai.com/user/Stellaaa"));
            uris.Add(new Uri("https://civitai.com/user/WiseBurrito"));
            uris.Add(new Uri("https://civitai.com/user/c31x5ruq380"));
            
            
            uris.Add(new Uri("https://civitai.com/user/UNDEAD2075"));
            uris.Add(new Uri("https://civitai.com/user/sheevlord"));
            uris.Add(new Uri("https://civitai.com/user/Zanka"));
            uris.Add(new Uri("https://civitai.com/user/rhult"));
            uris.Add(new Uri("https://civitai.com/user/DucHaiten"));
            uris.Add(new Uri("https://civitai.com/user/Xie"));
            uris.Add(new Uri("https://civitai.com/user/VEGETA99788"));
            uris.Add(new Uri("https://civitai.com/user/dapperdan00"));
            uris.Add(new Uri("https://civitai.com/user/Blackskullart"));
            uris.Add(new Uri("https://civitai.com/user/zhuanqianfish248"));
            uris.Add(new Uri("https://civitai.com/user/Orgrik"));
            uris.Add(new Uri("https://civitai.com/user/iamddtla"));
            uris.Add(new Uri("https://civitai.com/user/headupdef"));
            uris.Add(new Uri("https://civitai.com/user/Rem_Red"));
            uris.Add(new Uri("https://civitai.com/user/ClamJam"));
            uris.Add(new Uri("https://civitai.com/user/FitzerX"));
            uris.Add(new Uri("https://civitai.com/user/hornystoryteller"));
            uris.Add(new Uri("https://civitai.com/user/32duba207"));
            uris.Add(new Uri("https://civitai.com/user/abow19149324"));
            uris.Add(new Uri("https://civitai.com/user/LLIATATEJlb"));
            uris.Add(new Uri("https://civitai.com/user/Delnight"));
            uris.Add(new Uri("https://civitai.com/user/Redpioneer"));
            uris.Add(new Uri("https://civitai.com/user/DarkTwistedFantasies"));
            uris.Add(new Uri("https://civitai.com/user/EquivalentMeeting72389"));


            return uris;
        }

        public static List<Uri> GetCollectionPages()
        {
            List<Uri> uris = new();
            //uris.Add(new Uri("https://civitai.com/collections/3382518"));

            return uris;
        }
    }
}
namespace CivitParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (civit_parser.library.CivitParser parser = new civit_parser.library.CivitParser())
            {
                //Console.Write("Please go to the browser and log in.  Then return here and hit <enter> to continue.");
                for (int i = 0; i < 120; i++)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.Write("*");
                }

                foreach(Uri link in parser.GetImagesFromUserPage(new Uri("https://civitai.com/user/DoreenAI")))
                {
                    Console.WriteLine(link);
                }

                Console.Write("*");
                Console.WriteLine("++++++++++++++++++++");
                parser.Reset();

                foreach (Uri link in parser.GetImagesFromImageCollectionPage(new Uri("https://civitai.com/collections/3382518")))
                {
                    Console.WriteLine(link);
                }


                parser.ParseImagePage(new Uri("https://civitai.com/images/25662683"));
                parser.ParseImagePage(new Uri("https://civitai.com/images/25409481"));
                parser.ParseImagePage(new Uri("https://civitai.com/images/25941621"));
            }
            return;
        }
    }
}

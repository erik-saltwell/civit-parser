using CivitParser.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Saltworks.Trace;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CivitParser
{
    internal class ResultSaver
    {
        private static TraceLogger _log = TraceManager.Logger<ResultSaver>();
        public DirectoryInfo SaveDirectory { get; set; } = new DirectoryInfo("c:\\");
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

        internal void SaveImageData(List<ImageData> data, ParseContext original_context, CancellationTokenSource cancel_source)
        {
            AnsiConsole.Markup("[underline blue]Saving processed data.[/]");
            ParseContext ctxt = original_context.UpdateCancelToken(cancel_source);
            using (var scope = ctxt.Container.BeginLifetimeScope())
            {
                AnsiConsole.Progress()
                    .HideCompleted(false)
                    .Start(progress =>
                    {
                        var task = progress.AddTask("[yellow]Extracting ImageData[/]", true, data.Count);

                        List<Task> tasks = new();
                        foreach (ImageData imgdata in data)
                        {
                            string imgFileName = System.IO.Path.Combine(SaveDirectory.FullName, imgdata.ID + ".jpg");
                            string jsonFileName = System.IO.Path.Combine(SaveDirectory.FullName, imgdata.ID + ".json");
                            tasks.Add(Task.Run(() => DownloadImageAsync(imgdata.ImageUrl, imgFileName)));
                            WriteObjectToJsonFile(imgdata, jsonFileName);
                        }
                        Task.WaitAll(tasks.ToArray());
                    });
            }
        }

        internal static void WriteObjectToJsonFile(object obj, string filePath)
        {
            // Serialize the object to JSON format
            string jsonContent = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

            // Use a StreamWriter to write the JSON content to the specified file path
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(jsonContent);
            }

            Console.WriteLine("Object successfully written to file: " + filePath);
        }
    }
}
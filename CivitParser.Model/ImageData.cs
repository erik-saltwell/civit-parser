using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitParser.Model
{
    public enum ResourceType { lora, checkpoint, embedding, other };

    public record UsedResource
    {
        public static Uri EmptyUri = new Uri("http://www.google.com");

        public string Name { get; set; } = string.Empty;
        public string SubName { get; set; } = string.Empty;
        public ResourceType ResourceType { get; set; } = ResourceType.lora;
        public string Strength { get; set; } = string.Empty;
        public Uri ResourceURL { get; set; } = EmptyUri;
    }

    public record OtherMetaData
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public record ImageData
    {
        public static Uri EmptyUri = new Uri("http://www.google.com");

        public String ID { get; set; } = string.Empty;
        public Uri AuthorUri { get; set; } = EmptyUri;
        public Uri InfoUrl { get; set; } = EmptyUri;
        public Uri ImageUrl { get; set; } = EmptyUri;
        public string PositivePrompt { get; set; } = string.Empty;
        public string NegativePrompt { get; set; } = string.Empty;
        public UsedResource[] UsedResources { get; set; } = [];
        public OtherMetaData[] OtherMetaDatas { get; set; } = [];

    }
}

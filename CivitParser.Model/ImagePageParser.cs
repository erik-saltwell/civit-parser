using OpenQA.Selenium;
using Saltworks.Trace;
using Serilog;
using System.Diagnostics;

namespace CivitParser.Model
{
    public class ImagePageParser : BaseCivitPageParser
    {
        private static TraceLogger _log = TraceManager.Logger<ImagePageParser>();
        public ImageData Parse(Uri informationPageUrl, ParseContext ctxt)
        {
            _log.Debug("In ImagePageParser.Parse for {informationPageUrl}", informationPageUrl);
            c(ctxt);

            BrowseTo(informationPageUrl, 1, ctxt);
            c(ctxt);

            ExpandShowMores(ctxt);
            c(ctxt);

            List<UsedResource> resources = [];
            _log.Debug("Extracting Resources.");
            foreach (IWebElement resource in GetResources(ctxt))
            {
                resources.Add(ParseResource(resource));
                c(ctxt);
            }

            List<OtherMetaData> otherMetaData = ParseOtherMetaData(ctxt);
            c(ctxt);

            (string positive_prompt, string negative_prompt) = GetPrompts(ctxt);
            c(ctxt);

            string id = GetIDFromURL(informationPageUrl);
            c(ctxt);

            Uri imgUri = GetImageUri(ctxt);
            c(ctxt);
            Uri authorUri = GetAuthorUri(ctxt);
            c(ctxt);

            return new ImageData() { UsedResources = resources.ToArray(), NegativePrompt = negative_prompt, PositivePrompt = positive_prompt, OtherMetaDatas = otherMetaData.ToArray(), InfoUrl = informationPageUrl, AuthorUri = authorUri, ImageUrl = imgUri, ID = id };
        }

        private void ExpandShowMores(ParseContext ctxt)
        {
            _log.Debug("In ExpandShowMores");
            foreach (IWebElement elem in GetShowMores(ctxt))
            {
                elem.Click();
                c(ctxt);
            }
        }

        private Uri GetAuthorUri(ParseContext ctxt)
        {
            _log.Debug("In GetAuthorUri");
            string xpath = "//main/div[1]/div[2]//a[starts-with(@href, '/user/')]";
            IWebElement imgElement = ctxt.Driver.FindElements(By.XPath(xpath)).First();
            string uriTxt = imgElement.GetAttribute("href");
            return new Uri(uriTxt);
        }

        private string GetIDFromURL(Uri informationPageUrl)
        {
            _log.Debug("In GetIDFromUrl for {informationPageUrl}", informationPageUrl);
            string retVal = Path.GetFileName(informationPageUrl.AbsolutePath);
            return retVal;
        }

        private Uri GetImageUri(ParseContext ctxt)
        {
            _log.Debug("In GetImageUri");
            int count = ctxt.Driver.FindElements(By.XPath("//main/div[1]/div[1]//img")).Count;
            Debug.Assert(count == 1);
            IWebElement imgElement = ctxt.Driver.FindElements(By.XPath("//main/div[1]/div[1]//img")).First();
            string uriTxt = imgElement.GetAttribute("src");
            return new Uri(uriTxt);
        }
        private string GetNegativePrompt(IWebElement elem)
        {
            IWebElement textElem = elem.FindElement(By.XPath("div/following-sibling::div[1]"));
            string retVal = textElem.Text;
            return retVal;
        }

        private string GetPositivePrompt(IWebElement elem)
        {
            IWebElement textElem = elem.FindElement(By.XPath("following-sibling::div[1]"));
            string retVal = textElem.Text;
            return retVal;
        }

        private (string positive_prompt, string negative_prompt) GetPrompts(ParseContext ctxt)
        {
            _log.Debug("In GetPrompts");
            string positive = string.Empty;
            string negative = string.Empty;
            foreach (IWebElement elem in ctxt.Driver.FindElements(By.XPath("//*[text()='Prompt' or  text()='Negative prompt'][1]/parent::*/parent::*")))
            {
                if (string.IsNullOrEmpty(positive))
                    positive = GetPositivePrompt(elem);
                else
                    negative = GetNegativePrompt(elem);
            }

            return (positive, negative);
        }

        private IEnumerable<IWebElement> GetResources(ParseContext ctxt)
        {
            string resourcesXPath = "//*[normalize-space()='Resources used']//../following::ul//li";
            return ctxt.Driver.FindElements(By.XPath(resourcesXPath));
        }

        private ResourceType GetResourceType(string resourceText)
        {
            string type = resourceText.ToLowerInvariant();
            if (type.StartsWith("checkp")) return ResourceType.checkpoint;
            if (type.StartsWith("lora")) return ResourceType.lora;
            if (type.StartsWith("embed")) return ResourceType.embedding;

            return ResourceType.other;
        }

        private IEnumerable<IWebElement> GetShowMores(ParseContext ctxt)
        {
            string xpath = "//*[starts-with(text(), \"Show\")]";
            foreach (IWebElement elem in ctxt.Driver.FindElements(By.XPath(xpath)))
            {
                yield return elem;
            }
        }

        private (string name, string val) ParseMetaDataLine(IWebElement elem)
        {
            string name = elem.FindElement(By.XPath("div[1]")).Text;
            string val = elem.FindElement(By.XPath("div[2]")).Text;
            return (name, val);
        }

        private List<OtherMetaData> ParseOtherMetaData(ParseContext ctxt)
        {
            _log.Debug("In ParseOtherMetaData");
            List<OtherMetaData> retVal = [];
            foreach (IWebElement elem in ctxt.Driver.FindElements(By.XPath("//div[contains(text(), 'Other metadata')][1]/parent::*/following-sibling::div/div")))
            {
                c(ctxt);
                _ = elem.Text;
                (string name, string val) = ParseMetaDataLine(elem);
                retVal.Add(new OtherMetaData() { Name = name, Value = val });
            }
            return retVal;
        }
        private UsedResource ParseResource(IWebElement elem)
        {
            _log.Debug("In ParseResource");
            _ = new UsedResource();

            IWebElement linkEelem = elem.FindElement(By.XPath("div/a"));
            IWebElement sublink = elem.FindElement(By.XPath("a"));
            IWebElement resourceElem = elem.FindElement(By.XPath("div/div"));

            string linkTxt = linkEelem.Text;
            string linkRef = linkEelem.GetAttribute("href");
            string subTxt = sublink.Text;
            (string resourceTypeTxt, string strengthTxt) = ParseResourceTypeAndStrength(resourceElem);
            ResourceType type = GetResourceType(resourceTypeTxt);

            return new UsedResource() { Name = linkTxt, ResourceURL = new Uri(linkRef), SubName = subTxt, Strength = strengthTxt, ResourceType = type };
        }

        private (string, string) ParseResourceTypeAndStrength(IWebElement elem)
        {
            List<IWebElement> children = elem.FindElements(By.XPath("div")).ToList();
            if (children.Count == 0) return (string.Empty, string.Empty);
            if (children.Count == 1) return (children[0].Text, "1.0");
            return (children[0].Text, children[1].Text);
        }
    }
}
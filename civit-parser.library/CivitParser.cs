using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenQA.Selenium.Interactions;
using System.Reflection.PortableExecutable;

namespace civit_parser.library
{
    public class CivitParser : IDisposable
    {
        private bool _disposedValue;

        public CivitParser() {
            Driver = new ChromeDriver(new ChromeOptions() { BinaryLocation = "d:\\dev\\tools\\chrome\\chrome.exe" });
            Reset();
        }

        public void Reset()
        {
            BrowseTo(new Uri("https://civitai.com"), 1);
        }

        private IWebDriver Driver { get; set; }

        public ImageData ParseImagePage(Uri informationPageUrl)
        {
            BrowseTo(informationPageUrl, 1);
            ExpandShowMores();
            List<UsedResource> resources = new List<UsedResource>();
            foreach (IWebElement resource in GetResources(Driver))
            {
                resources.Add(ParseResource(resource));
            }

            List<OtherMetaData> otherMetaData = ParseOtherMetaData();

            (string positive_prompt, string negative_prompt) = GetPrompts();

            string id = GetIDFromURL(informationPageUrl);
            Uri imgUri = GetImageUri();
            Uri authorUri = GetAuthorUri();

            return new ImageData() { UsedResources = resources.ToArray(), NegativePrompt=negative_prompt, PositivePrompt=positive_prompt, OtherMetaDatas=otherMetaData.ToArray(), InfoUrl=informationPageUrl, AuthorUri=authorUri, ImageUrl=imgUri, ID=id };

        }

        public IEnumerable<Uri> GetImagesFromUserPage(Uri authorPage)
        {
            string classname = "mantine-deph6u";
            string cssSelector = "img."+classname;
            string xpathSelector = "//a[descendant::img[contains(@class, '"+classname+"')]]";

            BrowseTo(authorPage, 3.0); 
            IWebElement viewAll = Driver.FindElement(By.XPath("//a[contains(text(), 'View all images') or .//*[contains(text(), 'View all images')]]"));
            string href = viewAll.GetAttribute("href");
            foreach (Uri u in GetImagesFromImageCollectionPage(new Uri(href), cssSelector, xpathSelector))
            {
                yield return u; 
            }
        }

        public IEnumerable<Uri> GetImagesFromImageCollectionPage(Uri uri, string cssSelector = "img.mantine-deph6u", string xpathSelector= "//a[descendant::img[contains(@class, 'mantine-deph6u')]]")
        {
            BrowseTo(uri, 1.0);
            ForceLoadAllImages(cssSelector);
            foreach (Uri u in GetAllImages(xpathSelector))
            {
                yield return u;
            }
        }

        private void ForceLoadAllImages(string cssSelector)
        {
            
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("document.body.style.zoom = '.1%'");
            int old_count = -1;
            int new_count = Driver.FindElements(By.CssSelector(cssSelector)).Count; 
            while(new_count != old_count)
            {
                System.Threading.Thread.Sleep(10000);
                old_count = new_count;
                new_count = Driver.FindElements(By.CssSelector(cssSelector)).Count;
            }
            return;

        }

        private IEnumerable<Uri> GetAllImages(string xpathSelector)
        {
           foreach (IWebElement elem in Driver.FindElements(By.XPath(xpathSelector)))
           {
                string txtUri = elem.GetAttribute("href");
                if (txtUri.StartsWith("https://civitai.com/images/")) yield return new Uri(txtUri);
           }
        }

        private Uri GetAuthorUri()
        {
            string xpath = "//main/div[1]/div[2]//a[starts-with(@href, '/user/')]";
            IWebElement imgElement = Driver.FindElements(By.XPath(xpath)).First();
            string uriTxt = imgElement.GetAttribute("href");
            return  new Uri(uriTxt);
        }

        private Uri GetImageUri()
        {
            int count = Driver.FindElements(By.XPath("//main/div[1]/div[1]//img")).Count;
            Debug.Assert(count == 1);
            IWebElement imgElement = Driver.FindElements(By.XPath("//main/div[1]/div[1]//img")).First();
            string uriTxt =  imgElement.GetAttribute("src");
            return new Uri(uriTxt);

        }

        private string GetIDFromURL(Uri informationPageUrl)
        {
            string retVal = Path.GetFileName(informationPageUrl.AbsolutePath);
            return retVal;
        }

        private List<OtherMetaData> ParseOtherMetaData()
        {
            List<OtherMetaData> retVal = new();
            foreach(IWebElement elem in Driver.FindElements(By.XPath("//div[contains(text(), 'Other metadata')][1]/parent::*/following-sibling::div/div"))){
                string test = elem.Text;
                (string name, string val) = ParseMetaDataLine(elem);
                retVal.Add(new OtherMetaData() { Name = name, Value = val });
            }
            return retVal;

        }

        private (string name, string val) ParseMetaDataLine(IWebElement elem)
        {
            string name = elem.FindElement(By.XPath("div[1]")).Text;
            string val = elem.FindElement(By.XPath("div[2]")).Text;
            return (name, val);

        }

        private (string positive_prompt, string negative_prompt) GetPrompts()
        {
            string positive = string.Empty;
            string negative = string.Empty; 
            foreach(IWebElement elem in  Driver.FindElements(By.XPath("//*[text()='Prompt' or  text()='Negative prompt'][1]/parent::*/parent::*")))
            {
                if (string.IsNullOrEmpty(positive))
                    positive = GetPositivePrompt(elem);
                else
                    negative = GetNegativePrompt(elem);
            }

            return (positive,negative);
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


        private UsedResource ParseResource(IWebElement elem)
        {
            UsedResource resource  = new UsedResource();

            IWebElement linkEelem = elem.FindElement(By.XPath("div/a"));
            IWebElement sublink = elem.FindElement(By.XPath("a"));
            IWebElement resourceElem = elem.FindElement(By.XPath("div/div"));

            string linkTxt = linkEelem.Text;
            string linkRef = linkEelem.GetAttribute("href");
            string subTxt = sublink.Text;
            (string resourceTypeTxt, string strengthTxt) = ParseResourceTypeAndStringth(resourceElem);
            ResourceType type = GetResourceType(resourceTypeTxt);

            return new UsedResource() { Name=linkTxt, ResourceURL=new Uri(linkRef), SubName=subTxt, Strength= strengthTxt, ResourceType=type };
        }

        private (string, string) ParseResourceTypeAndStringth(IWebElement elem)
        {
            List<IWebElement> children = elem.FindElements(By.XPath("div")).ToList();
            if(children.Count == 0) return (string.Empty, string.Empty);
            if(children.Count == 1) return (children[0].Text, "1.0");
            return (children[0].Text, children[1].Text);

        }

        private ResourceType GetResourceType(string resourceText)
        {
            string type = resourceText.ToLowerInvariant();
            if (type.StartsWith("checkp")) return ResourceType.checkpoint;
            if (type.StartsWith("lora")) return ResourceType.lora;
            if (type.StartsWith("embed")) return ResourceType.embedding;

            return ResourceType.other;
        }

        private IEnumerable<IWebElement> GetResources(IWebDriver driver)
        {

            //get Resources element
            string resourcesXPath = "//*[normalize-space()='Resources used']//../following::ul//li";// \\\\..\\\\following::ul\\\\li\\\\div\\\\\a\")]";
            return driver.FindElements(By.XPath(resourcesXPath));

        }

        private void ExpandShowMores()
        {
            foreach (IWebElement elem in GetShowMores())
            {
                elem.Click();
            }
        }

        private void BrowseTo(Uri location, double delay_multiplier)
        {
            Driver.Navigate().GoToUrl(location.ToString());
            System.Threading.Thread.Sleep( (int)(CivitParserSettings.DefaultPageDelay * delay_multiplier));
        }

        private IEnumerable<IWebElement> GetShowMores()
        {
            string xpath = "//*[starts-with(text(), \"Show\")]";
            foreach (IWebElement elem in Driver.FindElements(By.XPath(xpath)))
            {
                yield return elem;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Driver.Quit();
                    Driver.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ImageDataParser()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}


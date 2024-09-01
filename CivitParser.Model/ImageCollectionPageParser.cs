using OpenQA.Selenium;
using Saltworks.Trace;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitParser.Model
{
    public class ImageCollectionPageParser : BaseCivitPageParser
    {
        private static TraceLogger _log = TraceManager.Logger<ImageCollectionPageParser>();

        public IEnumerable<Uri> ParseFromCollectionUrl(Uri page, ParseContext ctxt)
        {
            _log.Information("In ParseFromCollectionUrl for {page}", page);
            return ParseImagesInternal(page, false, ctxt);
        }

        public IEnumerable<Uri> ParseFromUserUrl(Uri page, ParseContext ctxt)
        {
            _log.Information("In ParseFromUserUrl for {page}", page);
            return ParseImagesInternal(page, true, ctxt);
        }


        private IEnumerable<Uri> ExtractImagesFromPageWithSortOrder(Uri uri, ParseContext ctxt)
        {
            _log.Information("In ExtractImagesFromPageWithSortOrder for {uri}", uri);

            _log.Debug("browsing to page {uri}", uri);
            BrowseTo(uri, 1.0, ctxt);
            c(ctxt);

            PreparePageForImageExtraction(ctxt);
            UpdateProgress(ctxt, 12.5);
            foreach (Uri u in GetAllImages(ctxt))
            {
                c(ctxt);
                _log.Trace("yielding image");
                yield return u;
            }
        }

        private IEnumerable<Uri> GetAllImages(ParseContext ctxt)
        {
            _log.Trace("In GetAllImages");
            string xpathSelector = "//a[descendant::img[contains(@class, 'mantine-deph6u')]]";
            _log.Trace("Calling FindElementsByXPath");
            List<IWebElement> images = ctxt.Driver.FindElements(By.XPath(xpathSelector)).ToList();
            double per_image_progress_amount = 12.5 / (double)images.Count;
            List<Uri> uris = new List<Uri>();
            _log.Trace("enumerating found elements");
            foreach (IWebElement elem in images)
            {
                string txtUri = GetAttribute(elem,"href");
                if (txtUri.StartsWith("https://civitai.com/images/"))
                    uris.Add(new Uri(txtUri));
                UpdateProgress(ctxt, per_image_progress_amount);
            }
            foreach (Uri uri in uris)
                yield return uri;
        }

        private IEnumerable<Uri> GetImagePageVariations(Uri page, bool isUserPage)
        {
            _log.Trace("In GetImagePageVariations: " + page.ToString());
            UriBuilder builder = new UriBuilder(page);
            if (isUserPage)
            {
                _log.Trace("Building images url from usesr url");
                string path = builder.Path;
                if (!path.EndsWith("/")) path = path + "/";
                path = path + "images";
                builder.Path = path;
            }
            string[] sort_query_strings = { "?sort=Newest", "?sort=Oldest", "?sort=Most+Reactions", "?sort=Most+Comments", "?sort=Most+Collected" };
            _log.Trace("Enumerating page variations");
            foreach (string sort in sort_query_strings)
            {
                UriBuilder new_builder = new UriBuilder(builder.Uri);
                new_builder.Query = sort;
                yield return new_builder.Uri;
            }
        }

        private string GetLastImageSrc(ParseContext ctxt)
        {
            _log.Trace("In GetLastImageSrc");
            string cssSelector = "img.mantine-deph6u:last-of-type";
            string retVal = string.Empty;
            _log.Trace("Getting last img");
            IWebElement elem = ctxt.Driver.FindElement(By.CssSelector(cssSelector));
            retVal = GetAttribute(elem, "src");
            _log.Trace("Out GetLastImgSrc");
            return retVal;
        }

        private IEnumerable<Uri> ParseImagesInternal(Uri page, bool isUserPage, ParseContext ctxt)
        {
            _log.Trace("ParseImagePageInternal: " + page.ToString());
            HashSet<Uri> extracted_links = new HashSet<Uri>();
            foreach (Uri uri in GetImagePageVariations(page, isUserPage))
            {
                _log.Trace("Processing variant: " + uri.ToString());
                
                int old_count = extracted_links.Count;
                c(ctxt);
                foreach (Uri link in ExtractImagesFromPageWithSortOrder(uri, ctxt))
                {
                    _log.Trace("extracting image link: " + link.ToString());
                    c(ctxt);
                    if(!extracted_links.Contains(link)) 
                        extracted_links.Add(link);
                }
                if (old_count == extracted_links.Count || extracted_links.Count<40)
                    break;
            }
            foreach(Uri link in extracted_links)
            {
                yield return link;  
            }
        }
        private void PreparePageForImageExtraction(ParseContext ctxt)
        {
            _log.Debug("In PreparePageForImageExtraction");

            IJavaScriptExecutor js = (IJavaScriptExecutor)ctxt.Driver;
            js.ExecuteScript("document.body.style.zoom = '" + ctxt.Settings.ImageCollectionZoom.ToString() + "%'");
            c(ctxt);
            string old_href;
            _log.Trace("Sleeping so first element can be generated");
            System.Threading.Thread.Sleep(2000);
            old_href = GetLastImageSrc(ctxt);
            c(ctxt);
            string new_href = string.Empty;

            while (old_href != new_href)
            {
                old_href = new_href;
                for (int i = 0; i < 25; i++)
                {
                    c(ctxt);
                    System.Threading.Thread.Sleep(400);
                }
                _log.Trace("getting last image to compare to prior");
                new_href = GetLastImageSrc(ctxt);
            }
            _log.Trace("Out PreparePageForImageExtraction");
            return;
        }

        private static string GetAttribute(IWebElement element, string attributeName)
        {
            _log.Trace("In GetAttribute:" + attributeName);
            string retVal = element.GetAttribute(attributeName);
            _log.Trace("Out GetAttribute");
            return retVal;
        }
    }
}
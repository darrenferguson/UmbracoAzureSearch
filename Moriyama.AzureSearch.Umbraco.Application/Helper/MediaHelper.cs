using System;
using System.IO;
using System.Reflection;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Moriyama.AzureSearch.Umbraco.Application.Helper
{
    public static class MediaHelper
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static byte[] Read(IMedia med)
        {
            if (med != null)
            {
                var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                var imagePath = med.Properties["umbracoFile"].Value.ToString();
                var fullOrgPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(imagePath));
                if (!mediaFileSystem.FileExists(fullOrgPath))
                {
                    return null;
                }
                byte[] res = null;

                using (var fileStream = mediaFileSystem.OpenFile(fullOrgPath))
                {
                    if (fileStream.CanSeek) fileStream.Seek(0, 0);
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        res = ms.ToArray();
                    }

                }
                return res;

            }
            return null;
        }

        public static string GetPath(IMedia med)
        {
            var mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var filePath = med.Properties["umbracoFile"].Value.ToString();

            if (IsValidJson(filePath))
            {
                var data = (JObject)JsonConvert.DeserializeObject(filePath);
                filePath = data.Value<string>("src");
            }
        
            var fullOrgPath = mediaFileSystem.GetFullPath(mediaFileSystem.GetRelativePath(filePath));

            return !mediaFileSystem.FileExists(fullOrgPath) ? null : fullOrgPath;
        }


        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }

            return false;
        }

        public static string GetTextContent(IMedia content)
        {
            return IndexPdf(content);

            //TODO: other methods here for Office file types.

        }


        public static string IndexPdf(IMedia content)
        {
            if (!content.Properties.Contains("umbracoExtension") || content.Properties["umbracoExtension"].Value == null)
            {
                return null;                
            }

            var umbracoExtension = content.Properties["umbracoExtension"].Value.ToString();

            if (umbracoExtension != "pdf")
            {
                return null;
            }

            var filePath = GetPath(content);
            string extractedText;

            if (Uri.TryCreate(filePath, UriKind.Absolute, out var outUri)
                && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            {
                extractedText = ReadPdfFile(outUri);
            }
            else
            {
                extractedText = ReadPdfFile(filePath);
            }

            if (content.Properties.Contains("textContents"))
            {
                var prop = content.Properties["textContents"];
                prop.Value = extractedText;
            }
            else
            {
                Log.Error($"textContents field for holding PDF text contents not configured in document type {content.ContentType.Name} ");   
            }

            return extractedText;
        }

        private static string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (System.IO.File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }

        private static string ReadPdfFile(Uri url)
        {
            StringBuilder text = new StringBuilder();

            PdfReader pdfReader = new PdfReader(url);

            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                text.Append(currentText);
            }
            pdfReader.Close();
            
            return text.ToString();
        }


    }
}

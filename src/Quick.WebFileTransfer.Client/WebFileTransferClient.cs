using Quick.WebFileTransfer.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Quick.WebFileTransfer.Client
{
    public class WebFileTransferClient
    {
        private string apiUrl;
        private string token;
        public WebFileTransferClient(string apiUrl, string token)
        {
            this.apiUrl = apiUrl;
            this.token = token;
        }

        public void Download(string remoteFolder, string remoteFile, string localFolder)
        {
            HttpClient client = new HttpClient();
            var url = $"{apiUrl}/Download?folder={HttpUtility.UrlEncode(remoteFolder)}&file={HttpUtility.UrlEncode(remoteFile)}";
            using (var fileStream = client.GetStreamAsync(url).Result)
            using (var cryptoStream = new CryptoReadStream(fileStream, token))
            using (var zipArchive = new ZipArchive(cryptoStream, ZipArchiveMode.Read))
                zipArchive.ExtractToDirectory(localFolder, true);
        }

        public void Upload(string remoteFolder, string localFolder, string localFile)
        {
            var di = new DirectoryInfo(localFolder);
            if (!di.Exists)
                throw new DirectoryNotFoundException($"Directory [{localFolder}] not found.");

            var ms = new MemoryStream();
            using (var cryptoStream = new CryptoWriteStream(ms, token))
            using (var zipArchive = new ZipArchive(cryptoStream, ZipArchiveMode.Create, true))
            {
                if (string.IsNullOrEmpty(localFile))
                {
                    foreach (var fi in di.GetFiles("*", SearchOption.AllDirectories))
                    {
                        var entryName = fi.FullName.Substring(di.FullName.Length + 1);
                        entryName = entryName.Replace("\\", "/");
                        zipArchive.CreateEntryFromFile(fi.FullName, entryName);
                    }
                }
                else
                {
                    foreach (var fi in di.GetFiles(localFile))
                        zipArchive.CreateEntryFromFile(fi.FullName, fi.Name);
                }
            }
            ms.Position = 0;

            HttpClient client = new HttpClient();
            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StreamContent(ms), "file","tmpFile" }
            };

            var url = $"{apiUrl}/Upload?folder={HttpUtility.UrlEncode(remoteFolder)}";
            var rep = client.PostAsync(url, content).Result;
            ms.Dispose();
            if (rep.IsSuccessStatusCode)
                return;
            throw new IOException($"StatusCode: {rep.StatusCode}, Content: {rep.Content.ReadAsStringAsync().Result}");
        }
    }
}

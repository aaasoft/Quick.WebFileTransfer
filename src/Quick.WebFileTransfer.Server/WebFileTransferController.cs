using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quick.WebFileTransfer.Core;
using System.IO.Compression;

namespace Quick.WebFileTransfer.Server
{
    [ApiController]
    [Route("Quick.WebFileTransfer")]
    public class WebFileTransferController : ControllerBase
    {
        private static string baseFolder;
        private static string token;
        public static void Init(string baseFolder, string token)
        {
            WebFileTransferController.baseFolder = baseFolder;
            WebFileTransferController.token = token;
        }

        [HttpGet(nameof(Download))]
        public ActionResult Download(string folder, string file)
        {
            if (string.IsNullOrEmpty(baseFolder)
                || string.IsNullOrEmpty(token))
                throw new ArgumentException($"Must call Init method first.");

            var fullFolder = baseFolder;
            if (!string.IsNullOrEmpty(folder))
                fullFolder = Path.Combine(fullFolder, folder);
            var di = new DirectoryInfo(fullFolder);
            if (!di.Exists)
                return NotFound();
            var ms = new MemoryStream();
            using (var cryptoStream = new CryptoWriteStream(ms, token))
            using (var zipArchive = new ZipArchive(cryptoStream, ZipArchiveMode.Create, true))
            {
                if (string.IsNullOrEmpty(file))
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
                    foreach (var fi in di.GetFiles(file))
                        zipArchive.CreateEntryFromFile(fi.FullName, fi.Name);
                }
            }
            ms.Position = 0;
            return File(ms, "application/octet-stream");
        }

        [HttpPost(nameof(Upload))]
        public ActionResult Upload(string folder, IFormFile file)
        {
            if (string.IsNullOrEmpty(baseFolder)
                || string.IsNullOrEmpty(token))
                throw new ArgumentException($"Must call Init method first.");

            var fullFolder = baseFolder;
            if (!string.IsNullOrEmpty(folder))
                fullFolder = Path.Combine(fullFolder, folder);
            var di = new DirectoryInfo(fullFolder);
            if (!di.Exists)
                return NotFound();

            using (var fileStream = file.OpenReadStream())
            using (var cryptoStream = new CryptoReadStream(fileStream, token))
            using (var zipArchive = new ZipArchive(cryptoStream, ZipArchiveMode.Read))
                zipArchive.ExtractToDirectory(di.FullName, true);

            return Ok();
        }
    }
}

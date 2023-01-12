using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quick.WebFileTransfer.AutoDownloader
{
    public class ConfigModel
    {
        public string ApiUrl { get; set; }
        public string ApiToken { get; set; }
        public string RemoteFolder { get; set; }
        public string RemoteFile{ get; set; }
        public string LocalFolder { get; set; }
        public string DownloadCrontab { get; set; }
    }
}

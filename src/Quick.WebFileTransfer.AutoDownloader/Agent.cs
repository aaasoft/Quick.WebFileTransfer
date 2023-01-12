using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;

namespace Quick.WebFileTransfer.AutoDownloader
{
    public class Agent : AbstractAgent
    {
        public override void Init(ContainerInfo containerInfo)
        {
            base.Init(containerInfo);
            var imageFolder = ImagePathUtils.GetImageFolder(containerInfo.ImageId);
            var containerFolder = ContainerPathUtils.GetContainerFolder(containerInfo.Id);

            AddFunction(new YiQiDong.Core.Functions.AppSettingsConfig(imageFolder, containerFolder, () => this.ContainerInfo.AutoStart));
        }

        public override void Start()
        {
            DownloadManager.Instance.Start();
        }

        public override void Stop()
        {
            DownloadManager.Instance.Stop();
        }
    }
}

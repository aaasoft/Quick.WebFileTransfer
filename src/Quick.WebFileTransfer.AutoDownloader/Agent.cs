using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;

namespace Quick.WebFileTransfer.AutoDownloader
{
    public class Agent : AbstractAgent
    {
        public override void Init()
        {
            if (AgentContext.IsContainerRuning)
            {
                var imageFolder = AgentContext.Container.ImageFolder;
                var containerFolder = AgentContext.Container.ContainerFolder;
                AddFunction(new YiQiDong.Core.Functions.AppSettingsConfig(imageFolder, containerFolder, () => AgentContext.Container.AutoStart));
            }
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

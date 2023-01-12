using Quick.WebFileTransfer.AutoDownloader;

using YiQiDong.Agent;

//初始化容器信息
AgentContext.Instance.InitContainerInfo(args);
//初始化Agent实例
AgentContext.Instance.InitAgentInstance(new Agent());
//开始运行
AgentContext.Instance.Run();
//销毁
AgentContext.Instance.Dispose();
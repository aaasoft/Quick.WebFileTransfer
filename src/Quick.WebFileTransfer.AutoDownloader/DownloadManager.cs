using Quick.WebFileTransfer.Client;

namespace Quick.WebFileTransfer.AutoDownloader
{
    public class DownloadManager
    {
        public static DownloadManager Instance { get; private set; } = new DownloadManager();
        private ConfigModel configModel;
        private CancellationTokenSource cts;
        private NCrontab.CrontabSchedule schedule;

        public void Start()
        {
            cts = new CancellationTokenSource();
            var appSettingModel = Quick.Fields.AppSettings.Model.Load();
            configModel = appSettingModel.Convert<ConfigModel>();
            schedule = NCrontab.CrontabSchedule.Parse(configModel.DownloadCrontab);
            run(cts.Token);
        }

        public void Stop()
        {
            cts?.Cancel();
            cts = null;
        }

        private void run(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            if (schedule == null)
                return;
            var nextRunTime = schedule.GetNextOccurrence(DateTime.Now);
            Console.WriteLine($"下次下载时间[{nextRunTime}]...");
            TaskUtils.RunAtTime(nextRunTime, () =>
             {
                 try
                 {
                     if (configModel == null)
                         throw new ApplicationException("configModel is null.");

                     Console.WriteLine($"开始下载...");
                     var client = new WebFileTransferClient(configModel.ApiUrl, configModel.ApiToken);
                     client.Logger = Console.WriteLine;
                     client.Download(configModel.RemoteFolder, configModel.RemoteFile, configModel.LocalFolder);
                     Console.WriteLine("下载完成！");
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.ToString());
                 }
                 finally
                 {
                     run(token);
                 }
             }, token);
        }
    }
}

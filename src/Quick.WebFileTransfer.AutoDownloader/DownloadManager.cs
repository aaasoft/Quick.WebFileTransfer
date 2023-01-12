using Quick.WebFileTransfer.Client;
using System.Diagnostics;
using System.Runtime.InteropServices;
using YiQiDong.Core.Utils;

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

        public void ExecuteScripts(string[] lines)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            List<string> prepareArgumentList = new List<string>();
            string tmpFile = null;
            //如果是在Windows平台
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                psi.FileName = "cmd";
                prepareArgumentList.Add("/c");
            }
            else
            {
                psi.FileName = "sh";
                tmpFile = Path.GetTempFileName();
            }


            foreach (var line in lines)
            {
                psi.ArgumentList.Clear();
                foreach (var arg in prepareArgumentList)
                    psi.ArgumentList.Add(arg);
                if (string.IsNullOrEmpty(tmpFile))
                {
                    psi.ArgumentList.Add(line);
                }
                else
                {
                    File.WriteAllText(tmpFile, line + Environment.NewLine);
                    psi.ArgumentList.Add(tmpFile);
                }
                try
                {
                    Console.WriteLine($">{line}");
                    var process = Process.Start(psi);
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.OutputDataReceived += (sender, e) => Console.WriteLine($"进程[{process?.Id}]: {e.Data}");
                    process.ErrorDataReceived += (sender, e) => Console.WriteLine($"进程[{process?.Id}]: {e.Data}");
                    process.WaitForExit();
                    Console.WriteLine($"进程[{process.Id}]已退出，退出码：{process.ExitCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"执行启动脚本[{line}]时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                }
                finally
                {
                    if (!string.IsNullOrEmpty(tmpFile)
                        && File.Exists(tmpFile))
                        File.Delete(tmpFile);
                }
            }
            if (!string.IsNullOrEmpty(tmpFile)
                && File.Exists(tmpFile))
                File.Delete(tmpFile);
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
                     if (!string.IsNullOrEmpty(configModel.BeginScript))
                     {
                         Console.WriteLine($"开始执行起始脚本...");
                         ExecuteScripts(configModel.BeginScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                     }
                     Console.WriteLine($"开始下载...");
                     var client = new WebFileTransferClient(configModel.ApiUrl, configModel.ApiToken);
                     client.Logger = Console.WriteLine;
                     client.Download(configModel.RemoteFolder, configModel.RemoteFile, configModel.LocalFolder);
                     Console.WriteLine("下载完成！");
                     if (!string.IsNullOrEmpty(configModel.EndScript))
                     {
                         Console.WriteLine($"开始执行结束脚本...");
                         ExecuteScripts(configModel.EndScript.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                     }
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

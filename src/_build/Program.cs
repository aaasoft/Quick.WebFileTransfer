using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Quick.Build;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

//版本号
var version = "3.7." + DateTime.Now.ToString("yyyy.Mdd");

//准备目录变量
var appFolder = QbFolder.GetAppFolder();
if (appFolder == Environment.CurrentDirectory)
    Environment.CurrentDirectory = Path.GetFullPath("../../../../../");
var baseFolder = Environment.CurrentDirectory;

var productDict =new Dictionary<string, string>();

foreach (var fi in new DirectoryInfo("src").GetDirectories())
{
    var yiqidongImageFile = Path.Combine(fi.FullName, "YiQiDong.Image.json");
    if (!File.Exists(yiqidongImageFile))
        continue;
    var yiqidongImageFileContent = File.ReadAllText(yiqidongImageFile);
    var jObj = JObject.Parse(yiqidongImageFileContent);
    productDict[fi.Name] = jObj.Property("Name").Value.ToString();
}

Console.WriteLine("请选择编译项目(一个都不勾选代表全选)：");
var productDirs = QbSelect.MultiSelect(productDict.ToArray(), selectedForegroundColor: ConsoleColor.Green);
if (productDirs == null || productDirs.Length == 0)
    productDirs = productDict.Keys.ToArray();
foreach (var productDir in productDirs)
{
    var publishFolder = $"src/{productDir}/bin/Release/publish";
    var productName = QbJson.ReadString(Path.Combine($"src/{productDir}/YiQiDong.Image.json"), "Name");
    var outFolder = Path.GetFullPath("bin");
    if (!Directory.Exists(outFolder))
        Directory.CreateDirectory(outFolder);
    var outFile = Path.Combine(outFolder, $"{productName}_{version}.ymg");

    //开始
    Console.WriteLine("----------------------------------");
    Console.WriteLine($"  欢迎使用[{productName}]发布脚本");
    Console.WriteLine("----------------------------------");
    Console.WriteLine("正在删除Release目录...");
    //先删除Release目录
    QbFolder.DeleteFolders("src", "Release", SearchOption.AllDirectories);
    //再删除ymg文件
    QbFile.DeleteFiles("bin", $"{productName}_{version}.ymg");

    Console.WriteLine($"正在发布{productName}项目...");
    QbCommand.Run("dotnet", $"publish -c Release src/{productDir}");
    //复制文件
    QbFile.CopyFiles($"src/{productDir}", publishFolder, "YiQiDong.Image.*", true);

    //修改容器信息文件中的版本号
    QbJson.WriteString(Path.Combine(publishFolder, "YiQiDong.Image.json"), "Version", version);

    Console.WriteLine("正在制作弈启动镜像...");
    using (var archive = ZipArchive.Create())
    {
        archive.AddAllFromDirectory(publishFolder);
        archive.SaveTo(outFile, CompressionType.LZMA);
    }
}
Console.WriteLine("完成");
//打开窗口
QbGui.OpenFolder("bin");

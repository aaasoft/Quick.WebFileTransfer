using Quick.WebFileTransfer.Client;

var apiUrl = "http://localhost:5088/Quick.WebFileTransfer";
var token = "12345678";
var client = new WebFileTransferClient(apiUrl, token);

var testMode = 1;
switch(testMode)
{
    case 0:
        Console.WriteLine("Downloading...");
        client.Download(null, "*.zip", @"D:\Test\tmp");
        Console.WriteLine("Download completed.");
        break;
    case 1:
        Console.WriteLine("Uploading...");
        client.Upload(null, @"D:\Test\tmp", null);
        Console.WriteLine("Upload completed.");
        break;
}
Console.ReadLine();
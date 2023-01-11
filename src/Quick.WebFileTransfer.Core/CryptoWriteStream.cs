using System.Security.Cryptography;
using System.Text;

namespace Quick.WebFileTransfer.Core
{
    public class CryptoWriteStream : CryptoBaseStream
    {
        public CryptoWriteStream(Stream stream, string token)
            : base(stream, token, CryptoStreamMode.Write)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quick.WebFileTransfer.Core
{
    public class CryptoReadStream : CryptoBaseStream
    {
        public CryptoReadStream(Stream stream, string token)
            : base(stream, token, CryptoStreamMode.Read)
        {
        }
    }
}

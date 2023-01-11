using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quick.WebFileTransfer.Core
{
    public class CryptoBaseStream : Stream, IDisposable
    {
        public override bool CanRead => innerStream.CanRead;
        public override bool CanSeek => innerStream.CanSeek;
        public override bool CanWrite => innerStream.CanWrite;
        public override long Length => innerStream.Length;
        public override long Position { get => innerStream.Position; set => innerStream.Position = value; }

        private CryptoStream innerStream;

        public CryptoBaseStream(Stream stream, string token, CryptoStreamMode cryptoStreamMode)
        {
            var tokenMd5Prefix = CryptographyUtils.ComputeMD5Hash(Encoding.UTF8.GetBytes(token)).Take(8).ToArray();
            DES des = DES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            ICryptoTransform cryptoTransform = null;
            switch (cryptoStreamMode)
            {
                case CryptoStreamMode.Read:
                    cryptoTransform = des.CreateDecryptor(tokenMd5Prefix, tokenMd5Prefix);
                    break;
                case CryptoStreamMode.Write:
                    cryptoTransform = des.CreateEncryptor(tokenMd5Prefix, tokenMd5Prefix);
                    break;
            }
            innerStream = new CryptoStream(stream, cryptoTransform, cryptoStreamMode, true);
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            innerStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
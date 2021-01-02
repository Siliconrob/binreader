using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace binreader
{
    public class Mps7Header
    {
        public string Identifier { get; set; }
        public byte Version { get; set; }
        public uint RecordCount { get; set; }
    }

    public static class Mps7HeaderExtensions
    {
        public static class HeaderFieldSizes
        {
            public const int Total = 9;
            public static readonly Range Version = new Range(4, 5);
            public static readonly Range Identifier = new Range(0, 4);
            public static readonly Range RecordCount = new Range(5, 9);
        }

        public const string MagicIdentifier = "MPS7";

        public static async Task<Mps7Header> ReadMps7HeaderAsync(this Stream txnFile)
        {
            var headerBytes = await txnFile.ReadBytesAsync(HeaderFieldSizes.Total);
            if (headerBytes.Eof || headerBytes.Data.Length != HeaderFieldSizes.Total)
            {
                throw new ArgumentException(
                    $"Header record missing or incomplete{Environment.NewLine}{string.Join(" ", headerBytes.Data.ToArray().Select(z => z.ToString("X2")))}");
            }
            var fileIdentifier =
                System.Text.Encoding.ASCII.GetString(headerBytes.Data.Span[HeaderFieldSizes.Identifier]);
            if (!string.Equals(fileIdentifier, MagicIdentifier, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File identifier does not match {MagicIdentifier}");
            }
            return new Mps7Header
            {
                Version = headerBytes.Data.Span[HeaderFieldSizes.Version].ToArray().FirstOrDefault(),
                Identifier = fileIdentifier,
                RecordCount = (uint) headerBytes.Data[HeaderFieldSizes.RecordCount].ToArray().ToUInt64()
            };
        }
    }
}
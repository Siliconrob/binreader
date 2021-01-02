using System;
using System.IO;
using System.Threading.Tasks;

namespace binreader
{
    public static class StreamExtensions
    {
        public class ReadResult
        {
            public bool Eof { get; set; }
            public ReadOnlyMemory<byte> Data { get; set; }
        }

        private static async Task<ReadResult> ReadByteAsync(this Stream inputStream)
        {
            if (!inputStream.CanRead || inputStream.Position >= inputStream.Length)
            {
                return new ReadResult {Eof = true};
            }
            var buffer = new byte[1];
            var next = await inputStream.ReadAsync(buffer, 0, 1) == -1;
            return new ReadResult { Data = buffer };
        }

        public static async Task<ReadResult> ReadBytesAsync(this Stream inputStream, int chunkSize = 1)
        {
            var memStream = new MemoryStream();
            var count = 0;
            do
            {
                var result = await inputStream.ReadByteAsync();
                if (result.Eof)
                {
                    return new ReadResult
                    {
                        Eof = true,
                        Data = memStream.ToArray()
                    };
                }
                await memStream.WriteAsync(result.Data);
                count++;
            } while (count < chunkSize);
            return new ReadResult
            {
                Eof = false,
                Data = memStream.ToArray()
            };
        }
    }
}
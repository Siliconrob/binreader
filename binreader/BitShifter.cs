using System;
using System.Collections.Generic;

namespace binreader
{
    public static class BitShifter
    {
        public static ulong ToUInt64(this byte[] input)
        {
            if (input.Length > 8)
            {
                throw new ArgumentException("Byte overflow limit to 8 byte input");
            }
            ulong result = 0;
            for (var index = 0; index < input.Length; index++)
            {
                result += (ulong) input[index] << Shift(input, index);
            }
            return result;
        }

        private static int Shift(IReadOnlyCollection<byte> input, int index)
        {
            return (input.Count - 1 - index) * sizeof(byte) * 8;
        }
    }
}
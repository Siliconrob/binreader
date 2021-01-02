namespace binreader
{
    public class Mps7Record
    {
        public byte TransactionCode { get; set; }
        public uint UnixTimeStamp { get; set; }
        public ulong UserId { get; set; }
        public double Amount { get; set; }
    }
}
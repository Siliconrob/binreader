namespace binreader
{
    public class Mps7ConsoleDisplay
    {
        public RecordType Transaction { get; set; }
        public uint UnixTimeStamp { get; set; }
        public ulong UserId { get; set; }
        public double Amount { get; set; }
    }
}
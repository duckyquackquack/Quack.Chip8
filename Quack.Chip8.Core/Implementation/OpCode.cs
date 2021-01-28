namespace Quack.Chip8.Implementation
{
    public class OpCode
    {
        public byte T { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte N { get; set; }
        public byte Nn { get; set; }
        public ushort Nnn { get; set; }

        public override string ToString() => $"{T:X}{X:X}{Y:X}{N:X}";
        public (byte, byte, byte, byte) Deconstruct() => (T, X, Y, N);
    }
}

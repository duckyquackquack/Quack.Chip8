namespace Quack.Chip8.Core.Implementation
{
    public class OpCodeDecoder : IOpCodeDecoder
    {
        public OpCode Decode(ushort value)
        {
            return new OpCode
            {
                T = (byte)(value >> 12),
                X = (byte)((value >> 8) & 15),
                Y = (byte)((value >> 4) & 15),
                N = (byte)(value & 15),
                Nn = (byte)(value & 255),
                Nnn = (ushort)(value & 4095)
            };
        }
    }
}

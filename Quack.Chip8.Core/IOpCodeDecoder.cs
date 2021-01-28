using Quack.Chip8.Implementation;

namespace Quack.Chip8
{
    public interface IOpCodeDecoder
    {
        OpCode Decode(ushort value);
    }
}

using Quack.Chip8.Core.Implementation;

namespace Quack.Chip8.Core
{
    public interface IOpCodeDecoder
    {
        OpCode Decode(ushort value);
    }
}

using Quack.Chip8.Core.Implementation;

namespace Quack.Chip8.Core
{
    public interface IRegisters
    {
        ushort Index { get; set; }
        void Reset();
        byte this[int i] { get; set; }
        byte this[Register r] { get; set; }
    }
}
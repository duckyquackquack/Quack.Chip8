using Quack.Chip8.Implementation;

namespace Quack.Chip8
{
    public interface IRegisters
    {
        ushort Index { get; set; }
        void Reset();
        byte this[int i] { get; set; }
        byte this[Register r] { get; set; }
    }
}
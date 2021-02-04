namespace Quack.Chip8.Core
{
    public interface IAddressStack
    {
        void Reset();
        void Push(ushort address);
        ushort Pop();
    }
}
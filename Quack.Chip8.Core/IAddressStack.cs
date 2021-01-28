namespace Quack.Chip8
{
    public interface IAddressStack
    {
        void Reset();
        void Push(ushort address);
        ushort Pop();
    }
}
namespace Quack.Chip8.Core
{
    public interface IMemory
    {
        ushort FetchShort(int location);
        byte this[int i] { get; set; }
        void Reset();
        void LoadFont(byte[] fontBytes);
        void LoadProgram(byte[] programBytes);
    }
}
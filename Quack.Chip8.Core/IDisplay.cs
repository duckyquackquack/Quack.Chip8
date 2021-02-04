namespace Quack.Chip8.Core
{
    public interface IDisplay
    {
        byte[,] DisplayData { get; }
        int DisplayWidth { get; }
        int DisplayHeight { get; }
        bool RequiresRedraw { get; set; }
        void Reset();
        byte this[int x, int y] { get; set; }
    }
}
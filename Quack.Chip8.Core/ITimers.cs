namespace Quack.Chip8
{
    public interface ITimers
    {
        byte DelayTimer { get; set; }
        byte SoundTimer { get; set; }
        void Reset();
        void DecrementTimers();
    }
}
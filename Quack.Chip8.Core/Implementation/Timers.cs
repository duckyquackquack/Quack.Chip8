namespace Quack.Chip8.Core.Implementation
{
    public class Timers : ITimers
    {
        public byte DelayTimer { get; set; }
        public byte SoundTimer { get; set; }

        public Timers()
        {
            DelayTimer = 0;
            SoundTimer = 0;
        }

        public void Reset()
        {
            DelayTimer = 0;
            SoundTimer = 0;
        }

        public void DecrementTimers()
        {
            if (DelayTimer > 0) DelayTimer--;
            if (SoundTimer > 0) SoundTimer--;
        }
    }
}

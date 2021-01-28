
namespace Quack.Chip8.Implementation
{
    public class Display : IDisplay
    {
        public byte[,] DisplayData { get; }
        public int DisplayWidth { get; }
        public int DisplayHeight { get; }
        public bool RequiresRedraw { get; set; }

        public Display(DisplayConfiguration displayConfiguration)
        {
            DisplayHeight = displayConfiguration.Height;
            DisplayWidth = displayConfiguration.Width;
            DisplayData = new byte[DisplayWidth, DisplayHeight];
            RequiresRedraw = false;
        }

        public void Reset()
        {
            for (var y = 0; y < DisplayHeight; y++)
                for (var x = 0; x < DisplayWidth; x++)
                    DisplayData[x, y] = 0;
            RequiresRedraw = false;
        }

        public byte this[int x, int y]
        {
            get => DisplayData[x, y];
            set
            {
                DisplayData[x, y] = value;
                RequiresRedraw = true;
            }
        }
    }
}

using Quack.Chip8.Implementation;

namespace Quack.Chip8
{
    public interface IFontFactory
    {
        byte[] GetFont(FontType fontType = FontType.Chip48);
    }
}
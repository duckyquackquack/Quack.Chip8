using Quack.Chip8.Core.Implementation;

namespace Quack.Chip8.Core
{
    public interface IFontFactory
    {
        byte[] GetFont(FontType fontType = FontType.Chip48);
    }
}
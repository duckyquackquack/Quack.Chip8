using System;

namespace Quack.Chip8.Core.Implementation
{
    public class Memory : IMemory
    {
        private readonly byte[] _ram;
        private readonly int _programStartLocation;
        private readonly int _fontStartLocation;

        public Memory(MemoryConfiguration memoryConfiguration)
        {
            _programStartLocation = memoryConfiguration.ProgramStartLocation;
            _fontStartLocation = memoryConfiguration.FontStartLocation;

            _ram = new byte[memoryConfiguration.Size];
        }

        public ushort FetchShort(int location)
        {
            var byteA = _ram[location];
            var byteB = _ram[location + 1];

            return (ushort)(byteA << 8 | byteB);
        }

        public byte this[int i]
        {
            get => _ram[i];
            set => _ram[i] = value;
        }

        public void Reset()
        {
            Array.Clear(_ram, 0, _ram.Length);
        }

        public void LoadFont(byte[] fontBytes)
        {
            Array.Copy(fontBytes, 0, _ram, _fontStartLocation, fontBytes.Length);
        }

        public void LoadProgram(byte[] programBytes)
        {
            Array.Copy(programBytes, 0, _ram, _programStartLocation, programBytes.Length);
        }
    }
}

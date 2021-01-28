using System.Collections.Generic;

namespace Quack.Chip8.Logging
{
    public class ChipLog
    {
        public string OpCode { get; set; }
        public string OpCodeDescription { get; set; }
        public IEnumerable<int> GeneralPurposeRegisters { get; set; }
        public ushort IndexRegister { get; set; }
        public ushort ProgramCounter { get; set; }
        public byte DelayTimer { get; set; }
        public byte SoundTimer { get; set; }
    }
}

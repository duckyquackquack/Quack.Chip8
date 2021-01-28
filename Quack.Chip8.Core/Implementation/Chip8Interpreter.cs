using System;
using System.IO;

namespace Quack.Chip8.Implementation
{
    public class Chip8Interpreter
    {
        private Chip8Configuration _configuration;

        private ushort _programCounter;

        private IOpCodeDecoder _opCodeDecoder;
        private IAddressStack _addressStack;
        private IFontFactory _fontFactory;
        private IMemory _memory;
        private ITimers _timers;
        private IRegisters _registers;

        private Random _rng;

        public bool[] KeyState { get; set; }
        public IDisplay Display { get; private set; }

        private int _targetFrequency;
        private const double OneSecondMillis = 1000.0;

        private void LoadDefaultConfiguration()
        {
            var defaultConfigurationJson = File.ReadAllText("defaultConfiguration.json");
            _configuration = Chip8Configuration.FromJson(defaultConfigurationJson);
        }

        private void Reset()
        {
            _targetFrequency = _configuration.Cpu.InitialTargetFrequency;

            _programCounter = (ushort)_configuration.MemoryConfiguration.ProgramStartLocation;

            const int numKeys = 0x10;
            KeyState = new bool[numKeys];
            for (var i = 0; i < KeyState.Length; i++)
                KeyState[i] = false;

            _addressStack.Reset();
            _registers.Reset();
            _timers.Reset();
            _memory.Reset();
            Display.Reset();
        }

        public void Load(string path)
        {
            LoadDefaultConfiguration();

            _opCodeDecoder = new OpCodeDecoder();
            _addressStack = new AddressStack(_configuration.AddressStackSize);
            _fontFactory = new FontFactory();
            _registers = new Registers();
            _timers = new Timers();
            _memory = new Memory(_configuration.MemoryConfiguration);
            _rng = new Random();
            Display = new Display(_configuration.DisplayConfiguration);

            Reset();

            LoadFontIntoMemory();
            LoadProgramIntoMemory(path);
        }

        public void Update(double elapsedMs)
        {
            Display.RequiresRedraw = false;
            var numInstructions = Math.Max(1, (int)(_targetFrequency * (elapsedMs / OneSecondMillis)));

            for (var i = 0; i < numInstructions; i++)
                Step();

            _timers.DecrementTimers();
            // TODO add sound
        }

        public void Step()
        {
            var nextInstruction = Fetch();
            var opCode = Decode(nextInstruction);
            Execute(opCode);
        }

        private ushort Fetch()
        {
            var instruction = _memory.FetchShort(_programCounter);
            _programCounter += 2;

            return instruction;
        }

        private OpCode Decode(ushort instruction) => _opCodeDecoder.Decode(instruction);

        private void Execute(OpCode opCode)
        {
            switch (opCode.Deconstruct())
            {
                case (0x0, 0x0, 0xE, 0x0): Display.Reset(); break;
                case (0x0, 0x0, 0xE, 0xE): ReturnFromSubroutine(); break;
                case (0x1,   _,   _,   _): Jump(opCode); break;
                case (0x2,   _,   _,   _): CallSubroutine(opCode); break;
                case (0x3,   _,   _,   _): SkipIfEqualToX(opCode); break;
                case (0x4,   _,   _,   _): SkipIfNotEqualToX(opCode); break;
                case (0x5,   _,   _, 0x0): SkipIfXEqualY(opCode); break;
                case (0x6,   _,   _,   _): LoadRegisterX(opCode); break;
                case (0x7,   _,   _,   _): AddToRegisterX(opCode); break;
                case (0x8,   _,   _, 0x0): LoadXToY(opCode); break;
                case (0x8,   _,   _, 0x1): BitwiseOrXY(opCode); break;
                case (0x8,   _,   _, 0x2): BitwiseAndXY(opCode); break;
                case (0x8,   _,   _, 0x3): BitwiseXorXY(opCode); break;
                case (0x8,   _,   _, 0x4): AddYToXWithCarry(opCode); break;
                case (0x8,   _,   _, 0x5): SubtractYFromXWithCarry(opCode); break;
                case (0x8,   _,   _, 0x6): ShiftRight(opCode); break;
                case (0x8,   _,   _, 0x7): SubtractXFromYWithCarry(opCode); break;
                case (0x8,   _,   _, 0xE): ShiftLeft(opCode); break;
                case (0x9,   _,   _, 0x0): SkipIfXNotEqualY(opCode); break;
                case (0xA,   _,   _,   _): SetIndexRegister(opCode); break;
                case (0xB,   _,   _,   _): JumpWithOffset(opCode); break;
                case (0xC,   _,   _,   _): RandomX(opCode); break;
                case (0xD,   _,   _,   _): DisplaySprite(opCode); break;
                case (0xE,   _, 0x9, 0xE): SkipIfKeyInRegisterPressed(opCode); break;
                case (0xE,   _, 0xA, 0x1): SkipIfKeyInRegisterNotPressed(opCode); break;
                case (0xF,   _, 0x0, 0x7): SetRegisterEqualToDelay(opCode); break;
                case (0xF,   _, 0x0, 0xA): WaitForKeyPress(opCode); break;
                case (0xF,   _, 0x1, 0x5): SetDelayEqualToRegister(opCode); break;
                case (0xF,   _, 0x1, 0x8): SetSoundEqualToRegister(opCode); break;
                case (0xF,   _, 0x1, 0xE): AddRegisterToIndex(opCode); break;
                case (0xF,   _, 0x2, 0x9): SetFontCharacter(opCode); break;
                case (0xF,   _, 0x3, 0x3): BinaryCodedDecimal(opCode); break;
                case (0xF,   _, 0x5, 0x5): StoreToMemory(opCode); break;
                case (0xF,   _, 0x6, 0x5): LoadFromMemory(opCode); break;
                default: throw new Exception($"Unsupported opcode: {opCode}");
            };
        }

        // FX0A - waits for key press, stores result in VX
        private void WaitForKeyPress(OpCode opCode)
        {
            for (var i = 0; i < KeyState.Length; i++)
            {
                if (KeyState[i])
                {
                    _registers[opCode.X] = (byte)i;
                    return;
                }
            }
            _programCounter -= 2;
        }

        // EX9E - skip next instruction if key with value of VX is pressed
        private void SkipIfKeyInRegisterPressed(OpCode opCode)
        {
            if (KeyState[_registers[opCode.X]])
                _programCounter += 2;
        }

        // EXA1 - skip next instruction if key with value of VX is not pressed
        private void SkipIfKeyInRegisterNotPressed(OpCode opCode)
        {
            if (!KeyState[_registers[opCode.X]])
                _programCounter += 2;
        }

        // Ambiguous - cosmac vip would increment the index register, most interpreters leave it be 
        // TODO - make this configurable
        // FX55 store register values into index
        private void StoreToMemory(OpCode opCode)
        {
            for (var i = 0; i <= opCode.X; i++)
                _memory[_registers.Index + i] = _registers[i];
        }

        // Ambiguous - cosmac vip would increment the index register, most interpreters leave it be 
        // TODO - make this configurable
        // FX65 load from index into registers
        private void LoadFromMemory(OpCode opCode)
        {
            for (var i = 0; i <= opCode.X; i++)
                _registers[i] = _memory[_registers.Index + i];
        }

        // FX33 convert VX to 3 decimal digits, stored at index+0, index+1, index+2 respectively
        private void BinaryCodedDecimal(OpCode opCode)
        {
            var value = _registers[opCode.X];

            _memory[_registers.Index + 0] = (byte)(value / 100);
            _memory[_registers.Index + 1] = (byte)((value / 10) % 10);
            _memory[_registers.Index + 2] = (byte)(value % 10);
        }

        // Ambiguous - some interpreters set VF to 1 if address overflows past the value of 0x1000
        // TODO - add configuration for the above ambiguity 
        // FX1E add VX to to index
        private void AddRegisterToIndex(OpCode opCode) => _registers.Index += _registers[opCode.X];

        // CXNN random number between 0 and 255, &'d with NN, stored in VX
        private void RandomX(OpCode opCode)
        {
            var randomValue = (byte)_rng.Next(0, 256);
            randomValue = (byte) (randomValue & opCode.Nn);

            _registers[opCode.X] = randomValue;
        }

        // BNNN Ambiguous instruction. Some implementations use this as BXNN i.e. jump to NNN + VX. Most jump to NNN + VO.
        // Made it configurable.
        private void JumpWithOffset(OpCode opCode)
        {
            if (_configuration.AmbiguousInstructionsOptions.UseBxnnForJumpWithOffset)
                _programCounter = (ushort) (opCode.Nnn + _registers[opCode.X]);
            else
                _programCounter = (ushort) (opCode.Nnn + _registers[Register.V0]);
        }

        // 8XY6 - Ambiguous instruction. Some implementations set VX = VY before shifting. Made it configurable.
        // Shifts VX to the right by 1 place. VF = 1 if it was a 1 that was shifted out.
        private void ShiftRight(OpCode opCode)
        {
            if (_configuration.AmbiguousInstructionsOptions.SetXToYBeforeShiftFor8Xy6And8Xye)
                _registers[opCode.X] = _registers[opCode.Y];

            _registers[Register.VF] = (byte)(_registers[opCode.X] & 1);
            _registers[opCode.X] = (byte)(_registers[opCode.X] >> 1);
        }

        // 8XYE - Ambiguous instruction. Some implementations set VX = VY before shifting. Made it configurable.
        // Shifts VX to the left by 1 place. VF = 1 if it was a 1 that was shifted out.
        private void ShiftLeft(OpCode opCode)
        {
            if (_configuration.AmbiguousInstructionsOptions.SetXToYBeforeShiftFor8Xy6And8Xye)
                _registers[opCode.X] = _registers[opCode.Y];

            _registers[Register.VF] = (byte)((_registers[opCode.X] & 128) == 128 ? 1 : 0);
            _registers[opCode.X] = (byte)(_registers[opCode.X] << 1);
        }

        // 8XY7 set VX to VY-VX, VF = 1 if it didn't underflow (opposite to what you'd think)
        private void SubtractXFromYWithCarry(OpCode opCode)
        {
            var vx = _registers[opCode.X];
            var vy = _registers[opCode.Y];

            _registers[Register.VF] = (byte)(vy > vx ? 1 : 0);
            _registers[opCode.X] = (byte)((vy - vx) % 256);
        }

        // 8XY5 set VX to VX-VY, VF = 1 if it didn't underflow (opposite to what you'd think)
        private void SubtractYFromXWithCarry(OpCode opCode)
        {
            var vx = _registers[opCode.X];
            var vy = _registers[opCode.Y];

            _registers[Register.VF] = (byte) (vx > vy ? 1 : 0);
            _registers[opCode.X] = (byte)((vx - vy) % 256);
        }

        // 8XY4 - add VY to VX, store result in VX, set VF = 1 if addition caused overflow
        private void AddYToXWithCarry(OpCode opCode)
        {
            var vx = (int)_registers[opCode.X];
            var vy = (int)_registers[opCode.Y];

            if (vx + vy > 255) _registers[Register.VF] = 1;
            _registers[opCode.X] = (byte)(vx + vy);
        }

        // 9XY0 skip an instruction if VX != VY
        private void SkipIfXNotEqualY(OpCode opCode)
        {
            if (_registers[opCode.X] != _registers[opCode.Y])
                _programCounter += 2;
        }

        // 5XY0 skip an instruction if VX == VY
        private void SkipIfXEqualY(OpCode opCode)
        {
            if (_registers[opCode.X] == _registers[opCode.Y])
                _programCounter += 2;
        }

        // 4XNN skip an instruction if VX != Nn
        private void SkipIfNotEqualToX(OpCode opCode)
        {
            if (_registers[opCode.X] != opCode.Nn)
                _programCounter += 2;
        }

        // 3XNN skip an instruction if VX == Nn
        private void SkipIfEqualToX(OpCode opCode)
        {
            if (_registers[opCode.X] == opCode.Nn)
                _programCounter += 2;
        }

        // 2NNN Saves current program counter to address stack and then sets it to Nnn
        private void CallSubroutine(OpCode opCode)
        {
            _addressStack.Push(_programCounter);
            _programCounter = opCode.Nnn;
        }

        private void ReturnFromSubroutine() => _programCounter = _addressStack.Pop(); // 00EE - Return from a subroutine.
        private void SetIndexRegister(OpCode opCode) => _registers.Index = opCode.Nnn; // ANNN Set index register to Nnn
        private void AddToRegisterX(OpCode opCode) => _registers[opCode.X] += opCode.Nn; // 7XNN Add Nn to VX
        private void LoadRegisterX(OpCode opCode) => _registers[opCode.X] = opCode.Nn; // 6XNN Set VX to Nn 
        private void Jump(OpCode opCode) => _programCounter = opCode.Nnn; // 1NNN Jump to Nnn
        private void LoadXToY(OpCode opCode) => _registers[opCode.X] = _registers[opCode.Y]; // 8XY0 - set VX equal to VY
        private void SetRegisterEqualToDelay(OpCode opCode) => _registers[opCode.X] = _timers.DelayTimer; // FX07 sets VX to the current value of the delay timer
        private void SetDelayEqualToRegister(OpCode opCode) => _timers.DelayTimer = _registers[opCode.X]; // FX15 sets the delay timer to the value in VX
        private void SetSoundEqualToRegister(OpCode opCode) => _timers.SoundTimer = _registers[opCode.X]; // FX18 sets the sound timer to the value in VX
        private void SetFontCharacter(OpCode opCode) => _registers.Index = (ushort)(_configuration.MemoryConfiguration.FontStartLocation + (_registers[opCode.X] * 5)); // FX29 The index register I is set to the address of the hexadecimal character in VX
        private void BitwiseXorXY(OpCode opCode) => _registers[opCode.X] = (byte)(_registers[opCode.X] ^ _registers[opCode.Y]); // 8XY3 bitwise xor between VX and VY, store result in VY
        private void BitwiseAndXY(OpCode opCode) => _registers[opCode.X] = (byte)(_registers[opCode.X] & _registers[opCode.Y]); // 8XY2 bitwise and between VX and VY, store result in VX
        private void BitwiseOrXY(OpCode opCode) => _registers[opCode.X] = (byte)(_registers[opCode.X] | _registers[opCode.Y]); // 8XY1 bitwise OR on VX and VY, store result in VX

        // DXYN Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
        private void DisplaySprite(OpCode opCode)
        {
            var x = _registers[opCode.X];
            var y = _registers[opCode.Y];

            _registers[Register.VF] = 0;

            for (var height = 0; height < opCode.N; height++)
            {
                var spriteData = _memory[_registers.Index + height];
                for (var width = 0; width < 8; width++)
                {
                    int xCoordinate = (x + width) % Display.DisplayWidth;
                    int yCoordinate = (y + height) % Display.DisplayHeight;

                    if ((spriteData & (0x80 >> width)) != 0)
                    {
                        if (Display[xCoordinate, yCoordinate] == 1) _registers[Register.VF] = 1;
                            Display[xCoordinate, yCoordinate] ^= 1;
                    }
                }
            }
        }

        private void LoadFontIntoMemory()
        {
            var fontBytes = _fontFactory.GetFont(Enum.Parse<FontType>(_configuration.Font));
           _memory.LoadFont(fontBytes);
        }

        private void LoadProgramIntoMemory(string path)
        {
            var programBytes = File.ReadAllBytes(path);
            _memory.LoadProgram(programBytes);
        }
    }
}

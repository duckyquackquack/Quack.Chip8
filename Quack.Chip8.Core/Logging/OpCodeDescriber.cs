using Quack.Chip8.Implementation;

namespace Quack.Chip8.Logging
{
    public class OpCodeDescriber
    {
        public string Describe(OpCode opCode)
        {
            return (opCode.T, opCode.X, opCode.Y, opCode.N) switch
            {
                (0x0, 0x0, 0xE, 0x0) => "CLS - Clearing screen",
                (0x0, 0x0, 0xE, 0xE) => "RET - Returning from sub-routine",
                (0x1,   _,   _,   _) => $"JP - Jumping to {opCode.Nnn} (0x{opCode.Nnn:X})",
                (0x2,   _,   _,   _) => $"CALL - Calling sub-routine at {opCode.Nnn} (0x{opCode.Nnn:X})",
                (0x3,   _,   _,   _) => $"SE - Skipping next instruction if V{opCode.X:X} == {opCode.Nn} (0x{opCode.Nn:X})",
                (0x4,   _,   _,   _) => $"SNE - Skipping next instruction if V{opCode.X:X} != {opCode.Nn} (0x{opCode.Nn:X})",
                (0x5,   _,   _, 0x0) => $"SE - Skipping next instruction if V{opCode.X:X} == V{opCode.Y:X}",
                (0x6,   _,   _,   _) => $"LD - Setting V{opCode.X:X} to {opCode.Nn} (0x{opCode.Nn:X})",
                (0x7,   _,   _,   _) => $"ADD - Setting V{opCode.X:X} to V{opCode.X:X} + {opCode.Nn} (0x{opCode.Nn:X})",
                (0x8,   _,   _, 0x0) => $"LD - Setting V{opCode.X:X} to V{opCode.Y:X}",
                (0x8,   _,   _, 0x1) => $"OR - Setting V{opCode.X:X} to (V{opCode.X:X} | V{opCode.Y:X})",
                (0x8,   _,   _, 0x2) => $"AND - Setting V{opCode.X:X} to (V{opCode.X:X} & V{opCode.Y:X})",
                (0x8,   _,   _, 0x3) => $"XOR - Setting V{opCode.X:X} to (V{opCode.X:X} ^ V{opCode.Y:X})",
                (0x8,   _,   _, 0x4) => $"ADD - Setting V{opCode.X:X} to (V{opCode.X:X} + V{opCode.Y:X}). VF = 1 if result > 255",
                (0x8,   _,   _, 0x5) => $"SUB - Setting V{opCode.X:X} to (V{opCode.X:X} - V{opCode.Y:X}). VF = 1 if VX > VY",
                (0x8,   _,   _, 0x6) => $"SHR - Shifting V{opCode.X} right by 1 (div by 2). VF = 1 if LSB was 1",
                (0x8,   _,   _, 0x7) => $"SUBN - Setting V{opCode.X:X} to (V{opCode.Y:X} - V{opCode.X:X}). VF = 1 if VY > VX",
                (0x8,   _,   _, 0xE) => $"SHL - Shifting V{opCode.X:X} left by 1 (mult by 2). VF = 1 if MSB was 1",
                (0x9,   _,   _, 0x0) => $"SNE - Skipping next instruction if V{opCode.X:X} != V{opCode.Y:X}",
                (0xA,   _,   _,   _) => $"LD - Setting index register to {opCode.Nnn} (0x{opCode.Nnn:X})",
                (0xB,   _,   _,   _) => $"JP - Jumping to {opCode.Nnn} (0x{opCode.Nnn:X}) with offset from V0",
                (0xC,   _,   _,   _) => $"RND - Setting V{opCode.X:X} to random number ANDed with {opCode.Nn} (0x{opCode.Nn:X})",
                (0xD,   _,   _,   _) => $"DRW - Drawing {opCode.N}byte sprite (located at Index) at (V{opCode.X:X}, V{opCode.Y:X}). VF = 1 if collision",
                (0xF,   _, 0x0, 0x7) => $"LD - Setting V{opCode.X:X} = delay timer",
                (0xF,   _, 0x1, 0x5) => $"LD - Setting delay timer to V{opCode.X:X}",
                (0xF,   _, 0x1, 0xE) => $"ADD - Setting Index = Index + V{opCode.X:X}",
                (0xF,   _, 0x2, 0x9) => $"LD - Setting index to font sprite data for {opCode.X:X}",
                (0xF,   _, 0x3, 0x3) => $"BCD - Storing binary coded decimal of V{opCode.X:X} into index+0,+1,+2",
                (0xF,   _, 0x5, 0x5) => $"LD - Storing registers V0...V{opCode.X:X} to index+0...index+{opCode.X}",
                (0xF,   _, 0x6, 0x5) => $"LD - Loading from index+0...index+{opCode.X} into registers V0 to V{opCode.X:X}",
                _ => "Unrecognized opcode!"
            };
        }
    }
}

namespace Quack.Chip8.Implementation
{
    public enum Register
    {
        V0 = 0, VF = 15
    }

    public class Registers : IRegisters
    {
        private readonly byte[] _registerValues;
        public ushort Index { get; set; }

        public Registers()
        {
            _registerValues = new byte[(int)Register.VF + 1];
        }

        public void Reset()
        {
            for (var i = 0; i < _registerValues.Length; i++)
                _registerValues[i] = 0;

            Index = 0;
        }

        public byte this[int i]
        {
            get => _registerValues[i];
            set => _registerValues[i] = value;
        }

        public byte this[Register r]
        {
            get => _registerValues[(int)r];
            set => _registerValues[(int)r] = value;
        }
    }
}

using System;

namespace Quack.Chip8.Implementation
{
    public class AddressStack : IAddressStack
    {
        private readonly ushort[] _stack;
        private uint _stackPointer;

        public AddressStack(int stackCapacity = 16)
        {
            _stack = new ushort[stackCapacity];
        }

        public void Reset()
        {
            for (var i = 0; i < _stack.Length; i++)
                _stack[i] = 0;

            _stackPointer = 0;
        }

        public void Push(ushort address)
        {
            if (IsFull())
                throw new Exception($"Address stack is full. Cannot push more than {_stack.Length} addresses.");

            _stackPointer++;
            _stack[_stackPointer] = address;
        }

        public ushort Pop()
        {
            if (IsEmpty())
                throw new Exception("Address stack is empty. Cannot pull.");

            var address = _stack[_stackPointer];
            _stackPointer--;
            return address;
        }

        private bool IsFull() => _stackPointer == _stack.Length - 1;
        private bool IsEmpty() => _stackPointer == 0;
    }
}

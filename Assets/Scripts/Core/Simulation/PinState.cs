using System;
using System.Collections;
using System.Collections.Generic;

namespace DLS.Core.Simulation
{
    public enum PinState
    {
        FLOATING = -1,
        LOW = 0,
        HIGH = 1
    }


    public class PinStates : List<PinState>
    {
        public PinStates(Pin.WireType wireType) : base(Pin.NumBits(wireType))
        {
        }

        private PinStates(PinState outputSignal) : base(1)
        {
            Add(outputSignal);
        }

        public PinStates(uint uuInt) : base()
        {
            byte[] bytes = BitConverter.GetBytes(uuInt);
            BitArray bitArray = new BitArray(bytes);

            for (var i = 0; i < bitArray.Count; i++)
                Add(bitArray[i]);
        }


        private void Add(bool b)
        {
            Add(b ? PinState.HIGH : PinState.LOW);
        }

        public static PinStates AllLow(Pin.WireType wiretype = Pin.WireType.Bus32)
        {
            return PinStates1(PinState.LOW, wiretype);
        }

        public static PinStates AllHigh(Pin.WireType wiretype = Pin.WireType.Bus32)
        {
            return PinStates1(PinState.HIGH, wiretype);
        }

        private static PinStates PinStates1(PinState pin, Pin.WireType wireType)
        {
            var States = new PinStates(wireType);

            for (int i = 0; i < States.Capacity; i++)
                States.Add(pin);

            return States;
        }

        public uint ToUInt()
        {
            BitArray bytearr = new BitArray(Capacity);
            for (var i = 0; i < Count; i++)
            {
                bytearr[i] = this[i].ToBit();
            }

            byte[] bytes = new byte[4];
            bytearr.CopyTo(bytes, 0);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static PinStates Zero => new PinStates(PinState.LOW);
        public static PinStates One => new PinStates(PinState.HIGH);
        public static PinStates Floating => new PinStates(PinState.FLOATING);

        public static PinStates Getstates(PinState pinstate)
        {
            return pinstate switch
            {
                PinState.HIGH => One,
                PinState.LOW => Zero,
                PinState.FLOATING => Floating,
                _ => throw new ArgumentOutOfRangeException(nameof(pinstate), pinstate, null)
            };
        }
    }


    public static class PinStateExtensions
    {
        public static bool ToBit(this PinState state)
        {
            return (state == PinState.HIGH);
        }

        public static int Toint(this PinState state)
        {
            return (state == PinState.HIGH) ? 1 : 0;
        }
    }
}
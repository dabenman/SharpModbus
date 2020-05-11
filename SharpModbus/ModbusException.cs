using System;

namespace SharpModbus
{
    public class ModbusException : Exception
    {
        public ModbusException(byte code) :
            base(string.Format("Modbus exception {0}", code))
        {
            Code = code;
        }

        public byte Code { get; }
    }
}
﻿namespace SharpModbus
{
    public class ModbusF03ReadHoldingRegisters : IModbusCommand
    {
        public ModbusF03ReadHoldingRegisters(byte slave, ushort address, ushort count)
        {
            Slave = slave;
            Address = address;
            Count = count;
        }

        public ushort Count { get; }

        public byte Code => 3;
        public byte Slave { get; }

        public ushort Address { get; }

        public int RequestLength => 6;
        public int ResponseLength => 3 + ModbusHelper.BytesForWords(Count);

        public void FillRequest(byte[] request, int offset)
        {
            request[offset + 0] = Slave;
            request[offset + 1] = 3;
            request[offset + 2] = ModbusHelper.High(Address);
            request[offset + 3] = ModbusHelper.Low(Address);
            request[offset + 4] = ModbusHelper.High(Count);
            request[offset + 5] = ModbusHelper.Low(Count);
        }

        public object ParseResponse(byte[] response, int offset)
        {
            var bytes = ModbusHelper.BytesForWords(Count);
            Assert.Equal(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            Assert.Equal(response[offset + 1], 3, "Function mismatch got {0} expected {1}");
            Assert.Equal(response[offset + 2], bytes, "Bytes mismatch got {0} expected {1}");
            return ModbusHelper.DecodeWords(response, offset + 3, Count);
        }

        public object ApplyTo(IModbusModel model)
        {
            return model.getWOs(Slave, Address, Count);
        }

        public void FillResponse(byte[] response, int offset, object value)
        {
            var bytes = ModbusHelper.BytesForWords(Count);
            response[offset + 0] = Slave;
            response[offset + 1] = 3;
            response[offset + 2] = bytes;
            var data = ModbusHelper.EncodeWords((ushort[]) value);
            ModbusHelper.Copy(data, 0, response, offset + 3, bytes);
        }

        public override string ToString()
        {
            return string.Format("[ModbusF03ReadHoldingRegisters Slave={0}, Address={1}, Count={2}]", Slave, Address,
                Count);
        }
    }
}
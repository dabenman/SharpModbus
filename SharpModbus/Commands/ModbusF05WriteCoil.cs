namespace SharpModbus
{
    public class ModbusF05WriteCoil : IModbusCommand
    {
        public ModbusF05WriteCoil(byte slave, ushort address, bool state)
        {
            Slave = slave;
            Address = address;
            Value = state;
        }

        public bool Value { get; }

        public byte Code => 5;
        public byte Slave { get; }

        public ushort Address { get; }

        public int RequestLength => 6;
        public int ResponseLength => 6;

        public void FillRequest(byte[] request, int offset)
        {
            request[offset + 0] = Slave;
            request[offset + 1] = 5;
            request[offset + 2] = ModbusHelper.High(Address);
            request[offset + 3] = ModbusHelper.Low(Address);
            request[offset + 4] = ModbusHelper.EncodeBool(Value);
            request[offset + 5] = 0;
        }

        public object ParseResponse(byte[] response, int offset)
        {
            Assert.Equal(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            Assert.Equal(response[offset + 1], 5, "Function mismatch got {0} expected {1}");
            Assert.Equal(ModbusHelper.GetUShort(response, offset + 2), Address,
                "Address mismatch got {0} expected {1}");
            Assert.Equal(response[offset + 4], ModbusHelper.EncodeBool(Value), "Value mismatch got {0} expected {1}");
            Assert.Equal(response[offset + 5], 0, "Pad mismatch {0} expected:{1}");
            return null;
        }

        public object ApplyTo(IModbusModel model)
        {
            model.setDO(Slave, Address, Value);
            return null;
        }

        public void FillResponse(byte[] response, int offset, object value)
        {
            FillRequest(response, offset);
        }

        public override string ToString()
        {
            return string.Format("[ModbusF05WriteCoil Slave={0}, Address={1}, Value={2}]", Slave, Address, Value);
        }
    }
}
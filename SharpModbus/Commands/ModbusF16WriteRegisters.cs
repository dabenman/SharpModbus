namespace SharpModbus
{
    public class ModbusF16WriteRegisters : IModbusCommand
    {
        private readonly ushort[] values;

        public ModbusF16WriteRegisters(byte slave, ushort address, ushort[] values)
        {
            Slave = slave;
            Address = address;
            this.values = values;
        }

        public ushort[] Values => ModbusHelper.Clone(values);

        public byte Code => 16;
        public byte Slave { get; }

        public ushort Address { get; }

        public int RequestLength => 7 + ModbusHelper.BytesForWords(values.Length);
        public int ResponseLength => 6;

        public void FillRequest(byte[] request, int offset)
        {
            FillResponse(request, offset, null);
            var bytes = ModbusHelper.EncodeWords(values);
            request[offset + 6] = (byte) bytes.Length;
            ModbusHelper.Copy(bytes, 0, request, offset + 7, bytes.Length);
        }

        public object ParseResponse(byte[] response, int offset)
        {
            Assert.Equal(response[offset + 0], Slave, "Slave mismatch got {0} expected {1}");
            Assert.Equal(response[offset + 1], 16, "Function mismatch got {0} expected {1}");
            Assert.Equal(ModbusHelper.GetUShort(response, offset + 2), Address,
                "Address mismatch got {0} expected {1}");
            Assert.Equal(ModbusHelper.GetUShort(response, offset + 4), values.Length,
                "Register count mismatch got {0} expected {1}");
            return null;
        }

        public object ApplyTo(IModbusModel model)
        {
            model.setWOs(Slave, Address, values);
            return null;
        }

        public void FillResponse(byte[] response, int offset, object value)
        {
            response[offset + 0] = Slave;
            response[offset + 1] = 16;
            response[offset + 2] = ModbusHelper.High(Address);
            response[offset + 3] = ModbusHelper.Low(Address);
            response[offset + 4] = ModbusHelper.High(values.Length);
            response[offset + 5] = ModbusHelper.Low(values.Length);
        }

        public override string ToString()
        {
            return string.Format("[ModbusF16WriteRegisters Slave={0}, Address={1}, Values={2}]", Slave, Address,
                values);
        }
    }
}
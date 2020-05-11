namespace SharpModbus
{
    public class ModbusRTUWrapper : IModbusWrapper
    {
        public ModbusRTUWrapper(IModbusCommand wrapped)
        {
            Wrapped = wrapped;
        }

        public byte Code => Wrapped.Code;
        public byte Slave => Wrapped.Slave;
        public ushort Address => Wrapped.Address;
        public IModbusCommand Wrapped { get; }

        public int RequestLength => Wrapped.RequestLength + 2;
        public int ResponseLength => Wrapped.ResponseLength + 2;

        public void FillRequest(byte[] request, int offset)
        {
            Wrapped.FillRequest(request, offset);
            var crc = ModbusHelper.CRC16(request, offset, Wrapped.RequestLength);
            request[offset + Wrapped.RequestLength + 0] = ModbusHelper.Low(crc);
            request[offset + Wrapped.RequestLength + 1] = ModbusHelper.High(crc);
        }

        public object ParseResponse(byte[] response, int offset)
        {
            var crc1 = ModbusHelper.CRC16(response, offset, Wrapped.ResponseLength);
            //crc is little endian page 13 http://modbus.org/docs/Modbus_over_serial_line_V1_02.pdf
            var crc2 = ModbusHelper.GetUShortLittleEndian(response, offset + Wrapped.ResponseLength);
            Assert.Equal(crc2, crc1, "CRC mismatch got {0:X4} expected {1:X4}");
            return Wrapped.ParseResponse(response, offset);
        }

        public object ApplyTo(IModbusModel model)
        {
            return Wrapped.ApplyTo(model);
        }

        public void FillResponse(byte[] response, int offset, object value)
        {
            Wrapped.FillResponse(response, offset, value);
            var crc = ModbusHelper.CRC16(response, offset, Wrapped.ResponseLength);
            response[offset + Wrapped.ResponseLength + 0] = ModbusHelper.Low(crc);
            response[offset + Wrapped.ResponseLength + 1] = ModbusHelper.High(crc);
        }

        public byte[] GetException(byte code)
        {
            var exception = new byte[5];
            exception[0] = Wrapped.Slave;
            exception[1] = (byte) (Wrapped.Code | 0x80);
            exception[2] = code;
            var crc = ModbusHelper.CRC16(exception, 0, 3);
            exception[3] = ModbusHelper.Low(crc);
            exception[4] = ModbusHelper.High(crc);
            return exception;
        }

        public void CheckException(byte[] response, int count)
        {
            if (count < 5) Thrower.Throw("Partial exception packet got {0} expected >= {1}", count, 5);
            var offset = 0;
            var code = response[offset + 1];
            if ((code & 0x80) != 0)
            {
                Assert.Equal(response[offset + 0], Wrapped.Slave, "Slave mismatch got {0} expected {1}");
                Assert.Equal(code & 0x7F, Wrapped.Code, "Code mismatch got {0} expected {1}");
                var crc1 = ModbusHelper.CRC16(response, offset, 3);
                //crc is little endian page 13 http://modbus.org/docs/Modbus_over_serial_line_V1_02.pdf
                var crc2 = ModbusHelper.GetUShortLittleEndian(response, offset + 3);
                Assert.Equal(crc2, crc1, "CRC mismatch got {0:X4} expected {1:X4}");
                throw new ModbusException(response[offset + 2]);
            }
        }

        public override string ToString()
        {
            return string.Format("[ModbusRTUWrapper Wrapped={0}]", Wrapped);
        }
    }
}
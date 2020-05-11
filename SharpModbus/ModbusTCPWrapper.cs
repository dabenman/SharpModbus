namespace SharpModbus
{
    public class ModbusTCPWrapper : IModbusWrapper
    {
        public ModbusTCPWrapper(IModbusCommand wrapped, int transactionId)
        {
            Wrapped = wrapped;
            TransactionId = transactionId;
        }

        public int TransactionId { get; }

        public byte Code => Wrapped.Code;
        public byte Slave => Wrapped.Slave;
        public ushort Address => Wrapped.Address;
        public IModbusCommand Wrapped { get; }

        public int RequestLength => Wrapped.RequestLength + 6;
        public int ResponseLength => Wrapped.ResponseLength + 6;

        public void FillRequest(byte[] request, int offset)
        {
            request[offset + 0] = ModbusHelper.High(TransactionId);
            request[offset + 1] = ModbusHelper.Low(TransactionId);
            request[offset + 2] = 0;
            request[offset + 3] = 0;
            request[offset + 4] = ModbusHelper.High(Wrapped.RequestLength);
            request[offset + 5] = ModbusHelper.Low(Wrapped.RequestLength);
            Wrapped.FillRequest(request, offset + 6);
        }

        public object ParseResponse(byte[] response, int offset)
        {
            Assert.Equal(ModbusHelper.GetUShort(response, offset + 0), TransactionId,
                "TransactionId mismatch got {0} expected {1}");
            Assert.Equal(ModbusHelper.GetUShort(response, offset + 2), 0, "Zero mismatch got {0} expected {1}");
            Assert.Equal(ModbusHelper.GetUShort(response, offset + 4), Wrapped.ResponseLength,
                "Length mismatch got {0} expected {1}");
            return Wrapped.ParseResponse(response, offset + 6);
        }

        public object ApplyTo(IModbusModel model)
        {
            return Wrapped.ApplyTo(model);
        }

        public void FillResponse(byte[] response, int offset, object value)
        {
            response[offset + 0] = ModbusHelper.High(TransactionId);
            response[offset + 1] = ModbusHelper.Low(TransactionId);
            response[offset + 2] = 0;
            response[offset + 3] = 0;
            response[offset + 4] = ModbusHelper.High(Wrapped.ResponseLength);
            response[offset + 5] = ModbusHelper.Low(Wrapped.ResponseLength);
            Wrapped.FillResponse(response, offset + 6, value);
        }

        public byte[] GetException(byte code)
        {
            var exception = new byte[9];
            exception[0] = ModbusHelper.High(TransactionId);
            exception[1] = ModbusHelper.Low(TransactionId);
            exception[2] = 0;
            exception[3] = 0;
            exception[4] = ModbusHelper.High(3);
            exception[5] = ModbusHelper.Low(3);
            exception[6 + 0] = Wrapped.Slave;
            exception[6 + 1] = (byte) (Wrapped.Code | 0x80);
            exception[6 + 2] = code;
            return exception;
        }

        public void CheckException(byte[] response, int count)
        {
            if (count < 9) Thrower.Throw("Partial exception packet got {0} expected >= {1}", count, 9);
            var offset = 6;
            var code = response[offset + 1];
            if ((code & 0x80) != 0)
            {
                Assert.Equal(response[offset + 0], Wrapped.Slave, "Slave mismatch got {0} expected {1}");
                Assert.Equal(code & 0x7F, Wrapped.Code, "Code mismatch got {0} expected {1}");
                throw new ModbusException(response[offset + 2]);
            }
        }

        public override string ToString()
        {
            return string.Format("[ModbusTCPWrapper Wrapped={0}, TransactionId={1}]", Wrapped, TransactionId);
        }
    }
}
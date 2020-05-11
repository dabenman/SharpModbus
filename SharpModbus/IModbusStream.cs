﻿using System;

namespace SharpModbus
{
    public interface IModbusStream : IDisposable
    {
        void Write(byte[] data);
        int Read(byte[] data);
    }
}
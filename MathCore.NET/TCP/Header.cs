using System;
using System.Collections.Generic;
using System.Text;
using MathCore.NET.TCP.Extensions;

namespace MathCore.NET.TCP
{
    public struct Header
    {
        public readonly struct HeaderChecksum
        {
            public readonly ushort Value;
            private readonly byte[] _HeaderData;
            private readonly int _Offset;

            public bool IsCorrect => _HeaderData.GetIPChecksum(offset: _Offset, checksum: Value) == Value;

            internal HeaderChecksum(ushort Value, byte[] HeaderData, int Offset)
            {
                this.Value = Value;
                _HeaderData = HeaderData;
                _Offset = Offset;
            }
        }

        public readonly struct Acknowledgement
        {
            public readonly long Data;
            private readonly DataOffsetAndFlags _Flags;

            public long Number => _Flags.NumberExist ? Data : 0;

            internal Acknowledgement(long data, DataOffsetAndFlags Flags)
            {
                this.Data = data;
                _Flags = Flags;
            }
        }

        public readonly struct DataOffsetAndFlags
        {
            [Flags]
            public enum FlagsSet : byte
            {
                /// <summary>Указывает на завершение соединения</summary>
                FIN = 0x01,
                /// <summary>Синхронизация номеров последовательности</summary>
                SYN = 0x02,
                /// <summary>Оборвать соединения, сбросить буфер (очистка буфера)</summary>
                RST = 0x04,
                /// <summary>
                /// Инструктирует получателя протолкнуть данные, накопившиеся в приемном буфере, в приложение пользователя
                /// </summary>
                PSH = 0x08,
                /// <summary>Номер подтверждения</summary>
                ACK = 0x10,
                /// <summary>Указатель важности</summary>
                URG = 0x20,
                /// <summary>
                /// Эхо ECN  — указывает, что данный узел способен на ECN (явное уведомление перегрузки) и для указания отправителю о перегрузках в сети
                /// </summary>
                ECE = 0x30,
                /// <summary>
                /// Окно перегрузки уменьшено — флаг установлен отправителем, чтобы указать, что получен пакет с установленным флагом ECE
                /// </summary>
                CWR = 0x40,
                /// <summary>ECN-nonce concealment protection</summary>
                NS = 0x50,
            }

            public readonly uint Data;

            public bool NumberExist => (Data & 0x10) == 0x10;
            public bool UrgentPointerExist => (Data & 0x20) == 0x20;

            public byte HeaderLength => (byte)((Data >> 12) << 2);

            public FlagsSet Flags => (FlagsSet)(Data & 0x3f);

            internal DataOffsetAndFlags(uint data) { this.Data = data; }
        }

        public readonly byte[] HeaderData;
        private readonly int _Offset;
        private DataOffsetAndFlags? _OffsetAndFlags;

        public int SourcePort => BitConverter.ToUInt16(HeaderData, _Offset + 0);

        public int DestinationPort => BitConverter.ToUInt16(HeaderData, _Offset + 2);

        public long SequenceNumber => BitConverter.ToUInt32(HeaderData, _Offset + 4);

        public Acknowledgement AcknowledgementNumber => new Acknowledgement(BitConverter.ToUInt32(HeaderData, _Offset + 8), OffsetAndFlags);

        public DataOffsetAndFlags OffsetAndFlags => _OffsetAndFlags ?? (DataOffsetAndFlags)(_OffsetAndFlags = new DataOffsetAndFlags(BitConverter.ToUInt16(HeaderData, _Offset + 12)));

        public ushort WindowSize => BitConverter.ToUInt16(HeaderData, _Offset + 14);
        public HeaderChecksum Checksum => new HeaderChecksum(BitConverter.ToUInt16(HeaderData, _Offset + 16), HeaderData, _Offset);
        public int UrgentPointer => OffsetAndFlags.UrgentPointerExist ? BitConverter.ToInt16(HeaderData, _Offset + 18) : 0;
        public int HeaderLength => OffsetAndFlags.HeaderLength;
        public int DataLength => HeaderData.Length - HeaderLength - _Offset;

        public int DataOffset => _Offset + HeaderLength;

        public DataOffsetAndFlags.FlagsSet Flags => OffsetAndFlags.Flags;

        public byte[] Data
        {
            get
            {
                var result = new byte[HeaderData.Length - HeaderLength - _Offset];
                HeaderData.CopyTo(result, DataOffset);
                return result;
            }
        }

        public Header(byte[] buffer, int offset = 0)
        {
            _OffsetAndFlags = null;
            HeaderData = buffer;
            _Offset = offset;
        }
    }
}

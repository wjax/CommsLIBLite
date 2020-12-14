using CommsLIBLite.Base;
using CommsLIBLite.Communications.FrameWrappers;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommsLIBLite.Communications.FrameWrappers.ProtoBuf
{
    /// <summary>
    /// Protobuf-net builtin serializer using SyncFrameWrapper as base and PrefixStyle.Base128
    /// </summary>
    /// <typeparam name="T">Data type to serialize To and From. It can be a base class from where others inherit</typeparam>
    public class ProtoBuffFrameWrapper<T> : SyncFrameWrapper<T>, IDisposable
    {
        private SpecialPipeStream pipeStreamReader;
        private MemoryStream memoryStreamTX;
        private PrefixStyle _prefixStyle = PrefixStyle.Base128;
        T message;


        public ProtoBuffFrameWrapper(PrefixStyle prefixStyle = PrefixStyle.Base128) : base(false)
        {
            pipeStreamReader = new SpecialPipeStream(65536, false);
            memoryStreamTX = new MemoryStream(8192);
            _prefixStyle = prefixStyle;
        }

        public override void AddBytes(byte[] bytes, int length)
        {
            pipeStreamReader.Write(bytes, 0, length);

            // Will raise exception if there is a problem with deserialization
            while ((message = Serializer.DeserializeWithLengthPrefix<T>(pipeStreamReader, _prefixStyle)) != null)
            {
                FireEvent(message);
            }
            
        }

        public override byte[] Data2BytesSync(T data, out int count)
        {
            memoryStreamTX.Seek(0, SeekOrigin.Begin);
            Serializer.SerializeWithLengthPrefix(memoryStreamTX, data, _prefixStyle);
            count = (int)memoryStreamTX.Position;

            return memoryStreamTX.GetBuffer();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    memoryStreamTX.Dispose();
                    UnsubscribeEventHandlers();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProtoBuffFrameWrapper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}

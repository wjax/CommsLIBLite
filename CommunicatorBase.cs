﻿using CommsLIBLite.Base;
using CommsLIBLite.Communications.FrameWrappers;
using System;
using System.Threading.Tasks;

namespace CommsLIBLite.Communications
{
    /// <summary>
    /// Factory to create ICommunicator based on Uri
    /// </summary>
    public static class CommunicatorFactory
    {
        /// <summary>
        /// Creates an ICommunicator
        /// </summary>
        /// <typeparam name="T">Base data type used by the serialilzer/framewrapper. If not using serializer, put object</typeparam>
        /// <param name="uri">ConnUri defining the connection</param>
        /// <param name="frameWrapper">Serializer/Framewrapper that will be used. It can be null if not used</param>
        /// <param name="circular">Use internal CircularBuffer if you do not own the byte array passed to SendASync and do not have control over its lifespan. It is not used in SendSync</param>
        /// <returns>Created ICommunicator. It still needs to be initialized and started</returns>
        public static ICommunicator CreateCommunicator<T>(ConnUri uri, FrameWrapperBase<T> frameWrapper, bool circular = false)
        {
            ICommunicator c = null ;
            switch (uri.UriType)
            {
                case ConnUri.TYPE.TCP:
                    c = new TCPNETCommunicator<T>(frameWrapper, circular);
                    break;
                case ConnUri.TYPE.UDP:
                    c = new UDPNETCommunicator<T>(frameWrapper, circular);
                    break;
            }

            return c;
        }

        /// <summary>
        /// Creates an ICommunicator
        /// </summary>
        /// <typeparam name="T">Base data type used by the serialilzer/framewrapper. If not using serializer, put object</typeparam>
        /// <param name="uri">string uri defining the connection</param>
        /// <param name="frameWrapper">Serializer/Framewrapper that will be used. It can be null if not used</param>
        /// <param name="circular">Use internal CircularBuffer if you do not own the byte array passed to SendASync and do not have control over its lifespan. It is not used in SendSync</param>
        /// <returns>Created ICommunicator. It still needs to be initialized and started</returns>
        public static ICommunicator CreateCommunicator<T>(string uriString, FrameWrapperBase<T> frameWrapper, bool circular = false)
        {
            ICommunicator c = null;
            var uri = new ConnUri(uriString);

            if (!uri.IsValid)
                return null;

            switch (uri.UriType)
            {
                case ConnUri.TYPE.TCP:
                    c = new TCPNETCommunicator<T>(frameWrapper, circular);
                    break;
                case ConnUri.TYPE.UDP:
                    c = new UDPNETCommunicator<T>(frameWrapper, circular);
                    break;
            }

            return c;
        }
    }

    /// <summary>
    /// Base class for TCP and UDP Communicators
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CommunicatorBase<T> : ICommunicator
    {
        public event DataReadyEventHandler DataReadyEvent;
        public event ConnectionStateDelegate ConnectionStateEvent;
        public event DataRateDelegate DataRateEvent;

        public enum STATE
        {
            RUNNING,
            STOP
        }
        public STATE State;

        public ConnUri CommsUri { get; protected set; }
        public ushort[] IpChunks { get; protected set; } = new ushort[4];
        public string ID { get; set; }

        public abstract void Init(ConnUri uri, bool persistent, string ID, int inactivityMS, int sendGAP = 0);
        public abstract void Start();
        public abstract Task Stop();
        public abstract void SendASync(byte[] bytes, int length);
        public abstract bool SendSync(byte[] bytes, int offset, int length);
        public abstract void SendSync(T protoBufMessage);
        public abstract FrameWrapperBase<T> FrameWrapper { get; }
        

        public virtual void FireDataEvent(string ip, int port, long time, byte[] bytes, int offset, int length, string ID, ushort[] ipChunks = null)
        {
            DataReadyEvent?.Invoke(ip, port, time, bytes, offset, length, ID, ipChunks);
        }

        public virtual void FireConnectionEvent(string ID, ConnUri uri, bool connected)
        {
            ConnectionStateEvent?.Invoke(ID, uri, connected);
        }

        public virtual void FireDataRateEvent(string ID, float dataRateMbpsRX, float dataRateMbpsTX)
        {
            DataRateEvent?.Invoke(ID, dataRateMbpsRX, dataRateMbpsTX);
        }

        protected virtual void SetIPChunks(string _ip)
        {
            string[] chunks = _ip.Split('.');
            if (chunks.Length == 4)
                for (int i = 0; i < 4; i++)
                    IpChunks[i] = ushort.Parse(chunks[i]);
        }

        public void UnsubscribeEventHandlers()
        {
            if (DataReadyEvent != null)
                foreach (var d in DataReadyEvent.GetInvocationList())
                    DataReadyEvent -= (d as DataReadyEventHandler);

            if (ConnectionStateEvent != null)
                foreach (var d in ConnectionStateEvent.GetInvocationList())
                    ConnectionStateEvent -= (d as ConnectionStateDelegate);
        }

        #region IDisposable Support
        protected bool disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UnsubscribeEventHandlers();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        
    }
}

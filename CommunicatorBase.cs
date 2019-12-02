﻿using CommsLIBLite.Base;
using CommsLIBLite.Communications.FrameWrappers;
using System;
using System.Threading.Tasks;

namespace CommsLIBLite.Communications
{
    public static class CommunicatorFactory
    {
        public static CommunicatorBase<T> CreateCommunicator<T>(ConnUri uri, FrameWrapperBase<T> frameWrapper, bool circular = false)
        {
            CommunicatorBase<T> c = null ;
            switch (uri.UriType)
            {
                case ConnUri.TYPE.TCP:
                    c = new TCPNETCommunicator<T>(frameWrapper, circular);
                    break;
                case ConnUri.TYPE.UDP:
                    c = new UDPNETCommunicator<T>(frameWrapper, circular);
                    break;
                //case ConnUri.TYPE.SERIAL:
                //    c = new SERIALCommunicator<T>(frameWrapper);
                //    break;

            }

            return c;
        }
    }

    public abstract class CommunicatorBase<T> : IDisposable
    {
        public event DataReadyEventHandler DataReadyEvent;
        public delegate void DataReadyEventHandler(string ip, int port, long time, byte[] bytes, int offset, int length , string ID, ushort[] ipChunks);

        public delegate void ConnectionStateDelegate(string ID, ConnUri uri, bool connected);
        public event ConnectionStateDelegate ConnectionStateEvent;

        public delegate void DataRateDelegate(string ID, float Mbps);
        public event DataRateDelegate DataRateEvent;

        public enum STATE
        {
            RUNNING,
            STOP
        }
        public STATE State;

        public ConnUri CommsUri { get; protected set; }
        public ushort[] ipChunks = new ushort[4];
        public string ID { get; protected set; }

        public abstract void Init(ConnUri uri, bool persistent, string ID, int inactivityMS, int sendGAP = 0);
        public abstract void Start();
        public abstract Task Stop();
        public abstract void SendASync(byte[] bytes, int length);
        public abstract void SendSync(byte[] bytes, int offset, int length);
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

        public virtual void FireDataRateEvent(string ID, float dataRateMbps)
        {
            DataRateEvent?.Invoke(ID, dataRateMbps);
        }

        protected virtual void SetIPChunks(string _ip)
        {
            string[] chunks = _ip.Split('.');
            if (chunks.Length == 4)
                for (int i = 0; i < 4; i++)
                    ipChunks[i] = ushort.Parse(chunks[i]);
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
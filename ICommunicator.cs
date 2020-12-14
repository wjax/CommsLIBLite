using CommsLIBLite.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommsLIBLite
{

    public delegate void DataReadyEventHandler(string ip, int port, long time, byte[] bytes, int offset, int length, string ID, ushort[] ipChunks);
    public delegate void ConnectionStateDelegate(string ID, ConnUri uri, bool connected);
    public delegate void DataRateDelegate(string ID, float MbpsRX, float MbpsTX);

    /// <summary>
    /// ICommunicator interface
    /// </summary>
    public interface ICommunicator : IDisposable
    {
        /// <summary>
        /// Event containing data rate information about TX and RX Mbps at 1Hz
        /// </summary>
        event DataRateDelegate DataRateEvent;
        /// <summary>
        /// Event containing connection status
        /// </summary>
        event ConnectionStateDelegate ConnectionStateEvent;
        /// <summary>
        /// Event containing new incoming data
        /// </summary>
        event DataReadyEventHandler DataReadyEvent;

        string ID { get; set; }
        ushort[] IpChunks { get; }
        ConnUri CommsUri {get;}

        /// <summary>
        /// Initialize Communicator. Call once
        /// </summary>
        /// <param name="uri">ConnUri with link data</param>
        /// <param name="persistent">Auto reconnection?</param>
        /// <param name="ID">ID of this Communicator. It will be included in events</param>
        /// <param name="inactivityMS">Maximum period of time (in ms) without incoming data before declaring disconnection. 0 means do not apply</param>
        /// <param name="sendGAP">Minimum time between two Send operations</param>
        void Init(ConnUri uri, bool persistent, string ID, int inactivityMS, int sendGAP = 0);
        void Start();
        Task Stop();
        /// <summary>
        /// Enqueue data to be sent and exits
        /// </summary>
        /// <param name="bytes">Byte array to send</param>
        /// <param name="length">Length of valid data</param>
        void SendASync(byte[] bytes, int length);
        /// <summary>
        /// Block until data is sent
        /// </summary>
        /// <param name="bytes">Byte array to send</param>
        /// <param name="offset">Offset of valid data in the array</param>
        /// <param name="length">Length of valid data</param>
        /// <returns></returns>
        bool SendSync(byte[] bytes, int offset, int length);
    }
}

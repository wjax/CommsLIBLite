using CommsLIBLite.Base;
using System;
using System.Threading.Tasks;

namespace CommsLIBLite.Communications.FrameWrappers
{
    /// <summary>
    /// Base class of every Serializer/FrameWrapper
    /// </summary>
    /// <typeparam name="T">Data type to serialize To and From. It can be a base class from where others inherit</typeparam>
    public abstract class FrameWrapperBase<T>
    {
        // Delegate and event
        /// <summary>
        /// Delegate invoked by the event when new Frame/Object has been deserialized and is ready to consume
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="payload">Deserialized object</param>
        public delegate void FrameAvailableDelegate(string ID, T payload);
        public event FrameAvailableDelegate FrameAvailableEvent;

        private bool useThreadPool4Event;
        public string ID { get; private set; }

        private BlockingQueue<T> fireQueue;
        private Task fireTask;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_useThreadPool4Event">Use or not a Task to fire the FrameAvailableEvent</param>
        public FrameWrapperBase(bool _useThreadPool4Event)
        {
            if (_useThreadPool4Event)
            {
                fireQueue = new BlockingQueue<T>();
                fireTask = new Task(FireQueuedEventLoop, TaskCreationOptions.LongRunning);
                fireTask.Start();
            }
                
        }

        public void SetID(string _id)
        {
            ID = _id;
        }

        /// <summary>
        /// Used to add bytes to the deserializer. It is called internally by the ICommunicator.
        /// </summary>
        /// <param name="bytes">Byte array with incoming bytes</param>
        /// <param name="length">Length of the valid bytes</param>
        public abstract void AddBytes(byte[] bytes, int length);

        /// <summary>
        /// Start processing
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Stop processing
        /// </summary>
        public abstract void Stop();

        protected void FireEvent(T toFire)
        {
            if (useThreadPool4Event)
                fireQueue.Enqueue(toFire);
            else
                FrameAvailableEvent?.Invoke(ID, toFire);
        }

        private void FireQueuedEventLoop()
        {
            // TODO Stop
            while(true)
            {
                T toFire = fireQueue.Dequeue();
                FrameAvailableEvent?.Invoke(ID, toFire);
            }
        }

        public void UnsubscribeEventHandlers()
        {
            if (FrameAvailableEvent != null)
                foreach (var d in FrameAvailableEvent.GetInvocationList())
                    FrameAvailableEvent -= (d as FrameAvailableDelegate);
        }

        /// <summary>
        /// Serialize T to byte array
        /// </summary>
        /// <param name="data">T data</param>
        /// <param name="count">Length of valid bytes in the returned byte array</param>
        /// <returns>byte array containg the deserialized T</returns>
        public virtual byte[] Data2BytesSync(T data, out int count)
        {
            throw new NotImplementedException();
        }
    }
}

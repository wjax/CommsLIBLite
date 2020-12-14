namespace CommsLIBLite.Communications.FrameWrappers
{
    /// <summary>
    /// Child class of FrameWrapperBase used to create other FrameWrappers that use a sync AddBytes. AddBytes process deserialization in the same calling thread so if deserialization is very costly, it may stop your callig thread
    /// </summary>
    /// <typeparam name="T">Data type to serialize To and From. It can be a base class from where others inherit</typeparam>
    public abstract class SyncFrameWrapper<T> : FrameWrapperBase<T>
    {
        public SyncFrameWrapper(bool _useThreadPool4Event) : base(_useThreadPool4Event)
        {

        }

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

    }
}

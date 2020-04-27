# CommsLIBLite
CommsLIBLite is a sockets communication library specifically tailored for IoT and low resource CPUs. It uses **low allocations/high performance** while providing built-in serialization, framewrapping, autoreconnection and more.

## Features
* Low allocation / High performace
* Sockets
  * TCP and UDP client sockets
  * Autoreconnection if peer down
  * Minimum configurable send gap
  * Events:
    * Connection
    * Data rate
    * Data available
  * TCP Server
* FrameWrapping and Serialization
  * Sync and Async serializer/deserializer base clases
  * Base classes ready to be inherited to create custom framewrappers/serializers
  * Built-in `MessagePack` and `protobuf-net` serializers ready to use

## Basic usage
This library has two types of objects, `ICommunicator` (the communication object) and `FrameWrapperBase<T>` (the serializer/framewrapper).
An `ICommunicator` can be used with or without a Serializer/FrameWrapper. Just pass null and you are done.

### Complete guide step by step
1. Create a Serializer/FrameWrapper if needed. We use here the builtin protobuf one with a serialization message called ``MessageBase``.
    ```csharp
    FrameWrapperBase<MessageBase> frameWrapper = new ProtobufFrameWrapper<MessageBase>(true);
    ```
2. Create ICommunicator from Factory based on the ConnUri, provided Serializer/FrameWrapper and whether CircularBuffer will be used or not (more on this later)
    ``` csharp
    var connUri = new ConnUri("tcp://127.0.0.1:8080");
    ICommunicator comm = CommunicatorFactory.CreateCommunicator<MessageBase>(connUri, frameWrapper, false);
    ```
3. Initialize ICommunicator
    ``` csharp
    // Init with Autoreconnection, no Inactivity detection and no send gap
    comm.Init(connUri, true, "MyCommunicator", 0, 0);
    ```
4. Subscribe to events
    ``` csharp
    // ICommunicator events
    comm.ConnectionStateEvent += OnConnection;
    comm.DataRateEvent += OnDataRateEvent;
    comm.DataReadyEvent += OnRawData;
    // Serializer/FrameWrapper event (only if using a Serializer/FrameWrapper)
    frameWrapper.FrameAvailableEvent += OnFrame;
    ```
5. Start both Serializer/FrameWrapper and ICommunicator
    ``` csharp
    frameWrapper.Start();
    comm.Start();
    ```
6. Send. Both SendSync (blocking) and SendASync(non_blocking fire and forget)
    ``` csharp
    // Sync
    comm.SendSync(byte[] bytes, int offset, int length);
    // ASync
    comm.SendASync(byte[] bytes, int length);
    ```

### Simple use without serialization/framewrapper
1. Create ICommunicator from Factory based on the ConnUri, provided Serializer/FrameWrapper and whether CircularBuffer will be used or not (more on this later)
    ``` csharp
    var connUri = new ConnUri("tcp://127.0.0.1:8080");
    ICommunicator comm = CommunicatorFactory.CreateCommunicator<object>(connUri, null, false);
    ```
2. Initialize ICommunicator
    ``` csharp
    // Init with Autoreconnection, no Inactivity detection and no send gap
    comm.Init(connUri, true, "MyCommunicator", 0, 0);
    ```
4. Subscribe to events
    ``` csharp
    // ICommunicator events
    comm.ConnectionStateEvent += OnConnection;
    comm.DataRateEvent += OnDataRateEvent;
    comm.DataReadyEvent += OnRawData;
    ```
5. Start ICommunicator
    ``` csharp
    comm.Start();
    ```

## Advanced usage
### Options 
`ICommunicator` and `FrameWrapperBase` have several paremeters that can be used to fit different situations.

#### CircularBuffer (for sending bytes)
`ICommunicator` can send bytes to underlying socket in **two flavors**, blocking and non-blocking.

For the __blocking__ one, it will just call `Socket.Send` and block until bytes are sent.

In the __non blocking__ flavour, sending of bytes is done by an internal `Task` that waits for data to be ready. There are two options here that are specified at `ICommunicator` creation `CommunicatorFactory.CreateCommunicator<T>(ConnUri connUri, FrameWrapperBase<T> frameWrapper, bool circular = false)`

* _CircularBuffer_ uses a custom CircularBuffer with SemaphoreSlim to keep "data frames" until they are consumed/sent by the sending task.
    
    Using this option, calling `ICommunicator.SendAsync` will copy source bytes to the internal CircularBuffer (retaining length info) and signal the sender task.
* _BlockingQueue_ uses a BlockingQueue to store the source byte array and signal the sender task. 

It should be noted that CircularBuffer flavour is slower but do not care about the source byte array lifespan. On the other side, BlockingQueue is faster but attention should be paid to the lifespan of the source buffer.
CircularBuffer allows the caller to reuse same byte array allowing allocation free code.

It is highly recommended tu use _CircularBuffer_ if working with `SendASync`. Using _BlockingQueue_ with `SendASync` should only be done if you control the lifetime of your byte array.
If doing `SendSync`, this option is ignored.

#### ICommunicator Init
Init method must be used only once. It configures certain internal parameters.
Method signature is: 
```csharp
void Init(ConnUri uri, bool persistent, string ID, int inactivityMS, int sendGAP = 0);
```

* _ConnUri uri_ is the link information including protocol, peer ip, port etc
* _bool persistent_ indicates whether the ICommunicator will attempt to reconnect if connection is broken. Timeout is hardcoded to 5s
* _string ID_ is the ID that will be included in all events to identify the event source
* _int inactivityMS_ defines the maximum period of time (in ms) without incoming data before declaring disconnection. 0 means do not apply
* _int sendGAP_ defines the minimum time between two Send operations

### Serializers/FrameWrappers
Communication is usually done by means of Messages. Peers exchange some kind of messages than can be mapped to objects.
`ICommunicator` can use a provided Serializer/FrameWrapper to Decode incoming bytes to form a Message and the other way around, it can encode a Message into a byte array.

Every Serializer/FrameWrapper must inherit from `FrameWrapperBase<T>`, being T the Message. Message is usually a base class.
The key part is the method `AddBytes` which is called with every incoming data.

There are two derived Serializer/FrameWrapper from where to inherit to create custom ones.
* `SyncFrameWrapper` provides a blocking `AddBytes`. So Deserialization is done in the same Thread that receives data from the underlying socket
* `AsyncFrameWrapper` provides a non-blocking `AddBytes` by using a Queue and a Task







using CommsLIBLite.Communications.FrameWrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommsLIBLite.FrameWrappers.JSON
{
    /// <summary>
    /// Json builtin serializer using SyncFrameWrapper as base class
    /// </summary>
    /// <typeparam name="T">Data type to serialize To and From. It can be a base class from where others inherit</typeparam>
    public class JsonFrameWrapper<T> : SyncFrameWrapper<T>
    {
        public JsonFrameWrapper() : base(false)
        {

        }

        public override void AddBytes(byte[] bytes, int length)
        {
            string t = Encoding.UTF8.GetString(bytes, 0, length);
            T value = JsonSerializer.Deserialize<T>(t);

            FireEvent(value);
        }

        public override byte[] Data2BytesSync(T data, out int count)
        {
            var v = JsonSerializer.SerializeToUtf8Bytes(data);
            count = v.Length;

            return v;
        }
    }
}

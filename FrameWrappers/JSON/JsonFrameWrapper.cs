using CommsLIBLite.Communications.FrameWrappers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommsLIBLite.FrameWrappers.JSON
{
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

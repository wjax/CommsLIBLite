using System;
using System.Collections.Generic;
using System.Text;

namespace CommsLIBLite.Base
{
    interface ICommsQueue
    {
        int Put(byte[] buff, int length);
        int Take(ref byte[] buff, int offset);
        void Reset();
    }
}

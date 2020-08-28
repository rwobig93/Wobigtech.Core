using System;
using System.Collections.Generic;
using System.Text;

namespace Wobigtech.Core.Enums
{
    public enum NatCommType
    {
        JoinReq = 0x82648,
        JoinResp = 0x48634,
        CmdReq = 0x67823,
        CmdReqBulk = 0x64346,
        CmdResp = 0x75648,
        Health = 0x69435,
        FileUp = 0x68423
    }
}

using NATS.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Wobigtech.Core.Enums;

namespace Wobigtech.Core.Comm
{
    public static class NatComm
    {
        public static byte[] NatMsgSend(NatCommType msgType, string msg)
        {
            return Encoding.ASCII.GetBytes($"{msgType};{msg}");
        }

        public static byte[] NatMsgSend(NatCommType msgType, object obj)
        {
            return Encoding.ASCII.GetBytes(NatDtoMsg.ConvertToString(msgType, obj));
        }

        public static NatDtoMsg NatMsgReceive(byte[] msg)
        {
            return NatDtoMsg.GetFromString(Encoding.ASCII.GetString(msg));
        }
    }
}

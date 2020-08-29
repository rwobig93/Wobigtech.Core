using Microsoft.AspNetCore.StaticFiles;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wobigtech.Core.Tools;

namespace Wobigtech.Core
{
    public static class AppConsts
    {
        public static ConnectionFactory NatFactory { get; set; }
        public static IConnection NatConn { get; set; }
        public static string PathCerts { get { return Path.Combine(OSDynamic.GetStoragePath(), "Certs"); } }
    }
}

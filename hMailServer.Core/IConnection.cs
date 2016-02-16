﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hMailServer.Core
{
    public interface IConnection
    {
        void SetTimeout(TimeSpan timeout);

        Task<string> ReadStringUntil(string delimiters);
        Task<MemoryStream> Read();
        Task WriteString(string data);
        Task SslHandshakeAsServer(X509Certificate2 certificate);
    }
}

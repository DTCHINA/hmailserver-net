﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using hMailServer.Core.Logging;
using hMailServer.Core.Protocols.SMTP;

namespace hMailServer.Core.Protocols.POP3
{
    public class Pop3ServerSession : ISession
    {
        private IConnection _connection;
        private readonly IPop3ServerCommandHandler _commandHandler;
        private readonly Pop3ServerSessionState _state = new Pop3ServerSessionState();
        private readonly Pop3ServerSessionConfiguration _configuration;
        private const int DataTransferMemoryBufferMaxSize = 1024*30;
        private ILog _log;

        public Pop3ServerSession(IPop3ServerCommandHandler commandHandler, ILog log, Pop3ServerSessionConfiguration configuration)
        {
            _commandHandler = commandHandler;
            _configuration = configuration;
            _log = log;
        }

        public async Task HandleConnection(IConnection connection)
        {
            _connection = connection;

            try
            {
                await SendBanner();

                while (true)
                {
                    _connection.SetTimeout(_configuration.CommandTimeout);

                    var data = await ReadUntilNewLine(_connection);

                    _log.LogInfo(new LogEvent()
                        {
                            Message = data,
                            RemoteEndpoint = _connection.RemoteEndpoint,
                            SessionId = _connection.SessionId
                        });

                    var command = POP3.CommandParser.ParseCommand(data);

                    if (!_state.IsCommandValid(command))
                    {
                        await SendCommandResult(new Pop3CommandResult(false, "Invalid command in current state"));
                        continue;
                    }

                    switch (command)
                    {
                        case Pop3Command.Quit:
                            await HandleQuit();
                            throw new DisconnectedException();
                    }
                }
            }
            catch (DisconnectedException)
            {
                // Connection gone.
            }
        }

        private async Task HandleStls()
        {
            throw new NotImplementedException();

            //await SendCommandResult(new SmtpCommandResult(220, "220 Go ahead"));

            await _connection.SslHandshakeAsServer(_configuration.SslCertificate);

            _commandHandler.HandleRset();

            _state.Reset();
        }

        private async Task HandleRset()
        {
            await SendCommandResult(_commandHandler.HandleRset());
        }

        private async Task HandleQuit()
        {
            await SendCommandResult(_commandHandler.HandleQuit());
        }

        private Task SendBanner()
        {
            string banner = string.Format(CultureInfo.InvariantCulture, "220 {0} ESMTP", Environment.MachineName);

            return SendLine(banner);
        }

        public Task<string> ReadUntilNewLine(IConnection connection)
        {
            return connection.ReadStringUntil("\r\n");
        }

        private async Task SendCommandResult(Pop3CommandResult commandResult)
        {
            if (commandResult == null)
                throw new ArgumentException("commandResult");

            string result = commandResult.Success ? "+OK" : "+ERR";

            var message = $"{result} {commandResult.Message}";
            await SendLine(message);
        }

        private Task SendLine(string message)
        {
            _log.LogInfo(new LogEvent()
            {
                Message = message,
                RemoteEndpoint = _connection.RemoteEndpoint,
                SessionId = _connection.SessionId
            });

            return _connection.WriteString(message + "\r\n");
        }

        
    }

}
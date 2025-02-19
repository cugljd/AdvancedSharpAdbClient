﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    internal class TracingAdbSocket(EndPoint endPoint) : AdbSocket(endPoint), IDummyAdbSocket
    {
        public bool DoDispose { get; set; }

        public Queue<AdbResponse> Responses { get; } = new Queue<AdbResponse>();

        public Queue<string> ResponseMessages { get; } = new Queue<string>();

        public Queue<SyncCommand> SyncResponses { get; } = new Queue<SyncCommand>();

        public Queue<byte[]> SyncDataReceived { get; } = new Queue<byte[]>();

        public Queue<byte[]> SyncDataSent { get; } = new Queue<byte[]>();

        public List<string> Requests { get; } = [];

        public List<(SyncCommand, string)> SyncRequests { get; } = [];

        public Queue<Stream> ShellStreams { get; } = new Queue<Stream>();

        public bool DidReconnect { get; private set; }

        public bool WaitForNewData { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (DoDispose)
            {
                base.Dispose(disposing);
            }
        }

        public override int Read(byte[] data, int length)
        {
            StackTrace trace = new();

            int read = base.Read(data, length);

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncDataReceived.Enqueue(data[..length]);
            }

            return read;
        }

        public override int Read(byte[] data, int offset, int length)
        {
            StackTrace trace = new();

            int read = base.Read(data, offset, length);

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncDataReceived.Enqueue(data.AsSpan(offset, length).ToArray());
            }

            return read;
        }

        public override int Read(Span<byte> data)
        {
            StackTrace trace = new();

            int read = base.Read(data);

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncDataReceived.Enqueue(data.ToArray());
            }

            return read;
        }

        public override AdbResponse ReadAdbResponse()
        {
            Exception exception = null;
            AdbResponse response;

            try
            {
                response = base.ReadAdbResponse();
            }
            catch (AdbException ex)
            {
                exception = ex;
                response = ex.Response;
            }

            Responses.Enqueue(response);

            return exception != null ? throw exception : response;
        }

        public override Stream GetShellStream()
        {
            StackTrace trace = new();

            Stream stream = base.GetShellStream();

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                ShellStreams.Enqueue(stream);
            }

            return stream;
        }

        public override string ReadString()
        {
            string value = base.ReadString();
            ResponseMessages.Enqueue(value);
            return value;
        }

        public override string ReadSyncString()
        {
            string value = base.ReadSyncString();
            ResponseMessages.Enqueue(value);
            return value;
        }

        public override async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            string value = await base.ReadStringAsync(cancellationToken);
            ResponseMessages.Enqueue(value);
            return value;
        }

        public override void SendAdbRequest(string request)
        {
            Requests.Add(request);
            base.SendAdbRequest(request);
        }

        public override void SendSyncRequest(SyncCommand command, string path)
        {
            SyncRequests.Add((command, path));
            base.SendSyncRequest(command, path);
        }

        public override void SendSyncRequest(SyncCommand command, int length)
        {
            StackTrace trace = new();

            if (trace != null && trace.GetFrames()[1].GetMethod().DeclaringType != typeof(AdbSocket))
            {
                SyncRequests.Add((command, length.ToString()));
            }

            base.SendSyncRequest(command, length);
        }

        public override SyncCommand ReadSyncResponse()
        {
            SyncCommand response = base.ReadSyncResponse();
            SyncResponses.Enqueue(response);
            return response;
        }

        public override void Reconnect(bool isForce = false)
        {
            base.Reconnect();
            DidReconnect = true;
        }
    }
}

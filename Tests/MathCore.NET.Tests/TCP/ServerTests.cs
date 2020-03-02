using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MathCore.NET.TCP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathCore.NET.Tests.TCP
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void Creating()
        {
            const int server_port = 18080;
            using var server = new Server(server_port);

            Assert.That.Value(server)
               .Where(s => s.Port).Check(port => port.IsEqual(server_port))
               .Where(s => s.AddressType).Check(address => address.IsEqual(IPAddress.Any))
               .Where(s => s.Enabled).Check(state => state.IsFalse());
        }

        [TestMethod]
        public void StartingServer()
        {
            const int server_port = 18080;
            using var server = new Server(server_port);

            var server_started = false;
            server.Started += delegate { server_started = true; };

            server.Start();
            Assert.That.Value(server.Enabled).IsTrue();
            Assert.That.Value(server_started).IsTrue();
        }

        [TestMethod]
        public void ServerStartStop()
        {
            const int server_port = 18080;
            using var server = new Server(server_port);

            var server_started = false;
            server.Started += delegate { server_started = true; };
            server.Stopped += delegate { server_started = false; };

            server.Start();

            Assert.That.Value(server.Enabled).IsTrue();
            Assert.That.Value(server_started).IsTrue();

            server.Stop();

            Assert.That.Value(server.Enabled).IsFalse();
            Assert.That.Value(server_started).IsFalse();
        }

        [TestMethod]
        public async Task ServerReceiveClientConnection()
        {
            const int server_port = 18080;
            const string host = "127.0.0.1";
            using var server = new Server(server_port);

            var client_connection = new TaskCompletionSource<Client>();

            server.ClientConnected += (s, e) => client_connection.SetResult(e.Client);
            server.Start();

            using var client = new Client(host, server_port);

            client.Start();

            var cancellation_timeout = new CancellationTokenSource(100);
            cancellation_timeout.Token.Register(() => client_connection.TrySetCanceled());

            var connected_client = await client_connection.Task.ConfigureAwait(false);

            Assert.That.Value(connected_client).AsNotNull()
               .Where(c => c.Host).Check(h => h.IsEqual(host))
               .Where(c => c.Enabled).Check(state => state.IsTrue());

            client.Stop();

            await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);

            Assert.That.Value(client.Enabled).IsFalse();
            Assert.That.Value(connected_client.Enabled).IsFalse();
        }

        [TestMethod]
        public async Task ReceivingDataFromClient()
        {
            const int server_port = 18080;
            const string host = "127.0.0.1";
            using var server = new Server(server_port);

            var tcs = new TaskCompletionSource<string>();
            server.DataReceived += (s, e) => tcs.TrySetResult(e.ClientData.Message);

            var cancellation = new CancellationTokenSource(100);
            cancellation.Token.Register(() => tcs.TrySetCanceled());

            server.Start();

            var client = new Client(host, server_port);
            client.Start();

            const string message = "Hello World!";

            client.Send(message);

            var received_message = await tcs.Task.ConfigureAwait(false);

            Assert.That.Value(received_message).IsEqual(message);
        }
    }
}

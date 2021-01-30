using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Fork.Logic.Model;

namespace Fork.Logic.Query
{
    public class Query
    {
        public Query(string serverIp, int serverPort)
        {
            this.serverIp = "localhost";
            this.serverPort = serverPort;

            //Setup connection
            //TODO may throw socketException
            serverIpAddresses = Dns.GetHostAddresses(serverIp);
            receivePoint = new IPEndPoint(IPAddress.Any, serverPort);
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), serverPort);
            udpClient = new UdpClient();
        }

        protected Query()
        {
        }

        public bool ServerAvailable()
        {
            try
            {
                Ping p = new Ping();
                // Sending ping to server
                PingReply reply = p.Send("localhost", 3000);

                // If not reply, server is offline
                if (reply == null) return false;

                // Get time of response
                responseTime = reply.RoundtripTime;

                // Return if the server is OK or not
                return reply.Status == IPStatus.Success;
            }
            catch (Exception e)
            {
                Console.WriteLine("Ping to server " + serverIp + ":" + serverPort + " failed: " + e.Message);
                return false;
            }
        }

        public FullStats FullServerStats()
        {
            int handshake = Handshake();
            byte[] fullStatsBytes = FullStats(handshake);
            FullStats fullStats = new FullStats(fullStatsBytes);
            return fullStats;
        }

        private byte[] ConnectToServer(byte[] inputBytes)
        {
            // Ping minecraft server
            if (!ServerAvailable()) throw new SocketException();

            // Connecting with minecraft server
            udpClient.Connect(endPoint);

            // Sending data
            udpClient.Send(inputBytes, inputBytes.Length);

            // Cleaning data of output
            outputBytes = new byte[] { };
            messageReceived = false;

            // Set datetime of timeout
            DateTime endTimeout = DateTime.Now.AddMilliseconds(timeoutPing);

            // Receiving data async
            try
            {
                udpClient.BeginReceive(ReceiveCallback, receivePoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Query encountered an excepption while connecting to server\nThe server query is probably disabled or the server stopped!\n" +
                    e.Message);
            }

            while (!messageReceived)
            {
                Thread.Sleep(100);
                if (DateTime.Now > endTimeout)
                    throw new TimeoutException("The server does not respond at specified port");
            }

            // Receiving data sync - Block application if not receive data of port
            // outputBytes = udpClient.Receive(ref receivePoint);

            // Returning data
            return outputBytes;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            outputBytes = udpClient.EndReceive(ar, ref receivePoint);
            messageReceived = true;
        }

        private int Handshake()
        {
            // Declare vars
            MemoryStream stream = new MemoryStream();

            // Writing stream bytes
            stream.WriteByte(0xFE); // Magic
            stream.WriteByte(0xFD); // Magic
            stream.WriteByte(0x09); // Type
            stream.WriteByte(0x01); // Session
            stream.WriteByte(0x01); // Session
            stream.WriteByte(0x01); // Session
            stream.WriteByte(0x01); // Session

            // Preparing byte array
            byte[] sendme = stream.ToArray();

            // Closing stream for save resources
            stream.Close();

            // Returned data
            byte[] receivedBytes = ConnectToServer(sendme);

            string number = "";

            for (int i = 0; i < receivedBytes.Length; i++)
                if (i > 4 && receivedBytes[i] != 0x00)
                    number += (char) receivedBytes[i];

            // Return token
            return int.Parse(number);
        }

        private byte[] FullStats(int number)
        {
            // Declare vars
            MemoryStream stream = new MemoryStream();
            byte[] numberbytes = BitConverter.GetBytes(number).Reverse().ToArray();

            // Writing stream bytes
            stream.WriteByte(0xFE); // Magic
            stream.WriteByte(0xFD); // Magic
            stream.WriteByte(0x00); // Type
            stream.WriteByte(0x01); // Session
            stream.WriteByte(0x01); // Session
            stream.WriteByte(0x01); // Session
            stream.WriteByte(0x01); // Session
            stream.Write(numberbytes, 0, 4); // Challenge
            stream.WriteByte(0x00); // Padding
            stream.WriteByte(0x00); // Padding
            stream.WriteByte(0x00); // Padding
            stream.WriteByte(0x00); // Padding

            // Preparing byte array
            byte[] sendme = stream.ToArray();

            // Closing stream for save resources
            stream.Close();

            // Return data
            return ConnectToServer(sendme);
        }

        #region Properties

        // Public properties
        public static long responseTime { get; set; }
        public UdpClient udpClient { get; set; } = new();

        // Private properties
        private string serverIp { get; }
        private int serverPort { get; }
        private int timeoutPing { get; } = 3000;
        private bool messageReceived { get; set; }
        private byte[] outputBytes { get; set; }

        #endregion

        #region Variables

        private IPEndPoint receivePoint;
        private readonly IPEndPoint endPoint;
        private static IPAddress[] serverIpAddresses;

        #endregion
    }
}
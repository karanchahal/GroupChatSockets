using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerData;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace Server
{
    class Server
    {
        static Socket listenerSocket;
        static List<ClientData> _clients;


        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server on " + Packet.GetIPAddress());
            try
            {
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clients = new List<ClientData>();

                IPEndPoint ip = new IPEndPoint(IPAddress.Parse(Packet.GetIPAddress()), 4242);


                listenerSocket.Bind(ip);

                Thread listenThread = new Thread(ListenThread);

                listenThread.Start();



            }
            catch
            {
                Console.WriteLine("Could NOT DO IT");
            }

        }

        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);
                _clients.Add(new ClientData(listenerSocket.Accept()));

            }
        }
        
        public static void Data_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;

            byte[] Buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    Buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        //handle data

                        Packet packet = new Packet(Buffer);
                        DataManager(packet);
                    }


                }
                catch(SocketException ex)
                {
                    Console.WriteLine("A client disconnnected");
                }
            }

        }

        
        public static void DataManager(Packet p)
        {
            switch(p.packetType)
            {
                case PacketType.Chat:
                    foreach(ClientData c in _clients)
                    {
                        c.clientSocket.Send(p.ToBytes());
                    }
                    break;
            }
        }
        // data manager

    }

    class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread; //
        public string id;
        

        public ClientData()
        {
            id = Guid.NewGuid().ToString();
            clientThread =new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            SendRegisterationPacket();
        }

        public ClientData(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            id = Guid.NewGuid().ToString();
            clientThread =new Thread(Server.Data_IN);
            clientThread.Start(clientSocket);
            SendRegisterationPacket();
        }

        public void SendRegisterationPacket()
        {
            Packet p = new Packet(PacketType.Registeration, "server");
            p.Gdata.Add(id);
            clientSocket.Send(p.ToBytes());
        }
    }
}

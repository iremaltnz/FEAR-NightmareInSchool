using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Client : MonoBehaviour
    {
        public static Client instance;
        public static int dataBufferSize = 4096;

        public string ip = "127.0.0.1";
        public int port = 26950;
        public int myId = 0;
        public TCP tcp;
        public UDP udp;

        private delegate void PacketHnadler(Packet packet);
        private static Dictionary<int, PacketHnadler> packetHandlers;

        private bool isConnected = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

            }
            else if (instance != this)
            {
                Debug.Log("Zaten var");
                Destroy(this);
            }



        }

        public void Start()
        {
            tcp = new TCP();
            udp = new UDP();

        }

        public void OnApplicationQuit()
        {
            Disconnect();
        }

        public void ConnectToServer()
        {
            InitializeClientData();

            isConnected = true;

            tcp.Connect();
        }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream stream;

            private Packet receiverData;
            private byte[] receiveBuffer;

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallBack, null);
            }

            private void ConnectCallBack(IAsyncResult _result)
            {
                socket.EndConnect(_result);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receiverData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiverCallBack, null);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Yollama Başarısız  Tcp : {e}");
                }
            }

            private void ReceiverCallBack(IAsyncResult _result)
            {
                try
                {
                    int _byteLenght = stream.EndRead(_result);
                    if (_byteLenght <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLenght];
                    Array.Copy(receiveBuffer, _data, _byteLenght);

                    receiverData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiverCallBack, null);
                }
                catch (Exception)
                {

                    Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLenght = 0;
                receiverData.SetBytes(data);

                if (receiverData.UnreadLength() >= 4)
                {
                    packetLenght = receiverData.ReadInt();

                    if (packetLenght <= 0)
                    {
                        return true;
                    }
                }

                while (packetLenght > 0 && packetLenght <= receiverData.UnreadLength())
                {
                    byte[] _packetBytes = receiverData.ReadBytes(packetLenght);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        Packet packet = new Packet(_packetBytes);
                        try
                        {
                            int packetId = packet.ReadInt();
                            packetHandlers[packetId](packet);

                        }
                        catch (Exception e)
                        {

                            throw e;
                        }

                    });

                    packetLenght = 0;
                    if (receiverData.UnreadLength() >= 4)
                    {
                        packetLenght = receiverData.ReadInt();
                        if (packetLenght <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLenght <= 1)
                {
                    return true;
                }

                return false;
            }

            private void Disconnect()
            {
                instance.Disconnect();

                stream = null;
                receiveBuffer = null;
                receiverData = null;
                socket = null;
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            public void Connect(int _localPort)
            {
                socket = new UdpClient(_localPort);
                socket.Connect(endPoint);

                socket.BeginReceive(ReceiveCallBack, null);

                Packet packet = new Packet();
                SendData(packet);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    _packet.InsertInt(instance.myId);
                    if (socket != null)
                    {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {

                    Debug.Log($"Udp göndermesi başarısız..:{e}");
                }
            }

            private void ReceiveCallBack(IAsyncResult _result)
            {
                try
                {
                    byte[] _data = socket.EndReceive(_result, ref endPoint);
                    socket.BeginReceive(ReceiveCallBack, null);

                    if (_data.Length < 4)
                    {
                        instance.Disconnect();
                        return;
                    }

                    HandleData(_data);
                }
                catch (Exception)
                {

                    Disconnect();
                }
            }

            private void HandleData(byte[] _data)
            {


                Packet packet = new Packet(_data);
                int packetLenght = packet.ReadInt();
                _data = packet.ReadBytes(packetLenght);

                ThreadManager.ExecuteOnMainThread(() =>
                {

                    Packet _packet = new Packet(_data);

                    int packetId = _packet.ReadInt();
                    packetHandlers[packetId](_packet);
                });


            }

            public void Disconnect()
            {

                instance.Disconnect();

                endPoint = null;
                socket = null;
            }
        }
        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHnadler>()
        {
            {(int) ServerPackets.welcome,ClientHandle.Welcome },

                {(int) ServerPackets.spawnPlayer,ClientHandle.SpawnPlayer },
                 { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
        };

            Debug.Log("Inıtalize packets");
        }

        private void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();

                Debug.Log("Server ile bağlantı kesildi..");
            }
        }
    }
}

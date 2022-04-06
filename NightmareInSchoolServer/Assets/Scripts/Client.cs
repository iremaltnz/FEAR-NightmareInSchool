using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client 
{

    public static int dataBufferSize = 4096;
    public int id;
    public TCP tcp;
    public UDP udp;
    public Player player;

    public Client(int _clientId)
    {
        id = _clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
    }
    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;

        public Packet receiverData;
        private byte[] receiveBuffer;

        private readonly int id;
        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = socket.GetStream();

            receiverData = new Packet();
            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiverCallBack, null);

            ServerSend.Welcome(id, "Welcome Server");

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
                Debug.Log($"Yollama Başarısız  oyuncu :  {id}  Tcp : {e}");
            }
        }

        public void ReceiverCallBack(IAsyncResult _result)
        {
            try
            {
                int _byteLenght = stream.EndRead(_result);
                if (_byteLenght <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLenght];
                Array.Copy(receiveBuffer, _data, _byteLenght);
                receiverData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiverCallBack, null);
            }
            catch (Exception)
            {
                Server.clients[id].Disconnect();

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
                        Server.packetHandlers[packetId](id, packet);

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

                if (packetLenght <= 1)
                {
                    return true;
                }

            }

            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receiverData = null;
            receiveBuffer = null;
            socket = null;
        }

    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
            // ServerSend.UdpTest(id);
        }

        public void SendData(Packet packet)
        {
            Server.SendUdpData(endPoint, packet);
        }

        public void HandleData(Packet packetData)
        {
            int packetLenght = packetData.ReadInt();
            byte[] packetBytes = packetData.ReadBytes(packetLenght);

            ThreadManager.ExecuteOnMainThread(() =>
            {

                Packet packet = new Packet(packetBytes);

                int packetId = packet.ReadInt();
                Server.packetHandlers[packetId](id, packet);
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }

    }

    public void SendIntoGame(string _playerName)
    {
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initalize(id,_playerName);

        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.id != id)
                {
                    ServerSend.SpawnPlayer(id, _client.player);
                }
            }
        }


        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.SpawnPlayer(_client.id, player);
            }
        }
    }

    public void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} ile bağlantı kesildi.");

        ThreadManager.ExecuteOnMainThread(() =>
        {
            UnityEngine.Object.Destroy(player.gameObject);
            player = null;
        });

     
        tcp.Disconnect();
        udp.Disconnect();
    }
}

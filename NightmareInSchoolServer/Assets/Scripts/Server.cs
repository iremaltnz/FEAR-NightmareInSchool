using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server 
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;
    public static TcpListener tcpListener { get; set; }
    public static UdpClient udpListener;

    public static void Start(int _maxPlayers, int _port)
    {
        MaxPlayers = _maxPlayers;
        Port = _port;

        Console.WriteLine("Server Başlatılıyor");
        InitalizeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallBack), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UdpReceiveCallBack, null);


        Console.WriteLine($"Server start {Port}.");

    }

    private static void TcpConnectCallBack(IAsyncResult _result)
    {

        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallBack), null);
        Console.WriteLine($"Server'a bağlanılıyor {_client.Client.RemoteEndPoint}");

        for (int i = 1; i < MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Console.WriteLine("Server dolu");
    }

    private static void UdpReceiveCallBack(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UdpReceiveCallBack, null);

            if (_data.Length < 4)
            {

                return;
            }

            Packet packet = new Packet(_data);

            int _clientId = packet.ReadInt();

            if (_clientId == 0)
            {
                return;
            }

            if (clients[_clientId].udp.endPoint == null)
            {
                clients[_clientId].udp.Connect(_clientEndPoint);
                return;
            }

            if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
            {
                clients[_clientId].udp.HandleData(packet);
            }
        }
        catch (Exception e)
        {

            Console.WriteLine($"Udp verileri alınırken hata oluştu :{e}");
        }
    }

    public static void SendUdpData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception e)
        {

            Console.WriteLine($"Udp veri yollama başarısız : {e}");
        }
    }
    private static void InitalizeServerData()
    {
        for (int i = 1; i < MaxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived,ServerHandle.WelcomeReceieved },

                  {(int)ClientPackets.playerMovement,ServerHandle.PlayerMovement }
            };

        Console.WriteLine("Inıtalize packet");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend 
{
    public static void SendTcpData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    public static void SendUdpData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    public static void SendTcpDataToAll(Packet _packet)
    {
        _packet.WriteLength();

        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }



    public static void SendTcpDataToAll(int _exeptClient, Packet _packet)
    {
        _packet.WriteLength();

        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exeptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }

        }
    }

    public static void SendUdpDataToAll(Packet _packet)
    {
        _packet.WriteLength();

        for (int i = 1; i <= 5; i++)
        {

            Server.clients[i].udp.SendData(_packet);
        }


    }



    public static void SendUdpDataToAll(int _exeptClient, Packet _packet)
    {
        _packet.WriteLength();

        for (int i = 1; i <= 5; i++)
        {
            if (i != _exeptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }

        }
    }
    #region Packets
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTcpData(_toClient, _packet);
        }
    }



    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTcpData(_toClient, _packet);
        }
    }

    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUdpDataToAll(_packet);
        }
    }


    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUdpDataToAll(_player.id, _packet);
        }
    }

    #endregion
}

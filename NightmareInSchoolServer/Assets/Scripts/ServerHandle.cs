using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceieved(int _fromClient, Packet packet)
    {
        int _clientIdCheck = packet.ReadInt();
        string _userName = packet.ReadString();

      Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected succesfuly and is now player {_fromClient} .");

        if (_fromClient != _clientIdCheck)
        {
          Debug.Log($"Oyuncu \"{_userName}\"(ID : {_fromClient} ) has assuned the wrong client ID ({_clientIdCheck})!!");
        }

        Server.clients[_fromClient].SendIntoGame(_userName);
    }




    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }
}
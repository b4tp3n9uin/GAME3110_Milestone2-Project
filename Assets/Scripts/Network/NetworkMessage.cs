﻿using System;
using UnityEngine;

namespace NetworkMessages
{
    public enum Commands
    {
        NEW_CLIENT,
        UPDATE,
        CLIENT_DROPPED,
        CLIENT_LIST,
        OWN_ID,
        INPUT
    }

    [Serializable]
    public class NetworkHeader
    {
        public Commands cmd;
    }

    [Serializable]
    public class Player
    {
        public string id;
        [Serializable]
        public struct receivedColor
        {
            public float R;
            public float G;
            public float B;
        }
        public receivedColor color;
        public Vector3 position;
        public bool flipX;
        public float playerHP = 100;

        public Player()
        {
            id = "-1";
        }
        public Player(Client c)
        {
            id = c.id;
            color = c.color;
            position = c.position;
            flipX = c.flipX;
        }
        public override string ToString()
        {
            string result = "Player : \n";
            result += "id : " + id + "\n";
            result += "position : " + position.ToString() + "\n";

            return result;
        }
    }

    public class Client : Player
    {
        public float interval;
        public override string ToString()
        {
            string result = base.ToString();
            result += "interval : " + interval + "\n";
            return result;
        }
    }

    [Serializable]
    public class NewPlayer : NetworkHeader
    {
        public Player player;

        public NewPlayer(Client c)
        {
            cmd = Commands.NEW_CLIENT;
            player = new Player(c);
        }
    }
    [Serializable]
    public class ConnectedPlayer : NetworkHeader
    {
        public Player[] connect;

        public ConnectedPlayer(System.Collections.Generic.List<Client> clients)
        {
            cmd = Commands.CLIENT_LIST;
            connect = new Player[clients.Count];
            for (int i = 0; i < clients.Count; i++)
            {
                connect[i] = new Player(clients[i]);

                if (i == 1)
                    connect[i].position = new Vector3(5, 0, 0);
                else
                    connect[i].position = new Vector3(14, 0, 0);
            }
        }
    }
    [Serializable]
    public class DisconnectedPlayer : NetworkHeader
    {
        public Player[] disconnect;
        public DisconnectedPlayer(System.Collections.Generic.List<Client> clients)
        {
            cmd = Commands.CLIENT_DROPPED;
            disconnect = new Player[clients.Count];
            for (int i = 0; i < clients.Count; i++)
            {
                disconnect[i] = new Player(clients[i]);
            }
        }
    }
    [Serializable]
    public class UpdatedPlayer : NetworkHeader
    {
        public Player[] update;
        public UpdatedPlayer(System.Collections.Generic.List<Client> clients)
        {
            cmd = Commands.UPDATE;
            update = new Player[clients.Count];
            for (int i = 0; i < clients.Count; i++)
            {
                update[i] = new Player(clients[i]);
            }
        }
    }

    [Serializable]
    public class PlayerInput : NetworkHeader
    {
        public Vector3 input;
        public bool direction;
        public PlayerInput()
        {
            cmd = Commands.INPUT;
        }
    }
}

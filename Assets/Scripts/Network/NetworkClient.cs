using NetworkMessages;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    private static NetworkClient instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.Parse(IP, Port);
        m_Connection = m_Driver.Connect(endpoint);

        players = new Dictionary<string, GameObject>();
    }

    public static NetworkClient Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public string IP;
    public ushort Port;
    public GameObject playerPrefab;

    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    public string myID { get; private set; }
    private Dictionary<string, GameObject> players;
    private float timer;

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }


    private void OnData(DataStreamReader stream)
    {
        NativeArray<byte> message = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(message);
        string returnData = Encoding.ASCII.GetString(message.ToArray());

        NetworkHeader header = new NetworkHeader();
        header = JsonUtility.FromJson<NetworkHeader>(returnData);

        switch (header.cmd)
        {
            case Commands.NEW_CLIENT:
                {
                    Debug.Log("New client");
                    NewPlayer np = JsonUtility.FromJson<NewPlayer>(returnData);
                    Debug.Log("NEW_CLIENT: " + np.player.ToString());
                    SpawnPlayers(np.player);
                    break;
                }
            case Commands.UPDATE:
                {
                    UpdatedPlayer up = JsonUtility.FromJson<UpdatedPlayer>(returnData);
                    UpdatePlayers(up.update);
                    break;
                }
            case Commands.CLIENT_DROPPED:
                {
                    DisconnectedPlayer dp = JsonUtility.FromJson<DisconnectedPlayer>(returnData);
                    DestroyPlayers(dp.disconnect);
                    Debug.Log("Client dropped");
                    break;
                }
            case Commands.CLIENT_LIST:
                {
                    ConnectedPlayer cp = JsonUtility.FromJson<ConnectedPlayer>(returnData);
                    SpawnPlayers(cp.connect);
                    Debug.Log("Client list");
                    break;
                }
            case Commands.OWN_ID:
                {
                    NewPlayer p = JsonUtility.FromJson<NewPlayer>(returnData);
                    myID = p.player.id;
                    SpawnPlayers(p.player);
                    Debug.Log("OWN_ID: " + myID);
                    break;
                }
            default:
                Debug.Log("Error");
                break;
        }

    }


    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
            return;

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to the server.");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnData(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {

                Debug.Log("Client disconnected");
                m_Connection = default(NetworkConnection);
            }
        }
    }

    private void SpawnPlayers(Player p)
    {
        if (players.ContainsKey(p.id))
            return;
        bool control;
        if (p.id == myID)
            control = true;
        else
            control = false;

        Debug.Log(p.ToString());
        GameObject temp;
        //temp = Instantiate(cube, p.position, Quaternion.identity);

        if (players.Count == 0)
        {
            temp = Instantiate(playerPrefab, new Vector3(5, 0, 0), Quaternion.identity);
            Debug.Log("player spawn: " + p.position);
        }
        else
        {
            temp = Instantiate(playerPrefab, new Vector3(14, 0, 0), Quaternion.identity);
            temp.GetComponent<SpriteRenderer>().flipX = true;
            Debug.Log("player spawn: " + p.position);
        }
        temp.GetComponent<PlayerControl>().SetNetID(p.id);
        temp.GetComponent<PlayerControl>().SetControl(control);
        temp.GetComponent<Renderer>().material.color = new Color(p.color.R, 1, 1, 1.0f);
        players.Add(p.id, temp);
    }
    private void SpawnPlayers(Player[] p)
    {
        foreach (Player player in p)
        {
            SpawnPlayers(player);
        }
    }
    private void UpdatePlayers(Player[] p)
    {
        foreach (Player player in p)
        {
            if (players.ContainsKey(player.id))
            {
                players[player.id].transform.position = player.position;
            }
        }
    }
    private void DestroyPlayers(Player[] p)
    {
        foreach (Player player in p)
        {
            if (players.ContainsKey(player.id))
            {
                Destroy(players[player.id]);
                players.Remove(player.id);
            }
        }
    }

    private void SendData(object data)
    {
        var writer = m_Driver.BeginSend(m_Connection);
        NativeArray<byte> sendBytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(JsonUtility.ToJson(data)), Allocator.Temp);
        writer.WriteBytes(sendBytes);
        m_Driver.EndSend(writer);
    }
    public void SendInput(Vector3 input, bool flip)
    {
        PlayerInput playerInput = new PlayerInput();
        playerInput.input = input;
        playerInput.direction = flip;
        SendData(playerInput);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Networking;

public class GameController : MonoBehaviour {

    public const string SERVER_ADDRESS = "142.232.18.98";
    public const ushort SERVER_PORT = 42069;
    public const int PACKET_SIZE = 1200;
    public const byte INIT_HEADER = 0;
    public const byte TICK_HEADER = 85;
    public const byte ACK_HEADER = 170;
    public const int ID_OFFSET = 373;
    public const int POSITION_OFFSET = 13;
    public const int ROTATION_OFFSET = 253;

    private byte currentPlayerId;
    private Dictionary<byte, GameObject> players;

    byte[] buffer;
    private Client client;

    public Camera MainCamera;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;

    void Start()
    {
        currentPlayerId = 0;
        players = new Dictionary<byte, GameObject>();

        buffer = new byte[PACKET_SIZE];
        client = new Client();
        client.Init(SERVER_ADDRESS, SERVER_PORT);

        byte[] initPacket = new byte[PACKET_SIZE];
        initPacket[0] = 69;

        client.Send(initPacket, PACKET_SIZE);
    }

    void Update()
    {
        if (currentPlayerId != 0)
        {
            this.updateGameState();
        }
        else
        {
            this.syncWithServer();
        }
    }

    void updateGameState()
    {
        this.sendPlayerDataToServer();

        if (client.Poll() == Client.SOCKET_NO_DATA)
        {
            return;
        }

        if (client.Recv(buffer, PACKET_SIZE) < PACKET_SIZE)
        {
            return;
        }

        List<byte> playerIDs = this.getPlayerIDs(buffer);
        List<Vector3> positions = this.getPlayerPositions(buffer);
        List<Quaternion> rotations = this.getPlayerRotations(buffer);

        // Add any new players
        if (playerIDs.Count > this.players.Count)
        {
            for (int i = 0; i < playerIDs.Count; i++)
            {
                if (this.players.ContainsKey(playerIDs[i]))
                {
                    continue;
                }
                this.addPlayer(playerIDs[i], positions[i], rotations[i]);
            }
        }

        this.movePlayers(playerIDs, positions, rotations);
    }

    void syncWithServer()
    {
        if (this.client.Poll() == Client.SOCKET_NO_DATA)
        {
            return;
        }

        if (this.client.Recv(buffer, PACKET_SIZE) < PACKET_SIZE)
        {
            return;
        }

        if (this.buffer[0] != INIT_HEADER)
        {
            return;
        }
        this.currentPlayerId = buffer[ID_OFFSET];
        this.addPlayer(this.currentPlayerId, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)));
    }

    List<byte> getPlayerIDs(byte[] data)
    {
        List<byte> playerIDs = new List<byte>();
        for (int i = 0; i < 30; i++)
        {
            byte id = data[ID_OFFSET + i];

            if (id == 0)
            {
                return playerIDs;
            }

            playerIDs.Add(id);
        }
        return playerIDs;
    }

    List<Vector3> getPlayerPositions(byte[] data)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < 30; i++)
        {
            float x = BitConverter.ToSingle(data, POSITION_OFFSET + (i * 8));
            float z = BitConverter.ToSingle(data, POSITION_OFFSET + (i * 8) + 4);
            Vector3 position = new Vector3(x, 0, z);
            positions.Add(position);
        }
        return positions;
    }

    List<Quaternion> getPlayerRotations(byte[] data)
    {
        List<Quaternion> rotations = new List<Quaternion>();
        for (int i = 0; i < 30; i++)
        {
            float pheta = BitConverter.ToSingle(data, ROTATION_OFFSET + (i * 4));
            Quaternion rotation = Quaternion.Euler(new Vector3(0, pheta, 0));
            rotations.Add(rotation);
        }
        return rotations;
    }

    void addPlayer(byte id, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        Player playerScript;

        if (id == this.currentPlayerId)
        {
            player = (GameObject)Instantiate(this.PlayerPrefab, position, rotation);
            // this.MainCamera.GetComponent<CameraFollow>().SetOffset(player.transform);

            playerScript = player.GetComponent<Player>();
            playerScript.ClientController = this;
            playerScript.PlayerId = this.currentPlayerId;
        }
        else
        {
            player = (GameObject)Instantiate(this.EnemyPrefab, position, rotation);

            playerScript = player.GetComponent<Player>();
            playerScript.PlayerId = id;
        }

        this.players.Add(id, player);
    }

    void movePlayers(List<byte> playerIds, List<Vector3> positions, List<Quaternion> rotations)
    {
        try
        {
            for (int i = 0; i < playerIds.Count; i++)
            {
                this.players[playerIds[i]].transform.position = positions[i];
                this.players[playerIds[i]].transform.rotation = rotations[i];
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void sendPlayerDataToServer()
    {
        GameObject currentPlayer = this.players[this.currentPlayerId];
        byte[] x = BitConverter.GetBytes(currentPlayer.transform.position.x);
        byte[] z = BitConverter.GetBytes(currentPlayer.transform.position.z);
        byte[] pheta = BitConverter.GetBytes(currentPlayer.transform.rotation.y);

        this.buffer[0] = TICK_HEADER;
        this.buffer[1] = this.currentPlayerId;
        Array.Copy(x    , 0, this.buffer, 2,  4);
        Array.Copy(z    , 0, this.buffer, 6,  4);
        Array.Copy(pheta, 0, this.buffer, 10, 4);

        this.client.Send(this.buffer, PACKET_SIZE);
    }
}
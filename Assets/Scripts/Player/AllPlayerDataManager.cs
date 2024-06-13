using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public class AllPlayerDataManager : NetworkBehaviour
{

    public static AllPlayerDataManager Instance;
    private NetworkList<PlayerData> allPlayerData;

    private const int HP = 10;
    private const int LIFEPOINTS_TO_REDUCE = 1;

    public event Action<ulong> OnPlayerDead;
    public event Action<ulong> OnPlayerHealthChanged;

    public void Awake()
    {
        allPlayerData = new NetworkList<PlayerData>();
        if(Instance != null && Instance != this)
        {
            Destroy(Instance);
        }    
        Instance = this;
    }

    public void AddPlacedPlayer(ulong id)
    {
        for(int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                PlayerData newData = new PlayerData(
                    allPlayerData[i].playerHP, 
                    allPlayerData[i].playerDamage,
                    allPlayerData[i].clientID,
                    true
                );
                allPlayerData[i] = newData;
            }
            
        }
    }

    public bool GetHasPlacePlayer(ulong id)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                return allPlayerData[i].isPlaced;
            }
        }
        return false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            AddNewClientToList(NetworkManager.LocalClientId);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;
        BulletData.OnHitPlayer += BulletDataOnOnHitPlayer;
        KillPlayer.OnKillPlayer += KillPlayerOnOnKillPlayer;
        RestartGame.OnRestartGame += RestartGameOnOnRestartGame;
    }
    public void OnDisable()
    {
        if (IsServer)
        {
            allPlayerData.Clear();
            NetworkManager.Singleton.OnClientConnectedCallback -= AddNewClientToList;
        }
        BulletData.OnHitPlayer -= BulletDataOnOnHitPlayer;
        KillPlayer.OnKillPlayer -= KillPlayerOnOnKillPlayer;
        RestartGame.OnRestartGame -= RestartGameOnOnRestartGame;
    }

    private void RestartGameOnOnRestartGame()
    {
        if (!IsServer) return;

        List<NetworkObject> playerObjects = FindObjectsOfType<PlayerMovement>()
            .Select(x => x.transform.GetComponent<NetworkObject>()).ToList();

        List<NetworkObject> bulletObjects = FindObjectsOfType<BulletData>()
            .Select(x => x.transform.GetComponent<NetworkObject>()).ToList();



        foreach (var playerobj in playerObjects)
        {
            playerobj.Despawn();
        }

        foreach (var bulletObject in bulletObjects)
        {
            bulletObject.Despawn();
        }

        ResetNetworkList();
    }


    void ResetNetworkList()
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            PlayerData resetPLayer = new PlayerData(
                playerHp: HP,
                playerDamage: 0,
                allPlayerData[i].clientID,
                isPlaced: false);

            allPlayerData[i] = resetPLayer;
        }
    }

    private void KillPlayerOnOnKillPlayer(ulong id)
    {
        (ulong, ulong) fromTO = new(555, id);
        BulletDataOnOnHitPlayer(fromTO);
    }

    public float GetPlayerHealth(ulong id)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == id)
            {
                return allPlayerData[i].playerHP;
            }
        }

        return default;
    }

    private void BulletDataOnOnHitPlayer((ulong from, ulong to) ids)
    {
        if (IsServer)
        {
            if (ids.from != ids.to)
            {
                for (int i = 0; i < allPlayerData.Count; i++)
                {
                    if (allPlayerData[i].clientID == ids.to)
                    {
                        int lifePointsToReduce = allPlayerData[i].playerHP == 0 ? 0 : LIFEPOINTS_TO_REDUCE;

                        PlayerData newData = new PlayerData(
                            allPlayerData[i].playerHP - lifePointsToReduce,
                            allPlayerData[i].playerDamage,
                            allPlayerData[i].clientID,
                            allPlayerData[i].isPlaced
                        );

                        

                        if (newData.playerHP <= 0)
                        {
                            OnPlayerDead?.Invoke(ids.to);
                        }

                        Debug.Log("Player got hit " + ids.to + " lifepoints left => " + newData.playerHP + " shot by " + ids.from);

                        allPlayerData[i] = newData;
                        break;
                    }
                }
            }
        }
        SyncReducePlayerHealthClientRpc(ids.to);
    }

    [ClientRpc]
    void SyncReducePlayerHealthClientRpc(ulong hitID)
    {
        OnPlayerHealthChanged?.Invoke(hitID);
    }

    void AddNewClientToList(ulong clienID)
    {
        if (!IsServer) return;

        foreach (var playerData in allPlayerData)
        {
            if (playerData.clientID == clienID) { return; }
        }
        PlayerData newPlayerData = new PlayerData();
        newPlayerData.clientID = clienID;
        newPlayerData.playerHP = HP;
        newPlayerData.playerDamage = 0;
        newPlayerData.isPlaced = false;

        if(allPlayerData.Contains(newPlayerData)) { return; }
        allPlayerData.Add(newPlayerData);
        PrintAllPlayerList();
    }

    void PrintAllPlayerList()
    {
        foreach (PlayerData playerData in allPlayerData)
        {
            Debug.Log("Player ID => " + playerData.clientID + " hasPlaced " + playerData.isPlaced + " Called by " + NetworkManager.Singleton.LocalClientId);
        }
    }
}

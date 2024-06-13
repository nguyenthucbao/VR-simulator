using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientID;
    public int playerHP;
    public int playerDamage;
    public bool isPlaced;

    public PlayerData(int playerHp,  int playerDamage, ulong clientID, bool isPlaced)
    {
        this.playerHP = playerHp;
        this.playerDamage = playerDamage;
        this.clientID = clientID;
        this.isPlaced = isPlaced;
    }

    public bool Equals(PlayerData other)
    {
        return (
            other.clientID == clientID &&
            other.playerHP == playerHP &&
            other.playerDamage == playerDamage &&
            other.isPlaced == isPlaced);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref playerHP);
        serializer.SerializeValue(ref playerDamage);   
        serializer.SerializeValue(ref isPlaced);

    }
}

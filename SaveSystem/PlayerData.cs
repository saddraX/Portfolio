using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] playerPosition;

    //player stats
    public float livesCount;
    public bool isDead;
    public int continueCount;
    public int collectablesCount;
    public float timeSpent;
    public bool isInCave;

    public bool canUseUmbrella;

    public PlayerData(Player player)
    {
        //player stats
        livesCount = Player.livesCount;
        isDead = Player.isDead;
        continueCount = Player.continueCount;
        collectablesCount = Player.collectablesCount;
        timeSpent = Player.timeSpent;
        isInCave = Player.isInCave;

        canUseUmbrella = Player.canUseUmbrella;
    }
}

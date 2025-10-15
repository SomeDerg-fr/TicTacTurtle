using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GlobalVariables : NetworkBehaviour
{
    public readonly SyncVar<int> players = new SyncVar<int>(1);
    public readonly SyncList<Cards> serverBoard = new SyncList<Cards>();

    public readonly SyncVar<string> player1 = new SyncVar<string>("");
    public readonly SyncVar<string> player2 = new SyncVar<string>("");

    [ServerRpc]
    public void addPlayer(string p)
    {
        if (player1.Equals(""))
        {
            player1.Value = p;
        }
        else if (player2.Equals("") && !p.Equals(player1))
        {
            player2.Value = p;
        }
    }
    
    public int getPlayerNo(string p)
    {
        if (player1.Equals(p))
        {
            return 1;
        }
        else if (player2.Equals(p))
        {
            return 2;
        }
        return 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTurn(int amount)
    {
        players.Value = amount;
        Debug.Log("peanits "+amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBoardCard(int index, Cards c)
    {
        serverBoard[index] = c;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

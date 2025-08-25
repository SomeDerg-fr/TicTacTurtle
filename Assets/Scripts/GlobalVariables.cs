using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GlobalVariables : NetworkBehaviour
{
    public readonly SyncVar<int> players = new SyncVar<int>(1);
    
    [ServerRpc(RequireOwnership = false)]
    public void SetTurn(int amount)
    {
        players.Value = amount;
        Debug.Log("peanits "+amount);
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

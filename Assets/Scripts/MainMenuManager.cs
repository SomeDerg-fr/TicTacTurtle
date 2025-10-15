using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;
 
    [SerializeField] private GameObject menuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;
 
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameButton;
    
    // Add reference to GlobalVariables
    private GlobalVariables globalVariables;
    
    private void Awake() => instance = this;
 
    private void Start()
    {
        // Find the GlobalVariables component in the scene
        globalVariables = FindObjectOfType<GlobalVariables>();
        OpenMainMenu();
    }
 
    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
    }
 
    public void OpenMainMenu()
    {
        CloseAllScreens();
        menuScreen.SetActive(true);
    }
 
    public void OpenLobby()
    {
        CloseAllScreens();
        lobbyScreen.SetActive(true);
    }
 
    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyTitle.text = lobbyName;
        //instance.startGameButton.gameObject.SetActive(isHost);
        instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        
        // Add player when entering lobby
        if (instance.globalVariables != null)
        {
            string playerName = SteamFriends.GetPersonaName();
            instance.globalVariables.addPlayer(playerName);
        }
        
        instance.OpenLobby();
    }
 
    void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
    }
 
    public void JoinLobby()
    {
        CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(steamID);
    }
 
    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        OpenMainMenu();
    }
 
    public void StartGame()
    {
        string[] scenesToClose = new string[] { "MenuSceneSteam" };
        BootstrapNetworkManager.ChangeNetworkScene("Game", scenesToClose);
    }
}
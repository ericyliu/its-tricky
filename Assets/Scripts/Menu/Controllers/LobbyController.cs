using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class LobbyController : MonoBehaviour {

  public static LobbyController current;
  Lobby lobby;
  int pingCount = 0;
  
  GameObject playerPrefab;
  
  int maxPingCountDiff = 5;
  
  GameObject lobbyPanel;

  void Start () {
    lobby = new Lobby();
    lobbyPanel = GameObject.Find("LobbyPanel");
    foreach (Transform child in lobbyPanel.transform) GameObject.Destroy(child.gameObject); 
    playerPrefab = Resources.Load("Menu/OnlinePlayer") as GameObject;
    current = this;
    InvokeRepeating("updatePingCount", 0f, 1f);
  }
  
  void Update () {
    //Remove idle players
    Player[] newPlayers = new Player[lobby.onlinePlayers.Count];
    lobby.onlinePlayers.CopyTo(newPlayers);
    foreach (Player player in newPlayers) {
      if (pingCount - player.lastPinged > maxPingCountDiff) {
        lobby.onlinePlayers.Remove(player);
      }
    }
    
    //Remove players on menu that are not in list
    foreach (Transform child in lobbyPanel.transform) {
      string name = child.name.Split(':')[1];
      if (lobby.onlinePlayers.Find(x => x.name == name) == null) {
        GameObject.Destroy(child.gameObject);
      }
    }
    
    //Add players not on menu
    foreach (Player player in lobby.onlinePlayers) {
      if (GameObject.Find("OnlinePlayer:" + player.name) == null) {
        GameObject playerObject = Instantiate(playerPrefab);
        playerObject.name = "OnlinePlayer:" + player.name;
      }
    }
  }
  
  public void UpdatePlayer (string name, TcpClient client) {
    Player player = lobby.onlinePlayers.Find(x => x.name == name);
    if (player != null) {
      player.lastPinged = pingCount;
    }
    else {
      player = new Player(name, client);
      lobby.onlinePlayers.Add(player);
    }
  }

  public Player GetPlayer (string name) {
    return lobby.onlinePlayers.Find(x => x.name == name);
  }
  
  void updatePingCount () {
    pingCount++;
  }

}

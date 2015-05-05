using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour {

  public static LobbyController current;
  Lobby lobby;
  
  GameObject playerPrefab;
  
  GameObject lobbyPanel;

  void Start () {
    lobby = new Lobby();
    lobbyPanel = GameObject.Find("LobbyPanel");
    foreach (Transform child in lobbyPanel.transform) GameObject.Destroy(child.gameObject); 
    playerPrefab = Resources.Load("Menu/OnlinePlayer") as GameObject;
    current = this;
  }
  
  void Update () {
    //Remove players on menu that are not in list
    foreach (Transform child in lobbyPanel.transform) {
      string name = child.name.Split(':')[1];
      if (lobby.onlinePlayers.Find(x => x.name == name) == null) {
        GameObject.Destroy(child.gameObject);
      }
    }
    
    //Add players not on menu
    int index = 0;
    foreach (Player player in lobby.onlinePlayers) {
      if (GameObject.Find("OnlinePlayer:" + player.name) == null) {
        createPlayerObject(player, index);
      }
      index++;
    }
  }
  
  public void UpdatePlayers (string[] playerIps) {
    List<Player>.Enumerator enumerator = this.lobby.onlinePlayers.GetEnumerator();
    while (enumerator.MoveNext()) {
      Player player = enumerator.Current;
      lobby.onlinePlayers.Remove(player);
    }
    
    for (int i = 0; i < playerIps.Length; i++) {
      Debug.Log("adding player " + i + " " +playerIps[i]);
      Player player = new Player(playerIps[i]);
      lobby.onlinePlayers.Add(player);
    }
  }

  public void RemovePlayer (string name) {
    Player player = lobby.onlinePlayers.Find(x => x.name == name);
    lobby.onlinePlayers.Remove(player);
  }

  public Player GetPlayer (string name) {
    return lobby.onlinePlayers.Find(x => x.name == name);
  }
  
  void createPlayerObject (Player player, int index) {
    GameObject playerObject = Instantiate(playerPrefab);
    playerObject.name = "OnlinePlayer:" + player.name;
    playerObject.transform.SetParent(lobbyPanel.transform, false);
    RectTransform rect = playerObject.GetComponent<RectTransform>();
    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -10 - (60*index));
    playerObject.transform.GetComponentInChildren<Text>().text = "Player " + player.name;
  }
  
}

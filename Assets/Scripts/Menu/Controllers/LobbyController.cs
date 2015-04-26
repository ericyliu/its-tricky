using UnityEngine;
using System.Collections;

public class LobbyController : MonoBehaviour {

  public LobbyController current;
  Lobby lobby;
  int nextId = 0;
  
  GameObject lobbyPanel;

  void Start () {
    lobby = new Lobby();
    lobbyPanel = GameObject.Find("LobbyPanel");
    foreach (Transform child in lobbyPanel.transform) GameObject.Destroy(child.gameObject); 
    current = this;
  }
  
  public void AddPlayer (Connection connection) {
    Player player = new Player(++nextId, connection);
    connection.player = player;
    lobby.onlinePlayers.Add(player);
  }
  
  public void RemovePlayer (Connection connection) {
    lobby.onlinePlayers.Remove(connection.player);
  }
  
  public Player GetPlayer (int id) {
    return lobby.onlinePlayers.Find(x => x.id == id);
  }

}

using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class Player
{

  public string name;
  public TcpClient client;
  public int lastPinged;
  
  public Player (string name, TcpClient client) {
    this.name = name;
    this.client = client;
  }
  
  public override bool Equals (System.Object obj) {
    if (obj == null) return false;
    Player p = obj as Player;
    if ((System.Object)p == null) return false;
    return name == p.name;
  }
  
  public override int GetHashCode() {
    return name.GetHashCode();
  }
  

}

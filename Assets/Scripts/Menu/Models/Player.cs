using UnityEngine;
using System.Collections;

public class Player
{

  public int id;
  public Connection connection;
  
  public Player (int id, Connection connection) {
    this.id = id;
    this.connection = connection;
  }
  
  public override bool Equals (System.Object obj) {
    if (obj == null) return false;
    Player p = obj as Player;
    if ((System.Object)p == null) return false;
    return id == p.id;
  }
  
  public override int GetHashCode() {
    return id;
  }
  

}

using UnityEngine;
using System.Collections;

public class Config {

  public const string GameVersion = "0.1";
  
  public const int udpPort = 9998;
  public const int tcpPort = 9999;
  
  public enum EventTypes : byte {Join};

}

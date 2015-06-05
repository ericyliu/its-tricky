using UnityEngine;
using System.Collections;

public class Config {

  public const string GameVersion = "0.1";
  
  public const int serverListenPort = 9998;
  public const int clientListenPort = 9999;
  public const int serverDiscoveryPort = 9997;
  
  public enum EventTypes : byte {Join};

}

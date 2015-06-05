using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;

public class Server : Networker {  
  public NetworkNode clientListener;
  public List<NetworkNode> connectedPlayers = new List<NetworkNode>(); 
  
  public float discoveryPingTime = 1f;
  public float clientPingTime = 3f;
  IPEndPoint broadcastEndPoint;

  UdpClient broadcaster;
  TcpListener tcpListener;
  

  void Awake() {
    GameObject.DontDestroyOnLoad(gameObject);
  }

  void Start() {
    //Start UDP Broadcaster
    Debug.Log("[SERVER] starting server");
    broadcaster = new UdpClient ();
    broadcastEndPoint = new IPEndPoint (IPAddress.Broadcast, Config.serverDiscoveryPort);
    InvokeRepeating("sendDiscoveryPing", 0f, discoveryPingTime);
    InvokeRepeating("pingEveryone", 0f, discoveryPingTime);
    
    this.clientListener = new NetworkNode(IPAddress.Any.ToString(), Config.serverListenPort);
    startListening(this.clientListener);
  }
  
  public void Update() {
    
  }

  void sendDiscoveryPing() {
    Debug.Log("[SERVER] sending discovery ping");
    DiscoveryPingMessage dpm = new DiscoveryPingMessage(NetworkService.GetSelfIP());
    dpm.setMessageHeaders(NetworkService.GetSelfIP());
    string message = dpm.encodeMessage();
    Byte[] bytes = Encoding.ASCII.GetBytes(message);
    broadcaster.Send(bytes, bytes.Length, broadcastEndPoint);
  }
  
  void pingEveryone() {
    broadcastMessage(new PingMessage ());
  }
  
  void broadcastMessage(NetworkMessage message) {
    foreach (NetworkNode node in this.connectedPlayers) {
      Debug.Log(whoAmI() +" broadcast message to " + node + " message " + message);
      sendMessageTo(node, message);
    }
  }
 
  string[] connectedPlayerIps() {
    string[] playerIps = new string[this.connectedPlayers.Count];
    int i = 0;
    foreach (NetworkNode node in this.connectedPlayers) {
      playerIps[i] = node.ipAddress;
      i++;
    }
    return playerIps;
  }
 
  override public void parseMessage(string message) {
    Debug.Log(whoAmI() + "parsing message");
    NetworkMessage networkMessage = NetworkMessage.decodeMessage(message);
    string messageType = networkMessage.thisMessageType();
    
    if (messageType == typeof(PlayerUpdateMessage).FullName) {
      PlayerUpdateMessage joinMsg = (PlayerUpdateMessage)networkMessage;
      if (joinMsg.action == "join") {
        this.connectedPlayers.Add(new NetworkNode(joinMsg.ipAddress, Config.clientListenPort));
      }
      
      Debug.Log("[SERVER] client " + joinMsg.ipAddress + " " + joinMsg.action + "-ed ");
      
      // update everyone on who is online
      broadcastMessage(new JoinBroadcastMessage (connectedPlayerIps()));
    } else if (messageType == typeof(PingMessage).FullName) {
      Debug.Log("[SERVER] ping!");
    } else {
      broadcastMessage(networkMessage);
    }
  }
  
  override public string whoAmI() {
    return "[SERVER]";
  }
}

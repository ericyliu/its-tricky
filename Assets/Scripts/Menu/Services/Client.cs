using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Client : Networker {

  UdpClient udpClient;
  TcpClient tcpClient;
  bool connected = false;
  bool shouldStartTcpConnection;
  float timeToWaitForServer = 5f;
  string ipAddress;
  string[] connectedPlayerIps;
  IPEndPoint serverEndPoint;
  float initTime;

  void Awake() {
    GameObject.DontDestroyOnLoad(gameObject);
    initTime = Time.timeSinceLevelLoad;
  }
  
  void Start() {
    Debug.Log("[CLIENT] Client started. Trying to find server");
    udpClient = new UdpClient (Config.udpPort);
    tcpClient = new TcpClient ();
    startListening();
    Debug.Log("Main thread: " + Thread.CurrentThread.ManagedThreadId);
  }
  
  public void Update() {
    if (!connected) {
      float timeSinceInit = Time.timeSinceLevelLoad - initTime;
      if (timeSinceInit > timeToWaitForServer) {
        if (gameObject.GetComponent<Server>() == null) {
          Debug.Log("[CLIENT] No server found, initializing server");
          gameObject.AddComponent<Server>();
        }
      }
    } 
    if (shouldStartTcpConnection && !connected) {
      connected = true;
      startTcpConnection();
    }
    
    NetworkerKV data = safeGetNextMessage();
    if (data != null) {
      parseMessage(data.Key);
    }
  }
  
  void startListening() {
    udpClient.BeginReceive(receive, new object ());
  }
  
  void receive(IAsyncResult ar) {
    IPEndPoint ip = new IPEndPoint (IPAddress.Any, 15000);
    byte[] bytes = udpClient.EndReceive(ar, ref ip);
    string message = Encoding.ASCII.GetString(bytes);
    if (message.Split('|') [0] == "discover") {
      if (!connected) {
        shouldStartTcpConnection = true;
        serverEndPoint = new IPEndPoint (ip.Address, 3000);
      }
    }
    startListening();
  }

  void stopListening() {
    udpClient.Close();
  }
  
  void startTcpConnection() {
    Debug.Log("[CLIENT] Starting TCP Connection To Server : thread " + Thread.CurrentThread.ManagedThreadId);
    tcpClient.Connect(serverEndPoint);
    startNetworkListening(tcpClient, "CLIENT " + this.ipAddress);
    ipAddress = NetworkService.GetSelfIP();
    PlayerUpdateMessage joinMsg = new PlayerUpdateMessage (ipAddress, "join");
    Debug.Log("About to send join message on thread " + Thread.CurrentThread.ManagedThreadId);
    sendTCPMessage(joinMsg);
  }
  
  void parseMessage(string message) {
    string messageType = NetworkMessage.messageType(message);
    if (messageType == JoinBroadcastMessage.type) {
      JoinBroadcastMessage jbm = (JoinBroadcastMessage)NetworkMessage.decodeMessage(message);
      this.connectedPlayerIps = jbm.ipAddresses;
      LobbyController.current.UpdatePlayers(this.connectedPlayerIps);
    } else if (messageType == PingMessage.type) {
      Debug.Log("[CLIENT + " + this.ipAddress + "] ping!");
    } else {
      Debug.LogError("[CLIENT + " + this.ipAddress + "] could not parseMessage: " + message);
    }
  }
}

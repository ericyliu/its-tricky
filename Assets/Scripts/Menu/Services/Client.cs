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
  private string serverIpAddress = "server";
  private ClientListener clientListener;

  public void setClientListener(ClientListener listener) {
    this.clientListener = listener;
    int indexOfThisClient = Array.IndexOf(connectedPlayerIps, this.ipAddress);
    this.clientListener.connectedPlayerIpsDidChange(this.connectedPlayerIps, indexOfThisClient);
  }

  void Awake() {
    GameObject.DontDestroyOnLoad(gameObject);
    initTime = Time.timeSinceLevelLoad;
  }
  
  void Start() {
    Debug.Log("[CLIENT] Client started. Trying to find server");
    udpClient = new UdpClient (Config.udpPort);
    tcpClient = new TcpClient ();
    startListening();
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
    
    // start tcp connection here so its on the main thread
    if (shouldStartTcpConnection && !connected) {
      connected = true;
      startTcpConnection();
    }
    
    // grab all client messages and send them to server
    if (this.clientListener != null) {
      List<NetworkMessage> messagesToSend = this.clientListener.getMessagesToSend();
      foreach (NetworkMessage message in messagesToSend) {
        this.sendMessageToServer(message);
      }
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
    Debug.Log("[CLIENT] Starting TCP Connection To Server");
    tcpClient.Connect(serverEndPoint);
    this.server = tcpClient;
    startNetworkListening(tcpClient, "CLIENT " + this.ipAddress);
    ipAddress = NetworkService.GetSelfIP();
    PlayerUpdateMessage joinMsg = new PlayerUpdateMessage (ipAddress, "join");
    sendMessageToServer(joinMsg);
  }
  
  void sendMessageToServer(NetworkMessage message) {
    sendMessageTo(this.serverIpAddress, message);
  }
  
  void parseMessage(string message) {
    NetworkMessage networkMessage = NetworkMessage.decodeMessage(message);
    string messageType = networkMessage.thisMessageType();
  
    if (messageType == typeof(JoinBroadcastMessage).FullName) {
      JoinBroadcastMessage jbm = (JoinBroadcastMessage)networkMessage;
      this.connectedPlayerIps = jbm.ipAddresses;
      LobbyController.current.UpdatePlayers(this.connectedPlayerIps);
      if (jbm.ipAddresses.Length == 2) {
        startDodger();
      }
    } else if (messageType == typeof(PingMessage).FullName) {
      Debug.Log("[CLIENT + " + this.ipAddress + "] ping!");
    } else {
      if (this.clientListener != null) {
        this.clientListener.onMessage(networkMessage);
      }
    }
  }
  
  void startDodger() {
    Application.LoadLevel("harden");
  }
}

public interface ClientListener {
  void onMessage(NetworkMessage message);
  List<NetworkMessage> getMessagesToSend();
  // updates the listener when the number of players changes
  void connectedPlayerIpsDidChange(string[] connectedPlayerIps, int playerIndex);
}

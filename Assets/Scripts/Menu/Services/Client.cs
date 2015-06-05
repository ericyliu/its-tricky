using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Client : Networker {
  UdpClient serverDiscoveryClient;
  IPEndPoint serverPingEndpoint;
  NetworkNode node;
  NetworkNode serverNode;
  ClientListener clientListener;
  float initTime;
  float timeToWaitForServer = 5f;
  string[] connectedPlayerIps;
  
  void searchForAndConnectToServer() {
    serverPingEndpoint = new IPEndPoint(IPAddress.Any, Config.serverDiscoveryPort);
    this.serverDiscoveryClient = new UdpClient (serverPingEndpoint);
    this.serverDiscoveryClient.BeginReceive(new AsyncCallback(receive), null);
  }
  
  void receive(IAsyncResult ar) {    
    Debug.Log("[CLIENT] listening for server ping");
    try {
    byte[] bytes = this.serverDiscoveryClient.EndReceive(ar, ref serverPingEndpoint);
    string message = Encoding.ASCII.GetString(bytes);
    parseConnectionSearchMessage(message);
      if (this.node == null) {
      searchForAndConnectToServer();
    }
    } catch (Exception e) {
      Debug.Log(whoAmI()+e);
    }
  }

  void parseConnectionSearchMessage(string message) {
    try {
    NetworkMessage networkMessage = NetworkMessage.decodeMessage(message);
    string messageType = networkMessage.thisMessageType();
    Debug.Log("[CLIENT] got connection ping from server. " + message);
    if (messageType == typeof(DiscoveryPingMessage).FullName) {
      DiscoveryPingMessage dpm = (DiscoveryPingMessage)networkMessage;
      this.node = new NetworkNode (dpm.sourceIp.ToString(), Config.serverListenPort);
        this.serverNode = new NetworkNode (dpm.sourceIp.ToString(), Config.clientListenPort);
        startListening(this.serverNode);
      
      // send initial connection message
      ipAddress = NetworkService.GetSelfIP();
      PlayerUpdateMessage joinMsg = new PlayerUpdateMessage (ipAddress, "join");
      this.sendMessageToServer(joinMsg);
      serverDiscoveryClient.Close();
    } 
    } catch (Exception e) {
      Debug.Log(whoAmI() + e);
    }
  }

  void Awake() {
    GameObject.DontDestroyOnLoad(gameObject);
    initTime = Time.timeSinceLevelLoad;
  }
  
  void Start() {
    Debug.Log("[CLIENT] Client started. Trying to find server");
    searchForAndConnectToServer();
  }

  public void Update() {
    if (this.node == null) {
      float timeSinceInit = Time.timeSinceLevelLoad - initTime;
      if (timeSinceInit > timeToWaitForServer) {
        if (gameObject.GetComponent<Server>() == null) {
          Debug.Log("[CLIENT] No server found, initializing server");
          gameObject.AddComponent<Server>();
        }
      }
    }
    
    // grab all client messages and send them to server
    if (this.clientListener != null) {
      List<NetworkMessage> messagesToSend = this.clientListener.getMessagesToSend();
      foreach (NetworkMessage message in messagesToSend) {
        this.sendMessageToServer(message);
      }
    }
  }

  void sendMessageToServer(NetworkMessage message) {
    sendMessageTo(this.node, message);
  }

  void startDodger() {
    Application.LoadLevel("harden");
  }

  override public void parseMessage(string message) {
    Debug.Log(whoAmI() + "parsing message");
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

  public void setClientListener(ClientListener listener) {
    this.clientListener = listener;
    int indexOfThisClient = Array.IndexOf(connectedPlayerIps, this.ipAddress);
    this.clientListener.connectedPlayerIpsDidChange(this.connectedPlayerIps, indexOfThisClient);
  }
  
  override public string whoAmI() {
    return "[CLIENT]";
  }
}

public interface ClientListener {
  void onMessage(NetworkMessage message);

  List<NetworkMessage> getMessagesToSend();
  // updates the listener when the number of players changes
  void connectedPlayerIpsDidChange(string[] connectedPlayerIps, int playerIndex);
}

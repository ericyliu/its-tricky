using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Client : Networker {
  string[] connectedPlayerIps;
  private string serverIpAddress = "server";
  private ClientListener clientListener;
 
  public void setClientListener(ClientListener listener) {
    this.clientListener = listener;
    int indexOfThisClient = Array.IndexOf(connectedPlayerIps, this.ipAddress);
    this.clientListener.connectedPlayerIpsDidChange(this.connectedPlayerIps, indexOfThisClient);
  }

  public void Awake() {
    base.Awake ();
    GameObject.DontDestroyOnLoad(gameObject);
  }

  public void Start() {
    base.Start ();
    Debug.Log("[CLIENT] Client started. Trying to find server");
    this.udpClient = new UdpClient (Config.udpPort);
    listenForServerBroadcast();
  }

  public void Update() {
    base.Update ();
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

  override public void onConnectToServer() {
    PlayerUpdateMessage joinMsg = new PlayerUpdateMessage (ipAddress, "join");
    sendMessageToServer(joinMsg);
  }
}

public interface ClientListener {
  void onMessage(NetworkMessage message);
  List<NetworkMessage> getMessagesToSend();
  // updates the listener when the number of players changes
  void connectedPlayerIpsDidChange(string[] connectedPlayerIps, int playerIndex);
}

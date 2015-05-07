﻿using UnityEngine;
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
  public float discoveryPingTime = 1f;
  public float clientPingTime = 3f;
  IPEndPoint broadcastEndPoint;

  UdpClient udpClient;
  TcpListener tcpListener;
  

  void Awake() {
    GameObject.DontDestroyOnLoad(gameObject);
  }

  void Start() {
    //Start UDP Broadcaster
    udpClient = new UdpClient ();
    broadcastEndPoint = new IPEndPoint (IPAddress.Broadcast, Config.udpPort);
    InvokeRepeating("sendDiscoveryPing", 0f, discoveryPingTime);
    InvokeRepeating("pingEveryone", 0f, discoveryPingTime);
    
    Thread socketThread = new Thread (new ParameterizedThreadStart (startListening));
    socketThread.Start();
  }
  
  public void Update() {
    NetworkerKV data = safeGetNextMessage();
    while (data != null) {
      parseMessage(data.Key, data.Value);
      data = safeGetNextMessage();
    }
  }

  void sendDiscoveryPing() {
    NetworkService.Send("discover", NetworkService.GetSelfIP(), broadcastEndPoint, udpClient);
  }
  
  void pingEveryone() {
    broadcastMessage(new PingMessage ());
  }
  
  void startListening(object o) {
    Debug.Log("[SERVER] Listening for connections");
    IPEndPoint ip = new IPEndPoint (IPAddress.Any, 3000);
    tcpListener = new TcpListener (ip);
    tcpListener.Start();
    
    while (true) {
      TcpClient client = tcpListener.AcceptTcpClient();
      Debug.Log("[SERVER] New Client Connected");
      startNetworkListening(client, "SERVER accept client");
    }
  }
  
  void broadcastMessage(NetworkMessage message) {
    foreach (KeyValuePair<string, TcpClient> kv in this.playerIps) {
      sendMessageTo(kv.Key, message);
    }
  }
 
  void parseMessage(string message, TcpClient client) {
    NetworkMessage networkMessage = NetworkMessage.decodeMessage(message);
    string messageType = networkMessage.thisMessageType();
    
    if (messageType == typeof(PlayerUpdateMessage).FullName) {
      PlayerUpdateMessage joinMsg = (PlayerUpdateMessage)networkMessage;
      if (joinMsg.action == "join") {
        playerIps.Add(joinMsg.ipAddress, client);
      }
      
      Debug.Log("[SERVER] client " + joinMsg.ipAddress + " " + joinMsg.action + "-ed ");
      
      // update everyone on who is online
      string[] ips = new string[playerIps.Count];
      Dictionary<string, TcpClient>.Enumerator enumerator = playerIps.GetEnumerator();
      int i = 0;
      while (enumerator.MoveNext()) {
        ips [i] = enumerator.Current.Key;
        i++;
      }
      broadcastMessage(new JoinBroadcastMessage (ips));
    } else if (messageType == typeof(PingMessage).FullName) {
      Debug.Log("[SERVER] ping!");
    } else {
      broadcastMessage(networkMessage);
    }
  }
  
  string getIpAddressOfClient(TcpClient client) {
    Dictionary<string, TcpClient>.Enumerator enumerator = playerIps.GetEnumerator();
    while (enumerator.MoveNext()) {
      if (enumerator.Current.Value == client) {
        return enumerator.Current.Key;
      }
    }
    throw new Exception ("Could not find ip address of client " + client);
  }
  
  /*
  // http://stackoverflow.com/questions/12019528/get-set-and-value-keyword-in-c-net
  bool IsConnected(TcpClient client) {
    try {
      if (client != null && client.Client != null && client.Client.Connected) {
        /* pear to the documentation on Poll:
                * When passing SelectMode.SelectRead as a parameter to the Poll method it will return 
                * -either- true if Socket.Listen(Int32) has been called and a connection is pending;
                * -or- true if data is available for reading; 
                * -or- true if the connection has been closed, reset, or terminated; 
                * otherwise, returns false
                * /
          
        // Detect if client disconnected
        if (client.Client.Poll(0, SelectMode.SelectRead)) {
          byte[] buff = new byte[1];
          if (client.Client.Receive(buff, SocketFlags.Peek) == 0) {
            // Client disconnected
            return false;
          } else {
            Debug.Log("-----1");
            return true;
          }
        }
        Debug.Log("-----2");
        return true;
      } else {
        return false;
      }
    } catch {
      return false;
    }
  }
  */
}

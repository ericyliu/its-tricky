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

public class Server : MonoBehaviour {

  public float discoveryPingTime = 1f;
  public float clientPingTime = 3f;
  IPEndPoint broadcastEndPoint;

  // index in array indicates the player number  
  private Dictionary<string, TcpClient> playerIps;
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
    Thread socketThread = new Thread (new ParameterizedThreadStart (startListening));
    socketThread.Start();
    
    playerIps = new Dictionary<string, TcpClient> ();
  }
  
  void sendDiscoveryPing() {
    NetworkService.Send("discover", NetworkService.GetSelfIP(), broadcastEndPoint, udpClient);
  }
  
  void startListening(object o) {
    Debug.Log("[SERVER] Listening for connections");
    IPEndPoint ip = new IPEndPoint (IPAddress.Any, 3000);
    tcpListener = new TcpListener (ip);
    tcpListener.Start();
    
    while (true) {
      TcpClient client = tcpListener.AcceptTcpClient();
      Debug.Log("[SERVER] New Client Connected");
      Thread clientThread = new Thread (new ParameterizedThreadStart (HandleClientComm));
      clientThread.Start(client);
    }
  }
  
  void HandleClientComm(object client) {
    TcpClient tcpClient = (TcpClient)client;
    
    while (tcpClient.Connected) {
      string message = NetworkService.readTCPMessage(tcpClient.GetStream());
      Debug.Log("[SERVER] Recieved data: " + message);
      parseMessage(message, tcpClient);
    }
    
    Debug.Log("[SERVER] A client has disconnected");
    //todo: add a player left message instead of this
    //LobbyController.current.RemovePlayer(tcpClient);
    tcpClient.Close();
  }
  
  void broadcastMessage(string message) {
    Dictionary<string, TcpClient>.Enumerator enumerator = playerIps.GetEnumerator();
    while (enumerator.MoveNext()) {
      string ipAddress = enumerator.Current.Key;
      sendMessageTo(ipAddress, message);
    }
  }
  
  void sendMessageTo(string ipAddress, string message) {
    TcpClient client = playerIps [ipAddress];
    NetworkService.sendTCPMessage(message, client.GetStream());
  }
  
  void parseMessage(string message, TcpClient client) {
    string messageType = NetworkMessage.messageType(message);
    if (messageType == JoinMessage.type) {
      JoinMessage joinMsg = (JoinMessage) NetworkMessage.decodeMessage(message);
      playerIps.Add(joinMsg.ipAddress, client);
      Debug.Log("[SERVER] client " + joinMsg.ipAddress + " joined");
      
      // update everyone on who is online
      broadcastMessage(message);
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

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
    NetworkStream clientStream = tcpClient.GetStream();
    
    byte[] message = new byte[4096];
    int bytesRead;
    
    while (tcpClient.Connected) {
      bytesRead = 0;
      try {
        Debug.Log("Reading...");
        //blocks until a client sends a message
        var asyncReader = clientStream.BeginRead(message, 0, 4096, null, null);
        WaitHandle handle = asyncReader.AsyncWaitHandle;
        
        // Give the reader 2seconds to respond with a value
        bool completed = handle.WaitOne(2000, false);
        if (completed) {
          bytesRead = clientStream.EndRead(asyncReader);
          Debug.Log("Read");
        }
      } catch {
        //a socket error has occured
        Debug.Log("Socket Exception");
        break;
      }
      
      if (bytesRead == 0) {
        //the client has disconnected from the server
        continue;
      }
      
      //message has successfully been received
      ASCIIEncoding encoder = new ASCIIEncoding ();
      string data = encoder.GetString(message, 0, bytesRead);
      Debug.Log("[SERVER] Recieved data: " + data);
      parseMessage(data, tcpClient);
    }
    
    Debug.Log("[SERVER] A client has disconnected");
    LobbyController.current.RemovePlayer(tcpClient);
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
    NetworkService.sendTCPMessage(message, client);
  }
  
  void parseMessage(string message, TcpClient client) {
    string[] splitMsg = message.Split('|');
    string route = splitMsg [0];
    switch (route) {
    case "join":
      string ipAddress = splitMsg [1];
      playerIps.Add(ipAddress, client);
      LobbyController.current.UpdatePlayer(ipAddress, client);
      break;
    default:
      break;
    } 
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

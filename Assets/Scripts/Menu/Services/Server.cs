using UnityEngine;
using System.Collections;
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
  IPEndPoint broadcastEndPoint;
  
  UdpClient udpClient;
  TcpListener tcpListener;

  void Awake () {
    GameObject.DontDestroyOnLoad(gameObject);
  }

  void Start () {
    //Start UDP Broadcaster
    udpClient = new UdpClient();
    broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Config.udpPort);
    InvokeRepeating("sendDiscoveryPing", 0f, discoveryPingTime);
    startListening();
  }
  
  void sendDiscoveryPing () {
    NetworkService.Send("discover",NetworkService.GetSelfIP(),broadcastEndPoint,udpClient);
  }
  
  void startListening () {
    IPEndPoint ip = new IPEndPoint(IPAddress.Any, 3000);
    tcpListener = new TcpListener(ip);
    tcpListener.Start();
    
    while (true) {
      TcpClient client = tcpListener.AcceptTcpClient();
      Debug.Log("New Client Connected");
      Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
      clientThread.Start(client);
    }
  }
  
  void HandleClientComm(object client) {
    TcpClient tcpClient = (TcpClient)client;
    NetworkStream clientStream = tcpClient.GetStream();
    
    byte[] message = new byte[4096];
    int bytesRead;
    
    while (true)
    {
      bytesRead = 0;
      
      try
      {
        //blocks until a client sends a message
        bytesRead = clientStream.Read(message, 0, 4096);
      }
      catch
      {
        //a socket error has occured
        break;
      }
      
      if (bytesRead == 0)
      {
        //the client has disconnected from the server
        break;
      }
      
      //message has successfully been received
      ASCIIEncoding encoder = new ASCIIEncoding();
      string data = encoder.GetString(message, 0, bytesRead);
      Debug.Log(data);
      parseMessage(data, tcpClient);
    }
    
    tcpClient.Close();
  }
  
  void parseMessage (string message, TcpClient client) {
    string route = message.Split('|')[0];
    string data = message.Split('|')[1];
    switch (route) {
      case "join":
        LobbyController.current.UpdatePlayer(data, client);
        break;
      default:
        break;
    } 
  }

}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class Client : MonoBehaviour {

  UdpClient udpClient;
  TcpClient tcpClient;
  bool connected = false;
  float timeToWaitForServer = 5f;
  string ipAddress;
  
  IPEndPoint serverEndPoint;

  float initTime;

  void Awake () {
    GameObject.DontDestroyOnLoad(gameObject);
    initTime = Time.timeSinceLevelLoad;
  }
  
  void Start () {
    Debug.Log("[CLIENT] Client started. Trying to find server");
    udpClient = new UdpClient(Config.udpPort);
    tcpClient = new TcpClient();
    startListening();
  }
  
  public void Update () {
    if (!connected) {
      float timeSinceInit = Time.timeSinceLevelLoad - initTime;
      if (timeSinceInit > timeToWaitForServer) {
        if (gameObject.GetComponent<Server>() == null) {
          Debug.Log("[CLIENT] No server found, initializing server");
          gameObject.AddComponent<Server>();
        }
      }
    }
  }
  
  void startListening () {
    udpClient.BeginReceive(receive, new object());
  }
  
  void receive(IAsyncResult ar) {
    IPEndPoint ip = new IPEndPoint(IPAddress.Any, 15000);
    byte[] bytes = udpClient.EndReceive(ar, ref ip);
    string message = Encoding.ASCII.GetString(bytes);
    if (message.Split('|')[0] == "discover") {
      if (!connected) {
        connected = true;
        serverEndPoint = new IPEndPoint(ip.Address, 3000);
        Thread networkThread = new Thread(new ParameterizedThreadStart(startTcpConnection));
        networkThread.Start();
      }
    }
    startListening();
  }
  void stopListening () {
    udpClient.Close();
  }
  
  void startTcpConnection (object o) {
    Debug.Log("[CLIENT] Starting TCP Connection To Server");
    tcpClient.Connect(serverEndPoint);
    ipAddress = NetworkService.GetSelfIP();
    JoinMessage joinMsg = new JoinMessage(ipAddress);
    NetworkService.sendTCPMessage(joinMsg.encodeMessage(), tcpClient.GetStream());
    Thread clientThread = new Thread (new ParameterizedThreadStart (listen));
    clientThread.Start(tcpClient);
  }
  
  void listen(object client) {
    TcpClient tcpClient = (TcpClient)client;
    
    while (tcpClient.Connected) {
      string message = NetworkService.readTCPMessage(tcpClient.GetStream());
      Debug.Log("[CLIENT " + ipAddress + "] Recieved data: " + message);
      parseMessage(message);
    }
    
    Debug.Log("[CLIENT + " + ipAddress + "] disconnected");
    tcpClient.Close();
  }
  
  void parseMessage(string message) {
    string messageType = NetworkMessage.messageType(message);
    if (messageType == JoinMessage.type) {
      JoinMessage joinMsg = (JoinMessage) NetworkMessage.decodeMessage(message);
      LobbyController.current.UpdatePlayer(ipAddress);
    }
  }
}

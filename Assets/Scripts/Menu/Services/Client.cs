using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour {

  UdpClient udpClient;
  TcpClient tcpClient;
  bool connected = false;
  float timeToWaitForServer = 5f;
  
  IPEndPoint serverEndPoint;

  float initTime;

  void Awake () {
    GameObject.DontDestroyOnLoad(gameObject);
    initTime = Time.timeSinceLevelLoad;
  }
  
  void Start () {
    Debug.Log("Client started. Trying to find server");
    udpClient = new UdpClient(Config.udpPort);
    tcpClient = new TcpClient();
    startListening();
  }
  
  public void Update () {
    if (!connected) {
      float timeSinceInit = Time.timeSinceLevelLoad - initTime;
      if (timeSinceInit > timeToWaitForServer) {
        if (gameObject.GetComponent<Server>() == null) {
          Debug.Log("No server found, initializing server");
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
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
        tcpClient.Connect(serverEndPoint);
        
        NetworkStream clientStream = tcpClient.GetStream();
        
        ASCIIEncoding encoder = new ASCIIEncoding();
        byte[] buffer = encoder.GetBytes("Hello Server!");
        
        clientStream.Write(buffer, 0 , buffer.Length);
        clientStream.Flush();
      }
    }
    startListening();
  }
  void stopListening () {
    udpClient.Close();
  }
  
  void sendJoin () {
    NetworkService.Send("join",NetworkService.GetSelfIP(),serverEndPoint, udpClient);
  }
  
}

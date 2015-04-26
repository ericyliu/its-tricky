using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : NetworkEntity {

  UdpClient udpClient;
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
    udpClient = new UdpClient(Config.port);
    startListening();
  }
  
  public void Update () {
    if (!connected) {
      float timeSinceInit = Time.timeSinceLevelLoad - initTime;
      if (timeSinceInit > timeToWaitForServer) {
        Debug.Log("No server found, initializing server");
        stopListening();
        gameObject.AddComponent<Server>();
        connected = true;
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
    Debug.Log(message);
    if (message.Split('|')[0] == "discover") {
      connected = true;
      serverEndPoint = new IPEndPoint(IPAddress.Parse(message.Split('|')[1]), Config.port);
      NetworkService.Send("join",NetworkService.GetSelfIP(),serverEndPoint);
    }
    startListening();
  }
  void stopListening () {
    udpClient.Close();
  }
  
}

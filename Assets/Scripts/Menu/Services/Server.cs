using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server : NetworkEntity {

  public float discoveryPingTime = 1f;
  IPEndPoint broadcastEndPoint;

  void Awake () {
    GameObject.DontDestroyOnLoad(gameObject);
  }

  void Start () {
    NetworkService.udpClient = new UdpClient();
    broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Config.port);
    InvokeRepeating("sendDiscoveryPing", 0f, discoveryPingTime);
  }
  
  void sendDiscoveryPing () {
    NetworkService.Send("discover",NetworkService.GetSelfIP(),broadcastEndPoint);
  }

}

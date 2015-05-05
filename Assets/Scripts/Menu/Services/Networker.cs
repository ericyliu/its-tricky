using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Networker : MonoBehaviour {
  private Queue queuedNetworkData = new Queue();
  public static Mutex mutex = new Mutex();

  public void startNetworkListening(TcpClient tcpClient) {
    Thread clientThread = new Thread (new ParameterizedThreadStart (listen));
    clientThread.Start(tcpClient);
  }
  
  void listen(object client) {
    TcpClient tcpClient = (TcpClient)client;
    
    while (tcpClient.Connected) {
      NetworkerKV data = new NetworkerKV();
      data.Key = NetworkService.readTCPMessage(tcpClient.GetStream());
      data.Value = tcpClient;
      Networker.mutex.WaitOne();
      this.queuedNetworkData.Enqueue(data);
      Networker.mutex.ReleaseMutex();
    }
    
    tcpClient.Close();
  }
  
  public NetworkerKV safeGetNextMessage() {
    Networker.mutex.WaitOne();
    NetworkerKV data = null;
    if (this.queuedNetworkData.Count > 0) {
      data = new NetworkerKV();
      data = (NetworkerKV)this.queuedNetworkData.Dequeue();
    }
    Networker.mutex.ReleaseMutex();
    return data;
  }
}

public class NetworkerKV {
  public string Key;
  public TcpClient Value;
}

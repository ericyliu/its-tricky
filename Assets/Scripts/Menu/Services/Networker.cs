using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class Networker : MonoBehaviour {
  private Queue queuedNetworkData = new Queue ();
  public static Mutex queueMutex = new Mutex ();
  public static Mutex readMutex = new Mutex ();
  public static Mutex writeMutex = new Mutex ();
  private string debugData;
  
  private TcpClient tcpClient;
  private NetworkStream networkStream;
  private StreamReader networkStreamReader;
  private StreamWriter networkStreamWriter;

  public void startNetworkListening(TcpClient tcpClient, string debugData) {
    this.tcpClient = tcpClient;
    this.networkStream = this.tcpClient.GetStream();
    this.networkStreamReader = new StreamReader (this.networkStream);
    this.networkStreamWriter = new StreamWriter (this.networkStream);
    this.debugData = debugData;
    Thread clientThread = new Thread (new ThreadStart (listen));
    clientThread.Start();
  }
  
  void listen() {
    while (tcpClient.Connected) {
      NetworkerKV data = new NetworkerKV ();
      try {
        string message = "";
        message = this.networkStreamReader.ReadLine();
        data.Key = message;
      } catch (Exception e) {
        Debug.LogError("IOException [" + this.debugData + "]: " + e);
        throw;
      }
      data.Value = tcpClient;
      Networker.queueMutex.WaitOne();
      this.queuedNetworkData.Enqueue(data);
      Networker.queueMutex.ReleaseMutex();
    }
    
    tcpClient.Close();
  }
  
  public NetworkerKV safeGetNextMessage() {
    Networker.queueMutex.WaitOne();
    NetworkerKV data = null;
    if (this.queuedNetworkData.Count > 0) {
      data = new NetworkerKV ();
      data = (NetworkerKV)this.queuedNetworkData.Dequeue();
    }
    Networker.queueMutex.ReleaseMutex();
    return data;
  }
  
  public void sendTCPMessage(NetworkMessage message) {
    if (this.networkStreamWriter == null) {
      Debug.LogError("Need to run startNetworkListening before you can send messages");
    }
    try {
      this.networkStreamWriter.WriteLine(message.encodeMessage());
      this.networkStreamWriter.Flush();
    } catch (Exception e) {
      Debug.Log(e);
      throw;
    }
  }
}

public class NetworkerKV {
  public string Key;
  public TcpClient Value;
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class Networker : MonoBehaviour {
  // index in array indicates the player number  
  public Dictionary<string, TcpClient> playerIps = new Dictionary<string, TcpClient> ();
  
  public static Mutex queueMutex = new Mutex ();
  public TcpClient server;
  
  private Queue queuedNetworkData = new Queue ();
  private string debugData;
  // tcpClient hash -> streamReader (because we dont have the ip address at this point)
  private Dictionary<int, StreamReader> networkStreamReaderMap = new Dictionary<int, StreamReader>();
  private Dictionary<int, TcpClient> networkTcpMap = new Dictionary<int, TcpClient>();
  // ipaddress -> streamWriter
  private Dictionary<string, StreamWriter> networkStreamWriterMap = new Dictionary<string, StreamWriter>();

  public void startNetworkListening(TcpClient tcpClient, string debugData) {
    this.debugData = debugData;
    int tcpHash = tcpClient.GetHashCode();
    this.networkStreamReaderMap.Add(tcpHash, new StreamReader(tcpClient.GetStream()));
    this.networkTcpMap.Add(tcpHash, tcpClient);
    Thread clientThread = new Thread (new ParameterizedThreadStart (listen));
    Debug.Log("tcpClient hash code " + tcpHash + " toString " +tcpClient.ToString());
    clientThread.Start(tcpHash);
  }
  
  void listen(object tcpHashUntyped) {
    int tcpHash = (int)tcpHashUntyped;
    Debug.Log("Created thread " + Thread.CurrentThread.ManagedThreadId + " for " + this.debugData);
    while (true) {
      NetworkerKV data = new NetworkerKV ();
      try {
        Debug.Log("Thread " + Thread.CurrentThread.ManagedThreadId + " reading for " + this.debugData);
        string message = "";
        StreamReader reader;
        bool streamReaderAvailable = this.networkStreamReaderMap.TryGetValue(tcpHash, out reader);
        if (!streamReaderAvailable) {
          Debug.Log("[" + this.debugData + "] could not get stream reader from streamReaderMap with hash " + tcpHash);
        } else {
          message = reader.ReadLine();
          Debug.Log("Thread " + Thread.CurrentThread.ManagedThreadId + " read for " + this.debugData + " || " + message);
        }
        data.Key = message;
      } catch (Exception e) {
        Debug.LogError("IOException [" + this.debugData + "]: " + e);
        throw;
      }
      data.Value = this.networkTcpMap[tcpHash];
      Networker.queueMutex.WaitOne();
      this.queuedNetworkData.Enqueue(data);
      Networker.queueMutex.ReleaseMutex();
    }
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
  
  public void sendMessageTo(string ipAddress, NetworkMessage message) {
  // todo get rid of this!!! its just late and want to make sure the rest of this is working
    TcpClient client;
    if (ipAddress == "server") {
      client = this.server;
    } else {
      client = this.playerIps [ipAddress];
    }
    StreamWriter writer;
    bool streamWriterExists = this.networkStreamWriterMap.TryGetValue(ipAddress, out writer);
    if (!streamWriterExists) {
      writer = new StreamWriter(client.GetStream());
      this.networkStreamWriterMap.Add(ipAddress, writer);
    }
    try {
      Debug.Log("[SERVER] to: " + ipAddress + " message " + message.encodeMessage());
      try {
        Debug.Log("Thread " + Thread.CurrentThread.ManagedThreadId + " writing for " + this.debugData + " || " + message.encodeMessage());
        writer.WriteLine(message.encodeMessage());
        writer.Flush();
      } catch (Exception e) {
        Debug.Log(e);
        throw;
      }
    } catch (SocketException e) {
      Debug.LogError("[SERVER] " + e);
    }
  }
}

public class NetworkerKV {
  public string Key;
  public TcpClient Value;
}

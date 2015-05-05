using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

public class NetworkService {

  public static string GetSelfIP() {
    IPHostEntry host;
    string localIP = "?";
    host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (IPAddress ip in host.AddressList) {
      if (ip.AddressFamily == AddressFamily.InterNetwork) {
        localIP = ip.ToString();
      }
    }
    return localIP;
  }
  
  public static void Send(string route, string message, IPEndPoint endpoint, UdpClient udpClient) {
    byte[] bytes = Formatter.Format(route, message);
    udpClient.Send(bytes, bytes.Length, endpoint);
  }
  
  public static void sendTCPMessage(string message, NetworkStream clientStream) {
    BinaryWriter writer = null;
    try {
      writer = new BinaryWriter(clientStream);
      writer.Write(message);
    } catch (Exception e) {
      Debug.Log(e);
      throw;
    }
  }
  
  public static string readTCPMessage(NetworkStream clientStream) {
    BinaryReader reader = null;
    string message = "";
    try {
      reader = new BinaryReader (clientStream);
      message = reader.ReadString();
    } catch (Exception e) {
      Debug.Log(e);
      throw;
    }
    
    return message;
  }
}

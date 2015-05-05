using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NetworkService {

  public static string GetSelfIP () {
    IPHostEntry host;
    string localIP = "?";
    host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (IPAddress ip in host.AddressList)
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        localIP = ip.ToString();
      }
    }
    return localIP;
  }
  
  public static void Send (string route, string message, IPEndPoint endpoint, UdpClient udpClient) {
    byte[] bytes = Formatter.Format(route, message);
    udpClient.Send(bytes, bytes.Length, endpoint);
  }
  
  public static void sendTCPMessage(string message, TcpClient client) {
    NetworkStream clientStream = client.GetStream();
    
    ASCIIEncoding encoder = new ASCIIEncoding();
    byte[] buffer = encoder.GetBytes(message);
    
    clientStream.Write(buffer, 0 , buffer.Length);
    clientStream.Flush();
  }
}

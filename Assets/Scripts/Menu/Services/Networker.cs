using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;

public abstract class Networker : MonoBehaviour { 
  public string ipAddress;
  
  public void startListening(NetworkNode node) {
    try {
      Debug.Log(whoAmI() + "starting listening " + node);
      node.udpClient.BeginReceive(new AsyncCallback (receive), node);
    } catch (Exception e) {
      Debug.Log(whoAmI() + e);
    }
  }
  
  void receive(IAsyncResult ar) {
    NetworkNode node = (NetworkNode)ar.AsyncState;
    // Really confusing, but the way you get back the state object you pass in to 
    // BeginReceive is stored in ar.AsyncState
    try {
      byte[] bytes = node.udpClient.EndReceive(ar, ref node.ipEndpoint);
      string message = Encoding.ASCII.GetString(bytes);
      Debug.Log(whoAmI() + " receiving message " + message);
      parseMessage(message);
    } catch (Exception e) {
      Debug.Log(whoAmI() + e);
    }
    startListening(node);
  }
  
  public void sendMessageTo(NetworkNode node, NetworkMessage message) {
    Debug.Log(whoAmI() + " sendMessageTo(" + node + ", " + message + ")");
    try {
      message.setMessageHeaders(this.ipAddress);
      Byte[] bytes = Encoding.ASCII.GetBytes(message.encodeMessage());
      node.udpClient.Send(bytes, bytes.Length);
    } catch (Exception e) {
      Debug.Log(whoAmI() + e);
    }
  }
  
  abstract public void parseMessage(string message);

  abstract public string whoAmI();
}

public class NetworkNode {
  public string ipAddress;
  public UdpClient udpClient;
  public IPEndPoint ipEndpoint;
  public int port;
  
  public NetworkNode (string ipAddress, int port) {
    try {
      // the SetSocketOption stuff is to allow the server and client to
      // listen and send on the same port. (otherwise the server and client
      // wouldnt be able to listen on the same machine)
      // http://stackoverflow.com/questions/687868/sending-and-receiving-udp-packets-between-two-programs-on-the-same-computer
      Debug.Log("asdfasd" + ipAddress + " -- " + port);
      if (ipAddress == "0.0.0.0") {
        // this is to solve a bug with mono :(
        // http://stackoverflow.com/questions/16056746/mono-and-wcf-net-tcp-binding-on-all-ip-addresses
        // it seems like the bug doesnt happen if you make an ipendpoint out of it
        IPAddress ipa = null;
        IPAddress.TryParse(ipAddress, out ipa);
        //this.udpClient = new UdpClient(this.ipEndpoint);
        this.udpClient = new UdpClient ();
        //this.udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        this.ipEndpoint = new IPEndPoint (ipa, port);
        this.udpClient.Client.Bind(this.ipEndpoint);
      } else {
        this.udpClient = new UdpClient (ipAddress, port);
      }
    
      this.ipAddress = ipAddress;
      this.port = port;
    } catch (Exception e) {
      Debug.Log("[NetworkNode]" + e);
    }
  }
  
  public override string ToString() {
    return string.Format("[NetworkNode]" + this.ipAddress + ":" + this.port);
  }
}

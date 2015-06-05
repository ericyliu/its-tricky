using UnityEngine;
using System.Collections;
using System;

public abstract class NetworkMessage {
  public const char TYPE_DELIMITER = '@';
  public const char DATA_DELIMITER = '|';
  public const int PROTOCOL_ID = 123456789;
  
  public int protocolId; 
  public string sourceIp;
  
  public abstract string encodeMessageData();
  public abstract string thisMessageType();
  protected abstract void decodeMessageData(string msgData);
  
  public static string messageType(string msg) {
    string[] splitMsg = msg.Split(TYPE_DELIMITER);
    return splitMsg [2];
  }
  
  public void setMessageHeaders(string sourceIp) {
    this.protocolId = PROTOCOL_ID;
    this.sourceIp = sourceIp;
  }
  
  public string encodeMessage() {
    if (this.protocolId == 0) {
      Debug.Log("Need to set message header before sending message " + thisMessageType() + TYPE_DELIMITER + encodeMessageData());
    }
    return "" + this.protocolId + TYPE_DELIMITER +
           this.sourceIp + TYPE_DELIMITER +
           thisMessageType() + TYPE_DELIMITER +
           encodeMessageData();
  }
  
  public static NetworkMessage decodeMessage(string msg) {
    string[] splitMsg = msg.Split(TYPE_DELIMITER);
    string type = (string)splitMsg [2];
    
    NetworkMessage networkMessage = (NetworkMessage)Activator.CreateInstance(null, type).Unwrap();
    networkMessage.decodeMessageData(splitMsg[3]);
    networkMessage.sourceIp = (string)splitMsg[1];
    networkMessage.protocolId = Convert.ToInt32(splitMsg[0]);
    
    if (networkMessage.protocolId != PROTOCOL_ID) {
      Debug.Log("Getting a random upd message: " + msg);
    }
    return networkMessage;
  }
}

public class PlayerUpdateMessage : NetworkMessage {
  public string ipAddress;
  public string action;

  public PlayerUpdateMessage () {
  
  }

  public PlayerUpdateMessage (string ipAddress, string action) {
    this.ipAddress = ipAddress;
    this.action = action;
  }

  public override string encodeMessageData() {
    return this.ipAddress + NetworkMessage.DATA_DELIMITER.ToString() + this.action;
  }

  public override string thisMessageType() {
    return this.GetType().FullName;
  }

  protected override void decodeMessageData(string msgData) {
    string[] splitString = msgData.Split(NetworkMessage.DATA_DELIMITER);
    this.ipAddress = splitString[0];
    this.action = splitString[1];
  }
}

public class JoinBroadcastMessage : NetworkMessage {
  public string[] ipAddresses;
  
  public JoinBroadcastMessage () {
  
  }
  
  public JoinBroadcastMessage (string[] ipAddresses) {
    this.ipAddresses = ipAddresses;
  }
  
  public override string encodeMessageData() {
    return String.Join(NetworkMessage.DATA_DELIMITER.ToString(), ipAddresses);
  }
  
  public override string thisMessageType() {
    return this.GetType().FullName;
  }
  
  protected override void decodeMessageData(string msgData) {
    this.ipAddresses = msgData.Split(NetworkMessage.DATA_DELIMITER);
  }
}

public class PingMessage : NetworkMessage {  

  public PingMessage () {
  }
  
  public override string encodeMessageData() {
    return "";
  }
  
  public override string thisMessageType() {
    return this.GetType().FullName;
  }
  
  protected override void decodeMessageData(string msgData) {
  }
}


// could just use ping for this now that source ip is included in
// the message
public class DiscoveryPingMessage : NetworkMessage {  
  public DiscoveryPingMessage () {
    
  }
  
  public DiscoveryPingMessage (string sourceIp) {
    this.sourceIp = sourceIp;
  }
  
  public override string encodeMessageData() {
    return sourceIp;
  }
  
  public override string thisMessageType() {
    return this.GetType().FullName;
  }
  
  protected override void decodeMessageData(string msgData) {
    this.sourceIp = msgData;
  }
}

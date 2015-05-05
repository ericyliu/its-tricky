using UnityEngine;
using System.Collections;
using System;

public abstract class NetworkMessage {
  public const char TYPE_DELIMITER = '@';
  public const char DATA_DELIMITER = '|';
  
  public abstract string encodeMessageData();

  public abstract string thisMessageType();
  // also needs to implement   public static NetworkMessage decodeMessageData(string msgData) {
  // but i cant fucking enforce static methods
  
  public static string messageType(string msg) {
    string[] splitMsg = msg.Split(TYPE_DELIMITER);
    return splitMsg [0];
  }
  
  public string encodeMessage() {
    return thisMessageType() + TYPE_DELIMITER + encodeMessageData();
  }
  
  public static NetworkMessage decodeMessage(string msg) {
    string[] splitMsg = msg.Split(TYPE_DELIMITER);
    string type = splitMsg [0];
    if (type == JoinMessage.type) {
      return JoinMessage.decodeMessageData(splitMsg [1]);
    } else if (type == JoinBroadcastMessage.type) {
      return JoinBroadcastMessage.decodeMessageData(splitMsg [1]);
    } else if (type == PingMessage.type) {
      return PingMessage.decodeMessageData(splitMsg [1]);
    } else {
      Debug.LogError("Need to add code to decode new message type for message: " + msg);
      return null;
    }
  }
  
}

public class JoinMessage : NetworkMessage {
  public static string type = "join";
  public string ipAddress;

  public JoinMessage (string ipAddress) {
    this.ipAddress = ipAddress;
  }

  public override string encodeMessageData() {
    return this.ipAddress;
  }

  public override string thisMessageType() {
    return JoinMessage.type;
  }

  public static NetworkMessage decodeMessageData(string msgData) {
    return new JoinMessage (msgData);
  }
}

public class JoinBroadcastMessage : NetworkMessage {
  public static string type = "join_broadcast";
  public string[] ipAddresses;
  
  public JoinBroadcastMessage (string[] ipAddresses) {
    this.ipAddresses = ipAddresses;
  }
  
  public override string encodeMessageData() {
    return String.Join(NetworkMessage.DATA_DELIMITER.ToString(), ipAddresses);
  }
  
  public override string thisMessageType() {
    return JoinBroadcastMessage.type;
  }
  
  public static NetworkMessage decodeMessageData(string msgData) {
    return new JoinBroadcastMessage (msgData.Split(NetworkMessage.DATA_DELIMITER));
  }
}

public class PingMessage : NetworkMessage {
  public static string type = "ping";
  
  public PingMessage () {
  }
  
  public override string encodeMessageData() {
    return "";
  }
  
  public override string thisMessageType() {
    return PingMessage.type;
  }
  
  public static NetworkMessage decodeMessageData(string msgData) {
    return new PingMessage();
  }
}
using UnityEngine;
using System.Collections;
using System;

public abstract class NetworkMessage {
  public const char TYPE_DELIMITER = '@';
  public const char DATA_DELIMITER = '|';
  
  public abstract string encodeMessageData();
  public abstract string thisMessageType();
  public abstract void decodeMessageData(string msgData);
  
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
    NetworkMessage networkMessage = (NetworkMessage)Activator.CreateInstance(null, type).Unwrap();
    networkMessage.decodeMessageData(splitMsg[1]);
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
    return typeof(PlayerUpdateMessage).FullName;
  }

  public override void decodeMessageData(string msgData) {
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
    return typeof(JoinBroadcastMessage).FullName;
  }
  
  public override void decodeMessageData(string msgData) {
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
    return typeof(PingMessage).FullName;
  }
  
  public override void decodeMessageData(string msgData) {
  }
}
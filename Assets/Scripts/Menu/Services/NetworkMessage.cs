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
      return JoinMessage.decodeMessageData(msg);
    } else {
      throw new Exception ("There is no message of type " + type);
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

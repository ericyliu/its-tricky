using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * structure of message
 * message type: string
 * data: whatever
 */

public class PlayerController : MonoBehaviour, ClientListener {
  public Transform Player;
  public float playerObjectCenterOffsetX = 0;
  public float playerObjectCenterOffsetY = 0;
  private bool spacePressed;
  private GameObject[] players;
  private int playerNumber = 0;
  private string previousState;
  private List<NetworkMessage> messagesToSend = new List<NetworkMessage> ();
  private string[] connectedPlayerIps;
  private bool gameStarted;

  // Use this for initialization
  void Start() {
  }
  
  void startNewGame() {
    createPlayers(this.connectedPlayerIps.Length);
    layoutPlayers();
    player player = getPlayer(playerNumber);
    player.networkPlayer = false;
  }
  
  // Update is called once per frame
  void Update() {
    GameObject networkingObject = GameObject.Find("Networking");
    Client client = networkingObject.GetComponent<Client>();
    if (client == null) {
      Debug.LogError("No client could be found. no networking");
    }
    
    client.setClientListener(this);
  
    // input logic
    if (Input.GetKeyDown("space")) {
      spacePressed = true;
    }
    if (Input.GetKeyUp("space")) {
      spacePressed = false;
    }
    Debug.Log("space pressed " + spacePressed);

    DodgerUpdateMessage dum = createDodgerControlMessage(spacePressed);
    string serializedMsg = dum.encodeMessage();
    if (serializedMsg != previousState) {
      Debug.Log("----------------------------------------------------");
      this.messagesToSend.Add(dum);
      previousState = serializedMsg;
      // this is to make sure its still working locally. get rid of this once
      // networking is actually working
      //receiveControlMessage(serializedMsg);
    }
  }

  GameObject createPlayer() {
    Transform transform = GameObject.Instantiate(Player, Vector2.zero, Quaternion.identity) as Transform;
    return transform.gameObject;
  }

  void createPlayers(int n) {
    players = new GameObject[n];
    for (int i = 0; i < n; i++) {
      players [i] = createPlayer();
    }
  }

  void layoutPlayers() {
    int numPlayers = players.Length;
    for (int i = 0; i < numPlayers; i++) {
      GameObject player = players [i];
      player.transform.position = new Vector2 (getPlayerXPosition(i, numPlayers), playerObjectCenterOffsetY);
    }
  }

  float getPlayerXPosition(int playerNumber, int totalPlayers) {
    float screenWidth = 10;
    float position = screenWidth * (1.0f * playerNumber + 1) / (1.0f * totalPlayers + 1);
    return position - screenWidth / 2.0f;
  }

  player getPlayer(int n) {
    GameObject player = players [n];
    return player.GetComponentInChildren<player>();
  }

  DodgerUpdateMessage createDodgerControlMessage(bool dodging) {
    DodgerUpdateMessage message = new DodgerUpdateMessage ();
    message.dodging = dodging;
    message.playerNumber = playerNumber;
    message.health = getPlayer(playerNumber).health;
    return message;
  }

  void receiveControlMessage(DodgerUpdateMessage dum) {
    Debug.Log("Getting DodgerUpdateMessage for player " + dum.playerNumber +". i am " + this.playerNumber);
    player playerScript = getPlayer(dum.playerNumber);
    playerScript.protect = dum.dodging;
    playerScript.health = dum.health;
  }
  
  // Implementing ClientListener
  public void onMessage(NetworkMessage message) {
    string messageType = message.thisMessageType();
    if (messageType == typeof(DodgerUpdateMessage).FullName) {
      receiveControlMessage((DodgerUpdateMessage)message);
    } else {
      Debug.LogError("Dodger PlayerController could not handle message " + message);
    }
  }
  
  public List<NetworkMessage> getMessagesToSend() {
    List<NetworkMessage> messagesToSendPointer = this.messagesToSend;
    this.messagesToSend = new List<NetworkMessage> ();
    return messagesToSendPointer;
  }
  
  public void connectedPlayerIpsDidChange(string[] connectedPlayerIps, int playerIndex) {
    this.connectedPlayerIps = connectedPlayerIps;
    this.playerNumber = playerIndex;
    if (!this.gameStarted) {
      this.gameStarted = true;
      startNewGame();
    }
  }
}

public class DodgerUpdateMessage : NetworkMessage {
  
  public bool dodging;
  public int health;
  public int playerNumber;

  public DodgerUpdateMessage () {
  
  }

  public DodgerUpdateMessage (bool dodging, int health, int playerNumber) {
    this.dodging = dodging;
    this.health = health;
    this.playerNumber = playerNumber;
  }

  public override string encodeMessageData() {
    return dodging + NetworkMessage.DATA_DELIMITER.ToString() + 
      health + NetworkMessage.DATA_DELIMITER.ToString() + 
      playerNumber;
  }

  protected override void decodeMessageData(string msgData) {
    string[] splitMsg = msgData.Split(NetworkMessage.DATA_DELIMITER);
    this.dodging = "True" == splitMsg [0];
    this.health = int.Parse(splitMsg [1]);
    this.playerNumber = int.Parse(splitMsg [2]);
  }
  
  public override string thisMessageType() {
    return typeof(DodgerUpdateMessage).FullName;
  }
}

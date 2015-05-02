using UnityEngine;
using System.Collections;

/*
 * structure of message
 * message type: string
 * data: whatever
 */

public class PlayerController : MonoBehaviour
{
  public Transform Player;
  public float playerObjectCenterOffsetX = 0;
  public float playerObjectCenterOffsetY = 0;
  private bool spacePressed;
  private GameObject[] players;
  private int playerNumber = 0;

  // Use this for initialization
  void Start ()
  {
    createPlayers (3);
    layoutPlayers ();
    player player = getPlayer (playerNumber);
    player.networkPlayer = false;
  }
  
  // Update is called once per frame
  void Update ()
  {
    // input logic
    if (Input.GetKeyDown ("space")) {
      spacePressed = true;
    }
    if (Input.GetKeyUp ("space")) {
      spacePressed = false;
    }

    sendControlUpdate (spacePressed);
    // this is to make sure its still working locally. get rid of this once
    // networking is actually working
    DodgerMessage msg = createDogderControlMessage (spacePressed);
    receiveControlMessage (msg);
  }

  GameObject createPlayer ()
  {
    Transform transform = GameObject.Instantiate (Player, Vector2.zero, Quaternion.identity) as Transform;
    return transform.gameObject;
  }

  void createPlayers (int n)
  {
    players = new GameObject[n];
    for (int i = 0; i < n; i++) {
      players [i] = createPlayer ();
    }
  }

  void layoutPlayers ()
  {
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
    return player.GetComponentInChildren<player> ();
  }

  void sendControlUpdate(bool dodging) {
    DodgerMessage msg = createDogderControlMessage (dodging);

    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(msg.GetType());
    // do the actual sending here.
  }

  DodgerMessage createDogderControlMessage(bool dodging) {
    DodgerUpdateMessage msgData = new DodgerUpdateMessage ();
    msgData.dodging = dodging;
    msgData.playerNumber = playerNumber;
    msgData.health = getPlayer (playerNumber).health;
    DodgerMessage msg = new DodgerMessage ();
    msg.data = msgData;
    msg.type = "control";
    return msg;
  }

  void receiveControlMessage(DodgerMessage msg) {
    if (msg.type == "control") {
      DodgerUpdateMessage controlMsg = (DodgerUpdateMessage) msg.data;
      if (controlMsg.playerNumber == playerNumber) {
        Debug.LogError("got a control message for self. somethings wrong");
      }

      player playerScript = getPlayer(controlMsg.playerNumber);
      playerScript.protect = spacePressed;
      playerScript.health = controlMsg.health;
    }
  }
}

public class DodgerMessage {
  public string type;
  public object data;
}

public class DodgerUpdateMessage {
  public bool dodging;
  public int health;
  public int playerNumber;
}

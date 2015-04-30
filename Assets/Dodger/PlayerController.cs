using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
  public Transform Player;
  public float playerObjectCenterOffsetX = 0;
  public float playerObjectCenterOffsetY = 0;
  private bool spacePressed;
  private GameObject[] players;

  // Use this for initialization
  void Start ()
  {
    createPlayers (3);
    layoutPlayers ();
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

    GameObject player = players [0];
    player playerScript = player.GetComponentInChildren<player> ();
    playerScript.protect = spacePressed;
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
}

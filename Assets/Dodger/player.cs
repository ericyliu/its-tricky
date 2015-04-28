using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class player : MonoBehaviour {

  public float damageTime = 1;
  public Color damageColor = Color.red;
  public int maxHealth = 100;

  private Rigidbody2D rigidBody;
  private float damageTimeLeft;
  private bool spacePressed;
  private Text healthText;
  private int health;

  // Use this for initialization
	void Start () {
    rigidBody = GetComponent<Rigidbody2D>();
    damageTimeLeft = damageTime;
    healthText = gameObject.GetComponentInChildren<Text> ();
    health = maxHealth;
	}
	
	// Update is called once per frame
	void Update () {
    // restore original player color
    damageTimeLeft -= Time.deltaTime;
    if (damageTimeLeft < 0) {
      damageTimeLeft = damageTime;
      gameObject.GetComponent<Renderer>().material.color = Color.white;
    }

    // input logic
    if (Input.GetKeyDown ("space")) {
      spacePressed = true;
    }
    if (Input.GetKeyUp ("space")) {
      spacePressed = false;
    }

    // update health label
    healthText.text = "" + health;
	}

  void FixedUpdate() {
    if (spacePressed) {
      health = Mathf.Max(health - 1, 0);
    }
  }

  void OnTriggerEnter2D (Collider2D collidee) {
    if (!spacePressed) {
      playerHit();
    }
    Rigidbody2D collideeRigidBody = collidee.gameObject.GetComponent<Rigidbody2D> ();
    collideeRigidBody.AddForce (Vector2.right * 100 + Vector2.up * 900);
  }

  void playerHit() {
    gameObject.GetComponent<Renderer>().material.color = damageColor;
    health = Mathf.Max(health - 10, 0);
  }

  void playerDead() {

  }
}

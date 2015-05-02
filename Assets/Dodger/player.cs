using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class player : MonoBehaviour
{

  public float damageTime = 1;
  public Color damageColor = Color.red;
  public int maxHealth = 100;
  public float protectDamageScale = .2f;
  public bool protect;
  public int health;
  public bool networkPlayer = true;
  private Rigidbody2D rigidBody;

  private float damageTimeLeft;
  private Text healthText;
  private float floatHealth;

  // Use this for initialization
  void Start ()
  {
    rigidBody = GetComponent<Rigidbody2D> ();
    damageTimeLeft = damageTime;
    healthText = gameObject.GetComponentInChildren<Text> ();
    floatHealth = maxHealth;
  }
  
  // Update is called once per frame
  void Update ()
  {
    // update health label
    health = (int)Mathf.RoundToInt (floatHealth);
    healthText.text = "" + health;
  }

  void FixedUpdate ()
  {
    if (protect) {
      floatHealth = Mathf.Max (floatHealth - protectDamageScale, 0);
    }

    // restore original player color
    damageTimeLeft -= Time.deltaTime;
    if (damageTimeLeft < 0) {
      Color colorToUpdate = Color.white;
      if (protect) {
        colorToUpdate = Color.yellow;
      }
      gameObject.GetComponent<Renderer> ().material.color = colorToUpdate;
    }
  }

  void OnTriggerEnter2D (Collider2D collidee)
  {
    if (!protect) {
      playerHit ();
    }
    GameObject collideeObject = collidee.gameObject;
    Rigidbody2D collideeRigidBody = collideeObject.GetComponent<Rigidbody2D> ();
    collideeObject.GetComponent<Collider2D> ().enabled = false;

    collideeRigidBody.AddForce (Vector2.right * 100 + Vector2.up * 900);
  }

  void playerHit ()
  {
    gameObject.GetComponent<Renderer> ().material.color = damageColor;
    damageTimeLeft = damageTime;
    if (!networkPlayer) {
      floatHealth = Mathf.Max (floatHealth - 10, 0);
    }
  }
}

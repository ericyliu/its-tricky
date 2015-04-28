using UnityEngine;
using System.Collections;

public class rocketEmitter : MonoBehaviour
{
  public float timeToNextEmit = 0;
  public Transform projectile;
  private GameObject player;
  // Use this for initialization
  void Start ()
  {
    
  }
  
  // Update is called once per frame
  void Update ()
  {
    timeToNextEmit -= Time.deltaTime;
    if (timeToNextEmit < 0) {
      timeToNextEmit = 2;

      Instantiate (projectile, new Vector3 (0, 6, 0), Quaternion.identity);
    }
  }

  void FixedUpdate ()
  {
  }

  void spawnProjectile ()
  {

  }
 
}

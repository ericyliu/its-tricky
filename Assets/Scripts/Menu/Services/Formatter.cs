using UnityEngine;
using System.Collections;
using System.Text;

public class Formatter {

  public static byte[] Format (string route, string message) {
    return Encoding.ASCII.GetBytes(route + "|" + message);
  }

}

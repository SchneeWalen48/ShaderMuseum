using UnityEngine;

public class FxSpin : MonoBehaviour
{
  public Vector3 axis = Vector3.up;
  public float degPerSec = 5f;
  public bool useWorld = false;
  void Update()
  {
    transform.Rotate(axis, degPerSec * Time.deltaTime, useWorld? Space.World : Space.Self);
  }
}

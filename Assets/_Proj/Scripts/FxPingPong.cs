using UnityEngine;

public class FxPingPong : MonoBehaviour
{
  public Vector3 dir = Vector3.right;
  public float dist = 1f;
  public float speed = 1f;
    public bool localSpace = true;

  Vector3 origin;
  void Awake()
  {
   origin = localSpace?transform.localPosition:transform.position; 
  }

  void Update()
  {
    float offset = Mathf.Sin(Time.time * speed) * dist;
    Vector3 pos = origin + (localSpace ? dir : transform.TransformDirection(dir) * offset);
    if (localSpace) transform.localPosition = pos;
    else transform.position = pos;
  }
}

using UnityEngine;

public class SkyboxTrigger : MonoBehaviour
{
  public EnvironmentMode mode;

  void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      Debug.Log("Entered: " + mode);
      FindObjectOfType<SkyboxManager>().ChangeEnvironment(mode);
    }
  }
}

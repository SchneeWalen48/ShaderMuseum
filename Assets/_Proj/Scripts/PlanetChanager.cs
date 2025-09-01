using UnityEngine;

public class PlanetChanager : MonoBehaviour
{
  public GameObject[] planetPrefabs;

  public Transform spawnPoint;

  private GameObject currPlanet;

  private int idx = 0;

  void Start()
  {
    ShowPlanet(idx);
  }

  void ShowPlanet(int i)
  {
    if (currPlanet != null)
      Destroy(currPlanet);
    currPlanet = Instantiate(planetPrefabs[i], spawnPoint.position, spawnPoint.rotation);
  }
  public void NextPlanet()
  {
    idx = (idx + 1) % planetPrefabs.Length;
    ShowPlanet(idx);
  }

  public void PreviousPlanet()
  {
    idx = (idx - 1 + planetPrefabs.Length) % planetPrefabs.Length;
    ShowPlanet(idx);
  }
}

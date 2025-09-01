using System.Collections;
using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
  public Material currSkybox;
  public float transitionSpeed = 2f;

  private bool isFading;

  void Start()
  {
    currSkybox = RenderSettings.skybox;
  }

  public void ChangeSkybox(Material skybox)
  {
    if(!isFading && skybox != null && skybox != currSkybox)
      StartCoroutine(FadeSkybox(skybox));
  }

  IEnumerator FadeSkybox(Material skybox)
  {
    isFading = true;

    float t = 0;
    while (t < transitionSpeed)
    {
      t += Time.deltaTime;
      float lerp = 1-(t/transitionSpeed);
      if(currSkybox.HasProperty("_Exposure"))
        currSkybox.SetFloat("_Exposure", lerp);
      yield return null;
    }

    currSkybox = skybox;
    RenderSettings.skybox = currSkybox;
    DynamicGI.UpdateEnvironment();

    t = 0;
    while(t< transitionSpeed)
    {
      t += Time.deltaTime;
      float lerp = (t/transitionSpeed);
      if (currSkybox.HasProperty("_Exposure"))
        currSkybox.SetFloat("_Exposure", lerp);
      yield return null;
    }
    isFading = false;
  }
}

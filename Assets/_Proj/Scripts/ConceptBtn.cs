using System;
using UnityEngine;

public class ConceptBtn : MonoBehaviour
{
  public enum ConcetpType { Rain, Bird, Ocean}
  public ConcetpType type;

  public AudioSource source;
  public AudioClip clip;

  public Material skyboxMat;

  public GameObject effectPrefab;
  private GameObject effectInstance;

  public void OnPressed()
  {
    PlayAudio();
    ChangeSkybox();
    PlayEffect();
  }

  void PlayAudio()
  {
    if(source && clip)
    {
      source.clip = clip;
      source.Play();
    }
  }

  void ChangeSkybox()
  {
    if (skyboxMat)
    {
      RenderSettings.skybox = skyboxMat;
      DynamicGI.UpdateEnvironment();
    }
  }

  void PlayEffect()
  {
    if (effectPrefab)
    {
      if(effectInstance != null) Destroy(effectInstance);
      effectInstance = Instantiate(effectPrefab, Vector3.zero, Quaternion.identity);
    }
  }
}

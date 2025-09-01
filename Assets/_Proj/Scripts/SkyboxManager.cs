using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public enum EnvironmentMode
{
  RoseGold,
  Space,
  Nature,
  Cyber
}
public class SkyboxManager : MonoBehaviour
{
  public Material mainMat;
  public Material spaceMat;
  public Material natureMat;
  public Material cyberMat;

  public GameObject spaceParticles;
  public GameObject natureParticles;
  public GameObject cyberParticles;

  public float transDuration = 5f;
  public float holdDuration = 20f;

  private Coroutine cycleRoutine;
  private EnvironmentMode currMode;

  private LightingCycle lightCycle;

  void Awake()
  {
    lightCycle = FindObjectOfType<LightingCycle>();
    spaceParticles.SetActive(false);
    natureParticles.SetActive(false);
    cyberParticles.SetActive(false);
  }
  void Start()
  {
    RenderSettings.skybox = mainMat;
    DynamicGI.UpdateEnvironment();
    ChangeEnvironment(EnvironmentMode.RoseGold);
  }

  public void ChangeEnvironment(EnvironmentMode mode)
  {
    if(currMode == mode) return;
    currMode = mode;

    switch (mode)
    {
      case EnvironmentMode.RoseGold:
        RenderSettings.skybox = mainMat;
        StopDayCycle();
        ActivateParticles(null);
        break;
      case EnvironmentMode.Space:
        RenderSettings.skybox = spaceMat;
        StopDayCycle();
        ActivateParticles(spaceParticles);
        break;
      case EnvironmentMode.Nature:
        RenderSettings.skybox = natureMat;
        StartDayCycle(natureMat, natureParticles);
        break;
      case EnvironmentMode.Cyber:
        RenderSettings.skybox = cyberMat;
        StartDayCycle(cyberMat, cyberParticles);
        break;
    }
    DynamicGI.UpdateEnvironment();
  }

  void ActivateParticles(GameObject target)
  {
    spaceParticles.SetActive(false);
    natureParticles.SetActive(false);
    cyberParticles.SetActive(false);

    if (target != null)
    {
      target.SetActive(true);

      // 파티클 강제 재생
      foreach (var ps in target.GetComponentsInChildren<ParticleSystem>())
      {
        ps.Clear();   // 이전 잔여 입자 삭제
        ps.Play();    // 강제 실행
      }
    }
  }

  void StartDayCycle(Material mat, GameObject particles)
  {
    StopDayCycle();
    ActivateParticles(particles);
    cycleRoutine = StartCoroutine(lightCycle.LightCycleRoutine());
  }

  void StopDayCycle()
  {
    if(cycleRoutine != null) StopCoroutine(cycleRoutine);
    cycleRoutine = null;
  }
}

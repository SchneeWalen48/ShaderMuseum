using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region Overlay Fx Spec Class
[Serializable]
public class OverlayFxSpec
{
  public string name;
  public GameObject prefab;

  [Header("Anchor & Transform")]
  public Transform anchor; // 비우면 NatureEffectOverlay 하위
  public Vector3 localPosition;
  public Vector3 localEuler;
  public Vector3 localScale = Vector3.one;

  [Header("Particle Tweaks (배수)")]
  [Min(0f)] public float simulationSpeed = 1f;
  [Min(0f)] public float startSpeedMul = 1f;
  [Min(0f)] public float startLifetimeMul = 1f;
  [Min(0f)] public float emissionMul = 1f;

  [Header("Optional Movement")]
  public bool spin;
  public Vector3 spinAxis = Vector3.up;
  public float spinDegPerSec = 0f;

  public bool pingPong;
  public Vector3 pingDir = Vector3.right;
  public float pingDistance = 0f;
  public float pingSpeed = 0f;
}
#endregion
public class NatureBtn : MonoBehaviour
{
  public enum ConcetpType { Rain, Jungle, Beach }
  public ConcetpType type;

  [Header("Managers")]
  public EnvironmentManager skyboxManager;

  [Header("Audio")]
  public AudioSource source;
  public AudioClip clip;
  [Range(0f, 2f)] public float volume = 1f;
  public float playDuration = 7f;
  public float fadeOutSeconds = 2f;

  [Header("Skybox")]
  public Material skyboxMat;             // 버튼 전용 스카이박스
  public Material defaultNatureSkybox;   // 버튼 종료 후 되돌아갈 스카이박스

  [Header("Effect")]
  public GameObject effectPrefab;        // 버튼 전용 이펙트 프리팹(오버레이)
  public Transform effectParent;         
  private GameObject effectInstance;

  private static NatureBtn activeBtn;
  private Coroutine routine;

  public List<OverlayFxSpec> extraOverlays = new();

  // 내부 인스턴스 추적
  readonly System.Collections.Generic.List<GameObject> _extraInstances = new();

  #region Spawn Splash params

  [SerializeField] GameObject splashPrefab;
  [SerializeField] Vector3 splashAreaSize = new Vector3(20f, 0, 20f);
  [SerializeField] float spawnIntervalMin = 0.01f;
  [SerializeField] float spawnIntervalMax = 0.1f;
  private Coroutine splashCoroutine;
  private readonly List<GameObject> _spawnedSplashes = new();

  #endregion

  #region Wave params

  [SerializeField] GameObject wavePrefab;
  [SerializeField] GameObject waveRipplePrefab;
  [SerializeField] Vector3 waveSpawnPos = new Vector3(0, 0, -10f);
  [SerializeField] float waveInterval = 1.5f;

  private Coroutine waveCoroutine;

  #endregion

  public void OnPressed()
  {
    // 다른 버튼 효과 즉시 정리
    if (activeBtn != null && activeBtn != this)
      activeBtn.StopCurrEffect();
    activeBtn = this;

    if (skyboxManager) skyboxManager.NatureBtn();
    else if (EnvironmentManager.I) EnvironmentManager.I.NatureBtn();

    if (routine != null) StopCoroutine(routine);
    routine = StartCoroutine(PlayRoutine());
  }

  IEnumerator PlayRoutine()
  {
    ApplySkyboxVariant();
    SpawnAndForcePlayEffect();
    PlayAudioOneShotLooped();

    if(type == ConcetpType.Beach && wavePrefab != null)
    {
      if (waveCoroutine != null) StopCoroutine(waveCoroutine);
      waveCoroutine = StartCoroutine(WaveRoutine(playDuration));
    }
    if (type == ConcetpType.Rain && splashPrefab != null)
    {
      if (splashCoroutine != null) StopCoroutine(splashCoroutine);
      splashCoroutine = StartCoroutine(SpawnSplashRoutine(playDuration));
    }

    yield return new WaitForSeconds(playDuration);

    yield return StartCoroutine(FadeOutAudio(fadeOutSeconds));
    ResetToNature();
  }

  #region Particle Spawn System
  void SpawnAndForcePlayEffect()
  {
    if (!effectParent)
    {
      var rootName = "NatureEffectOverlay";
      var rootTr = GameObject.Find(rootName)?.transform;
      if (!rootTr)
      {
        var go = new GameObject(rootName);
        rootTr = go.transform;
        if (EnvironmentManager.I) go.transform.SetParent(EnvironmentManager.I.transform);
      }
      effectParent = rootTr;
    }

    if (effectPrefab)
    {
      if (effectInstance) Destroy(effectInstance);
      effectInstance = Instantiate(effectPrefab, Vector3.zero, Quaternion.identity, effectParent);

      foreach (var ps in effectInstance.GetComponentsInChildren<ParticleSystem>(true))
      {
        var main = ps.main; main.loop = true; main.stopAction = ParticleSystemStopAction.None;
        var emission = ps.emission; emission.enabled = true;
        ps.Clear(true); ps.Play(true);
      }
      effectInstance.SetActive(true);
    }

    // (추가) 여러 개 오버레이 FX
    foreach (var s in extraOverlays)
    {
      if (s == null || s.prefab == null) continue;

      // 부모(앵커) 결정: Element에 Anchor가 있으면 그걸 쓰고, 없으면 effectParent 사용
      Transform parent = s.anchor != null ? s.anchor : effectParent;
      if (parent == null)
      {
        // effectParent가 아직 없다면 안전하게 하나 만들기
        var go = GameObject.Find("NatureEffectOverlay") ?? new GameObject("NatureEffectOverlay");
        effectParent = go.transform;
        if (EnvironmentManager.I) effectParent.SetParent(EnvironmentManager.I.transform);
        parent = effectParent;
      }

      var inst = Instantiate(s.prefab, parent);
      _extraInstances.Add(inst);

      inst.transform.localPosition = s.localPosition;
      inst.transform.localEulerAngles = s.localEuler;
      inst.transform.localScale = (s.localScale == Vector3.zero) ? Vector3.one : s.localScale;
      inst.SetActive(true);

      // 파티클 튜닝
      foreach (var ps in inst.GetComponentsInChildren<ParticleSystem>(true))
      {
        var m = ps.main;
        if (s.simulationSpeed <= 0f) s.simulationSpeed = 1f;
        if (s.startSpeedMul <= 0f) s.startSpeedMul = 1f;
        if (s.startLifetimeMul <= 0f) s.startLifetimeMul = 1f;
        if (s.emissionMul <= 0f) s.emissionMul = 1f;

        m.simulationSpeed *= s.simulationSpeed;
        if (s.startSpeedMul != 1f) m.startSpeed = ScaleCurve(m.startSpeed, s.startSpeedMul);
        if (s.startLifetimeMul != 1f) m.startLifetime = ScaleCurve(m.startLifetime, s.startLifetimeMul);

        var e = ps.emission;
        if (s.emissionMul != 1f)
        {
          e.rateOverTime = ScaleCurve(e.rateOverTime, s.emissionMul);
          e.rateOverDistance = ScaleCurve(e.rateOverDistance, s.emissionMul);
        }

        ps.Clear(true);
        ps.Play(true);
      }

      // 움직임 옵션
      if (s.spin && Mathf.Abs(s.spinDegPerSec) > 0.0001f)
      {
        var spin = inst.GetComponent<FxSpin>() ?? inst.AddComponent<FxSpin>();
        spin.axis = (s.spinAxis == Vector3.zero) ? Vector3.up : s.spinAxis;
        spin.degPerSec = s.spinDegPerSec;
        spin.useWorld = false;
      }
      if (s.pingPong && s.pingDistance > 0f && s.pingSpeed > 0f)
      {
        var ping = inst.GetComponent<FxPingPong>() ?? inst.AddComponent<FxPingPong>();
        ping.dir = (s.pingDir == Vector3.zero) ? Vector3.right : s.pingDir;
        ping.dist = s.pingDistance;
        ping.speed = s.pingSpeed;
        ping.localSpace = true;
      }
    }
  }
  void OnValidate()
  {
    // extraOverlays가 있다면 0값들을 1로 보정
    if (extraOverlays == null) return;
    foreach (var s in extraOverlays)
    {
      if (s == null) continue;
      if (s.localScale == Vector3.zero) s.localScale = Vector3.one;
      if (s.simulationSpeed <= 0f) s.simulationSpeed = 1f;
      if (s.startSpeedMul <= 0f) s.startSpeedMul = 1f;
      if (s.startLifetimeMul <= 0f) s.startLifetimeMul = 1f;
      if (s.emissionMul <= 0f) s.emissionMul = 1f;
    }
  }

  static ParticleSystem.MinMaxCurve ScaleCurve(ParticleSystem.MinMaxCurve c, float mul)
  {
    switch (c.mode)
    {
      case ParticleSystemCurveMode.Constant: c.constant *= mul; break;
      case ParticleSystemCurveMode.TwoConstants: c.constantMin *= mul; c.constantMax *= mul; break;
      case ParticleSystemCurveMode.Curve: c.curveMultiplier *= mul; break;
      case ParticleSystemCurveMode.TwoCurves: c.curveMultiplier *= mul; break;
    }
    return c;
  }

  void ApplySkyboxVariant()
  {
    if (skyboxMat)
    {
      RenderSettings.skybox = skyboxMat;
      DynamicGI.UpdateEnvironment();
    }
  }

  void CleanupOverlays()
  {
    // 단일 인스턴스
    if (effectInstance) Destroy(effectInstance);
    effectInstance = null;

    // 추가
    for (int i = _extraInstances.Count - 1; i >= 0; --i)
      if (_extraInstances[i]) Destroy(_extraInstances[i]);
    _extraInstances.Clear();

    // Splashes
    for (int i = _spawnedSplashes.Count - 1; i >= 0; --i)
      if (_spawnedSplashes[i]) Destroy(_spawnedSplashes[i]);
    _spawnedSplashes.Clear();
  }

  void ResetToNature()
  {
    if (type == ConcetpType.Rain)
    {
      if (splashCoroutine != null)
      {
        StopCoroutine(splashCoroutine);
        splashCoroutine = null;
      }
      for (int i = _spawnedSplashes.Count - 1; i >= 0; --i)
        if (_spawnedSplashes[i]) Destroy(_spawnedSplashes[i]);
      _spawnedSplashes.Clear();
    }

    CleanupOverlays();

    // 스카이박스 원복
    if (defaultNatureSkybox)
    {
      RenderSettings.skybox = defaultNatureSkybox;
      DynamicGI.UpdateEnvironment();
    }

    if (skyboxManager)
      skyboxManager.ResumeNatureAfterBtn();
    else if (EnvironmentManager.I)
      EnvironmentManager.I.ResumeNatureAfterBtn();

    CleanupOverlays();

    if (activeBtn == this) activeBtn = null;
  }
  #endregion

  #region Audio
  void PlayAudioOneShotLooped()
  {
    if (source && clip)
    {
      source.clip = clip;
      source.volume = volume;
      source.loop = true;
      source.Play();
    }
  }

  IEnumerator FadeOutAudio(float duration)
  {
    if (!source) yield break;

    float start = source.volume;
    float t = 0f;
    while (t < duration)
    {
      t += Time.deltaTime;
      source.volume = Mathf.Lerp(start, 0f, t / duration);
      yield return null;
    }
    source.Stop();
    source.volume = volume;
    source.loop = false;
  }
  #endregion

  public void StopCurrEffect()
  {
    if (routine != null) StopCoroutine(routine);
    routine = null;

    if (source)
    {
      source.Stop();
      source.volume = volume;
      source.loop = false;
    }

    CleanupOverlays();

    ResetToNature();
  }

  #region Splash
  IEnumerator SpawnSplashRoutine(float duration)
  {
    float timer = 0f;
    while (timer < duration)
    {
      Vector3 randPos = new Vector3(
        UnityEngine.Random.Range(-splashAreaSize.x * 0.5f, splashAreaSize.x * 0.5f),
            0f,
            UnityEngine.Random.Range(-splashAreaSize.z * 0.5f, splashAreaSize.z * 0.5f)
        );

      var splash = Instantiate(splashPrefab, effectParent);
      splash.transform.localPosition = randPos;
      _spawnedSplashes.Add(splash);

      float wait = UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
      yield return new WaitForSeconds(wait);

      timer += wait;
    }
  }
  #endregion

  #region Wave
  IEnumerator WaveRoutine(float duration)
  {
    float t = 0f;
    while (t < duration)
    {
      // 큰 파도 생성
      var wave = Instantiate(wavePrefab, effectParent);
      wave.transform.localPosition = new Vector3(0, 0, -15f);

      // 일정 시간 후 Ripple 생성
      yield return new WaitForSeconds(3f);
      for (int i = 0; i < 3; i++)
      {
        Vector3 offset = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-2f, 2f));
        var ripple = Instantiate(waveRipplePrefab, effectParent);
        ripple.transform.localPosition = offset;
      }

      // 다음 파도까지 대기
      yield return new WaitForSeconds(5f);
      t += 8f;
    }
  }
  #endregion
}

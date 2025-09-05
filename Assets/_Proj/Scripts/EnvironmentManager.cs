using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnvironmentMode
{
  None = -1,
  Default,
  Space,
  Nature,
  Cyber,
  Future
}

[System.Serializable]
public class EnvFxSpec
{
  public string name;
  public GameObject prefab;

  [Header("Anchor & Transform")]
  public Transform anchor;                // 비우면 해당 환경 루트 아래에 생성
  public Vector3 localPosition;
  public Vector3 localEuler;
  public Vector3 localScale = Vector3.one;

  [Header("Particle Tweaks (x배)")]
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
public class EnvironmentManager : MonoBehaviour
{
  public static EnvironmentManager I { get; private set; }

  [Header("Skybox Materials")]
  public Material mainMat;
  public Material spaceMat;
  public Material natureMat;
  public Material cyberMat;
  public Material futureMat;

  [Header("Particles (Root GameObjects)")]
  public GameObject spaceParticles;
  public GameObject natureParticles;
  public GameObject cyberParticles;
  public GameObject futureParticles;

  [Header("Extra FX (Optional, spawned under each root)")]
  public List<EnvFxSpec> spaceExtraFx = new();
  public List<EnvFxSpec> natureExtraFx = new();
  public List<EnvFxSpec> cyberExtraFx = new();
  public List<EnvFxSpec> futureExtraFx = new();

  // 내부에서 생성한 인스턴스 추적
  readonly List<GameObject> _spaceExtraInst = new();
  readonly List<GameObject> _natureExtraInst = new();
  readonly List<GameObject> _cyberExtraInst = new();
  readonly List<GameObject> _futureExtraInst = new();


  [Header("BGM Clips")]
  public AudioClip defaultBGM;
  public AudioClip spaceBGM;
  public AudioClip natureBGM;
  public AudioClip cyberBGM;
  public AudioClip futureBGM;

  [Header("Audio Settings")]
  [Range(0f, 2f)] public float bgmVolume = 1f;
  [Min(0f)] public float crossfadeSeconds = 0.8f;

  private Coroutine cycleRoutine;
  private EnvironmentMode currMode = EnvironmentMode.None;
  private LightingCycle lightCycle;

  // BGM 교차 페이드용
  private AudioSource _a;
  private AudioSource _b;
  private Coroutine _crossCo;

  // 파티클 제어용
  private readonly List<GameObject> _allFxRoots = new();

  private bool _natureBtn = false;

  void Awake()
  {
    if (I != null && I != this)
    {
      Destroy(gameObject);
      return;
    }
    I = this;

    lightCycle = FindObjectOfType<LightingCycle>();

    _a = gameObject.AddComponent<AudioSource>();
    _b = gameObject.AddComponent<AudioSource>();

    foreach (var s in new[] { _a, _b })
    {
      s.loop = true;
      s.playOnAwake = false;
      s.volume = 0f;
      s.spatialBlend = 0f; // 2D BGM
    }

    if (spaceParticles) _allFxRoots.Add(spaceParticles);
    if (natureParticles) _allFxRoots.Add(natureParticles);
    if (cyberParticles) _allFxRoots.Add(cyberParticles);
    if (futureParticles) _allFxRoots.Add(futureParticles);
    foreach (var r in _allFxRoots) if (r) r.SetActive(false);
  }

  void Start()
  {
    RenderSettings.skybox = mainMat;
    DynamicGI.UpdateEnvironment();
    ChangeEnvironment(EnvironmentMode.Default);
    CrossfadeBgm(defaultBGM, crossfadeSeconds, bgmVolume);
  }

  public void ChangeEnvironment(EnvironmentMode mode)
  {
    if (currMode == mode) return;
    currMode = mode;

    RenderSettings.skybox = GetMat(mode);
    DynamicGI.UpdateEnvironment();

    EnsureExtrasSpawned(mode);
    ActivateParticlesExclusive(GetFxRoot(mode));

    if (mode == EnvironmentMode.Nature)
    {
      if (_natureBtn)
        StopDayCycle();
      else
      {
        ActivateParticlesExclusive(natureParticles);
        StartDayCycle();
      }
    }
    else
    {
      //Day Cycle
      switch (mode)
      {
        case EnvironmentMode.Cyber:
          StartDayCycle();
          break;
        case EnvironmentMode.Future:
          StartDayCycle();
          break;
        default:
          StopDayCycle();
          break;
      }
    }
    if(!(mode == EnvironmentMode.Nature && _natureBtn))
    CrossfadeBgm(GetBgm(mode), crossfadeSeconds, bgmVolume);
  }

  #region Methods

  void EnsureExtrasSpawned(EnvironmentMode mode)
  {
    switch (mode)
    {
      case EnvironmentMode.Space:
        TrySpawnExtras(spaceParticles, spaceExtraFx, _spaceExtraInst);
        break;
      case EnvironmentMode.Nature:
        TrySpawnExtras(natureParticles, natureExtraFx, _natureExtraInst);
        break;
      case EnvironmentMode.Cyber:
        TrySpawnExtras(cyberParticles, cyberExtraFx, _cyberExtraInst);
        break;
      case EnvironmentMode.Future:
        TrySpawnExtras(futureParticles, futureExtraFx, _futureExtraInst);
        break;
    }
  }
  void OnValidate()
  {
    void Fix(List<EnvFxSpec> list)
    {
      if (list == null) return;
      foreach (var s in list)
      {
        if (s == null) continue;
        if (s.localScale == Vector3.zero) s.localScale = Vector3.one;
        if (s.simulationSpeed <= 0f) s.simulationSpeed = 1f;
        if (s.startSpeedMul <= 0f) s.startSpeedMul = 1f;
        if (s.startLifetimeMul <= 0f) s.startLifetimeMul = 1f;
        if (s.emissionMul <= 0f) s.emissionMul = 1f;
      }
    }
    Fix(spaceExtraFx); Fix(natureExtraFx); Fix(cyberExtraFx); Fix(futureExtraFx);
  }
  void TrySpawnExtras(GameObject root, List<EnvFxSpec> specs, List<GameObject> bucket)
  {
    if (root == null || specs == null) return;

    // 이미 스폰되어 있으면 Skip (루트 아래 활성/비활성만 환경 전환 로직이 담당)
    if (bucket.Count > 0) return;

    foreach (var s in specs)
    {
      if (s == null || s.prefab == null) continue;
      var parent = s.anchor != null ? s.anchor : root.transform;
      var inst = Instantiate(s.prefab, parent);

      inst.transform.localPosition = s.localPosition;
      inst.transform.localEulerAngles = s.localEuler;
      inst.transform.localScale = s.localScale;
      bucket.Add(inst);

      // 파티클 튜닝
      foreach (var ps in inst.GetComponentsInChildren<ParticleSystem>(true))
      {
        if (s.simulationSpeed != 1f) { var m = ps.main; m.simulationSpeed *= s.simulationSpeed; }
        if (s.startSpeedMul != 1f || s.startLifetimeMul != 1f)
        {
          var m = ps.main;
          if (s.startSpeedMul != 1f) m.startSpeed = ScaleCurve(m.startSpeed, s.startSpeedMul);
          if (s.startLifetimeMul != 1f) m.startLifetime = ScaleCurve(m.startLifetime, s.startLifetimeMul);
        }
        if (s.emissionMul != 1f)
        {
          var e = ps.emission;
          e.rateOverTime = ScaleCurve(e.rateOverTime, s.emissionMul);
          e.rateOverDistance = ScaleCurve(e.rateOverDistance, s.emissionMul);
        }
      }

      // 선택 움직임
      if (s.spin)
      {
        var sp = inst.AddComponent<FxSpin>();
        sp.axis = s.spinAxis;
        sp.degPerSec = s.spinDegPerSec;
        sp.useWorld = false;
      }
      if (s.pingPong && s.pingDistance > 0f && s.pingSpeed > 0f)
      {
        var pp = inst.AddComponent<FxPingPong>();
        pp.dir = s.pingDir;
        pp.dist = s.pingDistance;
        pp.speed = s.pingSpeed;
        pp.localSpace = true;
      }
    }
  }
  void DeactivateExtraFor(EnvironmentMode mode)
  {
    switch (mode)
    {
      case EnvironmentMode.Nature: SetActiveList(_natureExtraInst, false); break;
      case EnvironmentMode.Space: SetActiveList(_spaceExtraInst, false); break;
      case EnvironmentMode.Cyber: SetActiveList(_cyberExtraInst, false); break;
      case EnvironmentMode.Future: SetActiveList(_futureExtraInst, false); break;
    }
  }

  void SetActiveList(List<GameObject> list, bool on)
  {
    if (list == null) return;
    foreach (var go in list) if (go) go.SetActive(on);
  }

  // MinMaxCurve 배율 유틸
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

  Material GetMat(EnvironmentMode m) => m switch
  {
    EnvironmentMode.Default => mainMat,
    EnvironmentMode.Space => spaceMat,
    EnvironmentMode.Nature => natureMat,
    EnvironmentMode.Cyber => cyberMat,
    EnvironmentMode.Future => futureMat,
    _ => mainMat
  };

  GameObject GetFxRoot(EnvironmentMode m) => m switch
  {
    EnvironmentMode.Space => spaceParticles,
    EnvironmentMode.Nature => natureParticles,
    EnvironmentMode.Cyber => cyberParticles,
    EnvironmentMode.Future => futureParticles,
    _ => null
  };

  AudioClip GetBgm(EnvironmentMode m) => m switch
  {
    EnvironmentMode.Default => defaultBGM,
    EnvironmentMode.Space => spaceBGM,
    EnvironmentMode.Nature => natureBGM,
    EnvironmentMode.Cyber => cyberBGM,
    EnvironmentMode.Future => futureBGM,
    _ => null
  };

  void ActivateParticlesExclusive(GameObject target)
  {
    foreach (var r in _allFxRoots) if (r) r.SetActive(false);

    if (target != null)
    {
      target.SetActive(true);
      foreach (var ps in target.GetComponentsInChildren<ParticleSystem>(true))
      {
        ps.Clear(true);
        ps.Play(true);
      }
    }
  }

  void StartDayCycle()
  {
    StopDayCycle();
    if (lightCycle != null)
      cycleRoutine = StartCoroutine(lightCycle.LightCycleRoutine());
  }

  void StopDayCycle()
  {
    if (cycleRoutine != null) StopCoroutine(cycleRoutine);
    cycleRoutine = null;
  }

  void CrossfadeBgm(AudioClip next, float dur, float targetVol)
  {
    if (next == null)
    {
      if (_crossCo != null) StopCoroutine(_crossCo);
      _crossCo = StartCoroutine(FadeOutAll(dur));
      return;
    }

    var from = _a.volume >= _b.volume ? _a : _b;
    var to = from == _a ? _b : _a;

    to.clip = next;
    to.time = 0f;
    to.volume = 0f;
    to.Play();

    if (_crossCo != null) StopCoroutine(_crossCo);
    _crossCo = StartCoroutine(CoCrossfade(from, to, dur, targetVol));
  }

  IEnumerator CoCrossfade(AudioSource from, AudioSource to, float dur, float toVol)
  {
    float t = 0f;
    float fromStart = from.volume;
    while (t < dur)
    {
      t += Time.deltaTime;
      float k = dur > 0f ? t / dur : 1f;
      from.volume = Mathf.Lerp(fromStart, 0f, k);
      to.volume = Mathf.Lerp(0f, toVol, k);
      yield return null;
    }
    from.volume = 0f; from.Stop();
    to.volume = toVol;
  }

  IEnumerator FadeOutAll(float dur)
  {
    float t = 0f;
    float av = _a.volume;
    float bv = _b.volume;
    while (t < dur)
    {
      t += Time.deltaTime;
      float k = dur > 0f ? t / dur : 1f;
      _a.volume = Mathf.Lerp(av, 0f, k);
      _b.volume = Mathf.Lerp(bv, 0f, k);
      yield return null;
    }
    _a.Stop(); _b.Stop();
  }

  public void NatureBtn()
  {
    if (currMode != EnvironmentMode.Nature) return;
    _natureBtn = true;
    ActivateParticlesExclusive(null);
    CrossfadeBgm(null, 0.25f, 0f);
  }

  public void ResumeNatureAfterBtn()
  {
    if (currMode != EnvironmentMode.Nature) return;
    _natureBtn = false;

    ActivateParticlesExclusive(natureParticles);
    CrossfadeBgm(natureBGM, 0.25f, bgmVolume);
  }
  #endregion
}

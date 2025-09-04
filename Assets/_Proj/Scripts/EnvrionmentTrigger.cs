using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SubPlatformTrigger : MonoBehaviour
{
  [Header("환경 전환")]
  public EnvironmentMode mode = EnvironmentMode.Default;

  [Header("옵션")]
  [Tooltip("같은 플랫폼 위에서 진동/재트리거 방지용 쿨다운(초)")]
  public float retriggerCooldown = 0.5f;

  [Header("효과음(선택)")]
  public AudioSource sfxBus;        // 2D 원샷 버스 권장 (spatialBlend=0)
  public AudioClip enterSfx;
  [Range(0f, 2f)] public float sfxVolume = 1f;

  float _lastTrigTime = -999f;

  void Reset()
  {
    var col = GetComponent<Collider>();
    col.isTrigger = true;
  }

  void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player")) return;

    // 재트리거 방지
    if (Time.time - _lastTrigTime < retriggerCooldown) return;
    _lastTrigTime = Time.time;

    if (EnvironmentManager.I != null)
    {
      EnvironmentManager.I.ChangeEnvironment(mode);
    }

    if (sfxBus != null && enterSfx != null)
    {
      sfxBus.PlayOneShot(enterSfx, sfxVolume);
    }
  }
}

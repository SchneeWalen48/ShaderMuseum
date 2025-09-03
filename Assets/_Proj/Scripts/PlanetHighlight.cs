using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlanetHighlight : MonoBehaviour
{
  [Header("Material Target")]
  public Renderer highlightRenderer;   // Shader Graph 머티리얼 적용된 오브젝트

  [Header("Effect Settings")]
  public float speed = 2f;             // 퍼짐 속도
  public float fadeSpeed = 1.5f;       // 사라지는 속도
  public float maxProgress = 1f;       // 최대값 (1 권장)

  private Material mat;
  private float progress = 0f;
  private bool isExpanding = false;
  private bool isFading = false;

  void Start()
  {
    if (highlightRenderer != null)
    {
      // 공유 머티리얼 복사
      mat = new Material(highlightRenderer.sharedMaterial);
      mat = highlightRenderer.material;
      mat.SetFloat("_Progress", 0f);
    }
  }

  public void OnSelectEntered(SelectEnterEventArgs args)
  {
    // 햅틱
    var controller = args.interactorObject as XRBaseControllerInteractor;
    if (controller != null)
    {
      controller.SendHapticImpulse(0.6f, 0.2f);
    }

    // 효과 시작 (퍼져 나가기)
    progress = 0f;
    isExpanding = true;
    isFading = false;
  }

  public void OnSelectExited(SelectExitEventArgs args)
  {
    // 선택을 놓았을 때 바로 fade out 시작하게 하려면 이쪽에서 켜도 됨
    isExpanding = false;
    isFading = true;
  }

  void Update()
  {
    if (mat == null) return;

    if (isExpanding)
    {
      progress += Time.deltaTime * speed;
      if (progress >= maxProgress)
      {
        progress = maxProgress;
        isExpanding = false;
        isFading = true; // 자동으로 fade out 시작
      }
    }
    else if (isFading)
    {
      progress -= Time.deltaTime * fadeSpeed;
      if (progress <= 0f)
      {
        progress = 0f;
        isFading = false; // 완전히 꺼짐
      }
    }

    mat.SetFloat("_Progress", progress);
  }
}

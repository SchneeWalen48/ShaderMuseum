using UnityEngine;

public class RippleWave : MonoBehaviour
{
  public float lifetime = 1.5f;
  public float startScale = 0.1f;
  public float endScale = 2f;
  public float startAlpha = 1f;
  public float endAlpha = 0f;

  private float _time;
  private Material _mat;
  private Color _baseColor;

  void Awake()
  {
    var rend = GetComponent<MeshRenderer>();
    if (rend)
    {
      _mat = rend.material;
      _baseColor = _mat.color;
    }
    transform.localScale = Vector3.one * startScale;
  }

  void Update()
  {
    _time += Time.deltaTime;
    float t = Mathf.Clamp01(_time / lifetime);

    float scale = Mathf.Lerp(startScale, endScale, t);
    transform.localScale = new Vector3(scale, 1f, scale);

    if(_mat != null)
    {
      Color c = _baseColor;
      c.a = Mathf.Lerp(startAlpha, endAlpha, t);
      _mat.color = c;
    }

    if(_time >= lifetime)
    {
      Destroy(gameObject);
    }
  }
}

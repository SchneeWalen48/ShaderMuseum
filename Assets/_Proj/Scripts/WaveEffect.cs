using UnityEngine;

public class WaveEffect : MonoBehaviour
{
  public float lifetime = 4f;
  public float moveSpeed = 2f;
  public float startScale = 0.5f;
  public float endScale = 2f;

  private float _time;
  private Material _mat;
  private Color _baseColor;

  void Awake()
  {
    var mr = GetComponent<MeshRenderer>();
    if (mr) _mat = mr.material;
    transform.localScale = Vector3.one * startScale;
  }

  void Update()
  {
    _time += Time.deltaTime;
    float t = _time / lifetime;

    // 앞쪽 이동
    transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

    float scale = Mathf.Lerp(startScale, endScale, t);
    transform.localScale = new Vector3(scale, 1, scale);

    if (_mat)
    {
      Color c = _mat.color;
      c.a = Mathf.Lerp(1f, 0f, t);
      _mat.color = c;
    }

    if (_time >= lifetime) Destroy(gameObject);
  }
}

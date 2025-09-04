using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RGBColourUI : MonoBehaviour
{
  [SerializeField] Slider rSlider;
  [SerializeField] Slider gSlider;
  [SerializeField] Slider bSlider;

  [SerializeField] List<Renderer> targets = new();

  [SerializeField] string colorProperty = "_BaseColor";

  readonly List<MaterialPropertyBlock> _mpbs = new();

  int _colorId;
  int _emissionId;

  void Awake()
  {
    _colorId = Shader.PropertyToID(colorProperty);
    _emissionId = Shader.PropertyToID("_EmissionColor");

    foreach(var r in targets)
    {
      if(r == null) continue;
      var mpb = new MaterialPropertyBlock();
      r.GetPropertyBlock(mpb);
      _mpbs.Add(mpb);
    }

    if (rSlider) rSlider.onValueChanged.AddListener(_ => Apply());
    if (gSlider) gSlider.onValueChanged.AddListener(_ => Apply());
    if (bSlider) bSlider.onValueChanged.AddListener(_ => Apply());

    Apply();
  }

  public void Apply()
  {
    float r = rSlider ? rSlider.value : 1f;
    float g = gSlider ? gSlider.value : 1f;
    float b = bSlider ? bSlider.value : 1f;

    var col = new Color(r, g, b);

    for (int i = 0; i < targets.Count; i++)
    {
      var rend = targets[i];
      if (!rend) continue;

      var mpb = _mpbs[i];
      mpb.SetColor(_colorId, col);

      rend.SetPropertyBlock(mpb);
    }
  }

  public void SetHex(string hex)
  {
    if(ColorUtility.TryParseHtmlString(hex, out var c))
    {
      if (rSlider) rSlider.SetValueWithoutNotify(c.r);
      if (gSlider) gSlider.SetValueWithoutNotify(c.g);
      if (bSlider) bSlider.SetValueWithoutNotify(c.b);
      Apply();
    }
  }

}

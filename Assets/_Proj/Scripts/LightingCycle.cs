using UnityEngine;
using System.Collections;

public class LightingCycle : MonoBehaviour
{
  public Light sun; // Directional Light
  public float transitionDuration = 5f;
  public float holdDuration = 20f;

  public Color[] sunColors = {
        new Color32(201, 167, 235, 255), // dawn
        new Color32(255, 255, 255, 255), // day
        new Color32(255, 213, 128, 255), // sunset
        new Color32(74, 111, 165, 255)   // night
    };

  public float[] intensities = { 0.3f, 1f, 0.6f, 0.1f };

  public Color[] ambientColors = {
        new Color32(80, 70, 120, 255),  // dawn
        new Color32(200, 200, 200, 255),// day
        new Color32(180, 140, 90, 255), // sunset
        new Color32(30, 40, 60, 255)    // night
    };

  public IEnumerator LightCycleRoutine()
  {
    int idx = 0;
    while (true)
    {
      int next = (idx + 1) % sunColors.Length;
      float t = 0;
      while (t < 1)
      {
        t += Time.deltaTime / transitionDuration;

        sun.color = Color.Lerp(sunColors[idx], sunColors[next], t);
        sun.intensity = Mathf.Lerp(intensities[idx], intensities[next], t);
        RenderSettings.ambientLight = Color.Lerp(ambientColors[idx], ambientColors[next], t);

        yield return null;
      }

      yield return new WaitForSeconds(holdDuration);
      idx = next;
    }
  }
}

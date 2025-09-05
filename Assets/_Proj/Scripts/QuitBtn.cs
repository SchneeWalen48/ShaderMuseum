using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class QuitBtn : MonoBehaviour
{
  [SerializeField] private XRSimpleInteractable interactable;

  private void Awake()
  {
    if (interactable == null)
    {
      interactable = GetComponent<XRSimpleInteractable>();
      if (interactable == null)
        interactable = GetComponentInChildren<XRSimpleInteractable>(true);
    }
  }

  private void OnEnable()
  {
    interactable.selectEntered.AddListener(OnButtonPressed);
  }

  private void OnDisable()
  {
    interactable.selectEntered.RemoveListener(OnButtonPressed);
  }

  public void OnButtonPressed(SelectEnterEventArgs args)
  {
    Application.Quit();

#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#endif
  }
}

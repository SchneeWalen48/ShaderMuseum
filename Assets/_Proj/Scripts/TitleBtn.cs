using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleBtn : MonoBehaviour
{
  [SerializeField] private Button startButton;
  [SerializeField] private string gameSceneName = "MainScene";

  private void Awake()
  {
    startButton.onClick.AddListener(StartGame);
  }

  private void StartGame()
  {
    SceneManager.LoadScene(gameSceneName);
  }
}

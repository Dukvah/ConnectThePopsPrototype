using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Controllers
{
    public class UIController : MonoBehaviour
    {
        #region Variables

        [Header("UI Menu Panels")]
        [SerializeField] GameObject startPanel;
        [SerializeField] GameObject inGamePanel;
        [SerializeField] GameObject endGamePanel;
    
        [Header("UI Objects")]
        [SerializeField] Button startButton, restartButton; // Buttons to start and restart the game

        #endregion

        #region Unity Functions

        private void Awake()
        {
            ButtonInitialize();
        }
    
        private void Start()
        {
            ShowPanel(startPanel);
        }
    
        private void OnEnable()
        {
            EventManager.Instance.OnGameStart.AddListener(GameStart);
            EventManager.Instance.OnGameEnd.AddListener(GameEnd);
            EventManager.Instance.OnGameRestart.AddListener(() => SceneManager.LoadScene(0));
        }

        private void OnDisable()
        {
            if (EventManager.Instance)
            {
                EventManager.Instance.OnGameStart.RemoveListener(GameStart);
                EventManager.Instance.OnGameEnd.RemoveListener(GameEnd);
                EventManager.Instance.OnGameRestart.RemoveListener(() => SceneManager.LoadScene(0));
            }
        }

        #endregion
    
        #region UI Functions

        private void ButtonInitialize()
        {
            startButton.onClick.AddListener(() => EventManager.Instance.OnGameStart.Invoke());
            restartButton.onClick.AddListener(() => EventManager.Instance.OnGameRestart.Invoke());
        }

        private void GameStart()
        {
            ShowPanel(inGamePanel);
        }

        private void GameEnd()
        {
            ShowPanel(endGamePanel);
        }
    
        private void ShowPanel(GameObject panel)
        {
            CloseAllPanels();
            panel.SetActive(true);
        }
    
        private void CloseAllPanels()
        {
            startPanel.SetActive(false);
            inGamePanel.SetActive(false);
            endGamePanel.SetActive(false);
        }

        #endregion
    }
}

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        #region Variables
        
        // A point system can be added if desired. 
        // private float _playerScore;
        // public float PlayerScore
        // {
        //     get => _playerScore;
        //     set
        //     {
        //         _playerScore = value;
        //         EventManager.Instance.OnScoreChange.Invoke();
        //     }
        // }
        
        public bool HasGameStart { get; private set; }

        public bool IsTouching { get; set; }
        
        #endregion

        #region Unity Functions
        
        private void OnEnable()
        {
            EventManager.Instance.OnGameStart.AddListener(() => HasGameStart = true);
            EventManager.Instance.OnGameEnd.AddListener(() => HasGameStart = false);
        }
        private void OnDisable()
        {
            EventManager.Instance?.OnGameStart.RemoveListener(() => HasGameStart = true);
            EventManager.Instance?.OnGameEnd.RemoveListener(() => HasGameStart = false);
        }

        #endregion
    }
}

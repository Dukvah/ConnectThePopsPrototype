using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class EventManager : Singleton<EventManager>
    {
        // MAIN EVENTS
        [HideInInspector] public UnityEvent OnGameStart = new(); 
        [HideInInspector] public UnityEvent OnGameRestart = new();
        [HideInInspector] public UnityEvent OnGameEnd = new();
    
        // PLAYER EVENTS
        [HideInInspector] public UnityEvent OnPlayerTouchEnd = new();
    }
}
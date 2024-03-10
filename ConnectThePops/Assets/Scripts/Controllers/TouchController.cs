using Managers;
using UnityEngine;

namespace Controllers
{
    public class TouchController : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameManager.Instance.IsTouching = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                GameManager.Instance.IsTouching = false;
                EventManager.Instance.OnPlayerTouchEnd.Invoke();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using Managers;
using Scriptables;
using TMPro;
using Random = UnityEngine.Random;

namespace GameArea
{
    public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        #region Variables

        [Header("Cell Features")]
        
        [Tooltip("Text containing the value of the cell")]
        [SerializeField] TextMeshProUGUI cellText;
        
        [Tooltip("Visualization of the cell")]
        [SerializeField] Image cellVisual;
        
        [Tooltip("Rect Transform of the cell's visual")]
        [SerializeField] RectTransform visualRect;
        
        [Tooltip("Coordinate of the cell on the game screen")]
        [SerializeField] Vector2 cellCoordinate;
        
        [Tooltip("Position of the cell for reference to Line Renderer")]
        [SerializeField] Vector2 linePoint;
    
        
        // Getters
        public Vector2 LinePoint => linePoint;
        public Vector2 CellCoordinate => cellCoordinate;
        
        // Props
        public SOCellValue CellValue { get; private set; }
        public Vector2 VisualStartPos { get; private set; }
        public bool IsSelected { get; set; }
        public bool IsEmpty { get; set; }  // Indicates whether the cell is currently empty. Empty cells are filled by the cells above them.
        private bool _isActive; // Indicates whether the cell can currently be interacted with

        #endregion

        #region Unity Functions

        private void Start()
        {
            VisualStartPos = visualRect.anchoredPosition;
        
            CellValue = CellManager.Instance.GetRandomCellValue();  // Initially, each cell starts with a random value.
            NewCellCreated(true, true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isActive) return;
        
            GameManager.Instance.IsTouching = true;
            CellManager.Instance.TrySelectCell(this);
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GameManager.Instance.IsTouching && _isActive)
            { 
                CellManager.Instance.TrySelectCell(this);
            }
        }

        #endregion

        #region Select Functions

        public void OnSelect()
        {
            IsSelected = true;
            cellVisual.gameObject.transform.DOScale(Vector3.one * 1.15f, 0.2f);
        }

        public void OnUnselect()
        {
            IsSelected = false;
            cellVisual.gameObject.transform.DOScale(Vector3.one, 0.2f);
        }

        #endregion

        #region Main Functions
        
        // What needs to happen when cells are merged is done here.
        public void MergeCell(Cell targetCell,int newIndex)
        {
            IsSelected = false;
        
            if (targetCell == this) // If this is the cell where the merger will take place, it waits for other cells to arrive and then increases its level
            {
                StartCoroutine(IncreaseCell(newIndex, 0.4f));
            }
            else // The other cells move towards the cell where the merge will take place.
            {
                IsEmpty = true;
                var target = targetCell.gameObject.transform.position;
                visualRect.DOMove(target, 0.25f).SetEase(Ease.Linear).OnComplete(() => 
                    visualRect.localScale = Vector3.zero);
            }
        }

        // Once the merger is complete, the Cells begin to fill the gaps from top to bottom.
        public void MoveToAnotherCell(Cell targetCell)
        {
            var target = targetCell.gameObject.transform.position;
            visualRect.DOMove(target, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                targetCell.SetCellValue(CellValue);
                visualRect.localScale = Vector3.zero;
                visualRect.anchoredPosition = VisualStartPos;
            });
        }
        
        // The cell where the merger takes place is increased in level here.
        private IEnumerator IncreaseCell(int newIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
        
            if (newIndex > CellManager.Instance.AvailableCellValues.Count-1)
            {
                CellValue = CellManager.Instance.GetRandomCellValue();
                CellManager.Instance.FillGaps();
            }
            else
            {
                CellValue = CellManager.Instance.AvailableCellValues[newIndex];
            
                visualRect.anchoredPosition = VisualStartPos;
            
                cellVisual.color = CellValue.color;
                cellText.text = CellValue.value.ToString();

                visualRect.localScale = Vector3.one;
                visualRect.DORewind();
                visualRect.DOPunchScale(Vector3.one * 0.35f, 0.15f, 1, 1).OnComplete(() =>
                {
                    _isActive = true;
                    CellManager.Instance.FillGaps();
                });
            }
        }

        #endregion

        #region Auxiliary Functions

        public void RefillCell(float delay)
        {
            Invoke(nameof(RefillCellAsync), delay);
        }

        private void RefillCellAsync()
        {
            CellValue = CellManager.Instance.GetRandomCellValue();
            NewCellCreated(true);
            IsEmpty = false;
        }
        
        // It is used to make the value of this cell the same as in another cell.
        private void SetCellValue(SOCellValue newValue)
        {
            CellValue = newValue;
            NewCellCreated();
            IsEmpty = false;
        }
        
        // Each time the contents of the cell are refreshed, the necessary operations are performed here.
        private void NewCellCreated(bool delay = false, bool random = false) 
        {
        
            var delayTime = random ? Random.Range(0.5f, 1.5f) : 0.1f;
            var time = delay ? delayTime : 0f;
        
            _isActive = false;
        
            visualRect.localScale = Vector3.zero;
            visualRect.anchoredPosition = VisualStartPos;
        
            cellVisual.color = CellValue.color;
            cellText.text = CellValue.value.ToString();
        
            visualRect.DOScale(Vector3.one, time).OnComplete(() => _isActive = true);
        }
        
        #endregion
    }
}

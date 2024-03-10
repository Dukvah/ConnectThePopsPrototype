using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameArea
{
    public class SelectionView : MonoBehaviour
    {
        [SerializeField] Image cellVisual;
        [SerializeField] TextMeshProUGUI cellValueText;
    
        private void Start()
        {
            CellManager.Instance.OnNewCellSelect += SetView;
            CellManager.Instance.OnCellsUnselect += CloseView;
        
            CloseView();
        }
    
        // The player can see the result of the merge of the selected cells here.
        private void SetView(int value, Color color)
        {
            cellValueText.text = value.ToString();
            cellVisual.color = color;
        
            cellVisual.gameObject.transform.localScale = Vector3.one;
        }
        
        private void CloseView()
        {
            cellVisual.gameObject.transform.localScale = Vector3.zero;
        }
    }
}

using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "Channel Name", menuName = "ScriptableContents/CellValue")] 
    public class SOCellValue : ScriptableObject
    {
        public int index;
        public int value;
        public Color color;
        public int score;

        public void SetIndex(int newValue)
        {
            index = newValue;
        }
    }
}

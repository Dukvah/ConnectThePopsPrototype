using System;
using System.Collections.Generic;
using GameArea;
using Managers;
using Scriptables;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellManager : Singleton<CellManager>
{
    #region Variables

    [Tooltip("List of all cells in the game")]
    [SerializeField] List<Cell> cells = new();
    [SerializeField] LineRenderer lineRenderer;
    
    public List<SOCellValue> AvailableCellValues = new();
    public List<CellColumn> CellColumns = new();
    
    private List<Cell> _selectedSCells = new(); //List of cells instantly selected by the player
    
    #endregion

    #region Events

    public delegate void InGameCellSelectEvents(int value, Color color);
    public event InGameCellSelectEvents OnNewCellSelect;
    public delegate void InGameCellUnselectEvents();
    public event InGameCellUnselectEvents OnCellsUnselect;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        SetCellValues();
    }

    private void OnEnable()
    {
        EventManager.Instance.OnPlayerTouchEnd.AddListener(OnPlayerTouchEnd);
    }

    private void OnDisable()
    {
        EventManager.Instance?.OnPlayerTouchEnd.RemoveListener(OnPlayerTouchEnd);
    }

    #endregion

    #region Select Functions

    // When the player tries to select a cell, it is checked here whether the cell is available for selection.
    public void TrySelectCell(Cell cell)
    {
        if (_selectedSCells.Count <= 0) // If there is no cell already selected, the player can select the cell.
        {
            SelectCell(cell);
        }
        else if (_selectedSCells[^1] != cell && CheckNeighbors(_selectedSCells[^1],cell) && CheckSameValue(cell) && !cell.IsSelected)   // If the cell is available, it is included among the selected cells.
        {
            SelectCell(cell);
        }
        else if (_selectedSCells.Count > 1) // If the player selects the cell that was previously selected, the selection is removed.
        {
            if (_selectedSCells[^2] == cell && cell.IsSelected)
            {
                UnselectCell(_selectedSCells[^1]);
            }
        }
    }
    
    private void SelectCell(Cell addedCell)
    {
        _selectedSCells.Add(addedCell);
        addedCell.OnSelect();
        CalculateSelectedCells();
        DrawLine();
    }
 
    private void UnselectCell(Cell removedCell)
    {
        _selectedSCells.RemoveAt(_selectedSCells.Count-1);
        removedCell.OnUnselect();
        CalculateSelectedCells();
        DrawLine();
    }

    private void OnPlayerTouchEnd() // When the player removes his/her hand from the screen, the status of the selections made is calculated and acted upon accordingly.
    {
        switch (_selectedSCells.Count)
        {
            case 1: 
                UnselectCell(_selectedSCells[0]);
                break;
            case > 1:
                var mainMultiplier = _selectedSCells.Count == 1 ? 0 : 1;
                var scoreMultiplier = _selectedSCells.Count / 2;
                var newCellIndex = _selectedSCells[0].CellValue.index + mainMultiplier * scoreMultiplier;
                if (AvailableCellValues.Count - 1 < newCellIndex)
                    newCellIndex = AvailableCellValues.Count - 1;
                foreach (var cell in _selectedSCells)
                    cell.MergeCell(_selectedSCells[^1],newCellIndex);
                _selectedSCells.Clear();
                DrawLine();
                Invoke(nameof(CheckGameCanResume), 0.5f);
                break;
        }
        
        OnCellsUnselect?.Invoke();
    }
    
    #endregion

    #region Manage Functions
    
    // Finds the gaps created after the selected cells merge and fills them from top to bottom.
    public void FillGaps()
    {
        foreach (var column in CellColumns) // Since the process of filling in the blanks is from top to bottom, each column is looked at.
        {
            for (int i = column.cells.Count-1; i >= 0; i--)
            {
                if (column.cells[i].IsEmpty) // From bottom to top, the first empty cell is found.
                {
                    List<int> emptyIndexList = new();
                    List<int> fullIndexList = new();
                    
                    for (int j = i; j >= 0; j--) // Starting from the first empty cell, empty and full cells are determined.
                    {
                        if (column.cells[j].IsEmpty)
                        {
                            emptyIndexList.Add(j);
                        }
                        else
                        {
                            fullIndexList.Add(j);
                        }
                    }
                    
                    if (fullIndexList.Count > 0) 
                    {
                        
                        for (int j = 0; j < fullIndexList.Count ; j++) // Filled cells move to the spaces below them.
                        {
                            column.cells[fullIndexList[j]].MoveToAnotherCell(column.cells[emptyIndexList[0]]);
                            
                            emptyIndexList.RemoveAt(0);
                            emptyIndexList.Add(fullIndexList[j]);
                            emptyIndexList.Sort();
                            emptyIndexList.Reverse();
                        }

                        for (int j = 0; j < emptyIndexList.Count; j++)  // New cells are created for the gaps at the top.
                        {
                            column.cells[j].RefillCell(0.4f);
                        }
                    }
                    else // If there are no cells to move to fill the gaps, it is necessary to create them all from scratch.
                    {
                        
                        for (int j = i; j >= 0; j--)
                        {
                            column.cells[j].RefillCell(0f);
                        }
                    }
                    
                    break;
                }
            }
        }
    }

    #endregion
    
    #region Auxiliary Functions

    // After each move, the cells on the board are checked to see if they match each other. If not, the game is over.
    private void CheckGameCanResume() 
    {
        for (int i = 0; i < cells.Count; i++)
        {
            for (int j = i+1; j < cells.Count; j++)
            {
                var canResume = CheckNeighbors(cells[i], cells[j]);
                if (canResume) return;
            }
        }
        // No more cells to match, game over.
        EventManager.Instance.OnGameEnd.Invoke(); 
    }
    
    // Calculates the current state of selected cells.
    private void CalculateSelectedCells()
    {
        if (_selectedSCells.Count <= 0) return;
        
        var mainMultiplier = _selectedSCells.Count == 1 ? 0 : 1;
        var scoreMultiplier = _selectedSCells.Count / 2;

        var newCellIndex = _selectedSCells[0].CellValue.index + mainMultiplier * scoreMultiplier;

        if (AvailableCellValues.Count - 1 < newCellIndex)
        {
            newCellIndex = AvailableCellValues.Count - 1;
        }
        
        var promotedCell = AvailableCellValues[newCellIndex]; 
        OnNewCellSelect?.Invoke(promotedCell.value, promotedCell.color);
    }
    
    // Calculates the neighborhood of two given cells.
    private bool CheckNeighbors(Cell currentCell,Cell targetCell)
    {
        var currentCoordinate = currentCell.CellCoordinate;
        
        if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x-1,currentCoordinate.y-1))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x-1,currentCoordinate.y))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x-1,currentCoordinate.y+1))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x,currentCoordinate.y-1))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x,currentCoordinate.y+1))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x+1,currentCoordinate.y-1))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x+1,currentCoordinate.y))
        {
            return true;
        }
        else if (targetCell.CellCoordinate == new Vector2(currentCoordinate.x+1,currentCoordinate.y+1))
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    
    public SOCellValue GetRandomCellValue()
    {
        return AvailableCellValues[Random.Range(0, 4)];
    }

    private bool CheckSameValue(Cell targetCell)
    {
        return targetCell.CellValue.value == _selectedSCells[^1].CellValue.value;
    }

    
    // Indexes of cells are set
    private void SetCellValues()
    {
        for (int i = 0; i < AvailableCellValues.Count; i++)
        {
            AvailableCellValues[i].SetIndex(i);
        }
    }

    #endregion
    
    #region Line Renderer Functions

    // When cells are selected, the lines between them are created here.
    private void DrawLine()
    {
        lineRenderer.positionCount = _selectedSCells.Count;
        
        if (_selectedSCells.Count > 0)
        {
            lineRenderer.startColor = _selectedSCells[0].CellValue.color;
            lineRenderer.endColor = _selectedSCells[0].CellValue.color;
        }

        for (int i = 0; i < _selectedSCells.Count; i++)
        {
            lineRenderer.SetPosition(i,_selectedSCells[i].LinePoint);
        }
    }

    #endregion
}

[Serializable]
public class CellColumn 
{
    public List<Cell> cells = new();
}
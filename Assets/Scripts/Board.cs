using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    None,
    Friendly,
    Enemy,
    Free,
    OutOfBounds
}

public class Board : MonoBehaviour
{
    public GameObject mCellPrefab;

    [HideInInspector]
    public Cell[,] mAllCells = new Cell[8, 8];

    public void Create()
    {
        // 보드 생성
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                GameObject newCell = Instantiate(mCellPrefab, transform);

                RectTransform rectTransform = newCell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2((x * 100) + 50, (y * 100) + 50);

                mAllCells[x, y] = newCell.GetComponent<Cell>();
                mAllCells[x, y].Setup(new Vector2Int(x, y), this);
            }
        }

        // 보드 색 지정
        for (int x = 0; x < 8; x += 2)
        {
            for (int y = 0; y < 8; y++)
            {
                // Offset for every other line
                int offset = (y % 2 != 0) ? 0 : 1;
                int finalX = x + offset;

                // Color
                mAllCells[finalX, y].GetComponent<Image>().color = new Color32(202, 167, 132, 255);
            }
        }
    }

    public CellState ValidateCell(int x, int y, BasePiece checkingPiece)    // 칸 상태 체크
    {
        if (x < 0 || x > 7)
            return CellState.OutOfBounds;
        if (y < 0 || y > 7)
            return CellState.OutOfBounds;

        Cell targetCell = mAllCells[x, y];

        if(targetCell.mCurrentPiece != null)
        {
            if (checkingPiece.mColor == targetCell.mCurrentPiece.mColor)
                return CellState.Friendly;
            else
                return CellState.Enemy;
        }

        return CellState.Free;
    }

    public CellState ValidateCell(int x, int y, Color color)
    {
        if (x < 0 || x > 7)
            return CellState.OutOfBounds;
        if (y < 0 || y > 7)
            return CellState.OutOfBounds;

        Cell targetCell = mAllCells[x, y];

        if (targetCell.mCurrentPiece != null)
        {
            if (color == targetCell.mCurrentPiece.mColor)
                return CellState.Friendly;
            else
                return CellState.Enemy;
        }

        return CellState.Free;
    }
}

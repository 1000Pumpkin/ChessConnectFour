using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public abstract class BasePiece : EventTrigger
{
    [HideInInspector]
    public Color mColor = Color.clear;
    [HideInInspector]
    protected int mCost = 0;

    protected Cell mCurrentCell = null;
    protected Cell mTargetCell = null;

    protected RectTransform mRectTransform = null;
    protected PieceManager mPieceManager = null;

    protected Vector3Int mMovement = Vector3Int.one;
    protected List<Cell> mHighlightedCells = new List<Cell>();

    public virtual void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        mPieceManager = newPieceManager;

        mColor = newTeamColor;
        GetComponent<Image>().color = newSpriteColor;
        mRectTransform = GetComponent<RectTransform>();
        mRectTransform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public int GetCost()
    {
        return mCost;
    }

    public Color GetColor()
    {
        return mColor;
    }

    public void Place(Cell newCell)
    {
        mCurrentCell = newCell;
        mCurrentCell.mCurrentPiece = this;

        transform.position = newCell.transform.position;
        gameObject.SetActive(true);
    }

    public void Reset()
    {
        Kill(null);
    }

    public virtual void Kill(BasePiece killPiece)
    {
        mCurrentCell.mCurrentPiece = null;
        gameObject.SetActive(false);
        if (killPiece == null)
            return;
        Color enemyColor = (mColor == Color.white) ? Color.black : Color.white;
        float earnCost = mCost / (float)killPiece.mCost;
        GameManager.instance.AddCost(enemyColor, (int)Mathf.Ceil(earnCost));
        GameManager.instance.AddGraves(mColor, this);
    }

    #region Move
    private void CreateCellPath(int xDirection, int yDirection, int movement)
    {
        int curX = mCurrentCell.mBoardPosition.x;
        int curY = mCurrentCell.mBoardPosition.y;

        for(int i = 1; i <= movement; ++i)
        {
            curX += xDirection;
            curY += yDirection;

            CellState cellState = CellState.None;
            cellState = mCurrentCell.mBoard.ValidateCell(curX, curY, this);

            if (cellState == CellState.Enemy)
            {
                mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[curX, curY]);
                break;
            }
            
            if (cellState != CellState.Free)
                break;

            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[curX, curY]);
        }
    }

    protected virtual void CheckPathing()
    {
        CreateCellPath(1, 0, mMovement.x);
        CreateCellPath(-1, 0, mMovement.x);

        CreateCellPath(0, 1, mMovement.y);
        CreateCellPath(0, -1, mMovement.y);
        
        CreateCellPath(1, 1, mMovement.z);
        CreateCellPath(-1, 1, mMovement.z);
        CreateCellPath(1, -1, mMovement.z);
        CreateCellPath(-1, -1, mMovement.z);
    }

    protected void ShowCells()
    {
        foreach (Cell cell in mHighlightedCells)
            cell.mOutlineImage.enabled = true;
    }

    protected void ClearCells()
    {
        foreach (Cell cell in mHighlightedCells)
            cell.mOutlineImage.enabled = false;

        mHighlightedCells.Clear();
    }

    protected virtual void Move()
    {
        mTargetCell.RemovePiece(this);

        mCurrentCell.mCurrentPiece = null;

        mCurrentCell = mTargetCell;
        mCurrentCell.mCurrentPiece = this;

        transform.position = mCurrentCell.transform.position;
        mTargetCell = null;
    }
    #endregion

    #region Event
    public override void OnBeginDrag(PointerEventData eventData)
    {
        GameManager.instance.EndPlace();

        base.OnBeginDrag(eventData);

        CheckPathing();
        ShowCells();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        transform.position += (Vector3)eventData.delta;

        foreach(Cell cell in mHighlightedCells)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(cell.mRectTransform, Input.mousePosition))
            {
                mTargetCell = cell;
                break;
            }
            mTargetCell = null;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        ClearCells();

        if(!mTargetCell)
        {
            transform.position = mCurrentCell.gameObject.transform.position;
            return;
        }

        Move();

        mPieceManager.SwitchSides(mColor);
    }
    #endregion
}

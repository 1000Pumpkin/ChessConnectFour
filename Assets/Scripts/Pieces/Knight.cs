using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Knight : BasePiece
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Pieces/knight");

        mCost = 3;
    }

    private void CreateCellPath(int flipper)
    {
        int curX = mCurrentCell.mBoardPosition.x;
        int curY = mCurrentCell.mBoardPosition.y;

        MatchesState(curX - 2, curY + flipper);
        MatchesState(curX - 1, curY + 2 * flipper);
        MatchesState(curX + 1, curY + 2 * flipper);
        MatchesState(curX + 2, curY + flipper);
    }

    protected override void CheckPathing()
    {
        CreateCellPath(1);
        CreateCellPath(-1);
    }

    private void MatchesState(int x, int y)
    {
        CellState cellState = mCurrentCell.mBoard.ValidateCell(x, y, this);

        if (cellState != CellState.Friendly && cellState != CellState.OutOfBounds)
            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[x, y]);
    }
}

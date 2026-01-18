using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pawn : BasePiece
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        mMovement = (mColor == Color.white) ? new Vector3Int(0, 1, 1) : new Vector3Int(0, -1, -1);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Pieces/pawn");

        mCost = 1;
    }

    private bool MatchesState(int x, int y, CellState state)
    {
        CellState cellState = mCurrentCell.mBoard.ValidateCell(x, y, this);

        if(cellState == state)
        {
            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[x, y]);
            return true;
        }

        return false;
    }

    protected override void CheckPathing()
    {
        int curX = mCurrentCell.mBoardPosition.x;
        int curY = mCurrentCell.mBoardPosition.y;

        // ÁÂ»ó
        MatchesState(curX - mMovement.z, curY + mMovement.z, CellState.Enemy);

        // »ó
        MatchesState(curX, curY + mMovement.y, CellState.Free);

        // ¿ì»ó
        MatchesState(curX + mMovement.z, curY + mMovement.z, CellState.Enemy);
    }
}

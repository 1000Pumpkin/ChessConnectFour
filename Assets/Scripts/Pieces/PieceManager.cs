using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    [HideInInspector]
    public bool mIsBlackTurn = false;

    public GameObject mPiecePrefab;

    private List<BasePiece> mWhitePieces = null;
    private List<BasePiece> mBlackPieces = null;

    private Dictionary<string, Type> mPieceLibrary = new Dictionary<string, Type>()
    {
        {"P", typeof(Pawn)},
        {"KN", typeof(Knight)},
        {"B", typeof(Bishop)},
        {"R", typeof(Rook)},
        {"K", typeof(King)},
        {"Q", typeof(Queen)}
    };

    public void Setup(Board board)
    {
        mWhitePieces = new List<BasePiece>();
        mBlackPieces = new List<BasePiece>();

        SwitchSides(Color.black);
    }

    public BasePiece CreatePiece(string key)
    {
        GameObject newPieceObject = Instantiate(mPiecePrefab);
        newPieceObject.transform.SetParent(transform);

        newPieceObject.transform.localScale = new Vector3(1, 1, 1);
        newPieceObject.transform.localRotation = Quaternion.identity;

        Type pieceType = mPieceLibrary[key];

        BasePiece newPiece = (BasePiece)newPieceObject.AddComponent(pieceType);

        if (mIsBlackTurn)
        {
            newPiece.Setup(Color.black, GameManager.instance.mBlackColor, this);
            mBlackPieces.Add(newPiece); 
        }
        else
        {
            newPiece.Setup(Color.white, GameManager.instance.mWhiteColor, this);
            mWhitePieces.Add(newPiece); 
        }

        return newPiece;
    }

    private void SetInteractive(List<BasePiece> allPieces, bool value)  // 본인 턴에만 움직일 수 있게 함
    {
        foreach (BasePiece piece in allPieces)
            piece.enabled = value;
    }

    public void SwitchSides(Color color)    // 턴 변경
    {
        if(GameManager.instance.CheckingConnect(color))
            return;

        mIsBlackTurn = (color == Color.white);

        GameManager.instance.AddCost(mIsBlackTurn ? Color.black : Color.white, 1);
        GameManager.instance.ChangeTurn(mIsBlackTurn);

        SetInteractive(mWhitePieces, !mIsBlackTurn);
        SetInteractive(mBlackPieces, mIsBlackTurn);
    }

    public void ResetPieces()
    {
        foreach (BasePiece piece in mWhitePieces)
            piece.Reset();

        foreach(BasePiece piece in mBlackPieces)
            piece.Reset();
    }
}

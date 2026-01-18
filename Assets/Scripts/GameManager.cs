using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grave
{
    BasePiece mPiece;
    int mTurnCount = 0;

    public Grave(BasePiece piece)
    {
        mPiece = piece;
    }

    public bool AddTurnCount()
    {
        if (mTurnCount > 2)
            return true;

        ++mTurnCount;
        return false;
    }

    public string GetPieceKey()
    {
        if (mPiece.GetType() == typeof(Pawn))
            return "P";
        else if (mPiece.GetType() == typeof(Knight))
            return "KN";
        else if (mPiece.GetType() == typeof(Bishop))
            return "B";
        else if (mPiece.GetType() == typeof(King))
            return "K";
        else if (mPiece.GetType() == typeof(Rook))
            return "R";
        else if (mPiece.GetType() == typeof(Queen))
            return "Q";

        return "NULL";
    }
}

public class GameManager : MonoBehaviour
{
    #region Variable
    public static GameManager instance;

    public Board mBoard;
    public PieceManager mPieceManager;
    public ToggleGroup mPieceGroup;
    public GameObject mCostBar;
    public GameObject mTurnIndicator;
    public GameObject mGraveIndicator;
    public GameObject mResult;

    private int[] mCosts = new int[2];
    private int[][] mPieceCount = new int[2][]
    {
        new int[] { 8, 2, 2, 1, 2, 1},
        new int[] { 8, 2, 2, 1, 2, 1}
    };
    
    private string mSelectedKey = "";

    private Dictionary<string, int> mCostLibrary = new Dictionary<string, int>()
    {
        {"P", 1},
        {"KN", 3},
        {"B", 3},
        {"K", 4},
        {"R", 5},
        {"Q", 9}
    };
    private Dictionary<string, int> mIndexLibrary = new Dictionary<string, int>()
    {
        {"P", 0},
        {"KN", 1},
        {"B", 2},
        {"K", 3},
        {"R", 4},
        {"Q", 5}
    }; 
    private Dictionary<string, string> mNameLibrary = new Dictionary<string, string>()
    {
        {"P", "pawn"},
        {"KN", "knight"},
        {"B", "bishop"},
        {"K", "king"},
        {"R", "rook"},
        {"Q", "queen"}
    };

    private List<Grave> mWhiteGraves = new List<Grave>();
    private List<Grave> mBlackGraves = new List<Grave>();

    [HideInInspector]
    public Color32 mBlackColor = new Color32(210, 95, 64, 255);
    [HideInInspector]
    public Color32 mWhiteColor = new Color32(80, 124, 159, 255);
    #endregion

    private void Awake()
    {
        instance = this;

        mCosts[0] = 0;
        mCosts[1] = 2;
    }

    private void Start()
    {
        mBoard.Create();

        for(int y = 0; y < 8; ++y)
            for(int x = 0; x < 8; ++x)
                mBoard.mAllCells[x, y].GetComponent<Button>().onClick.AddListener(PlacePiece);

        mPieceManager.Setup(mBoard);
    }

    #region Piece
    public void SetPiece(bool isOn)
    {
        if (isOn)
        {
            Toggle toggle = mPieceGroup.ActiveToggles().FirstOrDefault();
            mSelectedKey = toggle.tag;

            for (int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                    if (mBoard.ValidateCell(x, y, Color.white) == CellState.Free)
                        mBoard.mAllCells[x, y].mOutlineImage.enabled = true;
        }
        else
        {
            EndPlace();
        }
    }

    public void PlacePiece()
    {
        if (mSelectedKey == "")
            return;

        int idx = mPieceManager.mIsBlackTurn ? 1 : 0;

        if (mCosts[idx] < mCostLibrary[mSelectedKey])
        {
            for (int j = 0; j < 8; ++j)
                for (int i = 0; i < 8; ++i)
                    mBoard.mAllCells[i, j].mOutlineImage.enabled = false;

            Toggle toggle = mPieceGroup.ActiveToggles().FirstOrDefault();
            toggle.isOn = false;

            return; 
        }

        if (mPieceCount[idx][mIndexLibrary[mSelectedKey]] < 1)
            return;

        for (int y = 0; y < 8; ++y)
        {
            for (int x = 0; x < 8; ++x)
            {
                Cell cell = mBoard.mAllCells[x, y];
                if (!cell.mOutlineImage.enabled)
                    continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(cell.mRectTransform, Input.mousePosition))
                {
                    BasePiece piece = mPieceManager.CreatePiece(mSelectedKey);
                    piece.Place(cell);
                    SubCost(piece.GetColor(), piece.GetCost());

                    --mPieceCount[idx][mIndexLibrary[mSelectedKey]];

                    mPieceManager.SwitchSides(piece.mColor);

                    EndPlace();

                    break;
                }
            }
        }
    }

    public void EndPlace()
    {
        for (int y = 0; y < 8; ++y)
            for (int x = 0; x < 8; ++x)
                mBoard.mAllCells[x, y].mOutlineImage.enabled = false;
        mSelectedKey = "";

        Toggle toggle = mPieceGroup.ActiveToggles().FirstOrDefault();
        if (toggle != null)
            toggle.isOn = false;
    }
    #endregion

    #region System
    public bool CheckingConnect(Color color)
    {
        for(int y = 0; y < 8; ++y)
        {
            for (int x = 0; x < 8; ++x)
            {
                if (IsConnected(x, x + 3, y, y, color))
                {
                    ShowWinner(color);
                    return true;
                }
                else if (IsConnected(x, x, y, y + 3, color))
                {
                    ShowWinner(color);
                    return true;
                }
                else if (IsConnected(x, x + 3, y, y + 3, color))
                {
                    ShowWinner(color);
                    return true;
                }
                else if (IsConnected(x + 3, x, y, y + 3, color))
                {
                    ShowWinner(color);
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsConnected(int startX, int endX, int startY, int endY, Color color)
    {
        int x = startX, y = startY;

        while(true)
        {
            switch(mBoard.ValidateCell(x, y, color))
            {
                case CellState.Enemy: case CellState.Free: case CellState.OutOfBounds:
                    return false;
            }

            if (x == endX && y == endY)
                break;

            if (x != endX)
                x += (endX - x) / Mathf.Abs(endX - x);
            if (y != endY)
                y += (endY - y) / Mathf.Abs(endY - y);
        }

        return true;
    }

    private void ShowWinner(Color color)
    {
        string str = (color == Color.white) ? "White Win!" : "Black Win!";
        mResult.SetActive(true);
        mResult.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = str;
    }

    public void ChangeTurn(bool isBlackTurn)
    {
        if(isBlackTurn)
        {
            //var rotation = Quaternion.Euler(0, 0, 180);
            //RotateBoard(rotation);
            mTurnIndicator.GetComponent<Image>().color = mBlackColor;
        }
        else
        {
            //var rotation = Quaternion.Euler(0, 0, 0);
            //RotateBoard(rotation);
            mTurnIndicator.GetComponent<Image>().color = mWhiteColor;
        }

        CostBarUpdate(isBlackTurn);
        GravesUpdate(isBlackTurn);
        PieceCountUpdate(isBlackTurn);
    }

    //private void RotateBoard(Quaternion rotation)
    //{
    //    mBoard.transform.rotation = rotation;
    //    mPieceManager.transform.rotation = rotation;
    //    for (int i = 0; i < mPieceManager.transform.childCount; ++i)
    //    { 
    //        mPieceManager.transform.GetChild(i).rotation = Quaternion.Euler(0, 0, 0);
    //    }
    //}

    private void PieceCountUpdate(bool isBlackTurn)
    {
        int idx = isBlackTurn ? 1 : 0;
        
        for(int i = 0; i < 6; ++i)
            mPieceGroup.transform.GetChild(i).Find("PieceCount").GetComponent<TextMeshProUGUI>().text = mPieceCount[idx][i].ToString();
    }

    public void AddGraves(Color color, BasePiece piece)
    {
        Grave grave = new Grave(piece);
        if (color == Color.white)
            mWhiteGraves.Add(grave);
        else
            mBlackGraves.Add(grave);
    }

    private void GravesUpdate(bool isBlackTurn)
    {
        foreach(Grave grave in isBlackTurn ? mBlackGraves : mWhiteGraves)
        {
            if (grave.AddTurnCount())
            {
                ++mPieceCount[isBlackTurn ? 1 : 0][mIndexLibrary[grave.GetPieceKey()]];
                if (isBlackTurn)
                    mBlackGraves.Remove(grave);
                else
                    mWhiteGraves.Remove(grave);
                break;
            }
        }

        for (int i = 0; i < 3; ++i)
        {
            string filePath;
            if (i > (isBlackTurn ? mBlackGraves.Count : mWhiteGraves.Count) - 1)
                filePath = "default";
            else
            { 
                filePath = "Pieces/" + mNameLibrary[isBlackTurn ? mBlackGraves[i].GetPieceKey() : mWhiteGraves[i].GetPieceKey()]; 
            }
            Transform grave = mGraveIndicator.transform.GetChild(i);
            grave.GetComponent<Image>().sprite = Resources.Load<Sprite>(filePath);
            grave.GetComponent<Image>().color = isBlackTurn ? mBlackColor : mWhiteColor;
        }
    }

    private void CostBarUpdate(bool isBlackTurn)
    {
        mCostBar.transform.Find("Bar").GetComponent<Image>().fillAmount = mCosts[isBlackTurn ? 1 : 0] / 10f;
        mCostBar.transform.Find("CostText").GetComponent<TextMeshProUGUI>().text = mCosts[isBlackTurn ? 1 : 0].ToString();
    }

    public void AddCost(Color color, int value)
    {
        int playerNum = (color == Color.white) ? 0 : 1;

        mCosts[playerNum] += value;

        if (mCosts[playerNum] > 10)
            mCosts[playerNum] = 10;
    }

    public bool SubCost(Color color, int value)
    {
        int playerNum = (color == Color.white) ? 0 : 1;

        if (mCosts[playerNum] < value)
            return false;

        mCosts[playerNum] -= value;
        return true;
    }
    #endregion
}

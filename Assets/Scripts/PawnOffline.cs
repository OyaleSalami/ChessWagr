using System.Collections;
using UnityEngine;

public class PawnOffline : ChessmanOffline
{

    public override bool[,] PossibleMoves()
    {
        bool[,] r = new bool[8, 8];

        ChessmanOffline c, c2;

        int[] e = BoardManagerOffline.Instance.EnPassantMove;

        if (isWhite)
        {
            ////// White team move //////

            // Diagonal left
            if (CurrentX != 0 && CurrentY != 7)
            {
                if(e[0] == CurrentX -1 && e[1] == CurrentY + 1)
                    r[CurrentX - 1, CurrentY + 1] = true;

                c = BoardManagerOffline.Instance.Chessmans[CurrentX - 1, CurrentY + 1];
                if (c != null && !c.isWhite)
                    r[CurrentX - 1, CurrentY + 1] = true;
            }

            // Diagonal right
            if (CurrentX != 7 && CurrentY != 7)
            {
                if (e[0] == CurrentX + 1 && e[1] == CurrentY + 1)
                    r[CurrentX + 1, CurrentY + 1] = true;

                c = BoardManagerOffline.Instance.Chessmans[CurrentX + 1, CurrentY + 1];
                if (c != null && !c.isWhite)
                    r[CurrentX + 1, CurrentY + 1] = true;
            }

            // Middle
            if (CurrentY != 7)
            {
                c = BoardManagerOffline.Instance.Chessmans[CurrentX, CurrentY + 1];
                if (c == null)
                    r[CurrentX, CurrentY + 1] = true;
            }

            // Middle on first move
            if (CurrentY == 1)
            {
                c = BoardManagerOffline.Instance.Chessmans[CurrentX, CurrentY + 1];
                c2 = BoardManagerOffline.Instance.Chessmans[CurrentX, CurrentY + 2];
                if (c == null && c2 == null)
                    r[CurrentX, CurrentY + 2] = true;
            }
        }
        else
        {
            ////// Black team move //////

            // Diagonal left
            if (CurrentX != 0 && CurrentY != 0)
            {
                if (e[0] == CurrentX - 1 && e[1] == CurrentY - 1)
                    r[CurrentX - 1, CurrentY - 1] = true;

                c = BoardManagerOffline.Instance.Chessmans[CurrentX - 1, CurrentY - 1];
                if (c != null && c.isWhite)
                    r[CurrentX - 1, CurrentY - 1] = true;
            }

            // Diagonal right
            if (CurrentX != 7 && CurrentY != 0)
            {
                if (e[0] == CurrentX + 1 && e[1] == CurrentY - 1)
                    r[CurrentX + 1, CurrentY - 1] = true;

                c = BoardManagerOffline.Instance.Chessmans[CurrentX + 1, CurrentY - 1];
                if (c != null && c.isWhite)
                    r[CurrentX + 1, CurrentY - 1] = true;
            }

            // Middle
            if (CurrentY != 0)
            {
                c = BoardManagerOffline.Instance.Chessmans[CurrentX, CurrentY - 1];
                if (c == null)
                    r[CurrentX, CurrentY - 1] = true;
            }

            // Middle on first move
            if (CurrentY == 6)
            {
                c = BoardManagerOffline.Instance.Chessmans[CurrentX, CurrentY - 1];
                c2 = BoardManagerOffline.Instance.Chessmans[CurrentX, CurrentY - 2];
                if (c == null && c2 == null)
                    r[CurrentX, CurrentY - 2] = true;
            }
        }

        return r;
    }
}

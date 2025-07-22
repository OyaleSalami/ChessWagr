using System.Collections;
using UnityEngine;

public class KingOffline : ChessmanOffline
{
    public bool hasMoved = false;

    public override bool[,] PossibleMoves()
    {
        bool[,] r = new bool[8, 8];

        Move(CurrentX + 1, CurrentY, ref r); // up
        Move(CurrentX - 1, CurrentY, ref r); // down
        Move(CurrentX, CurrentY - 1, ref r); // left
        Move(CurrentX, CurrentY + 1, ref r); // right
        Move(CurrentX + 1, CurrentY - 1, ref r); // up left
        Move(CurrentX - 1, CurrentY - 1, ref r); // down left
        Move(CurrentX + 1, CurrentY + 1, ref r); // up right
        Move(CurrentX - 1, CurrentY + 1, ref r); // down right

        // Castling
        if (!hasMoved)
        {
            // Left Rook
            ChessmanOffline leftRook = BoardManagerOffline.Instance.Chessmans[0, CurrentY];
            if (leftRook != null && leftRook.GetType() == typeof(RookOffline))
            {
                RookOffline rook = leftRook as RookOffline;
                if (!rook.hasMoved)
                {
                    // Check if path is clear
                    if (BoardManagerOffline.Instance.Chessmans[1, CurrentY] == null &&
                        BoardManagerOffline.Instance.Chessmans[2, CurrentY] == null &&
                        BoardManagerOffline.Instance.Chessmans[3, CurrentY] == null)
                    {
                        // Make sure king is not in check and doesn't move through check
                        if (!BoardManagerOffline.Instance.IsInCheck(isWhite) &&
                            !BoardManagerOffline.Instance.IsSquareAttacked(2, CurrentY, isWhite) &&
                            !BoardManagerOffline.Instance.IsSquareAttacked(3, CurrentY, isWhite))
                        {
                            r[2, CurrentY] = true; // Queenside castling
                        }
                    }
                }
            }

            // Right Rook
            ChessmanOffline rightRook = BoardManagerOffline.Instance.Chessmans[7, CurrentY];
            if (rightRook != null && rightRook.GetType() == typeof(RookOffline))
            {
                RookOffline rook = rightRook as RookOffline;
                if (!rook.hasMoved)
                {
                    // Check if path is clear
                    if (BoardManagerOffline.Instance.Chessmans[5, CurrentY] == null &&
                        BoardManagerOffline.Instance.Chessmans[6, CurrentY] == null)
                    {
                        // Make sure king is not in check and doesn't move through check
                        if (!BoardManagerOffline.Instance.IsInCheck(isWhite) &&
                            !BoardManagerOffline.Instance.IsSquareAttacked(5, CurrentY, isWhite) &&
                            !BoardManagerOffline.Instance.IsSquareAttacked(6, CurrentY, isWhite))
                        {
                            r[6, CurrentY] = true; // Kingside castling
                        }
                    }
                }
            }
        }

        return r;
    }
}

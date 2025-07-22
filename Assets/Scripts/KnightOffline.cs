using System.Collections;
using UnityEngine;

public class KnightOffline : ChessmanOffline
{
    public override bool[,] PossibleMoves()
    {
        bool[,] r = new bool[8, 8];
    
        Move(CurrentX + 1, CurrentY + 2, ref r); // up right
        Move(CurrentX - 1, CurrentY + 2, ref r); // up left
        Move(CurrentX + 1, CurrentY - 2, ref r); // down right
        Move(CurrentX - 1, CurrentY - 2, ref r); // down left
        Move(CurrentX + 2, CurrentY + 1, ref r); // right up
        Move(CurrentX + 2, CurrentY - 1, ref r); // right down
        Move(CurrentX - 2, CurrentY + 1, ref r); // left up
        Move(CurrentX - 2, CurrentY - 1, ref r); // left down
    
        return r;
    }

}

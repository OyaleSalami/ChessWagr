using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    // Current state variables
    // About chessman to be moved
    public (ChessmanAi chessman, (int x, int y) oldPosition, (int x, int y) newPosition, bool isMoved) movedChessman;
    // Chessman to be captured
    public (ChessmanAi chessman, (int x, int y) Position) capturedChessman;
    // Current allowed EnPassant move Status
    public (int x, int y) EnPassantStatus;
    // About Promotion move : (was Promotion), Queen(promoted chessman)
    public (bool wasPromotion, ChessmanAi promotedChessman) PromotionMove;
    // About Castling move : (was Castling done, King Side or not)
    public (bool wasCastling, bool isKingSide) CastlingMove;
    public int depth;

    public void SetState((ChessmanAi chessman, (int x, int y) oldPosition, (int x, int y) newPosition, bool isMoved) movedChessman,
                          (ChessmanAi chessman, (int x, int y) Position) capturedChessman,
                          (int x, int y) EnPassantStatus,
                          (bool wasPromotion, ChessmanAi promotedChessman) PromotionMove,
                          (bool wasCastling, bool isKingSide) CastlingMove,
                          int depth)
    {
        this.movedChessman = movedChessman;
        this.capturedChessman = capturedChessman;
        this.EnPassantStatus = EnPassantStatus;
        this.PromotionMove = PromotionMove;
        this.CastlingMove = CastlingMove;
        this.depth = depth;
    }
}

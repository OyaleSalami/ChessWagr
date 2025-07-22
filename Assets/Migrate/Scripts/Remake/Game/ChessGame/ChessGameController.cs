using UnityEngine;

public class ChessGameController : MonoBehaviour
{
    private Board board;
    public bool isWhiteTurn;

    public PiecesCreator piecesCreator;

    private void Awake()
    {
        if (piecesCreator == null)
            piecesCreator = GetComponent<PiecesCreator>();
    }

    public void SetGameBoard(Board board) => this.board = board;

    // Positioning
    public void PositionAllPieces()
    {
        if (board != null)
        {
            for (int x = 0; x < board.BOARD_SIZE; x++)
                for (int y = 0; y < board.BOARD_SIZE; y++)
                    if (board.chessPieces[x, y] != null)
                        PositionSinglePiece(x, y, true);
        }
    }

    public void PositionSinglePiece(int x, int y, bool force = false)
    {
        board.chessPieces[x, y].currentX = x;
        board.chessPieces[x, y].currentY = y;
        board.chessPieces[x, y].SetPosition(BoardSetup.instance.GetTileCenter(x, y), force);
    }

    public void InitChessInBoard()
    {
        if (board.chessPieces != null)
        {
            // Clean up
            for (int x = 0; x < board.BOARD_SIZE; x++)
            {
                for (int y = 0; y < board.BOARD_SIZE; y++)
                {
                    if (board.chessPieces[x, y] != null)
                        Destroy(board.chessPieces[x, y].gameObject);
                    board.chessPieces[x, y] = null;
                }
            }
            board.initDeathPiecesLists();
        }
        else
        {
            // Debug.Log("The board or chessPieces is null, cannot clean up.");
        }
    }

    public void InitPieces()
    {
        if (board == null)
        {
            Debug.Log("The board is null, cannot init pieces.");
        }
        else
        {
            piecesCreator.SpawnAllPieces(board);
            PositionAllPieces();
        }
    }

    public void RestartGame()
    {
        InitChessInBoard();
        InitPieces();
        ResetChessBoardInfo();
    }

    public void ResetChessBoardInfo() => board.GameReset(); // It is using on reset gameobject for chess game rather than being on networking
    public Board getBoard() => this.board;

    #region Handling the Pieces of Movement on Network

    public void MovePieceTo(int movePieceType, int killPieceType, int originalX, int originalY, int x, int y, int specialMove = (int)SpecialMove.None)
    {
        // Validate the move coordinates are within bounds
        if (originalX < 0 || originalX >= board.BOARD_SIZE || 
            originalY < 0 || originalY >= board.BOARD_SIZE ||
            x < 0 || x >= board.BOARD_SIZE || 
            y < 0 || y >= board.BOARD_SIZE) 
        {
            Debug.LogError("Move coordinates out of bounds: " + originalX + "," + originalY + " to " + x + "," + y);
            return;
        }
        
        // Check that there is a piece at the original position
        if (board.chessPieces[originalX, originalY] == null)
        {
            Debug.LogError("No piece at source position: " + originalX + "," + originalY);
            return;
        }
        
        ChessPiece ocp = board.chessPieces[x, y];
        KillPiece(killPieceType, ocp);
        MovingPiece(originalX, originalY, x, y);
        
        // Update move history
        board.SaveLocalMoveList(originalX, originalY, x, y); 
        NetworkTurnManager.Instance.SaveStep(movePieceType, killPieceType, originalX, originalY, x, y, specialMove.ToString());

        // Process special moves if applicable
        if (specialMove != (int)SpecialMove.None)
        {
            MovingSpecialMove((SpecialMove)specialMove);
        }

        // Check for win/stalemate conditions
        if (board.CheckForCheckmate())
        {
            HandleWin(board.getLastMoveTeamColor() == TeamColor.White ? (int)TeamColor.Black : (int)TeamColor.White);
        }
        else if (board.CheckStalemate())
        {
            // Use network manager to handle draw
            NetworkTurnManager.Instance.SetResultDrawAndTexts("Stalemate - Draw!");
        }
    }

    public void MovingSpecialMove(SpecialMove specialMove)
    {
        board.SetSpecialMove(specialMove);
        board.ProcessSpecialMove(); // Processing special move
    }

    public void MovingPiece(int previousX, int previousY, int newX, int newY)
    {
        try
        {
            // Validate coordinates
            if (previousX < 0 || previousX >= board.BOARD_SIZE || 
                previousY < 0 || previousY >= board.BOARD_SIZE ||
                newX < 0 || newX >= board.BOARD_SIZE || 
                newY < 0 || newY >= board.BOARD_SIZE)
            {
                Debug.LogError("Invalid move coordinates in MovingPiece");
                return;
            }

            // Validate source piece exists
            if (board.chessPieces[previousX, previousY] == null)
            {
                Debug.LogError("No piece at source position in MovingPiece");
                return;
            }

            // Move the piece
            board.chessPieces[newX, newY] = board.chessPieces[previousX, previousY];
            board.chessPieces[previousX, previousY] = null;
            
            // Update piece's internal coordinates
            board.chessPieces[newX, newY].currentX = newX;
            board.chessPieces[newX, newY].currentY = newY;
            
            // Position the piece on the board
            PositionSinglePiece(newX, newY);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in MovingPiece: " + e.Message);
        }
    }

    public void KillPiece(int killPieceType, ChessPiece ocp)
    {
        if (killPieceType != (int)ChessPieceType.None && ocp != null)
        {
            try
            {
                board.atePiece(ocp, ocp.team == (int)TeamColor.White);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error in KillPiece: " + e.Message);
            }
        }
    }

    private void HandleWin(int winningTeam)
    {
        Debug.Log("HandleWin called for team: " + winningTeam);
        
        // Make sure we have valid team
        if (winningTeam != (int)TeamColor.White && winningTeam != (int)TeamColor.Black)
        {
            Debug.LogError("Invalid winning team: " + winningTeam);
            return;
        }

        // Determine winner message
        string winMessage = winningTeam == (int)TeamColor.White ? "White Win!" : "Black Win!";
        
        // Let the NetworkTurnManager handle the rest
        // This ensures consistent win handling across the network
        if (NetworkTurnManager.Instance != null)
        {
            // Update UI state through VictoryManager
            if (winningTeam == (int)TeamColor.White)
            {
                if (NetworkTurnManager.Instance.isWhitePlayer())
                {
                    VictoryManager.instance.ShowVictoryPanel(winMessage);
                }
                else
                {
                    VictoryManager.instance.ShowDefeatPanel(winMessage);
                }
            }
            else
            {
                if (NetworkTurnManager.Instance.isBlackPlayer())
                {
                    VictoryManager.instance.ShowVictoryPanel(winMessage);
                }
                else
                {
                    VictoryManager.instance.ShowDefeatPanel(winMessage);
                }
            }
            
            VictoryManager.instance.active = true;
        }
        else
        {
            Debug.LogError("NetworkTurnManager.Instance is null in HandleWin");
        }
    }

    #endregion
}

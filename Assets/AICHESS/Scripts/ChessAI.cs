using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAI : MonoBehaviour
{
    public static ChessAI Instance { set; get; }

    /* ------------------------- Cloning the Environment ----------------------*/
    private List<ChessmanAi> ActiveChessmans;
    private ChessmanAi[,] Chessmans;
    private int[] EnPassant;

    // One refernece of actual Chessmans[,]
    // As we will have to change the main Chessmans[,] array in BoardManager with its clone
    // (Because class Chessman and its descendent classes are using BoardManager.Instance.Chessmans)
    // And restore that again after thinking about NPC's next move (After Think() call gets over in NPCMove())
    // (Same goes for all Kings and Rooks, EnPassant move array)
    private ChessmanAi[,] ActualChessmansReference;
    private ChessmanAi ActualWhiteKing;
    private ChessmanAi ActualBlackKing;
    private ChessmanAi ActualWhiteRook1;
    private ChessmanAi ActualWhiteRook2;
    private ChessmanAi ActualBlackRook1;
    private ChessmanAi ActualBlackRook2;
    private int[] ActualEnPassant;

    // Stack to store state history
    private Stack< State> History;

    // Maximum depth of exploration (No of total further moves to see the outcomes)
    private int maxDepth;

    // NPC chessman to be moved and position to where to move
    private ChessmanAi NPCSelectedChessman = null;
    private int moveX = -1;
    private int moveY = -1;
    private int winningValue = 0;

    // Variable to count Avg Response Time 
    private long totalTime = 0;
    private long totalRun = 0;
    public  long averageResponseTime = 0;

    string detail, board;

    private void Start()
    {
        Instance = this;
    }

    // Funtion that makes NPC move
    public void NPCMove()
    {
        StartCoroutine(CalculateMove());
    }

    private IEnumerator CalculateMove()
    {
        // Start the StopWatch
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        // Create a new History stack
        History = new Stack<State>();

        // Prepare the environment for AI calculation
        PrepareEnvironment();

        // Use a yield return to allow a frame to process in between
        // This helps prevent freezing during calculation
        yield return null;

        // Think for the move
        Think();

        // Restore the environment
        RestoreEnvironment();

        // Yield again before applying the move
        yield return null;

        // Apply the move
        ApplyMove();

        // Stop the StopWatch
        stopwatch.Stop();
        totalTime += stopwatch.ElapsedMilliseconds;
        totalRun++;

        // Update average response time
        averageResponseTime = totalTime / totalRun;
    }

    // Split NPCMove into helper methods for better organization
    private void PrepareEnvironment()
    {
        ActualChessmansReference = BoardManager.Instance.Chessmans;
        ActualWhiteKing = BoardManager.Instance.WhiteKing;
        ActualBlackKing = BoardManager.Instance.BlackKing;
        ActualWhiteRook1 = BoardManager.Instance.WhiteRook1;
        ActualWhiteRook2 = BoardManager.Instance.WhiteRook2;
        ActualBlackRook1 = BoardManager.Instance.BlackRook1;
        ActualBlackRook2 = BoardManager.Instance.BlackRook2;
        ActualEnPassant = BoardManager.Instance.EnPassant;

        ActiveChessmans = new List<ChessmanAi>();
        Chessmans = new ChessmanAi[8, 8];

        for(int x=0; x<8; x++)
            for(int y=0; y<8; y++)
            {
                if(ActualChessmansReference[x, y] != null)
                {
                    ChessmanAi currChessman = ActualChessmansReference[x, y].Clone();
                    ActiveChessmans.Add(currChessman);
                    Chessmans[x, y] = currChessman;
                }
                else
                {
                    Chessmans[x, y] = null;
                }
            }

        Shuffle(ActiveChessmans);

        EnPassant = new int[2]{ActualEnPassant[0], ActualEnPassant[0]};
        
        // Set up the environment for AI calculation
        BoardManager.Instance.Chessmans = Chessmans;
        BoardManager.Instance.WhiteKing = Chessmans[ActualWhiteKing.currentX, ActualWhiteKing.currentY];
        BoardManager.Instance.BlackKing = Chessmans[ActualBlackKing.currentX, ActualBlackKing.currentY];
        if(ActualWhiteRook1 != null) BoardManager.Instance.WhiteRook1 = Chessmans[ActualWhiteRook1.currentX, ActualWhiteRook1.currentY];
        if(ActualWhiteRook2 != null) BoardManager.Instance.WhiteRook2 = Chessmans[ActualWhiteRook2.currentX, ActualWhiteRook2.currentY];
        if(ActualBlackRook1 != null) BoardManager.Instance.BlackRook1 = Chessmans[ActualBlackRook1.currentX, ActualBlackRook1.currentY];
        if(ActualBlackRook2 != null) BoardManager.Instance.BlackRook2 = Chessmans[ActualBlackRook2.currentX, ActualBlackRook2.currentY];
        BoardManager.Instance.EnPassant = EnPassant;
    }

    private void RestoreEnvironment()
    {
        // Restore the environment
        BoardManager.Instance.Chessmans = ActualChessmansReference;
        BoardManager.Instance.WhiteKing = ActualWhiteKing;
        BoardManager.Instance.BlackKing = ActualBlackKing;
        BoardManager.Instance.WhiteRook1 = ActualWhiteRook1;
        BoardManager.Instance.WhiteRook2 = ActualWhiteRook2;
        BoardManager.Instance.BlackRook1 = ActualBlackRook1;
        BoardManager.Instance.BlackRook2 = ActualBlackRook2;
        BoardManager.Instance.EnPassant = ActualEnPassant;
    }

    private void ApplyMove()
    {
        // Apply the move to the actual board
        Debug.Log(NPCSelectedChessman.GetType() + " to (" + moveX + ", " + moveY + ") " + winningValue + "\n");
        BoardManager.Instance.SelectedChessman = BoardManager.Instance.Chessmans[NPCSelectedChessman.currentX, NPCSelectedChessman.currentY];
        BoardManager.Instance.allowedMoves = BoardManager.Instance.SelectedChessman.PossibleMoves();
        BoardManager.Instance.MoveChessman(moveX, moveY);
    }

    private void Think()
    {
        // Use a dynamic depth based on game phase for better performance
        int piecesOnBoard = ActiveChessmans.Count;
        
        // Adjust depth based on number of pieces
        if (piecesOnBoard > 24) // Opening
            maxDepth = 3;
        else if (piecesOnBoard > 10) // Middlegame
            maxDepth = 4;
        else // Endgame
            maxDepth = 5;
        
        int depth = maxDepth-1;
        winningValue = AlphaBeta(depth, true, System.Int32.MinValue, System.Int32.MaxValue);
    }

    private int MiniMax(int depth, bool isMax)
    {
        // If max depth is reached or Game is Over
        if(depth == 0 || isGameOver())
        {
            // Static Evaluation Function
            int value = StaticEvaluationFunction();
            
            return value;
        }

        // string ActiveChessmansDetail = "";

        // If it is max turn(NPC turn : Black)
        if(isMax)
        {
            int hValue = System.Int32.MinValue;
            // int ind = 0;
            // Get list of all possible moves with their heuristic value
            // For all chessmans
            foreach(ChessmanAi chessman in ActiveChessmans.ToArray())
            {
                // ActiveChessmansDetail = ActiveChessmansDetail + "(" + ++ind + ")" + (chessman.isWhite?"White":"Black") + chessman.GetType() + "(" + chessman.currentX + ", " + chessman.currentY + ")" + "\t\t ";

                if(chessman.isWhite) continue;

                bool[,] allowedMoves = chessman.PossibleMoves();

                // detail = detail + "(" + ind + ") " + (chessman.isWhite?"White":"Black") + chessman.GetType() + " at (" + chessman.currentX + ", " + chessman.currentY + ") moves :" + printMoves(allowedMoves);

                // For all possible moves
                for(int x=0; x<8; x++)
                {
                    for(int y=0; y<8; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            // detail = detail + printTabs(maxDepth - depth) + "(" + ind + ") " + " " + (depth + " Moving Black " + chessman.GetType() + " to (" + x + ", " + y + ")");
                            
                            // Critical Section : 
                            // 1) Making the current move to see next possible moves after this move in next calls
                            Move(chessman, x, y, depth);

                            // 2 ) Calculate heuristic value current move
                            int thisMoveValue = MiniMax(depth-1, !isMax);

                            if(hValue < thisMoveValue) 
                            {
                                hValue = thisMoveValue;

                                // Remember which move gave the highest hValue
                                if(depth == maxDepth-1)
                                {
                                    NPCSelectedChessman = chessman;
                                    moveX = x;
                                    moveY = y;
                                }
                            }
                           
                            // if(depth-1 == 0) detail = detail + " " + thisMoveValue + "\n";
                            // else detail = detail + "\n";

                            // 3 ) Undo the current move to get back the same state that was there before making the current move
                            Undo(depth);
                        }
                    }
                }
            }

            // if(depth == maxDepth-1) detail += "ActiveChessmans : \n" + ActiveChessmansDetail + "\n";

            return hValue;
        }
        // If it is min turn(Player turn : White)
        else
        {
            int hValue = System.Int32.MaxValue;
            // int ind = 0;

            // Get list of all possible moves with their heuristic value
            // For all chessmans
            foreach(ChessmanAi chessman in ActiveChessmans.ToArray())
            {
                // ActiveChessmansDetail = ActiveChessmansDetail + "\n(" + ++ind + ")" + (chessman.isWhite?"White":"Black") + chessman.GetType() + "(" + chessman.currentX + ", " + chessman.currentY + ")" + "\t\t ";

                if(!chessman.isWhite) continue;

                bool[,] allowedMoves = chessman.PossibleMoves();

                // if(depth == 2) detail = detail + "(" + ind + ") " + (chessman.isWhite?"White":"Black") + chessman.GetType() + " at (" + chessman.currentX + ", " + chessman.currentY + ") moves :" + printMoves(allowedMoves);

                // For all possible moves
                for(int x=0; x<8; x++)
                {
                    for(int y=0; y<8; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            // detail = detail + printTabs(maxDepth - depth) + "(" + ind + ") " + " " + (depth + " Moving White " + chessman.GetType() + " to (" + x + ", " + y + ")\n");
                            
                            // Critical Section : 
                            // 1) Making the current move to see next possible moves after this move in next calls
                            Move(chessman, x, y, depth);

                            // 2 ) Calculate heuristic value current move
                            int thisMoveValue = MiniMax(depth-1, !isMax);

                            if(hValue > thisMoveValue) 
                            {
                                hValue = thisMoveValue;
                                // The following 6-7 lines are commented, that is suggesting that 
                                // We won't update NPCSelectedChessman, moveX and moveY in min turn
                                // if(depth == maxDepth-1)
                                // {
                                //     NPCSelectedChessman = chessman;
                                //     moveX = x;
                                //     moveY = y;
                                // }
                            }

                            // if(depth-1 == 0) detail = detail + " " + thisMoveValue + "\n";
                            // else detail = detail + "\n";

                            // 3 ) Undo the current move to get back the same state that was there before making the current move
                            Undo(depth);
                        }
                    }
                }
            }

            // if(depth == maxDepth-1) detail += "ActiveChessmans : \n" + ActiveChessmansDetail + "\n";

            return hValue;
        }
    }

    private int AlphaBeta(int depth, bool isMax, int alpha, int beta)
    {
        // Early return for terminal nodes
        if(depth == 0 || isGameOver())
        {
            return StaticEvaluationFunction();
        }

        // For max player (AI/Black)
        if(isMax)
        {
            int hValue = System.Int32.MinValue;
            
            // Filter relevant chessmen first to improve performance
            List<ChessmanAi> relevantChessmen = ActiveChessmans.FindAll(chessman => !chessman.isWhite);
            
            foreach(ChessmanAi chessman in relevantChessmen)
            {
                bool[,] allowedMoves = chessman.PossibleMoves();
                bool hasMoves = false;
                
                // Check if there are any moves before full iteration
                for(int x=0; x<8 && !hasMoves; x++)
                    for(int y=0; y<8 && !hasMoves; y++)
                        if(allowedMoves[x,y]) hasMoves = true;
                        
                if(!hasMoves) continue; // Skip pieces with no moves
                
                // Process moves
                for(int x=0; x<8 && alpha < beta; x++)
                {
                    for(int y=0; y<8 && alpha < beta; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            Move(chessman, x, y, depth);
                            int thisMoveValue = AlphaBeta(depth-1, !isMax, alpha, beta);
                            Undo(depth);

                            if(hValue < thisMoveValue) 
                            {
                                hValue = thisMoveValue;
                                if(depth == maxDepth-1)
                                {
                                    NPCSelectedChessman = chessman;
                                    moveX = x;
                                    moveY = y;
                                }
                            }

                            alpha = System.Math.Max(alpha, hValue);
                            if(beta <= alpha) break;
                        }
                    }
                    if(beta <= alpha) break;
                }
            }
            return hValue;
        }
        // For min player (Human/White)
        else
        {
            int hValue = System.Int32.MaxValue;
            
            // Filter relevant chessmen first
            List<ChessmanAi> relevantChessmen = ActiveChessmans.FindAll(chessman => chessman.isWhite);
            
            foreach(ChessmanAi chessman in relevantChessmen)
            {
                bool[,] allowedMoves = chessman.PossibleMoves();
                bool hasMoves = false;
                
                // Check if there are any moves before full iteration
                for(int x=0; x<8 && !hasMoves; x++)
                    for(int y=0; y<8 && !hasMoves; y++)
                        if(allowedMoves[x,y]) hasMoves = true;
                        
                if(!hasMoves) continue; // Skip pieces with no moves
                
                // Process moves
                for(int x=0; x<8 && alpha < beta; x++)
                {
                    for(int y=0; y<8 && alpha < beta; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            Move(chessman, x, y, depth);
                            int thisMoveValue = AlphaBeta(depth-1, !isMax, alpha, beta);
                            Undo(depth);

                            hValue = System.Math.Min(hValue, thisMoveValue);
                            beta = System.Math.Min(beta, hValue);
                            
                            if(beta <= alpha) break;
                        }
                    }
                    if(beta <= alpha) break;
                }
            }
            return hValue;
        }
    }

    // This function is simply calculating the summation of Chessman values
    private int StaticEvaluationFunction()
    {
        int TotalScore = 0;
        int curr = 0;
        foreach(ChessmanAi chessman in ActiveChessmans)
        {
            if(chessman.GetType() == typeof(KingAi))
                curr = 900;
            if(chessman.GetType() == typeof(QueenAi))
                curr = 90;
            if(chessman.GetType() == typeof(RookAi))
                curr = 50;
            if(chessman.GetType() == typeof(Bishup))
                curr = 30;
            if(chessman.GetType() == typeof(KnightAi))
                curr = 30;
            if(chessman.GetType() == typeof(PawnAi))
                curr = 10;

            if(chessman.isWhite)
                TotalScore -= curr;
            else
                TotalScore += curr;
        }
        return TotalScore;
    }

    // Checking for checkmate (Game Over)
    private bool isGameOver()
    {
        // To be implemented
        int currScore = StaticEvaluationFunction();
        if((currScore < -290 ) || (currScore > 290))
            return true;
        return false;
    }

    private void Move(ChessmanAi chessman, int x, int y, int depth)
    {
        // Current state variables to be stored
        // About chessman to be moved
        (ChessmanAi chessman, (int x, int y) oldPosition, (int x, int y) newPosition, bool isMoved) movedChessman;
        // Chessman to be captured
        (ChessmanAi chessman, (int x, int y) Position) capturedChessman = (null, (-1, -1));
        // Current allowed EnPassant move Status
        (int x, int y) EnPassantStatus;
        // About Promotion move : (was Promotion), Queen(promoted chessman)
        (bool wasPromotion, ChessmanAi promotedChessman) PromotionMove = (false, null);
        // About Castling move : (was Castling done, King Side or not)
        (bool wasCastling, bool isKingSide) CastlingMove;

        movedChessman.chessman = chessman;
        movedChessman.oldPosition = (chessman.currentX, chessman.currentY);
        movedChessman.newPosition = (x, y);
        movedChessman.isMoved = chessman.isMoved;

        EnPassantStatus = (EnPassant[0], EnPassant[1]);

        // Capturing
        ChessmanAi opponent = Chessmans[x, y];
        if(opponent != null)
        {
            // Capture an opponent piece
            capturedChessman.chessman = opponent;
            capturedChessman.Position = (x, y);

            Chessmans[x, y] = null;
            ActiveChessmans.Remove(opponent);
        }
        
        // --------------EnPassant move manager--------------
        // If it is an EnPassant move than Capture the opponent
        if (EnPassant[0] == x && EnPassant[1] == y && chessman.GetType() == typeof(PawnAi))
        {           
            if(chessman.isWhite)
            {
                opponent = Chessmans[x, y + 1];

                capturedChessman.chessman = opponent;
                capturedChessman.Position = (x, y + 1);
                Chessmans[x, y + 1] = null;
            }
            else
            {
                opponent = Chessmans[x, y - 1];

                capturedChessman.chessman = opponent;
                capturedChessman.Position = (x, y - 1);
                Chessmans[x, y - 1] = null;
            }

            ActiveChessmans.Remove(opponent);
        }

        // Reset the EnPassant move
        EnPassant[0] = EnPassant[1] = -1;

        // Set EnPassant available for opponent
        if(chessman.GetType() == typeof(PawnAi))
        {
            //-------Promotion Move Manager------------
            if (y == 7 || y == 0)
            {
                ActiveChessmans.Remove(chessman);
                Chessmans[x, y] = gameObject.AddComponent<QueenAi>(); 
                Chessmans[x, y].SetPosition(x, y);
                Chessmans[x, y].isWhite = chessman.isWhite;
                chessman = Chessmans[x, y];
                ActiveChessmans.Add(chessman);

                PromotionMove = (true, chessman);
            }
            //-------Promotion Move Manager Over-------

            if (chessman.currentY == 1 && y == 3)
            {
                EnPassant[0] = x;
                EnPassant[1] = y - 1;
            }
            if (chessman.currentY == 6 && y == 4)
            {
                EnPassant[0] = x;
                EnPassant[1] = y + 1;
            }
        }
        // -------EnPassant Move Manager Over-------

        // -------Castling Move Manager------------
        CastlingMove = (false, false);

        // If the selected chessman is King and is trying Castling move(two steps movement)
        if(chessman.GetType() == typeof(KingAi) && System.Math.Abs(x - chessman.currentX) == 2)
        {          
            // King Side (towards (0, 0))
            if(x - chessman.currentX < 0)
            {
                // Moving Rook1
                Chessmans[x + 1, y] = Chessmans[x - 1, y];
                Chessmans[x - 1, y] = null;
                Chessmans[x + 1, y].SetPosition(x + 1, y);
                Chessmans[x + 1, y].isMoved = true;

                CastlingMove = (true, true);
            }
            // Queen side (away from (0, 0))
            else
            {
                // Moving Rook2
                Chessmans[x - 1, y] = Chessmans[x + 2, y];
                Chessmans[x + 2, y] = null;
                Chessmans[x - 1, y].SetPosition(x - 1, y);
                Chessmans[x - 1, y].isMoved = true;

                CastlingMove = (true, false);
            }
            // Note : King will move as a chessman by this function later
        }
        // -------Castling Move Manager Over-------

        // Now moving
        Chessmans[chessman.currentX, chessman.currentY] = null;
        Chessmans[x, y] = chessman;
        chessman.SetPosition(x, y);
        chessman.isMoved = true;

        // Save the current state to the History Stack
        State currentState = new State();
        currentState.SetState(movedChessman, capturedChessman, EnPassantStatus, PromotionMove, CastlingMove, depth);
        History.Push(currentState);
    }

    private void Undo(int depth)
    {
        // Get current state from the top of the stack
        State currentState = History.Pop();

        // Current depth should be matched with the currentState.depth from the stack
        if(depth != currentState.depth)
        {
            Debug.Log("Depth not matched!!!");
            return;
        }

        // Current state variables
        // About chessman to be moved
        var movedChessman = currentState.movedChessman;
        // Chessman to be captured
        var capturedChessman = currentState.capturedChessman;
        // Current allowed EnPassant move Status
        var EnPassantStatus = currentState.EnPassantStatus;
        // About Promotion move : (was Promotion), Queen(promoted chessman)
        var PromotionMove = currentState.PromotionMove;
        // About Castling move : (was Castling done, King Side or not)
        var CastlingMove = currentState.CastlingMove;

        // Restore EnPassant
        EnPassant[0] = EnPassantStatus.x;
        EnPassant[1] = EnPassantStatus.y;

        // Restore the moved chessman from newPosition to oldPosition
        ChessmanAi chessman = movedChessman.chessman;
        chessman.isMoved = movedChessman.isMoved;
        chessman.SetPosition(movedChessman.oldPosition.x, movedChessman.oldPosition.y);
        Chessmans[movedChessman.oldPosition.x, movedChessman.oldPosition.y] = chessman;
        Chessmans[movedChessman.newPosition.x, movedChessman.newPosition.y] = null;

        // Restore Promotion move
        if(PromotionMove.wasPromotion)
        {
            ActiveChessmans.Remove(PromotionMove.promotedChessman);
            ActiveChessmans.Add(chessman);
        }

        // Restore the captured piece to its position
        var opponent = capturedChessman;
        if(opponent.chessman != null)
        {
            Chessmans[opponent.Position.x, opponent.Position.y] = opponent.chessman;
            opponent.chessman.SetPosition(opponent.Position.x, opponent.Position.y);
            ActiveChessmans.Add(opponent.chessman);
        }
        

        // Restore Castling move
        if(CastlingMove.wasCastling)
        {
            int x = movedChessman.newPosition.x;
            int y = movedChessman.newPosition.y;
            // King Side (towards (0, 0))
            if(CastlingMove.isKingSide)
            {
                // Restore Rook1
                Chessmans[x - 1, y] = Chessmans[x + 1, y];
                Chessmans[x + 1, y] = null;
                Chessmans[x - 1, y].SetPosition(x - 1, y);
                Chessmans[x - 1, y].isMoved = false;
            }
            // Queen side (away from (0, 0))
            else
            {
                // Restore Rook2
                Chessmans[x + 2, y] = Chessmans[x - 1, y];
                Chessmans[x - 1, y] = null;
                Chessmans[x + 2, y].SetPosition(x + 2, y);
                Chessmans[x + 2, y].isMoved = false;
            }
            // Note : King is already moved as a chessman by this function before
        }
    }

    public void Shuffle(List<ChessmanAi> list)  
    {  
        System.Random rng = new System.Random();

        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);
            ChessmanAi value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    // private string printTabs(int num)
    // {
    //     string detail = "";
    //     while(--num > 0) detail = detail + "\t";
    //     return detail;
    // }

    // private string printMoves(bool[,] moves)
    // {
    //     string str = "\n";
    //     for(int i=0; i<8; i++)
    //     {
    //         for(int j=7; j>=0; j--)
    //         {
    //             str = str + (moves[j, i]?1:0) + " ";
    //         }
    //         str = str + "\n";
    //     }
    //     return str;
    // }

    // //to be deleted
    // private void printBoard()
    // {
    //     Chessman[,] Chessmans = BoardManager.Instance.Chessmans;
    //     for(int i=0; i<8; i++)
    //     {
    //         for(int j=7; j>=0; j--)
    //         {
    //             if(Chessmans[j,i] == null)
    //             {
    //                 board = board + "[] ";
    //                 continue;
    //             }

    //             board = board + (Chessmans[j,i].isWhite ? "W":"B");
    //             Chessman chessman = Chessmans[j,i];

    //             if(chessman.GetType() == typeof(King))
    //                 board = board + "K ";
    //             if(chessman.GetType() == typeof(Queen))
    //                 board = board + "Q ";
    //             if(chessman.GetType() == typeof(Rook))
    //                 board = board + "R ";
    //             if(chessman.GetType() == typeof(Bishup))
    //                 board = board + "B ";
    //             if(chessman.GetType() == typeof(Knight))
    //                 board = board + "k ";
    //             if(chessman.GetType() == typeof(Pawn))
    //                 board = board + "P ";
    //         }

    //         board = board + "\n";
    //     }
    // }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public class BoardManagerOffline : MonoBehaviour
{
    public static BoardManagerOffline Instance { get; set; }
    private bool[,] allowedMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public ChessmanOffline[,] Chessmans { get; set; }
    private ChessmanOffline selectedChessman;

    public bool isWhiteTurn = true;

    private Material previousMat;
    public Material selectedMat;

    private List<GameObject> whiteCapturedPieces = new List<GameObject>();
    private List<GameObject> blackCapturedPieces = new List<GameObject>();

    public Vector3 whiteCaptureZone = new Vector3(-2f, 0f, 8f);  // Adjust this based on your board size
    public Vector3 blackCaptureZone = new Vector3(10f, 0f, 8f);  // Adjust this based on your board size
    private float capturedPieceOffset = 0.7f;  // Spacing between captured pieces
    public float moveSpeed = 2.0f; // Speed of the movement

    public AudioSource audioSource;  // The AudioSource component to play sounds
    public AudioClip moveSound;      // Sound for moving pieces
    public AudioClip captureSound;   // Sound for capturing pieces
    public AudioClip turnSwitchSound; // Sound for switching turns
    public AudioClip turnVictorySound; // Sound for switching turns

    private HighlightHelper highlightHelper; // Add this at the top of BoardManagerOffline

    public int[] EnPassantMove { set; get; }

    // Add these variables for promotion UI
    public GameObject promotionUI;
    public GameObject[] whitePromotionPieces;
    public GameObject[] blackPromotionPieces;
    private int promotionX;
    private int promotionY;
    private bool waitingForPromotion = false;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllChessmans();
        EnPassantMove = new int[2] { -1, -1 };

        highlightHelper = GetComponent<HighlightHelper>(); // Attach HighlightHelper to the BoardManagerOffline object
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null) return;

        if (Chessmans[x, y].isWhite != isWhiteTurn) return;

        bool hasAtLeastOneMove = false;

        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
            return;

        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            ChessmanOffline c = Chessmans[x, y];

            // Clean previous team's highlights before the new move
            highlightHelper.CleanTeamHighlights();

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece
                if (c.GetType() == typeof(KingOffline))
                {
                    // End the game if the king is captured
                    EndGame();
                    return;
                }
    
                // Move the captured piece to the side, instead of destroying it
                MoveToCapturedZone(c);
    
                // Remove the piece from the active chessman list
                activeChessman.Remove(c.gameObject);

                // Play capture sound
                PlaySound(captureSound);
            }
    
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];
    
                // Move the en passant captured piece to the side
                MoveToCapturedZone(c);
                activeChessman.Remove(c.gameObject);

                // Play capture sound
                PlaySound(captureSound);
            }
    
            // Reset En Passant Move
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
    
            // Handle castling
            if (selectedChessman.GetType() == typeof(KingOffline))
            {
                KingOffline king = selectedChessman as KingOffline;
                
                // Kingside castling
                if (x == king.CurrentX + 2)
                {
                    ChessmanOffline rook = Chessmans[7, king.CurrentY];
                    if (rook != null && rook.GetType() == typeof(RookOffline))
                    {
                        // Move the rook to the left of the king
                        Chessmans[7, king.CurrentY] = null;
                        rook.SetPosition(x - 1, y);
                        Chessmans[x - 1, y] = rook;
                        rook.transform.position = GetTileCenter(x - 1, y);
                    }
                }
                // Queenside castling
                else if (x == king.CurrentX - 2)
                {
                    ChessmanOffline rook = Chessmans[0, king.CurrentY];
                    if (rook != null && rook.GetType() == typeof(RookOffline))
                    {
                        // Move the rook to the right of the king
                        Chessmans[0, king.CurrentY] = null;
                        rook.SetPosition(x + 1, y);
                        Chessmans[x + 1, y] = rook;
                        rook.transform.position = GetTileCenter(x + 1, y);
                    }
                }
                
                // Set hasMoved flag for king
                king.hasMoved = true;
            }
            
            // Set hasMoved flag for rook
            if (selectedChessman.GetType() == typeof(RookOffline))
            {
                RookOffline rook = selectedChessman as RookOffline;
                rook.hasMoved = true;
            }
    
            if (selectedChessman.GetType() == typeof(PawnOffline))
            {
                // Check if pawn moved two spaces (for en passant)
                if (Mathf.Abs(selectedChessman.CurrentY - y) == 2)
                {
                    EnPassantMove[0] = x;
                    EnPassantMove[1] = y + (selectedChessman.isWhite ? -1 : 1);
                }
                
                // Handle pawn promotion
                if ((isWhiteTurn && y == 7) || (!isWhiteTurn && y == 0)) 
                {
                    // Store the promotion position
                    promotionX = x;
                    promotionY = y;
                    
                    // Move the pawn to the promotion square
                    Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
                    selectedChessman.transform.position = GetTileCenter(x, y);
                    selectedChessman.SetPosition(x, y);
                    Chessmans[x, y] = selectedChessman;
                    
                    // Show promotion UI
                    ShowPromotionUI(isWhiteTurn);
                    
                    // Set the flag to wait for user choice
                    waitingForPromotion = true;
                    
                    // Don't continue the move sequence
                    selectedChessman.GetComponent<MeshRenderer>().material = previousMat;
                    BoardHighlights.Instance.HideHighlights();
                    selectedChessman = null;
                    return;
                }
            }

            // Update chessman position
            Vector3 oldPosition = selectedChessman.transform.position; // Get the old position before moving


            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y); // Move to the new position
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;

            // Highlight the move
            highlightHelper.HighlightMove(oldPosition, GetTileCenter(x, y));

            // Play move sound
            PlaySound(moveSound);

            // Toggle turn
            isWhiteTurn = !isWhiteTurn;
            OfflineUicontroller.instance.AutoSwitch();
            
            // Check for checkmate or stalemate
            if (IsCheckmate(!isWhiteTurn))
            {
                EndGame(true);
                return;
            }
            else if (IsStalemate(!isWhiteTurn))
            {
                EndGame(false);
                return;
            }
            
            // Play turn switch sound
            PlaySound(turnSwitchSound);
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


    private void MoveToCapturedZone(ChessmanOffline capturedPiece)
    {
        Vector3 targetPosition;

        if (capturedPiece.isWhite)
        {
            // Move to white's captured zone along the Z-axis with spacing
            targetPosition = whiteCaptureZone + new Vector3(0, 0, whiteCapturedPieces.Count * capturedPieceOffset);
            whiteCapturedPieces.Add(capturedPiece.gameObject);
        }
        else
        {
            // Move to black's captured zone along the Z-axis with spacing
            targetPosition = blackCaptureZone + new Vector3(0, 0, -blackCapturedPieces.Count * capturedPieceOffset);
            blackCapturedPieces.Add(capturedPiece.gameObject);
        }

        // Start the coroutine to move the piece smoothly to the target position
        StartCoroutine(SmoothMove(capturedPiece.transform, targetPosition));
    }

    // Coroutine to move the piece over time
    private IEnumerator SmoothMove(Transform piece, Vector3 targetPosition)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = piece.position;

        while (elapsedTime < 1f) // Adjust 1f to control the total duration of the move
        {
            piece.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * moveSpeed; // Use moveSpeed to control how fast it moves
            yield return null; // Wait for the next frame
        }

        // Ensure the piece is set to the exact target position at the end
        piece.position = targetPosition;
    }



    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<ChessmanOffline>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    private void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new ChessmanOffline[8, 8];

        /////// White ///////

        // King
        SpawnChessman(0, 3, 0, true);

        // Queen
        SpawnChessman(1, 4, 0, true);

        // Rooks
        SpawnChessman(2, 0, 0, true);
        SpawnChessman(2, 7, 0, true);

        // Bishops
        SpawnChessman(3, 2, 0, true);
        SpawnChessman(3, 5, 0, true);

        // Knights
        SpawnChessman(4, 1, 0, true);
        SpawnChessman(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false);

        // Queen
        SpawnChessman(7, 3, 7, false);

        // Rooks
        SpawnChessman(8, 0, 7, false);
        SpawnChessman(8, 7, 7, false);

        // Bishops
        SpawnChessman(9, 2, 7, false);
        SpawnChessman(9, 5, 7, false);

        // Knights
        SpawnChessman(10, 1, 7, false);
        SpawnChessman(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
    }

    private void EndGame(bool isCheckmate = true)
    {
        audioSource.clip = turnVictorySound;
        audioSource.Play();

        if (isCheckmate)
        {
            if (isWhiteTurn)
            {
                print("Black wins by checkmate");
                WinPanel.SetActive(true);
                DisplayText.text = "Black wins by checkmate";
            }
            else
            {
                print("White wins by checkmate");
                WinPanel.SetActive(true);
                DisplayText.text = "White wins by checkmate";
            }
        }
        else
        {
            // Stalemate
            print("Draw by stalemate");
            WinPanel.SetActive(true);
            DisplayText.text = "Draw by stalemate";
        }
    }

    public void Pause()
    {
        if(isPause == true)
        {
            Time.timeScale = 0;
            isPause = false;
        }
        else if (isPause == false)
        {
            Time.timeScale = 1;
            isPause = true;
        }
    }

    public GameObject WinPanel;
    public TMP_Text DisplayText;
    public bool isPause;

    public void RestartGame()
    {
        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        // Clear captured pieces lists
        foreach (GameObject go in whiteCapturedPieces)
        {
            Destroy(go);
        }
        whiteCapturedPieces.Clear();

        foreach (GameObject go in blackCapturedPieces)
        {
            Destroy(go);
        }
        blackCapturedPieces.Clear();

        WinPanel.SetActive(false);
        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans();

        // Clean all highlights
        highlightHelper.CleanAllHighlights();
    }

    public bool IsInCheck(bool isWhite)
    {
        // Find the king's position
        int kingX = -1;
        int kingY = -1;
        
        // Find the king
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessmanOffline piece = Chessmans[x, y];
                if (piece != null && piece.GetType() == typeof(KingOffline) && piece.isWhite == isWhite)
                {
                    kingX = x;
                    kingY = y;
                    break;
                }
            }
            if (kingX != -1)
                break;
        }
        
        // If somehow king is not found
        if (kingX == -1 || kingY == -1)
            return false;
            
        // Check if any opponent piece can attack the king's position
        return IsSquareAttacked(kingX, kingY, isWhite);
    }
    
    public bool IsSquareAttacked(int x, int y, bool isWhite)
    {
        // Check if any opponent piece can attack this square
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ChessmanOffline piece = Chessmans[i, j];
                if (piece != null && piece.isWhite != isWhite)
                {
                    bool[,] moves = piece.PossibleMoves();
                    if (moves[x, y])
                        return true;
                }
            }
        }
        
        return false;
    }
    
    // Add this method to check for checkmate
    public bool IsCheckmate(bool isWhite)
    {
        // If not in check, it's not checkmate
        if (!IsInCheck(isWhite))
            return false;
            
        // Check if any move can get the king out of check
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessmanOffline piece = Chessmans[x, y];
                if (piece != null && piece.isWhite == isWhite)
                {
                    bool[,] moves = piece.PossibleMoves();
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (moves[i, j])
                            {
                                // Try the move
                                ChessmanOffline originalPiece = Chessmans[i, j];
                                int originalX = piece.CurrentX;
                                int originalY = piece.CurrentY;
                                
                                // Temporarily make the move
                                Chessmans[originalX, originalY] = null;
                                piece.SetPosition(i, j);
                                Chessmans[i, j] = piece;
                                
                                // Check if still in check
                                bool stillInCheck = IsInCheck(isWhite);
                                
                                // Undo the move
                                piece.SetPosition(originalX, originalY);
                                Chessmans[originalX, originalY] = piece;
                                Chessmans[i, j] = originalPiece;
                                
                                if (!stillInCheck)
                                    return false; // Found a legal move
                            }
                        }
                    }
                }
            }
        }
        
        return true; // No legal moves found - checkmate
    }
    
    // Add this method to check for stalemate
    public bool IsStalemate(bool isWhite)
    {
        // If in check, it's not stalemate
        if (IsInCheck(isWhite))
            return false;
            
        // Check if player has any legal moves
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessmanOffline piece = Chessmans[x, y];
                if (piece != null && piece.isWhite == isWhite)
                {
                    bool[,] moves = piece.PossibleMoves();
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (moves[i, j])
                            {
                                // Try the move
                                ChessmanOffline originalPiece = Chessmans[i, j];
                                int originalX = piece.CurrentX;
                                int originalY = piece.CurrentY;
                                
                                // Temporarily make the move
                                Chessmans[originalX, originalY] = null;
                                piece.SetPosition(i, j);
                                Chessmans[i, j] = piece;
                                
                                // Check if the move puts the player in check
                                bool putsInCheck = IsInCheck(isWhite);
                                
                                // Undo the move
                                piece.SetPosition(originalX, originalY);
                                Chessmans[originalX, originalY] = piece;
                                Chessmans[i, j] = originalPiece;
                                
                                if (!putsInCheck)
                                    return false; // Found a legal move
                            }
                        }
                    }
                }
            }
        }
        
        return true; // No legal moves found - stalemate
    }

    private void ShowPromotionUI(bool isWhite)
    {
        if (promotionUI != null)
        {
            promotionUI.SetActive(true);
            
            // Show appropriate promotion pieces based on color
            foreach (GameObject piece in whitePromotionPieces)
            {
                piece.SetActive(isWhite);
            }
            
            foreach (GameObject piece in blackPromotionPieces)
            {
                piece.SetActive(!isWhite);
            }
        }
    }
    
    public void PromoteTo(int pieceType)
    {
        if (waitingForPromotion)
        {
            // Get the pawn to be promoted
            ChessmanOffline pawn = Chessmans[promotionX, promotionY];
            if (pawn != null)
            {
                // Remove the pawn
                activeChessman.Remove(pawn.gameObject);
                Destroy(pawn.gameObject);
                
                // Spawn the selected piece
                int pieceIndex = pawn.isWhite ? pieceType : pieceType + 6; // Adjust for black pieces
                SpawnChessman(pieceIndex, promotionX, promotionY, pawn.isWhite);
                
                // Reset promotion flags
                waitingForPromotion = false;
                promotionUI.SetActive(false);
                
                // Toggle turn
                isWhiteTurn = !isWhiteTurn;
                OfflineUicontroller.instance.AutoSwitch();
                
                // Check for checkmate or stalemate
                if (IsCheckmate(!isWhiteTurn))
                {
                    EndGame(true);
                    return;
                }
                else if (IsStalemate(!isWhiteTurn))
                {
                    EndGame(false);
                    return;
                }
                
                // Play turn switch sound
                PlaySound(turnSwitchSound);
            }
        }
    }
}



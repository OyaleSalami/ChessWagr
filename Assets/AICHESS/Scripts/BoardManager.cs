using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = TILE_SIZE / 2;

    // Camera
    private Camera cam;

    // List of Chessman prefabs
    public List<GameObject> ChessmanPrefabs;
    // List of chessmans being on the board
    private List<GameObject> ActiveChessmans;
    // Array of the chessmans present on the particular board cell
    public ChessmanAi[,] Chessmans { set; get; }
    // Currently Selected Chessman
    public ChessmanAi SelectedChessman;
    // Kings
    public ChessmanAi WhiteKing;
    public ChessmanAi BlackKing;
    public ChessmanAi WhiteRook1;
    public ChessmanAi WhiteRook2;
    public ChessmanAi BlackRook1;
    public ChessmanAi BlackRook2;

    // Allowed moves
    public bool[,] allowedMoves;
    // EnPassant move
    public int[] EnPassant { set; get; }

    // The selected tile
    private int selectionX = -1;
    private int selectionY = -1;

    // Variable to store turn
    public bool isWhiteTurn = true;

    private List<GameObject> whiteCapturedPieces = new List<GameObject>();
    private List<GameObject> blackCapturedPieces = new List<GameObject>();

    public Vector3 whiteCaptureZone = new Vector3(-2f, 0f, 8f);  // Adjust this based on your board size
    public Vector3 blackCaptureZone = new Vector3(10f, 0f, 8f);  // Adjust this based on your board size
    private float capturedPieceOffset = 0.7f;  // Spacing between captured pieces
    public float moveSpeed = 2.0f; // Speed of the movement

    public HighlightManager highlightManager; // Drag your HighlightManager in the inspector

    // Audio components
    public AudioSource audioSource;  // The AudioSource component to play sounds
    public AudioClip moveSound;      // Sound for moving pieces
    public AudioClip captureSound;   // Sound for capturing pieces
    public AudioClip turnSwitchSound; // Sound for switching turns
    public AudioClip turnVictorySound; // Sound for victory
    public AudioClip turnDefeatSound; // Sound for victory

    private void Start()
    {
        Instance = this;
        cam = FindObjectOfType<Camera>();
        ActiveChessmans = new List<GameObject>();
        Chessmans = new ChessmanAi[8, 8];
        EnPassant = new int[2] { -1, -1 };

        // Spawning all chessmans on the board
        SpawnAllChessmans();
    }

    private void Update()
    {
        // Update Selected tile
        UpdateSelection();
        // Draw chessboard in every frame update
        DrawChessBoard();

        // Select/Move chessman on mouse click & it is Player's turn : White
        if (Input.GetMouseButtonDown(0) && isWhiteTurn)
        {
            if (selectionX >= 0 && selectionY >= 0 && selectionX <= 7 && selectionY <= 7)
            {
                // if no chessman is selected then we need to select it first
                if (SelectedChessman == null)
                {
                    SelectChessman();
                }
                // if chessman is already selected then we need to move it
                else
                {
                    MoveChessman(selectionX, selectionY);
                }
            }
        }
        // If it is NPC's turn : Black
        // Remove direct AI call from Update to prevent lag
    }

    private void SelectChessman()
    {
        // if no chessman is on the clicked tile
        if (Chessmans[selectionX, selectionY] == null) return;
        // if it is not the turn of the selected chessman's team
        if (Chessmans[selectionX, selectionY].isWhite != isWhiteTurn) return;

        // Selecting chessman with yellow highlight
        SelectedChessman = Chessmans[selectionX, selectionY];
        BoardHighlightsAi.Instance.SetTileYellow(selectionX, selectionY);

        // Allowed moves highlighted in blue and enemy in Red
        allowedMoves = SelectedChessman.PossibleMoves();
        BoardHighlightsAi.Instance.HighlightPossibleMoves(allowedMoves, isWhiteTurn);
    }

    public void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            OfflineUicontroller.instance.BotTurn();
            ChessmanAi opponent = Chessmans[x, y];

            // Save the old position before moving
            Vector3 oldPosition = new Vector3(SelectedChessman.currentX, 0, SelectedChessman.currentY);

            if (opponent != null)
            {
                // Capture an opponent piece
                ActiveChessmans.Remove(opponent.gameObject);
                MoveCapturedPieceToZone(opponent.gameObject); // Move the captured piece to the capture zone

                // Play capture sound
                audioSource.PlayOneShot(captureSound);
            }

            // -------EnPassant Move Manager------------
            // If it is an EnPassant move than Destroy the opponent
            if (EnPassant[0] == x && EnPassant[1] == y && SelectedChessman.GetType() == typeof(PawnAi))
            {
                if (isWhiteTurn)
                    opponent = Chessmans[x, y + 1];
                else
                    opponent = Chessmans[x, y - 1];

                ActiveChessmans.Remove(opponent.gameObject);
                MoveCapturedPieceToZone(opponent.gameObject); // Move the captured piece to the capture zone
            }

            // Reset the EnPassant move
            EnPassant[0] = EnPassant[1] = -1;

            // Set EnPassant available for opponent
            if (SelectedChessman.GetType() == typeof(PawnAi))
            {
                //-------Promotion Move Manager------------
                if (y == 7)
                {
                    ActiveChessmans.Remove(SelectedChessman.gameObject);
                    Destroy(SelectedChessman.gameObject);
                    SpawnChessman(10, new Vector3(x, 0, y));
                    SelectedChessman = Chessmans[x, y];
                }
                if (y == 0)
                {
                    ActiveChessmans.Remove(SelectedChessman.gameObject);
                    Destroy(SelectedChessman.gameObject);
                    SpawnChessman(4, new Vector3(x, 0, y));
                    SelectedChessman = Chessmans[x, y];
                }
                //-------Promotion Move Manager Over-------

                if (SelectedChessman.currentY == 1 && y == 3)
                {
                    EnPassant[0] = x;
                    EnPassant[1] = y - 1;
                }
                if (SelectedChessman.currentY == 6 && y == 4)
                {
                    EnPassant[0] = x;
                    EnPassant[1] = y + 1;
                }
            }
            // -------EnPassant Move Manager Over-------

            // -------Castling Move Manager------------
            // If the selected chessman is King and is trying Castling move which needs two steps
            if (SelectedChessman.GetType() == typeof(KingAi) && System.Math.Abs(x - SelectedChessman.currentX) == 2)
            {
                // King Side (towards (0, 0))
                if (x - SelectedChessman.currentX < 0)
                {
                    // Moving Rook1
                    Chessmans[x + 1, y] = Chessmans[x - 1, y];
                    Chessmans[x - 1, y] = null;
                    Chessmans[x + 1, y].SetPosition(x + 1, y);
                    Chessmans[x + 1, y].transform.position = new Vector3(x + 1, 0, y);
                    Chessmans[x + 1, y].isMoved = true;
                }
                // Queen side (away from (0, 0))
                else
                {
                    // Moving Rook2
                    Chessmans[x - 1, y] = Chessmans[x + 2, y];
                    Chessmans[x + 2, y] = null;
                    Chessmans[x - 1, y].SetPosition(x - 1, y);
                    Chessmans[x - 1, y].transform.position = new Vector3(x - 1, 0, y);
                    Chessmans[x - 1, y].isMoved = true;
                }
                // Note : King will move as a SelectedChessman by this function later
            }
            // -------Castling Move Manager Over-------

            // Call the HighlightManager to highlight the move
            highlightManager.HighlightMove(oldPosition, new Vector3(x, 0, y));

            Chessmans[SelectedChessman.currentX, SelectedChessman.currentY] = null;
            Chessmans[x, y] = SelectedChessman;
            SelectedChessman.SetPosition(x, y);
            // Start smooth move coroutine instead of immediate position change
            StartCoroutine(SmoothMove(SelectedChessman.gameObject.transform, new Vector3(x, 0, y)));
            SelectedChessman.isMoved = true;
            isWhiteTurn = !isWhiteTurn;

            // Play the move sound
            audioSource.PlayOneShot(moveSound);

            // If it's now AI's turn, start the AI turn with delay
            if (!isWhiteTurn)
            {
                StartAITurn();
            }

            // ------- King Check Alert Manager -----------
            // Is it Check to the King
            // If now White King is in Check
            if (isWhiteTurn)
            {
                if (WhiteKing.InDanger())
                    BoardHighlightsAi.Instance.SetTileCheck(WhiteKing.currentX, WhiteKing.currentY);
            }
            // If now Black King is in Check
            else
            {
                if (BlackKing.InDanger())
                    BoardHighlightsAi.Instance.SetTileCheck(BlackKing.currentX, BlackKing.currentY);
            }
            // ------- King Check Alert Manager Over ----

            // Check if it is Checkmate
            isCheckmate();
        }

        // De-select the selected chessman
        SelectedChessman = null;
        // Disabling all highlights
        BoardHighlightsAi.Instance.DisableAllHighlights();
    }

    // New method to move the chessman smoothly
    private IEnumerator SmoothMove(Transform pieceTransform, Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = pieceTransform.position;

        while (elapsedTime < moveSpeed)
        {
            pieceTransform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / moveSpeed));
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        pieceTransform.position = targetPosition; // Ensure the final position is set
    }
    // New method to move captured pieces to the respective zones
    private void MoveCapturedPieceToZone(GameObject capturedPiece)
    {
        // Get the ChessmanAi component to access the isWhite property
        ChessmanAi chessman = capturedPiece.GetComponent<ChessmanAi>();
        Vector3 targetPosition;

        if (chessman.isWhite)
        {
            // Move to white's captured zone along the Z-axis with spacing
            targetPosition = whiteCaptureZone + new Vector3(0, 0, -whiteCapturedPieces.Count * capturedPieceOffset);
            whiteCapturedPieces.Add(capturedPiece);
        }
        else
        {
            // Move to black's captured zone along the Z-axis with spacing
            targetPosition = blackCaptureZone + new Vector3(0, 0, blackCapturedPieces.Count * capturedPieceOffset);
            blackCapturedPieces.Add(capturedPiece);
        }

        // Start coroutine to move the piece smoothly to the target position
        StartCoroutine(MovePieceSmoothly(capturedPiece, targetPosition));
    }
    private IEnumerator MovePieceSmoothly(GameObject piece, Vector3 targetPosition)
    {
        // Get the original position of the piece
        Vector3 originalPosition = piece.transform.position;
        float elapsedTime = 0f;
        float duration = 1.0f; // Adjust this for speed (1 second for the whole movement)

        while (elapsedTime < duration)
        {
            // Calculate the fraction of time that has passed
            float t = elapsedTime / duration;

            // Interpolate the position between the original and target positions
            piece.transform.position = Vector3.Lerp(originalPosition, targetPosition, t);

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Yield until the next frame
            yield return null;
        }

        // Ensure the piece ends exactly at the target position
        piece.transform.position = targetPosition;
    }

    private void UpdateSelection()
    {
        if (cam == null)
        {
            cam = Camera.main; // Ensure the camera is assigned
        }

        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
            {
                // Debug.Log(hit.point);
                selectionX = (int)(hit.point.x + 0.5f);
                selectionY = (int)(hit.point.z + 0.5f);
            }
            else
            {
                selectionX = -1;
                selectionY = -1;
            }
        }
    }

    private void DrawChessBoard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heightLine = Vector3.forward * 8;
        Vector3 offset = new Vector3(0.5f, 0f, 0.5f);
        for (int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i - offset;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 8; j++)
            {
                start = Vector3.right * i - offset;
                Debug.DrawLine(start, start + heightLine);
            }
        }


        // Draw Selection
        if (selectionX >= 0 && selectionY >= 0 && selectionX <= 7 && selectionY <= 7)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX - offset,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1) - offset
                );
            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX - offset,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1) - offset
                );
        }
    }
    private void SpawnChessman(int index, Vector3 position)
    {
        GameObject ChessmanObject = Instantiate(ChessmanPrefabs[index], position, ChessmanPrefabs[index].transform.rotation) as GameObject;
        ChessmanObject.transform.SetParent(this.transform);
        ActiveChessmans.Add(ChessmanObject);

        int x = (int)(position.x);
        int y = (int)(position.z);
        Chessmans[x, y] = ChessmanObject.GetComponent<ChessmanAi>();
        Chessmans[x, y].SetPosition(x, y);

    }
    private void SpawnAllChessmans()
    {
        // Spawn White Pieces
        // Rook1
        SpawnChessman(0, new Vector3(0, 0, 7));
        // Knight1
        SpawnChessman(1, new Vector3(1, 0, 7));
        // Bishop1
        SpawnChessman(2, new Vector3(2, 0, 7));
        // King
        SpawnChessman(3, new Vector3(3, 0, 7));
        // Queen
        SpawnChessman(4, new Vector3(4, 0, 7));
        // Bishop2
        SpawnChessman(2, new Vector3(5, 0, 7));
        // Knight2
        SpawnChessman(1, new Vector3(6, 0, 7));
        // Rook2
        SpawnChessman(0, new Vector3(7, 0, 7));
        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, new Vector3(i, 0, 6));
        }

        // Spawn Black Pieces
        // Rook1
        SpawnChessman(6, new Vector3(0, 0, 0));
        // Knight1
        SpawnChessman(7, new Vector3(1, 0, 0));
        // Bishop1
        SpawnChessman(8, new Vector3(2, 0, 0));
        // King
        SpawnChessman(9, new Vector3(3, 0, 0));
        // Queen
        SpawnChessman(10, new Vector3(4, 0, 0));
        // Bishop2
        SpawnChessman(8, new Vector3(5, 0, 0));
        // Knight2
        SpawnChessman(7, new Vector3(6, 0, 0));
        // Rook2
        SpawnChessman(6, new Vector3(7, 0, 0));
        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, new Vector3(i, 0, 1));
        }

        WhiteKing = Chessmans[3, 7];
        BlackKing = Chessmans[3, 0];

        WhiteRook1 = Chessmans[0, 7];
        WhiteRook2 = Chessmans[7, 7];
        BlackRook1 = Chessmans[0, 0];
        BlackRook2 = Chessmans[7, 0];
    }

    public void EndGame()
    {
        if (!isWhiteTurn)
            Debug.Log("White team wins");
        else
            Debug.Log("Black team wins");

        foreach (GameObject go in ActiveChessmans)
            Destroy(go);

        // New Game
        isWhiteTurn = true;
        BoardHighlightsAi.Instance.DisableAllHighlights();
        SpawnAllChessmans();
    }
    private void isCheckmate()
    {
        bool hasAllowedMove = false;
        foreach (GameObject chessman in ActiveChessmans)
        {
            if (chessman.GetComponent<ChessmanAi>().isWhite != isWhiteTurn)
                continue;

            bool[,] allowedMoves = chessman.GetComponent<ChessmanAi>().PossibleMoves();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (allowedMoves[x, y])
                    {
                        hasAllowedMove = true;
                        break;
                    }
                }
                if (hasAllowedMove) break;
            }
        }

        if (!hasAllowedMove)
        {
            if (isWhiteTurn && !WhiteKing.InDanger() || !isWhiteTurn && !BlackKing.InDanger())
            {
                Debug.Log("Stalemate");
                // Handle stalemate logic here
            }
            else
            {
                BoardHighlightsAi.Instance.HighlightCheckmate(isWhiteTurn);
                Debug.Log("CheckMate");
                // Handle checkmate logic here
            }

            Time.timeScale = 1;
            Debug.Log("Average Response Time of computer (in seconds): " + (ChessAI.Instance.averageResponseTime / 1000.0));

            // Display Game Over Menu
            GameOver.Instance.GameOverMenu();

            // EndGame();
        }
    }

    // Call this when player's turn ends
    private void StartAITurn()
    {
        StartCoroutine(AITurnWithDelay());
    }

    // Coroutine to add delay before AI makes its move
    private IEnumerator AITurnWithDelay()
    {
        // Wait for 1.5 seconds before AI makes a move
        yield return new WaitForSeconds(1.5f);
        
        // Now let the AI make its move
        ChessAI.Instance.NPCMove();
        OfflineUicontroller.instance.PlayerTurn();
    }
}

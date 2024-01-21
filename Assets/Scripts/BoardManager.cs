using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;
    public Transform[] whiteLocations;
    public Transform[] blackLocations;
    public GameObject[]  gameObjects;
    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 0, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 180, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;

    public bool isWhiteTurn = true;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    private AudioSource audioSource;
    public AudioClip moveSound;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllChessmans();
        EnPassantMove = new int[2] { -1, -1 };
        audioSource = GetComponent<AudioSource>();
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
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                if (c.GetType() == typeof(King))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if (y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true, whiteLocations[0] , false);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false , whiteLocations[0], false);
                    selectedChessman = Chessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
            PlayMoveSound();
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
       // if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit, 50.0f, //LayerMask.GetMask("ChessPieces")))
      //  {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPieces")))
            {
                int i = 0;
            foreach(GameObject gameObject in gameObjects)
            {
                if(hit.collider.gameObject.GetInstanceID() == gameObject.GetInstanceID() )
                {
                    i = Array.IndexOf(gameObjects, gameObject);
                }
            }
            selectionX = i%8;
            selectionX = (int)i/8 ;

            //selectionX = (int)hit.point.x;
            Debug.Log(selectionX);
            //selectionY = (int)hit.point.z;
            Debug.Log(selectionY);
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite , Transform location , bool spwanByGameObject)
    {
        Vector3 position;
        if (spwanByGameObject)
        {
            position = location.position;
        }
        else
        {
            position = GetTileCenter(x, y);
        }
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], location.position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
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
        Chessmans = new Chessman[8, 8];

        /////// White ///////

        // King
        SpawnChessman(0, 3, 0, true , whiteLocations[0] ,true);

        // Queen
        SpawnChessman(1, 4, 0, true , whiteLocations[1], true);

        // Rooks
        SpawnChessman(2, 0, 0, true , whiteLocations[2], true);
        SpawnChessman(2, 7, 0, true , whiteLocations[3], true);

        // Bishops
        SpawnChessman(3, 2, 0, true, whiteLocations[4], true);
        SpawnChessman(3, 5, 0, true, whiteLocations[5], true);

        // Knights
        SpawnChessman(4, 1, 0, true, whiteLocations[6], true);
        SpawnChessman(4, 6, 0, true, whiteLocations[7], true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true , whiteLocations[i+8], true);
        }


        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false , blackLocations[0], true);

        // Queen
        SpawnChessman(7, 3, 7, false, blackLocations[1], true);

        // Rooks
        SpawnChessman(8, 0, 7, false , blackLocations[2], true);
        SpawnChessman(8, 7, 7, false, blackLocations[3], true);

        // Bishops
        SpawnChessman(9, 2, 7, false, blackLocations[4], true);
        SpawnChessman(9, 5, 7, false, blackLocations[5], true);

        // Knights
        SpawnChessman(10, 1, 7, false, blackLocations[6], true);
        SpawnChessman(10, 6, 7, false, blackLocations[7], true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false, blackLocations[i+8], true);
        }
    }

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("White wins");
        else
            Debug.Log("Black wins");

        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans();
    }


    private void PlayMoveSound()
    {
        // Check if an AudioClip is assigned
        if (moveSound != null)
        {
            // Check if the AudioSource is not null
            if (audioSource != null)
            {
                // Play the assigned move sound
                audioSource.PlayOneShot(moveSound);
            }
            else
            {
                Debug.LogError("AudioSource component not found!");
            }
        }
        else
        {
            Debug.LogError("Move sound not assigned!");
        }
    }
}


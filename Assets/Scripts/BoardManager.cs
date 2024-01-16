using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking;


public class BoardManager : NetworkBehaviour
{
    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }
    private bool isSpawn = false;
    public const float TILE_SIZE = 1.0f;
    public const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;
    public bool isWhiteTurn = true;
    //public NetworkVariable<bool> isWhiteTurn = new NetworkVariable<bool>(true , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<CustomMovement> whiteMovement = new NetworkVariable<CustomMovement>(new CustomMovement {
        _x = 0,
        _y = 0
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<CustomMovement> whiteMovementFinal = new NetworkVariable<CustomMovement>(new CustomMovement
    {
        _x = 0,
        _y = 0
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<CustomMovement> blackMovement = new NetworkVariable<CustomMovement>(new CustomMovement
    {
        _x = 0,
        _y = 0
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<CustomMovement> blackMovementFinal = new NetworkVariable<CustomMovement>(new CustomMovement
    {
        _x = 0,
        _y = 0
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }


    public struct CustomMovement :INetworkSerializable
    {
        public int _x;
        public int _y;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _x);
            serializer.SerializeValue(ref _y);

        }

    }

    // Use this for initialization
    void Start()
    {
        Instance = this;

       // activeChessman = new List<GameObject>();
        //Chessmans = new Chessman[8, 8];
        EnPassantMove = new int[2] { -1, -1 };
    }

    // Update is called once per frame
    public override void OnNetworkSpawn()
    {

       
        SpawnAllChessmans();
        if(!IsHost)
        {  
            whiteMovement.OnValueChanged += whiteMovementChanged;
            whiteMovementFinal.OnValueChanged += whiteMovementFinalChanged;
        }
        else
        {
            blackMovement.OnValueChanged += blackMovementChanged;
            blackMovementFinal.OnValueChanged += blackMovementFinalChanged;
        }

    }

    public void whiteMovementChanged(CustomMovement prev, CustomMovement current) 
    {
        Debug.Log("Initial Changed");
        SelectChessman(current._x, current._y);
    }
    public void whiteMovementFinalChanged(CustomMovement prev, CustomMovement current)
    {
        Debug.Log("Final Changed");
        MoveChessman(current._x, current._y);
    }
    public void blackMovementChanged(CustomMovement prev, CustomMovement current)
    {
        Debug.Log("Initial Changed");
        SelectChessman(current._x, current._y);
    }
    public void blackMovementFinalChanged(CustomMovement prev, CustomMovement current)
    {
        Debug.Log("Final Changed");
        MoveChessman(current._x, current._y);
    }

    public NetworkObject FindGameObjectsInSpheres(Vector3 center , float sphereRadius , LayerMask excludedLayer)
    {

        GameObject hitObject;
            Collider[] colliders = Physics.OverlapSphere(center, sphereRadius , ~excludedLayer);

            foreach (Collider collider in colliders)
            {
            NetworkObject netObject = collider.GetComponent<NetworkObject>();

            // Do something with the found object
            if (netObject != null)
                return netObject;

        }
        return null;
    }
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
                    Debug.Log("Selection X " + selectionX);
                    if(IsHost && isWhiteTurn)
                    { 
                    SelectChessman(selectionX, selectionY);
                    }
                    if(!IsHost && !isWhiteTurn)
                    {
                        SelectChessman(selectionX, selectionY);
                    }
                    //Debug.Log(Chessmans[selectionX, selectionY].isWhite);
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
        //Chessmans = new Chessman[8, 8];
        if (Chessmans == null || Chessmans[x, y] == null)
        {

            Debug.Log("Chessman or Chessmans array is null");
            return;
        }

        if (Chessmans[x, y].isWhite != isWhiteTurn) return;

        if (isWhiteTurn)
        {
            Debug.Log("Value Set");
            whiteMovement.Value = new CustomMovement
            {
                _x = x,
                _y = y
            };
        }
        else
        {
            
            CustomMovement blackMovement_temp = new CustomMovement
            {
                _x = x,
                _y = y
            };
            blackTurnServerRPC(blackMovement_temp);

            
        }
        
        
    
        
        if(IsClient)
        {
            Debug.Log("X movement" + whiteMovement.Value._x);
        }
        Debug.Log(Chessmans[x,y]);
        bool hasAtLeastOneMove = false;
        Debug.Log("Entered to slect");
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

            if (isWhiteTurn)
            {
                Debug.Log("Value Set");
                whiteMovementFinal.Value = new CustomMovement
                {
                    _x = x,
                    _y = y
                };
            }
            else
            {
                
                CustomMovement blackMovementFinal_temp = new CustomMovement
                {
                    _x = x,
                    _y = y
                };
                blackTurnFinalServerRPC(blackMovementFinal_temp);
            }
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
                if(y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false);
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
            // ChangeTurnServerRPC();
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
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
            
            //go = FindGameObjectsInSpheres(position, 0.9f, mask);
            // Debug.Log(go);
            Chessmans[x, y] = go.GetComponent<Chessman>();
            Chessmans[x, y].SetPosition(x, y);
            activeChessman.Add(go);
            go.transform.SetParent(transform);

           

       

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


    [ServerRpc(RequireOwnership = false)]
    public void blackTurnServerRPC(CustomMovement blackData)
    {
        blackMovement.Value = new CustomMovement
        {
            _x = blackData._x,
            _y = blackData._y
        };
    
    }

    [ServerRpc(RequireOwnership = false)]
    public void blackTurnFinalServerRPC(CustomMovement blackData)
    {
        blackMovementFinal.Value = new CustomMovement
        {
            _x = blackData._x,
            _y = blackData._y
        };


    }


    //public void 



    [ServerRpc(RequireOwnership = false)]
    public void ChangeTurnServerRPC()
    {
        Debug.Log("Turn Changed");
        isWhiteTurn = !isWhiteTurn;
    }
}



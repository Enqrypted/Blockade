using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    private int VerticalsLeft = 9;
    private int HorizontalsLeft = 9;

    private int boardWidth = 11;
    private int boardHeight = 14;
    private float lastTap;

    private bool isMyTurn = false;
    private bool choosingWall = false;
    private Pawn selectedPawn;

    [SerializeField] private GameObject WinPanel, WinText;

    private Tile[,] tileMap = new Tile[11, 14];
    private List<Pawn> myPawns = new List<Pawn>();
    private List<Pawn> allPawns = new List<Pawn>();

    [SerializeField] private TextMeshProUGUI horizLeftText, vertLeftText;

    [SerializeField] private Camera p1cam, p2cam;

    private PhotonView photonView;

    [SerializeField] private Material p1Material, p2Material;

    [SerializeField] private GameObject cheatsPanel;

    Pawn SpawnPawn(Vector3 pos, Material mat)
    {
        GameObject pawn = Instantiate(Resources.Load("Pawn"), Vector3.zero, Quaternion.identity) as GameObject;
        pawn.GetComponent<Pawn>().targetVector = pos;
        pawn.GetComponent<MeshRenderer>().material = mat;
        return pawn.GetComponent<Pawn>();
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(.4f);
        photonView.RPC("SwitchTurns", RpcTarget.All, true);
    }

    IEnumerator HoldForCheats()
    {
        float t = Time.time;
        lastTap = t;

        yield return new WaitForSeconds(1f);

        if ((t == lastTap) && Input.GetMouseButton(0))
        {
            cheatsPanel.SetActive(true);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        //Instanciate all tiles in board for the client
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                GameObject tile = Instantiate(Resources.Load("Tile"), Vector3.zero, Quaternion.identity,
                    GameObject.Find("Board").transform) as GameObject;

                tile.transform.localPosition = new Vector3(x + 1, 0, y + 1);

                Tile tileManager = tile.GetComponent<Tile>();
                tileManager.x = x;
                tileManager.y = y;

                tileMap[x, y] = tileManager;

                if (y + 1 == 4)
                {
                    //p1 spawns
                    if (x + 1 == 4 || x + 1 == 8)
                    {
                        tileManager.P1.SetActive(true);
                        Pawn pawn = SpawnPawn(tile.transform.position, p1Material);

                        if (PhotonNetwork.IsMasterClient)
                        {
                            myPawns.Add(pawn);
                        }

                        allPawns.Add(pawn);
                        pawn.id = allPawns.Count - 1;

                    }

                }

                if (y + 1 == 11)
                {
                    //p2 spawns
                    if (x + 1 == 4 || x + 1 == 8)
                    {
                        tileManager.P2.SetActive(true);
                        Pawn pawn = SpawnPawn(tile.transform.position, p2Material);

                        if (!PhotonNetwork.IsMasterClient)
                        {
                            myPawns.Add(pawn);
                        }

                        allPawns.Add(pawn);
                        pawn.id = allPawns.Count - 1;

                    }

                }

            }
        }

        photonView = PhotonView.Get(this);

        if (PhotonNetwork.IsMasterClient)
        {
            //Start off the game
            p1cam.enabled = true;
            StartCoroutine(DelayStart());
        }
        else
        {
            p2cam.enabled = true;
        }


    }

    void SetAnnouncement(string announcement)
    {
        GameObject.Find("Canvas").transform.Find("Panel").Find("AnnouncementText").GetComponent<TextMeshProUGUI>()
            .text = announcement;
    }

    [PunRPC]
    void SwitchTurns(bool isHostTurn)
    {
        if (isHostTurn)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                isMyTurn = true;

                SetAnnouncement("Your turn");
            }
            else
            {
                SetAnnouncement(PhotonNetwork.PlayerList[1].NickName + "'s turn");
                isMyTurn = false;
            }
        }
        else if (!isHostTurn)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                isMyTurn = true;
                SetAnnouncement("Your turn");
            }
            else
            {
                SetAnnouncement(PhotonNetwork.PlayerList[0].NickName + "'s turn");
                isMyTurn = false;
            }
        }
    }

    void ShowButtons()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                tileMap[x, y].ShowAvailableButtons();
            }
        }
    }

    void HideButtons()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                tileMap[x, y].HideButtons();
            }
        }

        choosingWall = false;
        photonView.RPC("SwitchTurns", RpcTarget.All, !PhotonNetwork.IsMasterClient);
    }

    [PunRPC]
    void AddWall(int x, int y, bool isHorizontal)
    {
        if (isHorizontal)
        {
            tileMap[x, y].horizWall.SetActive(true);
        }
        else
        {
            tileMap[x, y].vertWall.SetActive(true);
        }
    }

    [PunRPC]
    void MovePawn(int id, Vector3 targPos)
    {
        allPawns[id].targetVector = targPos;

        if (PhotonNetwork.IsMasterClient)
        {
            CheckWin(id, targPos);
        }

    }

    [PunRPC]
    void AnnounceWin(string msg)
    {
        WinPanel.SetActive(true);
        WinText.GetComponent<TextMeshProUGUI>().text = msg;
    }

    void CheckWin(int id, Vector3 targPos)
    {
        Ray ray = new Ray();
        ray.origin = targPos + new Vector3(0, 2, 0);
        ray.direction = new Vector3(0, -10f, 0);
        RaycastHit foundTile;
        if (Physics.Raycast(ray, out foundTile, 10f, 1 << 8))
        {
            Pawn pawn = allPawns[id];
            if (foundTile.transform.Find("P1").gameObject.activeSelf && !myPawns.Contains(pawn))
            {
                //p2 wins
                photonView.RPC("AnnounceWin", RpcTarget.All, PhotonNetwork.PlayerListOthers[0].NickName + " Wins!");
            }

            if (foundTile.transform.Find("P2").gameObject.activeSelf && myPawns.Contains(pawn))
            {
                //p1 wins
                photonView.RPC("AnnounceWin", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName + " Wins!");
            }
        }
    }

    public void BackToMenu()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LobbyScene");
    }

    // Update is called once per frame
    void Update()
    {
        horizLeftText.text = HorizontalsLeft.ToString();
        vertLeftText.text = VerticalsLeft.ToString();

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(HoldForCheats());
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 100f))
            {
                if (isMyTurn)
                {
                    GameObject obj = hit.collider.gameObject;

                    Tile tile = obj.GetComponent<Tile>();
                    Pawn pawn = obj.GetComponent<Pawn>();

                    if (!choosingWall)
                    {
                        if (pawn && myPawns.Contains(pawn))
                        {
                            //Select pawn
                            pawn.isSelected = !pawn.isSelected;
                            pawn.TogglePoints(pawn.isSelected);

                            if (pawn.isSelected)
                            {
                                selectedPawn = pawn;
                            }

                            //Deselect other pawns
                            foreach (Pawn otherPawn in myPawns)
                            {
                                if (otherPawn != pawn)
                                {
                                    otherPawn.isSelected = false;
                                    otherPawn.TogglePoints(false);
                                }
                            }
                        
                        }

                        if (obj.name == "PossibleMovement")
                        {
                            
                            photonView.RPC("MovePawn", RpcTarget.All, selectedPawn.GetComponent<Pawn>().id,obj.transform.position);
                            selectedPawn.GetComponent<Pawn>().TogglePoints(false);
                            selectedPawn.isSelected = false;

                            if (VerticalsLeft > 0 || HorizontalsLeft > 0)
                            {
                                choosingWall = true;
                                ShowButtons();
                            }
                            else
                            {
                                photonView.RPC("SwitchTurns", RpcTarget.All, !PhotonNetwork.IsMasterClient);
                            }

                        }
                    }
                    else
                    {
                        
                        if (obj.name == "HorizontalButton")
                        {
                            HorizontalsLeft--;
                            tile = obj.transform.parent.GetComponent<Tile>();
                            HideButtons();
                            photonView.RPC("AddWall", RpcTarget.All, tile.x, tile.y, true);
                        }

                        if (obj.name == "VerticalButton")
                        {
                            VerticalsLeft--;
                            tile = obj.transform.parent.GetComponent<Tile>();
                            HideButtons();
                            photonView.RPC("AddWall", RpcTarget.All, tile.x, tile.y, false);
                        }
                    }
                    
                    
                    
                }
            }
        }
    }
}

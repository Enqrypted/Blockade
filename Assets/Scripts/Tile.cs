using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public GameObject vertBtn;
    public GameObject horizBtn;

    public GameObject vertWall;
    public GameObject horizWall;

    public GameObject P1;
    public GameObject P2;

    public int x, y;
    
    // Start is called before the first frame update
    void Start()
    {
        //deactivate at first
        vertBtn.SetActive(false);
        vertWall.SetActive(false);
        horizBtn.SetActive(false);
        horizWall.SetActive(false);
    }

    public void ShowAvailableButtons()
    {
        vertBtn.SetActive(!vertWall.activeSelf);
        horizBtn.SetActive(!horizWall.activeSelf);
    }

    public void HideButtons()
    {
        vertBtn.SetActive(false);
        horizBtn.SetActive(false);
    }
}

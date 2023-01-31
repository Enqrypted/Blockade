using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : MonoBehaviour
{

    public Vector3 targetVector = Vector3.zero;
    public bool isSelected = false;

    public List<GameObject> points = new List<GameObject>();
    public int id;

    private void Start()
    {
        foreach (Transform point in transform)
        {
            point.gameObject.SetActive(false);
            points.Add(point.gameObject);
        }
    }

    public void TogglePoints(bool toggle)
    {
        foreach (GameObject point in points)
        {
            Ray ray = new Ray();
            ray.origin = transform.position;
            ray.direction = point.transform.position - transform.position;
            Debug.DrawLine(ray.origin, ray.origin+ray.direction*10f, Color.cyan, 1f);
            RaycastHit hit;
            bool intercepted = false;
            if (Physics.Raycast(ray, out hit, 10f,  1 << 7))
            {
                intercepted = true;
            }
            point.SetActive(toggle && (!intercepted));
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (isSelected)
        {
            targetVector.y = 1.5f;
        }
        else
        {
            targetVector.y = 1f;
        }
        
        transform.position = Vector3.Lerp(transform.position, targetVector, 5f*Time.deltaTime);
    }
}

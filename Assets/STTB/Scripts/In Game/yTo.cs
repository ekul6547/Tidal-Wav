using UnityEngine;
using System.Collections;

public class yTo : MonoBehaviour
{

    public float rate = 0.01f;
    public float To;

    public GameObject control;

    // Use this for initialization
    void Start()
    {
        To = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        var pos = transform.position;
        if (pos.y != To)
        {
            float dif = To - pos.y;
            if (pos.y > To)
            {
                pos.y -= rate * (Time.deltaTime * 30);
            }
            else
            {
                pos.y += rate * (Time.deltaTime * 30);
            }
            pos.y += (dif / 100);
        }
        transform.position = pos;
    }
}

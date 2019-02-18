using UnityEngine;
using System.Collections;
using System;

public class MoveRight : MonoBehaviour {
    
    public bool Left = false;
    public float AudioTime;
    public float TimeCreate;

    void OnEnable() {
        TimeCreate = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Move(GameManager.active.Mode);
    }
    void Move(string mode)
    {
        GameObject contobj = GameObject.Find("Main Controller");
        if (contobj != null)
        {
            controller cont = contobj.GetComponent<controller>();
            float speed = cont.speedMulti * (Time.deltaTime);
            float move = transform.position.x;
            if (Left == true && speed > 0)
            {
                speed *= -1;
            }
            switch (mode)
            {
                case "spiral":
                    float mod = Left == true ? -60 : 0;
                    float multi = (Time.time - TimeCreate) * 3;
                    float x = Mathf.Sin(((Time.time - TimeCreate) * (cont.speedMulti / 6)) + mod) * multi;
                    float y = Mathf.Cos(((Time.time - TimeCreate) * (cont.speedMulti / 6)) + mod) * multi;
                    Vector3 pos = new Vector3(x - (Left ? -0.5f : 0.5f), y, transform.position.z);
                    transform.position = pos;
                    transform.rotation = Quaternion.FromToRotation(pos, Vector3.zero);
                    break;
                default:
                    move += speed;
                    float newY = transform.position.y;
                    if (!GameManager.active.IsInGame)
                    {
                        newY = GetMenuY(Left, move, AudioTime);
                    }
                    transform.position = new Vector3(move, newY, transform.position.z);
                    break;
            }
        }
    }

    public static float GetMenuY(bool isLeft, float newX, float time)
    {
        /*
        Return Y value based off of audio data. Doesn't really work :/
        Often too high up on songs - can't average to be lower because that defeats the point. Just leave it at 
        int o = 1;
        return GameManager.active.audioData.Get(isLeft ? 0 : 1, o, audioTime)/(o < 2 ? 250 : 0.125f);
        */
        if (isLeft)
            return Mathf.Sin(newX + time) * GameManager.active.MenuYChange;
        else
            return -Mathf.Sin(newX - time) * GameManager.active.MenuYChange;
    }
}

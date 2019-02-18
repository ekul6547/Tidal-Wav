using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour {

    public Camera Cam;
    SpriteRenderer sprite;
    Color ColTo;

    void Awake()
    {
        Cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        ColTo = GameManager.active.blockColours[Random.Range(0, GameManager.active.blockColours.Count)];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
            Pickup();
    }

    public void Pickup()
    {
        GameManager.active.OnPickupCollectable.Invoke();
        Destroy();
    }

    void Update()
    {
        if (transform.position.x < Cam.ViewportToWorldPoint(new Vector3(Cam.rect.xMin - 0.1f, 0, 0)).x)
        {
            Destroy();
        }
        sprite.color = Color.Lerp(sprite.color, ColTo, Time.deltaTime * 1);
    }

    public void Destroy()
    {
        DestroyObject(gameObject);
    }
}

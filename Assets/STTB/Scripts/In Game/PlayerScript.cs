using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour {

    GameObject skin;
    public Animator anim;

    public float gravity = 1;
    float preGrav;
    public bool Collide;
    public Material matB;
    public Camera cam;
    public Vector3[] edges = new Vector3[2];

	// Use this for initialization
	void Start () {
        skin = Instantiate(GameManager.active.playerSkinPrefab, gameObject.transform);
        anim = skin.GetComponent<Animator>();
        edges[0] = cam.ViewportToWorldPoint(new Vector3(0, cam.rect.yMax, 0));
        edges[1] = cam.ViewportToWorldPoint(new Vector3(0, cam.rect.yMin, 0));
        preGrav = gravity;
        gravity = 0;
        Invoke("SetAble", 0.75f);
    }
    void SetAble()
    {
        gravity = preGrav;
    }
	
	// Update is called once per frame
	void Update ()
    {
        edges[0] = cam.ViewportToWorldPoint(new Vector3(0, cam.rect.yMax, 0));
        edges[1] = cam.ViewportToWorldPoint(new Vector3(0, cam.rect.yMin, 0));
        bool freeze = false;
        if (Input.GetMouseButton(0))
        {
            if (gameObject.transform.position.y < edges[0].y)
            {
                gameObject.transform.position = new Vector3(cam.transform.position.x, gameObject.transform.position.y + (gravity*(Time.deltaTime*30)), gameObject.transform.position.z);
            }
        }else
        {
            if (gameObject.transform.position.y > edges[1].y)
            {
                gameObject.transform.position = new Vector3(cam.transform.position.x, gameObject.transform.position.y - (gravity * (Time.deltaTime * 30)), gameObject.transform.position.z);
                freeze = true;
            }
            else
            {
                gameObject.transform.position = new Vector3(cam.transform.position.x, gameObject.transform.position.y + (gravity * (Time.deltaTime * 30)), gameObject.transform.position.z);
            }
        }
        if(gameObject.transform.position.y - 1 < edges[1].y)
        {
            freeze = false;
        }
        if (freeze)
        {
            anim.speed = 1f;
        }
        else
        {
            anim.speed = 2f;
        }
	}

    void OnCollisionEnter2D(Collision2D other)
    {
		if (other.gameObject.name == "Platform")
        {
            other.gameObject.GetComponent<RedCollider>().red();
            if (Collide) { SceneManager.LoadScene("InGame"); }
        }
        if (other.gameObject.name == "Collectable")
        {
            other.gameObject.transform.parent.GetComponent<Collectable>().Pickup();
        }
    }
}

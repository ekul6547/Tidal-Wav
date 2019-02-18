using UnityEngine;
using System.Collections;

public class DestroyAfter : MonoBehaviour {

    public float frames = 10f;
	public float framesStart = 0;
	private float yAdd;
    bool toLock = true;
    public RedCollider Red;
    public Camera Cam;
    GameObject contObj;
    Color col;
    bool doCol = false;
    bool hasTexture = false;

	// Use this for initialization
	void Awake() {
		framesStart = frames;
        Cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
    public void SetNextColour(Color newCol)
    {
        doCol = true;
        col = newCol;
        SetCol(new Color(0, 0, 0, 0));
    }
    void SetCol(Color newCol)
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().material.color = newCol;
    }

    public void SetTexture(Texture2D tex)
    {
        hasTexture = true;
        transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = tex;
    }

    public void CancelTexture()
    {
        hasTexture = false;
        transform.GetChild(0).GetComponent<Renderer>().material.mainTexture = null;
    }

	// Update is called once per frame
	void Update ()
    {
        if (Cam == null)
            Awake();
        if (doCol)
        {
            doCol = false;
            SetCol(col);
        }
        
        if (contObj != null)
        {
            controller cont = contObj.GetComponent<controller>();
            //var scale = gameObject.transform.localScale;
            //scale.x = cont.speedMulti/30 + 0.05f;
            //gameObject.transform.localScale = scale;
            if(GameManager.active.audio_Music.isPlaying)
                yAdd = cont.dis / 2;
            //If off screen
            if (transform.position.x < Cam.ViewportToWorldPoint(new Vector3(Cam.rect.xMin-0.1f, 0, 0)).x || (transform.position.x > Cam.ViewportToWorldPoint(new Vector3(Cam.rect.xMax, 0, 0)).x && !GameManager.active.IsInGame))
            {
                frames = framesStart;
                Red.deRed();
                toLock = true;
                if (transform.localScale.y < 0)
                {
                    Vector3 Sc = transform.localScale;
                    Sc.y = -Sc.y;
                    transform.localScale = Sc;
                }
                CancelTexture();
                gameObject.SetActive(false);
            }
            if (framesStart - 1 >= frames && transform.position.z == 1 && toLock)
            {
                yTo();
            }
            frames -= 1 * (Time.deltaTime * 30); 
        }
        else
        {
            contObj = GameObject.Find("Main Controller");
        }
	}

    void LateUpdate()
    {
        transform.GetChild(0).GetComponent<Renderer>().material.mainTextureOffset = new Vector2(Time.time, 0);
    }

    void yTo()
    {
        toLock = false;
        float posTo = gameObject.transform.position.y + (yAdd);
        GameObject.Find("Main Camera").GetComponent<yTo>().To = posTo;
    }


}

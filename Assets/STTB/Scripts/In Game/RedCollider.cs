using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RedCollider : MonoBehaviour {
    
    public Material matA;
    public Material matADark;
    public Material matB;
    
    public void red()
    {
        if (gameObject.GetComponent<MeshRenderer>().sharedMaterial != matADark && gameObject.GetComponent<MeshRenderer>().sharedMaterial != matB)
        {
            Texture temp = GetComponent<Renderer>().material.mainTexture;
            gameObject.GetComponent<MeshRenderer>().material = matB;
            GetComponent<MeshRenderer>().material.mainTexture = temp;
            GameObject.Find("Main Controller").GetComponent<controller>().addRed();
        }
    }
    public void gray()
    {
        if (gameObject.GetComponent<MeshRenderer>().sharedMaterial != matADark)
        {
            gameObject.GetComponent<MeshRenderer>().material = matADark;
        }
    }
    public void deRed()
    {
        gameObject.GetComponent<MeshRenderer>().material = matA;
    }
}

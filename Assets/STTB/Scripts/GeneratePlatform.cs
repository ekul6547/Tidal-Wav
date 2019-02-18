using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;

public class GeneratePlatform : MonoBehaviour {

    #region Variables

    [Header("Main")]
    public controller control; //controller class
    Camera cam; //Main Camera
    public GameObject platform; //Prefab for platform
    public Transform platformParent; //Object to store all platforms under
    AudioSource AudioObject
    {
        get
        {
            return GameManager.active.audio_Music;
        }
    }//The audio component
    public RetroBars retroBars;

    [Header("Control")]
    public bool Menu = false; //Is in the menu
    public bool drawGraphs = true;
    public bool HillClimb = false; //Should the objects be affected by hillclimb
    [Range(0.0f, 3.0f)]
    public float Exaggerate = 0.0f; //Exaggeration of the object's y values

    [Header("Variables")]
    [Range(0, 1)]
    public float min = 0.05f; //Minimum scale for the Menu
    public float dis; //Distance between objects
    private float xDis; //Xdistance to set the platforms too
    private float preY = 0; //Previous y Value
    private float max = 3f; //Maximum distance between previous Y and new Y
    private int count = 0; //Count to activate every 8 objects
    public int speedForArray = 0; // Use this for initialization

    //private GameObject[] platforms = new GameObject[1000];
    [Space]
    public Material[] LeftRight = new Material[2]; //Materials for the objects in menu

    public Texture2D ScrollingTexture;

    #endregion

    void Awake()
    {
        LoadOptions(); //Get Custom Options
        speedForArray = 50;
        platformParent = GameObject.Find("Platforms").transform;
        if (platformParent.childCount < 10)
        {
            DontDestroyOnLoad(platformParent);
            AddToArray(speedForArray);
        }
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            Menu = true;
        }
        ResetPlatforms();
    }

    void Start()
    {
        volumeB = volume;
        samples = new float[qSamples];
        spectrum = new float[qSamples];
        /*amount = (int) (audioClipGen.length / frequency);
        dis = audioClipGen.length / amount;
        for(float i = 0; i < amount; i++)
        {
            int y = AudioSource.timeSamples
            Instantiate(platform, new Vector3(i * dis, 1, 0),Quaternion.identity);
        }*/
        GameManager.active.gen = this;
        GameManager.active.audioData.clear();
        ScrollingTexture = GameManager.active.platformSkinTexture;
    }

    void Update()
    {
        LoadOptions();
        SetBlockColour();
        if (GameManager.active.Mode == "bars")
        {
            retroBars.gameObject.SetActive(true);
            AudioObject.panStereo = ((Input.mousePosition.x / Screen.width) * 2) - 1; //Leftright volumes
            if (GameManager.active.MenuDebug)
                AudioObject.panStereo = 0;
            for (int channel = 0; channel < 2; channel++) //For each channel
            {
                GetVolume(channel);
                GenBars((Mathf.Pow(pitchValue[channel], modiA) / modiB), channel);
            }
        }
        else
        {
            if(retroBars != null)
                GenBars(0, 0, true);
            for (int channel = 0; channel < 2; channel++) //For each channel
            {
                GetVolume(channel);
                if (Menu)
                {
                    AudioObject.panStereo = ((Input.mousePosition.x / Screen.width) * 2) - 1; //Leftright volumes
                    if (GameManager.active.MenuDebug)
                        AudioObject.panStereo = 0;
                    GenMenu((Mathf.Pow(pitchValue[channel], modiA) / modiB), channel);
                    continue;
                }
                AudioObject.panStereo = 0;
                xDis = cam.ViewportToWorldPoint(new Vector3(cam.rect.xMax, 0, 0)).x + 2f; //new right edge of camera in world space
                Gen((Mathf.Pow(pitchValue[channel], modiA) / modiB), channel);
            }
        }

        if (Time.time - countTime > timeDif && !canSpawnCollectable)
        {
            canSpawnCollectable = true;
        }

        SetGraphs();
    }

    #region Block Functions

    public Material defaultMat;
    //Reset platforms scales, positions and active state when changing scene
    public void ResetPlatforms()
    {
        for (int i = 0; i < platformParent.childCount; i++)
        {
            GameObject ab = platformParent.GetChild(i).gameObject;
            ab.SetActive(true);
            ab.transform.localScale = platform.transform.localScale;
            ab.GetComponent<MoveRight>().Left = true;
            ab.GetComponent<DestroyAfter>().frames = 30f / dis;
            ab.GetComponent<DestroyAfter>().framesStart = ab.GetComponent<DestroyAfter>().frames + 1f;
            ab.transform.GetChild(0).GetComponent<MeshRenderer>().material = defaultMat;
            ab.GetComponent<DestroyAfter>().SetNextColour(defaultMat.color);
            ab.SetActive(false);
        }
    }

    /// <summary>
    /// Create new objects for the list of objects
    /// </summary>
    /// <param name="amount">The amount to create</param>
    void AddToArray(int amount)
    {
        Debug.Log("Adding " + amount + " new blocks");
        for (int i = 0; i < amount; i++)
        {
            GameObject ab = (GameObject)Instantiate(platform, Vector3.zero, Quaternion.identity);
            ab.GetComponent<DestroyAfter>().frames = 30f / dis;
            ab.GetComponent<DestroyAfter>().framesStart = ab.GetComponent<DestroyAfter>().frames + 1f;
            ab.transform.GetChild(0).GetComponent<MeshRenderer>().material = defaultMat;
            ab.transform.SetParent(platformParent);
            ab.SetActive(false);
        }
    }

    /// <summary>
    /// Get the first object that is not active
    /// </summary>
    /// <returns>The returned object</returns>
    GameObject GetFirstAble()
    {
        for (int i = 0; i < platformParent.childCount; i++)
            if (platformParent.GetChild(i).gameObject.activeSelf == false)
                return platformParent.GetChild(i).gameObject;
        AddToArray(50);
        return GetFirstAble();
    }
    #endregion

    #region Get Volume
    [Space]
    [Header("Get Volume")]
    public float volume = 2; // set how much the scale will vary
    private int qSamples = 1024;  // array size
    private float refValue = 0.1F; // RMS value for 0 dB
    private float[] rmsValue = new float[2];   // sound level - RMS
    private float[] dbValue = new float[2];    // sound level - dB
    private float volumeB;
    public float[] pitchValue = new float[2];
    private float[] samples; // audio samples
    private float[] spectrum;
    public bool Different = false;
    /// <summary>
    /// Generate volume and pitch values
    /// </summary>
    /// <param name="channel">Left or Right speaker</param>
    void GetVolume(int channel)
    {
        volume = volumeB * control.GetComponent<controller>().scale; //volume = base volume * controlled volume
        AudioObject.GetOutputData(samples, channel); // fill array with samples
        if (Different)
            AudioObject.GetSpatializerFloat(channel, out volume); //Slightly different value for volume - not much different
        int i;
        float sum = 0;
        for (i = 0; i < qSamples; i++)
        {
            sum += samples[i] * samples[i]; // sum squared samples
        }
        rmsValue[channel] = Mathf.Sqrt(sum / qSamples); // rms = square root of average
        dbValue[channel] = 20 * Mathf.Log10(rmsValue[channel] / refValue); // calculate dB
        if (dbValue[channel] < -10) dbValue[channel] = -10; // clamp it to -160dB min
                                          //if (dbValue > -10) dbValue = -10;
        if (rmsValue[channel] > 0.4f) rmsValue[channel] = 0.4f;


        AudioObject.GetSpectrumData(spectrum, channel, FFTWindow.Rectangular); //fill array with samples
        float maxV = 0;
        int maxN = 0;
        for (i = 0; i < qSamples; i++)
        { // find max 
            if (spectrum[i] > maxV && spectrum[i] > GameManager.active.PitchThreshold) //if above the threshhold and above the current maximum - used this way to get index, not just max
            {
                maxV = spectrum[i];
                maxN = i; // maxN is the index of max
            }
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < qSamples - 1)
        { // interpolate index using neighbours
            float dL = spectrum[maxN - 1] / spectrum[maxN];
            float dR = spectrum[maxN + 1] / spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        pitchValue[channel] = freqN * (AudioSettings.outputSampleRate / 2) / qSamples; // convert index to frequency
    }

    #endregion

    #region Graphs
    [Space]
    private float[] averagePitch = new float[4]; //SumLeft - CountLeft - SumRight - CountRight  --  Do Sum/Count to get average
    private float[] averageRMS = new float[4];
    private void SetGraphs()
    {
        if (!drawGraphs)
            return;
        float t = AudioObject.time;
        if (GameManager.active.audioData.hasGreaterTime(0, 0, t) || GameManager.active.audioData.left.Length == 0)
        {
            return;
        }


        averagePitch[0] += pitchValue[0];
        averagePitch[1] += 1;
        averagePitch[2] += pitchValue[1];
        averagePitch[3] += 1;
        averageRMS[0] += yVis[0];
        averageRMS[1] += 1;
        averageRMS[2] += yVis[1];
        averageRMS[3] += 1;
        float[] average = new float[2];
        float[] recentRight;
        float[] recentLeft;
        //Pitch
        GameManager.active.audioData.Set(0, 0, new Keyframe(t, pitchValue[0]));
        GameManager.active.audioData.Set(1, 0, new Keyframe(t, pitchValue[1]));


        //Average Pitch
        recentLeft = GameManager.active.audioData.GetFromTime(0, 0, GameManager.active.audioData.GetKeyAtTime(0,0,t),16,true);
        recentRight = GameManager.active.audioData.GetFromTime(1, 0, GameManager.active.audioData.GetKeyAtTime(1, 0, t), 16, true);
        average[0] = AverageFloats(recentLeft);
        average[1] = AverageFloats(recentRight);
        GameManager.active.audioData.Set(0, 1, new Keyframe(t, average[0]));
        GameManager.active.audioData.Set(1, 1, new Keyframe(t, average[1]));


        //RMS
        GameManager.active.audioData.Set(0, 2, new Keyframe(t, yVis[0]));
        GameManager.active.audioData.Set(1, 2, new Keyframe(t, yVis[1]));

        //Average RMS
        recentLeft = GameManager.active.audioData.GetFromTime(0, 2, GameManager.active.audioData.GetKeyAtTime(0, 2, t), 16, true);
        recentRight = GameManager.active.audioData.GetFromTime(1, 2, GameManager.active.audioData.GetKeyAtTime(1, 2, t), 16, true);
        average[0] = AverageFloats(recentLeft);
        average[1] = AverageFloats(recentRight);
        GameManager.active.audioData.Set(0, 3, new Keyframe(t, average[0]));
        GameManager.active.audioData.Set(1, 3, new Keyframe(t, average[1]));

        //BeatDetection(t);
        preTime = t;
    }

    float AverageFloats(float[] input, bool alternate = false)
    {
        float sum = 0.0f;
        if (alternate)
        {
            for (int i = 0; i < input.Length; i++)
            {
                sum += input[i] * input[i];
            }
            return Mathf.Sqrt(sum / input.Length);
        }
        for (int i = 0; i < input.Length; i++)
        {
            sum += input[i];
        }
        return sum / input.Length;
    }

    #endregion

    #region Generation
    [Space]
    [Header("Generation")]
    public float[] yVis = new float[] { 0.0f, 0.0f };
    private float preTime;
    private float[] preDif = new float[2];
    /*
    public void BeatDetection(float time)
    {
        float[] newDif = new float[2];
        //float[] prePitch = new float[] { GameManager.active.audioData.Get(0, 0, preTime), GameManager.active.audioData.Get(1, 0, preTime), GameManager.active.audioData.Get(0, 1, preTime), GameManager.active.audioData.Get(1, 1, preTime) };
        var leftDif = GameManager.active.audioData.GetFromBack(0, 0, 16);
        var rightDif = GameManager.active.audioData.GetFromBack(1, 0, 16);
        newDif[0] = leftDif.Average() //Mathf.Max(leftDif) - Mathf.Min(leftDif);
        newDif[1] = rightDif.Average() //Mathf.Max(rightDif) - Mathf.Min(rightDif);
        //var lVals = leftDif.Select(obj => obj.value).ToList(); List of subvariable from list of classes/structs - requires System.linq
        //var rVals = rightDif.Select(obj => obj.value).ToList();
        GameManager.active.audioData.Set(0, 2, new Keyframe(time, newDif[0]));
        GameManager.active.audioData.Set(1, 2, new Keyframe(time, newDif[1]));
        preDif = newDif;
    }*/


    public float modiA = 0.1f;
    [Range(0.01f,3)]
    public float modiB = 1f;

    public float[] Xoffset = new float[] { 0.0f, 0.0f };
    private float countTime = 0.0f;
    public float timeDif = 10.0f;
    private bool canSpawnCollectable = true;
    private GameObject[] previous = new GameObject[4];

    void GenLine(Vector3 pos,Vector3? Sc)
    {
        Vector3 left = cam.ViewportToWorldPoint(new Vector3(cam.rect.xMin,0, 0));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(pos.x, pos.y, pos.z));
        while (left.x < right.x)
        {
            GameObject ab = GenAt(pos);
            if (Sc != null)
                ab.transform.localScale = (Vector3)Sc;
            right.x -= control.speedMulti;
        }
    }
    /// <summary>
    /// Generate method for In Game platforms
    /// </summary>
    /// <param name="pitch">Pitch modification</param>
    /// <param name="channel">left or Right speaker channel</param>
    void Gen(float pitch, int channel)
    {
        bool isLeft = (channel == 0);
        float sizeTo = 1;
        Xoffset[channel] += control.speedMulti * Time.deltaTime;
        //Debug.Log(Xoffset + " | " + sizeTo);
        if (Xoffset[channel] < sizeTo)
        {
            return;
        }

        Xoffset[channel] -= sizeTo;

        GameObject ab = null;
        //Debug.Log (dbValue);
        float y = (volume * rmsValue[channel]); //volume to y height
        //Minimum distance from previous y values - works somewhat well
        if (preY != 0)
        {
            if (y - max > preY)
            {
                y = preY + max;
            }
            else if (preY - max > y)
            {
                y = preY - max;
            }
        }
        if (count == 8)
        {
            count = 0;
            preY = y;
        }
        else
        {
            count++;
        }

        y += ((dbValue[channel] / 10) * (Exaggerate)) + pitch; //Adjustment values - decibels * exaggerations, and pitch adjustment
        yVis[channel] = y; //Update visible y in inspector
        if (HillClimb == true) { y += control.yEdit; } //Hill climbing y value added on

        Vector3 scl;
        //Generate new object at right of camera - bottom line
        Vector3 newPos = new Vector3(xDis, y, channel + 1);
        ab = GenAt(newPos);
        //if (first)
        //GenLine(newPos, null); //Generate starting line at beginning
        GameManager.active.totalGen++;

        scl = platform.transform.localScale;
        if (previous[channel] != null && previous[channel].activeSelf)
        {
            scl.x = (previous[channel].transform.localPosition.x - ab.transform.localPosition.x) + (-control.speedMulti * Time.deltaTime);

            if (canSpawnCollectable && isLeft)
            {
                if (pitch < GameManager.active.audioData.Get(0, 1, preTime))
                {
                    SpawnCollectable(previous[channel]);
                }
            }
        }
        else
            scl.x *= -1;
        scl.z = 1;
        ab.transform.localScale = scl;
        if (isLeft)
        {
            ab.transform.GetChild(0).GetComponent<MeshRenderer>().material = defaultMat;
            if (ScrollingTexture != null)
            {
                ab.GetComponent<DestroyAfter>().SetTexture(ScrollingTexture);
            }
        }
        else{
            RedCollider RAB = ab.transform.GetChild(0).GetComponent<RedCollider>();
            RAB.gray();
        }
        previous[channel] = ab;

        //Generate new object at right of camera - top line
        newPos = new Vector3(xDis, y + control.dis, isLeft ? 0 : 2);
        ab = GenAt(newPos);
        scl.y *= -1; //Invert scale so platform is from roof
        ab.transform.localScale = scl;
        if (isLeft)
        {
            ab.transform.GetChild(0).GetComponent<MeshRenderer>().material = defaultMat;
            if (ScrollingTexture != null)
            {
                ab.GetComponent<DestroyAfter>().SetTexture(ScrollingTexture);
            }
        }
        else
        {
            //Disable collisions - means that wont collide twice
            RedCollider RAB = ab.transform.GetChild(0).GetComponent<RedCollider>();
            RAB.gray();
        }
        //if (first)
        //GenLine(newPos, Sc);

    }


    /// <summary>
    /// Generate method for Menu Platforms
    /// </summary>
    /// <param name="pitch">Pitch modification</param>
    /// <param name="channel">left or right speaker channel</param>
    void GenMenu(float pitch, int channel)
    {
        bool isLeft = channel == 0; //Is it left
        int modi = isLeft ? -1 : 1; //-1 for left, 1 for right
        int spiral = GameManager.active.Mode == "spiral" ? 0 : 1;
        float sizeTo = 0.1f;
        Xoffset[channel] += control.speedMulti * Time.deltaTime;
        //Debug.Log(Xoffset + " | " + sizeTo);
        if (Xoffset[channel] < sizeTo)
        {
            //return;
        }

        Xoffset[channel] -= sizeTo;
        dis = 1;
        //Debug.Log (dbValue);
        //float y = cam.transform.position.y; //Set y to middle of camera
        float yS = ((volume * rmsValue[channel]) / 10); //Y scale made from volume
        yS += ((dbValue[channel] / 20) * (Exaggerate)) + pitch; //Adjustments - Decibel to exaggerate value - pitch increase
        if (yS < min) { yS = min; } //Minimum scale to get a constant line. Disabling this makes some parts invisible
        yVis[channel] = yS; //Visible y value in inspector
        float nextMoveDis = control.speedMulti * Time.deltaTime; //The (almost) amount the platforms will move this frame - forget about this and you get gaps

        //When loading audio up, deltaTime becomes large, and the platforms stretch massively in the x direction
        if (Time.deltaTime > 0.1)
        {
            nextMoveDis *= 0.1f;
        }
        float blockTime = Time.time; //Time.time for normal use - AudioObject.time when comparing GetMenuY to Graph
        //Spawning The Block
        Vector3 scl; //Block scale
        //New position of block - opposite side of 0 to how they move - so first frame they will be on 0
        Vector3 newPos = new Vector3(nextMoveDis*modi*-1, MoveRight.GetMenuY(isLeft, nextMoveDis*modi*-1,blockTime)*spiral, 0);
        GameObject ab = GenAt(newPos);//Generate at new posiition

        //Set the scale to certain amount - bottom part
        scl = platform.transform.localScale; //Get the default scale
        if (previous[channel] != null && previous[channel].activeSelf && spiral == 1) //If there is a previous
        {
            scl.x = (previous[channel].transform.localPosition.x - ab.transform.localPosition.x) + (nextMoveDis * modi); //Stretch to distance between previous and new + the distance previous will move
        }
        else
        {
            scl.x = isLeft ? 1 : -1;
        }
        if ((isLeft && scl.x > 0) || (!isLeft && scl.x < 0)) //If they are pointing the wrong way in scale
            scl.x *= -1;
        scl.y = yS; //Y scale
        scl.z = 1;
        ab.transform.localScale = scl; //Set the scale

        ab.GetComponent<MoveRight>().Left = isLeft; //Set if it is left or right
        ab.GetComponent<MoveRight>().AudioTime = blockTime; //Store the time of audio it was created - used when comparing to audio graph
        //ab.transform.GetChild(0).GetComponent<MeshRenderer>().material = LeftRight[channel]; //Set to left colour
        ab.GetComponent<DestroyAfter>().SetNextColour(currentCol); //Set the colour after 1 frame
        

        previous[channel] = ab; //Set as previous


        ab = GenAt(newPos); //Generate at new posiition

        //Set the scale to certain amount - top part
        scl.y *= -1; //Flip the y scale for top
        ab.transform.localScale = scl;

        ab.GetComponent<MoveRight>().Left = isLeft;
        ab.GetComponent<MoveRight>().AudioTime = blockTime;
        //ab.transform.GetChild(0).GetComponent<MeshRenderer>().material = LeftRight[channel]; //Set to left colour
        ab.GetComponent<DestroyAfter>().SetNextColour(currentCol); //Set the colour after 1 frame

        previous[channel + 2] = ab;
    }

    void GenBars(float pitch, int channel,bool toZero = false)
    {
        if (!toZero)
        {
            int dataWidth = Mathf.FloorToInt(qSamples / retroBars.barAmount);
            float parentwidth = retroBars.GetComponent<RectTransform>().rect.width;
            float width = parentwidth / retroBars.barAmount;
            float start = (parentwidth / 2) - (width / 2);
            float value = 0.0f;
            float dbV = 0.0f;
            for (int i = 0; i < retroBars.barAmount; i++)
            {
                float sum = 0;
                for (int z = 0; z < dataWidth; z++)
                {
                    sum += samples[(i * dataWidth) + z] * samples[(i * dataWidth) + z];
                }
                value = Mathf.Sqrt(sum / dataWidth);
                dbV = 20 * Mathf.Log10(value / refValue);
                if (dbV < 0) dbV = 0;

                RectTransform rectT = retroBars.bars[i + (channel * retroBars.barAmount)].GetComponent<RectTransform>();
                float yS = ((volume * value));
                yS += ((dbV / 20) * (Exaggerate * 50)) + pitch;
                if (yS < 0) yS *= -1;

                if (channel == 0)
                {
                    float dif = (AudioObject.panStereo * -1) + 1;
                    yS *= (1 - ((float)i / retroBars.barAmount)) * retroBars.height * 0.02f * dif;
                }
                else
                {
                    float dif = (AudioObject.panStereo) + 1;
                    yS *= (((float)i / retroBars.barAmount)) * retroBars.height * 0.02f * dif;
                }
                rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yS);
                rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                Vector2 pos = rectT.anchoredPosition;
                pos.x = (width * i) - start;
                rectT.anchoredPosition = pos;
            }
        }
        else
        {
            for (int i = 0; i < retroBars.barAmount*2; i++)
            {
                RectTransform rectT = retroBars.bars[i].GetComponent<RectTransform>();
                rectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            }
        }
    }
    /// <summary>
    /// Generate a new block at position
    /// </summary>
    /// <param name="pos">Position to generate at</param>
    /// <returns>Instance of the new block</returns>
    GameObject GenAt(Vector3 pos)
    {
        GameObject obj = GetFirstAble();
        if (obj != null)
        {
            obj.SetActive(true);
            obj.transform.position = pos;
        }
        return obj;
    }

    #endregion

    #region Misc

    void LoadOptions()
    {
        if (GameManager.active != null)
        {
            Exaggerate = GameManager.active.GetOption("Exaggeration");
        }
    }

    [Header("Collectable")]
    public GameObject collectablePrefab;
    public Vector3 collectableOffset;

    private void SpawnCollectable(GameObject toSpawnAt)
    {
        Vector3 pos = toSpawnAt.transform.position;
        pos.x += collectableOffset.x;
        pos.z += collectableOffset.z;
        pos.y += Random.Range(0.5f,control.dis/2);
        GameObject coll = Instantiate(collectablePrefab, pos, new Quaternion());
        canSpawnCollectable = false;
        countTime = Time.time;
    }
    [Header("Colour")]
    public Color currentCol = new Color(255,255,255); //Current Colour
    public int queueIndex = 0; //Index of colour to aim for
    public float colorLerpRate = 1f; //Rate of change
    public Text TitleText; //Title

    /// <summary>
    /// Sets the colour based off time and list of colours
    /// </summary>
    void SetBlockColour()
    {
        if (GameManager.active.DoRainbowColors)
        {
            if (Time.deltaTime > 0.1)
                return;
            if (currentCol == GameManager.active.blockColours[queueIndex])
            {
                queueIndex++;

                if (queueIndex >= GameManager.active.blockColours.Count)
                {
                    queueIndex = 0;
                }
            }

            //Debug.Log("Time: " + Time.deltaTime);
            currentCol = Color.Lerp(currentCol, GameManager.active.blockColours[queueIndex], Time.deltaTime * colorLerpRate); //Changes from currentCol to blockColours[queueIndex]

            //Change Title Colour
            if (TitleText != null)
            {
                TitleText.color = currentCol;
                //TitleText.transform.GetChild(0).GetComponent<Text>().color = currentCol;
            }
        }
        else
        {
            currentCol = Color.Lerp(currentCol, Color.white, Time.deltaTime * colorLerpRate);
        }
    }
    #endregion
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;
//using YoutubeExtractor;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

public class ImportTrack : MonoBehaviour
{
    void Awake()
    {
        StartImport();
        //ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        
    }

    string[] valid = new string[] { ".wav", ".ogg" };

    /// <summary>
    /// Is it a valid file that Unity can load
    /// </summary>
    /// <param name="fileName">The file name with extension</param>
    /// <returns>Bool is valid</returns>
    bool IsValidFileType(string fileName)
    {
        return valid.Contains(Path.GetExtension(fileName));
    }

    /// <summary>
    /// The main import method for locating files from the Music Folder.
    /// Does not load files - only stores the name and path to them
    /// </summary>
    public void StartImport()
    {
        string ThisDir = Directory.GetCurrentDirectory(); //Gets the Local directory of the application
        string Musicpath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic); //Get the music folder location
        //And special modifications not identified
        //Android
        if (Application.platform == RuntimePlatform.Android)
        {
            valid = valid.Concat(new string[] { ".mp3" }).ToArray();
            Musicpath = Path.Combine(AndroidJavaUti.CurrentSDCardPath, "Music"); //Android requires an sdcard path before /Music in path, function to get that
        }
        GameManager.active.DebugMobile("System: " + Application.platform + " Path set to: " + Musicpath);

        var info = new DirectoryInfo(Musicpath); //Load directory info - GetFiles() returns full paths instead of paths relative to directory, like Directory.GetFiles()
        FileInfo[] files = info.GetFiles().Where(f => IsValidFileType(f.FullName)).ToArray(); //Returns all valid files in an array
        foreach (FileInfo s in files)
        {
            GameManager.active.DebugMobile("Added new clip: " + Path.GetFileName(s.FullName));

            GameManager.active.AddClip(new GameManager.AudioData(s.FullName)); //Add name and path to a list of names and paths
        }
    }

    /// <summary>
    /// [Legacy Code]
    /// Loads audio files from Resources folder.
    /// Useless because there is no need when we can now load from the Music folder.
    /// </summary>
    public void ReloadResources()
    {
        AudioClip[] newClips = Resources.LoadAll<AudioClip>("");
        GameManager.active.ClearTracks();
        foreach (AudioClip A in newClips)
        {
            GameManager.active.AddClip(new GameManager.AudioData(A.name, Application.streamingAssetsPath));
            A.UnloadAudioData();
        }
        newClips = Resources.LoadAll<AudioClip>("Music");
        foreach (AudioClip A in newClips)
        {
            GameManager.active.AddClip(new GameManager.AudioData(A.name, Application.streamingAssetsPath));
            A.UnloadAudioData();
        }
    }

    [ContextMenu("Test URL")]
    public void TestURL()
    {
        //Youtube - youtube asset
        //Soundcloud - API and App registration
        fromURL("http://media-ice.musicradio.com/HeartNorfolkMP3");
    }

    public void fromURL(string URL)
    {
        Debug.Log(URL);
        string newURL = URL;
        if (!URL.ToLower().Contains("http")) newURL = "http://" + URL;
        GameManager.active.SetClip(newURL);
    }

    [ContextMenu("Test Youtube")]
    public void TestYoutube()
    {
        FromYoutube("https://www.youtube.com/watch?v=gxnvxtYfsd4");
    }

    public void FromYoutube(string URL)
    {
        
        /*
         * Get the available video formats.
         * We'll work with them in the video and audio download examples.
        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(URL);


        /*
         * We want the first extractable video with the highest audio quality.
        VideoInfo[] videos = videoInfos
            .Where(info => info.CanExtractAudio).ToArray();

        if(!videos.Any())
        {
            Debug.Log("No audio extractable from video");
        }

        var video = videos.First();

        /*
         * If the video has a decrypted signature, decipher it
        if (video.RequiresDecryption)
        {
            DownloadUrlResolver.DecryptDownloadUrl(video);
        }

        /*
         * Create the audio downloader.
         * The first argument is the video where the audio should be extracted from.
         * The second argument is the path to save the audio file.
        var audioDownloader = new AudioDownloader(video, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), video.Title + video.AudioExtension));

        // Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
        // because the download will take much longer than the audio extraction.
        audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
        audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

        /*
         * Execute the audio downloader.
         * For GUI applications note, that this method runs synchronously.
        audioDownloader.Execute();
        */
    }

        /*
    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
    */
}

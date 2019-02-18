using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AndroidJavaUti
{
    private static string m_pkgName;
    private static string m_sdCardPath;
    public static AndroidJavaObject Activity
    {
        get
        {
            AndroidJavaClass jcPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return jcPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }


    public static string CurrentPkgName
    {
        get
        {
            if (m_pkgName == null)
                m_pkgName = Activity.Call<string>("getPackageName");
            return m_pkgName;
        }
    }


    public static string CurrentSDCardPath
    {
        get
        {
            if (m_sdCardPath == null)
            {
                AndroidJavaClass jc = new AndroidJavaClass("android.os.Environment");
                IntPtr getExternalStorageDirectoryMethod = AndroidJNI.GetStaticMethodID(jc.GetRawClass(), "getExternalStorageDirectory", "()Ljava/io/File;");
                IntPtr file = AndroidJNI.CallStaticObjectMethod(jc.GetRawClass(), getExternalStorageDirectoryMethod, new jvalue[] { });
                IntPtr getPathMethod = AndroidJNI.GetMethodID(AndroidJNI.GetObjectClass(file), "getPath", "()Ljava/lang/String;");
                IntPtr path = AndroidJNI.CallObjectMethod(file, getPathMethod, new jvalue[] { });
                m_sdCardPath = AndroidJNI.GetStringUTFChars(path);
                AndroidJNI.DeleteLocalRef(file);
                AndroidJNI.DeleteLocalRef(path);
                Debug.Log("m_sdCardPath = " + m_sdCardPath);
            }
            return m_sdCardPath;
        }

    }
}
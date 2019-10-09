using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    //Dereceden radyana çevirme oranı
    public static float DEG2RAD = 0.0174532925f;

    //Altıgen objelerin birbiri üzerine tam oturması için gerekli değişkenler.
    public static float XCOLLOFFSET = 0.7489f;
    public static float YROWOFFSET = 0.8656f;

    //Altıgen boyutları
    public static float WIDTH = 1.0f;
    public static float HEIGHT = 1f;

    //Orjin noktası
    public static Vector3 ORIGIN_POINT = new Vector3(0,0,0);
}

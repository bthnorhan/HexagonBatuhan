using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.Progress;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BombHexagon : Hexagon
{
    //Bomba için varsayılan hareket geri sayım
    public int bombCounter;

    //Geri sayımı ekrana yazmak için gerekli obje
    public TextMeshPro bombCounterText;

    public BombHexagon(Color[] colors) : base(colors) { }

    public delegate void BombExplosion();
    public BombExplosion bombExplosion;

    public void decreaseBombCounter()
    {
        bombCounter--;
        bombCounterText.SetText(bombCounter.ToString());
        if (bombCounter <= 0)
        {
            bombExplosion();
        }
    }

    public void setBombCounter(int counter)
    {
        bombCounter = counter;
        bombCounterText.SetText(bombCounter.ToString());
    }
}

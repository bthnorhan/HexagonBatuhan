using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hexagon : MonoBehaviour
{
    //Arkaplan rengini değiştirmek için
    public SpriteRenderer background;

    //Renk Listesi
    public Color[] colors;

    //Altıgenin rengini tanımlamak ve tanımak için
    private int colorIndex = 0;
    public int ColorIndex { get => colorIndex; set => row = colorIndex; }

    //Altıgenin konumu
    private int row, col, maxRow, maxCol;
    public int Row { get => row; set => row = value; }
    public int Col { get => col; set => col = value; }

    public Hexagon(Color[] colors)
    {
        //Renk listesini günceller.
        this.colors = colors;
    }

    private void Start()
    {
        //Başlangıçta rastgele altıgen renk indeksi seçilir
        colorIndex = Random.Range(0, colors.Length);

        //Rastgele seçilmiş renk indeksinde bulunan renk arkaplana uygulanır
        background.color = colors[colorIndex];
    }

    //Altıgenin Satır ve sütununu güncelleme
    public void updateHexagon(Hexagon updateHexagon)
    {
        Row = updateHexagon.Row;
        Col = updateHexagon.Col;
    }

    /// <summary>
    /// Kolon numarası çift mi?
    /// </summary>
    /// <returns>Boolean -> Çift ise true değil ise false</returns>
    public bool colIsEven() {
        return col % 2 == 0;
    }
}

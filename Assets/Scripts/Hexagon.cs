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

    //Varsayılan tanımlı renkler. Editörden kolayca değiştirilebilir
    public Color[] colors = {
        new Color(0.95f, 0.26f, 0.21f),
        new Color(0.61f, 0.15f, 0.69f),
        new Color(0.24f, 0.31f, 0.7f),
        new Color(0, 0.73f, 0.83f),
        new Color(0.29f, 0.68f, 0.31f),
    };

    //Altıgenin rengini tanımlamak ve tanımak için
    private int colorIndex = 0;

    public int ColorIndex { get => colorIndex; set => row = colorIndex; }

    //Altıgenin konumu
    private int row, col, maxRow, maxCol;

    public int Row { get => row; set => row = value; }
    public int Col { get => col; set => col = value; }

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

    public bool colIsEven() {
        return col % 2 == 0;
    }

    public bool rowIsEven() {
        return row % 2 == 0;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    //Oyun boyutları. Editörden değiştirilebilir.
    public int columnSize = 8;
    public int rowSize = 9;

    //Oynanan hareket sayısı ve puan
    public int point = 0, movement = 0;

    //Altıgen prefabi
    public GameObject hexagonPrefab;

    //Seçlmiş altıgen grubunu içerisinde tutan ve dönmelerde kolaylık sağlayan obje
    public GameObject selectedHexagonsContainer;


    //Obje oluşturmak için yardımcı vektör.
    private Vector3 objectPoint = Constants.ORIGIN_POINT;

    //Altıgenin hangi komuşusunun seçildiğini belirten değişken
    private int neighborIndex = 0;

    //Altıgenler döndürülüyor mu ?
    private bool isTurning = false;

    //Oluşturulan altıgenlerin içinde tutulduğu dizi
    private GameObject[,] hexagons;

    //Seçilmiş altıgen
    private GameObject selectedHexagon;
    public GameObject SelectedHexagon { get => selectedHexagon; }

    private MenuController menuController;

    private List<GameObject> explosionList = new List<GameObject>();
    private List<int> explosionColumnList = new List<int>();

    //Oyun ekranı açıldığında kamerayı oluşturulan altıgenlere göre ayarlanır ve altıgenleri oluştur.
    void Start()
    {
        Camera.main.transform.position = new Vector3(((columnSize / 2.0f) - 0.5f) * Constants.XCOLLOFFSET, (rowSize / 2.0f) * Constants.YROWOFFSET, Camera.main.transform.position.z);
        Camera.main.orthographicSize = ((columnSize + 2) * Constants.XCOLLOFFSET);

        menuController = GetComponent<MenuController>();

        hexagons = new GameObject[rowSize, columnSize];
        for (int row = 0; row < rowSize; row++)
        {
            for (int col = 0; col < columnSize; col++)
            {
                GenerateHexagon(row, col);
            }
        }

        //StartCoroutine(CheckHexExplosion(true));
    }

    /// <summary>
    /// Belirtilen satır sütunda altıgen oluşturur.
    ///     row -> Satır
    ///     col -> Sütun
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    private void GenerateHexagon(int row, int col)
    {
        objectPoint.Set(col * Constants.XCOLLOFFSET, row * Constants.YROWOFFSET, 0);
        if (col % 2 == 1)
        {
            objectPoint.Set(col * Constants.XCOLLOFFSET, row * Constants.YROWOFFSET + (Constants.YROWOFFSET / 2.0f), 0);
        }
        GameObject hex = Instantiate(hexagonPrefab, objectPoint, Quaternion.identity);
        hex.name = "HEX_" + row + "_" + col + "_" + movement;
        Hexagon hexagon = hex.GetComponent<Hexagon>();
        hexagon.Row = row;
        hexagon.Col = col;
        hexagons[row, col] = hex;
    }

    /// <summary>
    /// Seçilen altıgen tanımlanır.
    /// </summary>
    /// <param name="gameObject"></param>
    public void SetSelectedHexagon(GameObject gameObject)
    {
        selectedHexagonsContainer.transform.DetachChildren();

        if (gameObject != null)
        {
            if (selectedHexagon != null)
            {
                if (gameObject.name.Equals(selectedHexagon.name))
                {
                    neighborIndex++;
                    neighborIndex %= 6;
                }
                else
                {
                    neighborIndex = 0;
                }
            }
            else
            {
                neighborIndex = 0;
            }

            selectedHexagon = gameObject;
            MoveSelectedHexagonsContainer();
        } else
        {
            selectedHexagon = null;
            neighborIndex = 0;
            Vector3 position = new Vector3(-10, 2, -9);
            selectedHexagonsContainer.transform.position = position;
        }
    }

    /// <summary>
    /// Seçilmiş altıgenlerin tutulacağı objeyi pivot noktası seçilmiş altıgenin istenen
    /// köşesinde olacak şekilde taşınır.
    /// </summary>
    private void MoveSelectedHexagonsContainer()
    {
        var row = selectedHexagon.GetComponent<Hexagon>().Row;
        var col = selectedHexagon.GetComponent<Hexagon>().Col;

        if (row == 0)
        {
            if (col == 0)
            {
                neighborIndex = 1;
            }
            else if (col == columnSize - 1)
            {
                if (columnSize % 2 == 1)
                {
                    neighborIndex = 2;
                }
                else
                {
                    if (neighborIndex != 2 && neighborIndex != 3)
                    {
                        neighborIndex = 2;
                    }
                }
            }
            else
            {
                if (col % 2 == 1)
                {
                    if (neighborIndex == 4 || neighborIndex == 5)
                    {
                        neighborIndex = 0;
                    }
                } else
                {
                    if (neighborIndex != 1 && neighborIndex != 2)
                    {
                        neighborIndex = 1;
                    }
                }
            }
        }
        else if (row == rowSize - 1)
        {
            if (col == 0)
            {
                if (neighborIndex != 0 && neighborIndex != 5)
                {
                    neighborIndex = 5;
                }
            }
            else if (col == columnSize - 1)
            {
                if (columnSize % 2 == 0)
                {
                    neighborIndex = 4;
                }
                else
                {
                    if (neighborIndex != 3 && neighborIndex != 4)
                    {
                        neighborIndex = 3;
                    }
                }
            }
            else
            {
                if (col % 2 == 1)
                {
                    if (neighborIndex != 4 && neighborIndex != 5)
                    {
                        neighborIndex = 4;
                    }
                }
                else
                {
                    if (neighborIndex == 1 || neighborIndex == 2)
                    {
                        neighborIndex = 3;
                    }
                }
            }
        }
        else
        {
            if (col == 0)
            {
                if (neighborIndex != 0 && neighborIndex != 1 && neighborIndex != 5)
                {
                    neighborIndex = 5;
                }
            }
            else if (col == columnSize - 1)
            {
                if (neighborIndex != 2 && neighborIndex != 3 && neighborIndex != 4)
                {
                    neighborIndex = 2;
                }
            }
        }

        var angleRadian = (60 * neighborIndex) * Constants.DEG2RAD;
        var position = new Vector3(selectedHexagon.transform.position.x + ((Constants.WIDTH / 2) * Mathf.Cos(angleRadian)), selectedHexagon.transform.position.y + ((Constants.HEIGHT / 2) * Mathf.Sin(angleRadian)), -9);

        var tempRotation = selectedHexagonsContainer.transform.eulerAngles;
        tempRotation.z = neighborIndex * 60;
        selectedHexagonsContainer.transform.eulerAngles = tempRotation;

        selectedHexagonsContainer.transform.position = position;
    }

    /// <summary>
    /// Obje grubunun dönmesini sağlar.
    ///     Sağa dönme -> true
    ///     Sola dönme -> false
    /// </summary>
    /// <param name="isClockWise"></param>
    public void RotateSelectedHexagonsContainer(bool isClockWise)
    {
        if (selectedHexagon == null || isTurning) return;

        movement++;
        menuController.setMovementText(movement.ToString());

        var row = selectedHexagon.GetComponent<Hexagon>().Row;
        var col = selectedHexagon.GetComponent<Hexagon>().Col;
        var colIsEven = selectedHexagon.GetComponent<Hexagon>().colIsEven();

        hexagons[row, col].transform.SetParent(selectedHexagonsContainer.transform);

        switch (neighborIndex)
        {
            case 0:
                hexagons[colIsEven ? row : row + 1, col + 1].transform.SetParent(selectedHexagonsContainer.transform);
                hexagons[colIsEven ? row - 1 : row, col + 1].transform.SetParent(selectedHexagonsContainer.transform);
                break;
            case 1:
                hexagons[row + 1, col].transform.SetParent(selectedHexagonsContainer.transform);
                hexagons[colIsEven ? row : row + 1, col + 1].transform.SetParent(selectedHexagonsContainer.transform);
                break;
            case 2:
                hexagons[colIsEven ? row : row + 1, col - 1].transform.SetParent(selectedHexagonsContainer.transform);
                hexagons[row + 1, col].transform.SetParent(selectedHexagonsContainer.transform);
                break;
            case 3:
                hexagons[colIsEven ? row - 1 : row, col - 1].transform.SetParent(selectedHexagonsContainer.transform);
                hexagons[colIsEven ? row : row + 1, col - 1].transform.SetParent(selectedHexagonsContainer.transform);
                break;
            case 4:
                hexagons[row - 1, col].transform.SetParent(selectedHexagonsContainer.transform);
                hexagons[colIsEven ? row - 1 : row, col - 1].transform.SetParent(selectedHexagonsContainer.transform);
                break;
            case 5:
                hexagons[colIsEven ? row - 1 : row, col + 1].transform.SetParent(selectedHexagonsContainer.transform);
                hexagons[row - 1, col].transform.SetParent(selectedHexagonsContainer.transform);
                break;
        }

        StartCoroutine(RotateContainer(isClockWise));
    }


    /// <summary>
    /// Seçili altıgen grubunu döndürür ve diziyi güncellemesi için swap fonksiyonunu çağırır.
    ///     Sağa dönme -> true
    ///     Sola dönme -> false
    /// </summary>
    /// <param name="isClockWise"></param>
    /// <returns></returns>
    private IEnumerator RotateContainer(bool isClockWise)
    {
        isTurning = true;
        for (int i = 0; i < 3 && isTurning; i++)
        {
            selectedHexagonsContainer.transform.Rotate(Vector3.forward, 120 * (isClockWise ? -1 : 1));
            SwapHexagons(isClockWise);
            StartCoroutine(CheckHexagonExplosion(false));
            yield return new WaitForSeconds(0.5f);
        }

        selectedHexagon = null;
        neighborIndex = 0;
        Vector3 position = new Vector3(-10, 2, -9);
        selectedHexagonsContainer.transform.DetachChildren();
        selectedHexagonsContainer.transform.position = position;
        isTurning = false;
    }

    /// <summary>
    /// Her dönüşte altıgenlerin konumunu dizide günceller
    ///     Sağa dönme -> true
    ///     Sola dönme -> false
    /// </summary>
    /// <param name="isClockWise"></param>
    private void SwapHexagons(bool isClockWise) {
        GameObject first = selectedHexagonsContainer.transform.GetChild(0).gameObject;
        GameObject second = selectedHexagonsContainer.transform.GetChild(1).gameObject;
        GameObject third = selectedHexagonsContainer.transform.GetChild(2).gameObject;

        Hexagon swap1 = first.GetComponent<Hexagon>();
        Hexagon swap2 = second.GetComponent<Hexagon>();
        Hexagon swap3 = third.GetComponent<Hexagon>();

        int tempRow = swap1.Row, tempCol = swap1.Col;

        selectedHexagonsContainer.transform.DetachChildren();

        if (isClockWise)
        {
            swap1.updateHexagon(swap2);
            swap2.updateHexagon(swap3);
            swap3.Row = tempRow;
            swap3.Col = tempCol;

            selectedHexagon = third;

            third.transform.SetParent(selectedHexagonsContainer.transform);
            first.transform.SetParent(selectedHexagonsContainer.transform);
            second.transform.SetParent(selectedHexagonsContainer.transform);
        }
        else
        {
            swap1.updateHexagon(swap3);
            swap3.updateHexagon(swap2);
            swap2.Row = tempRow;
            swap2.Col = tempCol;

            selectedHexagon = second;

            second.transform.SetParent(selectedHexagonsContainer.transform);
            third.transform.SetParent(selectedHexagonsContainer.transform);
            first.transform.SetParent(selectedHexagonsContainer.transform);
        }

        hexagons[swap1.Row, swap1.Col] = first;
        hexagons[swap2.Row, swap2.Col] = second;
        hexagons[swap3.Row, swap3.Col] = third;
    }

    /// <summary>
    /// Oyun alanındaki bütün eşleşmeleri kontrol eder.
    /// overridePointMechanic puan verilmemesi gereken durumlarda true gönderilmelidir.
    /// </summary>
    /// <param name="overridePointMechanic"></param>
    /// <returns></returns>
    private IEnumerator CheckHexagonExplosion(bool overridePointMechanic)
    {
        yield return new WaitForSeconds(0.1f);
        explosionList.Clear();

        Dictionary<GameObject, byte> tempHexagonDictionary = new Dictionary<GameObject, byte>();

        for (int i = 1; i < rowSize - 1; i++)
        {
            for (int j = 1; j < columnSize - 1; j++)
            {
                GameObject go0 = hexagons[i, j];
                GameObject go1 = hexagons[j % 2 == 0 ? i : i + 1, j + 1];
                GameObject go2 = hexagons[j % 2 == 0  ? i - 1 : i, j + 1];
                GameObject go3 = hexagons[i - 1, j];
                GameObject go4 = hexagons[j % 2 == 0  ? i - 1 : i, j - 1];
                GameObject go5 = hexagons[j % 2 == 0  ? i : i + 1, j - 1];
                GameObject go6 = hexagons[i + 1, j];

                int c0 = go0.GetComponent<Hexagon>().ColorIndex;
                int c1 = go1.GetComponent<Hexagon>().ColorIndex;
                int c2 = go2.GetComponent<Hexagon>().ColorIndex;
                int c3 = go3.GetComponent<Hexagon>().ColorIndex;
                int c4 = go4.GetComponent<Hexagon>().ColorIndex;
                int c5 = go5.GetComponent<Hexagon>().ColorIndex;
                int c6 = go6.GetComponent<Hexagon>().ColorIndex;

                if (c0 == c1 && c0 == c2)
                {
                    tempHexagonDictionary[go0] = 0;
                    tempHexagonDictionary[go1] = 0;
                    tempHexagonDictionary[go2] = 0;
                }

                if (c0 == c2 && c0 == c3)
                {
                    tempHexagonDictionary[go0] = 0;
                    tempHexagonDictionary[go1] = 0;
                    tempHexagonDictionary[go2] = 0;
                }

                if (c0 == c3 && c0 == c4)
                {
                    tempHexagonDictionary[go0] = 0;
                    tempHexagonDictionary[go1] = 0;
                    tempHexagonDictionary[go2] = 0;
                }

                if (c0 == c4 && c0 == c5)
                {
                    tempHexagonDictionary[go0] = 0;
                    tempHexagonDictionary[go1] = 0;
                    tempHexagonDictionary[go2] = 0;
                }

                if (c0 == c5 && c0 == c6)
                {
                    tempHexagonDictionary[go0] = 0;
                    tempHexagonDictionary[go1] = 0;
                    tempHexagonDictionary[go2] = 0;
                }

                if (c0 == c6 && c0 == c1)
                {
                    tempHexagonDictionary[go0] = 0;
                    tempHexagonDictionary[go1] = 0;
                    tempHexagonDictionary[go2] = 0;
                }
            }
        }

        explosionList = tempHexagonDictionary.Keys.ToList();
        StartCoroutine(ExplodeHexagons(overridePointMechanic));
    }

    private IEnumerator ExplodeHexagons(bool overridePointMechanic)
    {
        if (explosionList.Count == 0) StopCoroutine("ExplodeHexagons");
        yield return new WaitForSeconds(0.1f);

        if (!overridePointMechanic)
        {
            point += (5 * explosionList.Count);
            menuController.setPointText(point.ToString());
        }

        Dictionary<int, byte> tempColumnDictionary = new Dictionary<int, byte>();

        for (int i = 0; i < explosionList.Count; i++)
        {
            Hexagon temp = explosionList[i].GetComponent<Hexagon>();
            tempColumnDictionary[temp.Col] = 0;
            hexagons[temp.Row, temp.Col] = null;
            if (explosionList[i].transform.IsChildOf(selectedHexagonsContainer.transform))
            {
                isTurning = false;
            }
            Destroy(explosionList[i]);
        }
        explosionList.Clear();
        explosionColumnList = tempColumnDictionary.Keys.ToList();
        StartCoroutine(RefillHexagons());
    }

    //public List<GameObject> tempColumnList = new List<GameObject>();
    private IEnumerator RefillHexagons()
    {
        yield return new WaitForSeconds(0.2f);
        int counter = 0;

        for (int i = 0; i < explosionColumnList.Count; i++)
        {
            for (int j = 0; j < rowSize; j++)
            {
                if (hexagons[j, explosionColumnList[i]] != null)
                {
                    hexagons[j, explosionColumnList[i]].transform.position = new Vector3(explosionColumnList[i] * Constants.XCOLLOFFSET, j * Constants.YROWOFFSET + (explosionColumnList[i] % 2 == 0 ? 0 : (Constants.YROWOFFSET / 2.0f)), 0);
                    hexagons[j, explosionColumnList[i]].GetComponent<Hexagon>().Row = counter;
                    hexagons[j, explosionColumnList[i]].GetComponent<Hexagon>().Col = explosionColumnList[i];
                    counter++;
                    //tempColumnList.Add(hexagons[j, explosionColumnList[i]]);
                }
            }

            for (int j = counter; j < rowSize; j++)
            {
                GenerateHexagon(j, explosionColumnList[i]);
            }
        }
    }
}

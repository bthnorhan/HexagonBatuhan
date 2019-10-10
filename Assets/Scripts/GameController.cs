using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{

    #region Değişkenler

    //Oyun boyutları. Editörden değiştirilebilir.
    public int columnSize = 8;
    public int rowSize = 9;

    //Oyun bitti mi ?
    public bool isGameOver = false;

    // Varsayılan renk listesi editörden değiştirilebilir.
    public Color[] hexagonColors = new Color[]
    {
        new Color(0.95f, 0.26f, 0.21f),
        new Color(0.61f, 0.15f, 0.69f),
        new Color(0.24f, 0.31f, 0.7f),
        new Color(0, 0.73f, 0.83f),
        new Color(0.29f, 0.68f, 0.31f),
    };

    //Oynanan hareket sayısı ve puan
    public int point = 0, movement = 0;

    //Bombanın geleceği eşik değeri
    public int bombThresholdPoint = 1000;

    //Altıgen prefabi
    public GameObject hexagonPrefab;

    //Bomba altıgen prefabi
    public GameObject bombHexagonPrefab;

    //Seçlmiş altıgen grubunu içerisinde tutan ve dönmelerde kolaylık sağlayan obje
    public GameObject selectedHexagonsContainer;

    //Bağlı olan bombalara geri sayımını azaltmak için gerekli fonksiyonlarını çağırır
    public delegate void MovementEmitter();
    public MovementEmitter movementEmitter;


    //Obje oluşturmak için yardımcı vektör.
    private Vector3 objectPoint = Constants.ORIGIN_POINT;

    //Altıgenin hangi komuşusunun seçildiğini belirten değişken
    private int neighborIndex = 0;

    private GameObject[] neighborHexagons = new GameObject[] {};

    //Altıgenler döndürülüyor mu ?
    private bool isTurning = false;

    //Yeni altıgenler oluşturuluyor mu ?
    private bool isRefilling = false;

    //Oluşturulan bomba sayısı
    private int bombCount = 0;

    //Bomba oyun içerisinde oluşturulmalı mı ?
    public bool shouldInstantiateBomb = false;

    //Oluşturulan altıgenlerin içinde tutulduğu dizi
    public GameObject[,] hexagons;

    //Seçilmiş altıgen
    private GameObject selectedHexagon;
    public GameObject SelectedHexagon { get => selectedHexagon; }

    //Menü kontrolcüsü
    private MenuController menuController;

    //Patlatılacak altıgen listesi
    private List<GameObject> explosionList = new List<GameObject>();

    //Patlatılmış altıgen kolonları
    private List<int> explosionColumnList = new List<int>();

    #endregion

    #region Unity Uygulama Hayat Döngüsü

    private void Start()
    {
        //Ekran asla kapanmasın modu.
        Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
    }

    void Awake()
    {
        menuController = GetComponent<MenuController>();

        //Kamerayı oluşturulacak altıgenlere göre ayarlar.
        Camera.main.transform.position = new Vector3(((columnSize / 2.0f) - 0.5f) * Constants.XCOLLOFFSET, (rowSize / 2.0f) * Constants.YROWOFFSET, Camera.main.transform.position.z);
        Camera.main.orthographicSize = ((columnSize + 2) * Constants.XCOLLOFFSET);

        StopAllCoroutines();

        StartCoroutine(SetupGame());
    }


    /// <summary>
    /// Oyun kapatılmasında veya sahne sonlandırılmasında arkaplanda çalışan işlemleri durdurur.
    /// </summary>
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    #endregion

    /// <summary>
    ///  Oyunun değişkenlerini ayarlar ve yükleniyor görselini ekrana getirir.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetupGame()
    {
        menuController.fadeInLoadingCanvas();
        selectedHexagon = null;
        neighborIndex = 0;
        Vector3 position = new Vector3(-10, 2, -9);
        selectedHexagonsContainer.transform.position = position;

        bombCount = 0;
        point = 0;
        movement = 0;

        menuController.setPointText(point.ToString());
        menuController.setMovementText(movement.ToString());

        shouldInstantiateBomb = false;
        isGameOver = false;
        isTurning = false;
        isRefilling = false;

        hexagons = new GameObject[rowSize, columnSize];
        for (int row = 0; row < rowSize; row++)
        {
            for (int col = 0; col < columnSize; col++)
            {
                yield return new WaitForSeconds(Constants.HEX_INSTANTIATE_TIME);
                GenerateHexagon(row, col);
            }
        }

        StartCoroutine(CheckHexagonExplosion(true));
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

        if (shouldInstantiateBomb)
        {
            GameObject bombHex = Instantiate(bombHexagonPrefab, objectPoint, Quaternion.identity);
            bombHex.name = "BOMBHEX_" + row + "_" + col + "_" + movement;
            BombHexagon bombHexagon = bombHex.GetComponent<BombHexagon>();
            bombHexagon.bombExplosion += SetGameOver;
            movementEmitter += bombHexagon.decreaseBombCounter;
            bombHexagon.setBombCounter(Constants.BOMB_COUNTER);
            bombHexagon.Row = row;
            bombHexagon.Col = col;
            hexagons[row, col] = bombHex;
            shouldInstantiateBomb = false;
        }
        else
        {
            GameObject hex = Instantiate(hexagonPrefab, objectPoint, Quaternion.identity);
            hex.name = "HEX_" + row + "_" + col + "_" + movement;
            Hexagon hexagon = hex.GetComponent<Hexagon>();
            hexagon.Row = row;
            hexagon.Col = col;
            hexagons[row, col] = hex;
        }
    }

    /// <summary>
    /// Seçilen altıgen tanımlanır.
    /// </summary>
    /// <param name="gameObject"></param>
    public void SetSelectedHexagon(GameObject gameObject)
    {
        if (!isTurning && !isRefilling)
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

                neighborHexagons = GetNeighborHexagons(gameObject);
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
    }

    /// <summary>
    /// Seçilmiş altıgenlerin tutulacağı objeyi pivot noktası seçilmiş altıgenin istenen
    /// köşesinde olacak şekilde taşınır.
    /// </summary>
    private void MoveSelectedHexagonsContainer()
    {
        while (neighborHexagons[neighborIndex] == null || neighborHexagons[neighborIndex + 1] == null)
        {
            neighborIndex++;
            neighborIndex %= 6;
        }

        float angleRadian = (60 * neighborIndex) * Constants.DEG2RAD;
        Vector3 position = new Vector3(selectedHexagon.transform.position.x + ((Constants.WIDTH / 2) * Mathf.Cos(angleRadian)), selectedHexagon.transform.position.y + ((Constants.HEIGHT / 2) * Mathf.Sin(angleRadian)), -9);

        Vector3 tempRotation = selectedHexagonsContainer.transform.eulerAngles;
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
        if (movementEmitter != null) movementEmitter();

        int row = selectedHexagon.GetComponent<Hexagon>().Row;
        int col = selectedHexagon.GetComponent<Hexagon>().Col;

        GameObject firstSelected  = hexagons[row, col];
        GameObject secondSelected = neighborHexagons[neighborIndex + 1];
        GameObject thirdSelected  = neighborHexagons[neighborIndex];

        firstSelected.transform.SetParent(selectedHexagonsContainer.transform);
        secondSelected.transform.SetParent(selectedHexagonsContainer.transform);
        thirdSelected.transform.SetParent(selectedHexagonsContainer.transform);

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
        for (int i = 0; i < 3; i++)
        {
            selectedHexagonsContainer.transform.Rotate(Vector3.forward, 120 * (isClockWise ? -1 : 1));
            yield return new WaitForSeconds(Constants.WAIT_TIME);
            StartCoroutine(SwapHexagons(isClockWise));
            StartCoroutine(CheckHexagonExplosion(false));
            yield return new WaitForSecondsRealtime(Constants.HEX_ROTATION_WAIT_TIME);
            if (!isTurning) break;
        }

        selectedHexagon = null;
        neighborIndex = 0;
        Vector3 position = new Vector3(-10, 2, -9);
        selectedHexagonsContainer.transform.DetachChildren();
        selectedHexagonsContainer.transform.position = position;
        isTurning = false;
        yield return null;
    }

    /// <summary>
    /// Her dönüşte altıgenlerin konumunu dizide günceller
    ///     Sağa dönme -> true
    ///     Sola dönme -> false
    /// </summary>
    /// <param name="isClockWise"></param>
    private IEnumerator SwapHexagons(bool isClockWise) {
        GameObject first  = selectedHexagonsContainer.transform.GetChild(0).gameObject;
        GameObject second = selectedHexagonsContainer.transform.GetChild(1).gameObject;
        GameObject third  = selectedHexagonsContainer.transform.GetChild(2).gameObject;

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

        yield return new WaitForSeconds(Constants.WAIT_TIME);
    }

    /// <summary>
    /// Oyun alanındaki bütün eşleşmeleri kontrol eder.
    /// overridePointMechanic puan verilmemesi gereken durumlarda true gönderilmelidir.
    /// </summary>
    /// <param name="overridePointMechanic"></param>
    /// <returns></returns>
    private IEnumerator CheckHexagonExplosion(bool overridePointMechanic)
    {
        explosionList.Clear();
        Dictionary<GameObject, byte> tempHexagonDictionary = new Dictionary<GameObject, byte>();

        //////////// Altıgen renk kontrolleri
        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < columnSize; j++)
            {
                GameObject g0 = GetHexagon(i, j);
                GameObject[] tempNeighborHexagons = GetNeighborHexagons(g0);
                GameObject g1 = tempNeighborHexagons[0];
                GameObject g2 = tempNeighborHexagons[1];
                GameObject g3 = tempNeighborHexagons[2];
                GameObject g4 = tempNeighborHexagons[3];
                GameObject g5 = tempNeighborHexagons[4];
                GameObject g6 = tempNeighborHexagons[5];

                if (CheckHexagonsColors(g0, g1, g2))
                {
                    tempHexagonDictionary[g0] = 0;
                    tempHexagonDictionary[g1] = 0;
                    tempHexagonDictionary[g2] = 0;
                }

                if (CheckHexagonsColors(g0, g2, g3))
                {
                    tempHexagonDictionary[g0] = 0;
                    tempHexagonDictionary[g2] = 0;
                    tempHexagonDictionary[g3] = 0;
                }

                if (CheckHexagonsColors(g0, g3, g4))
                {
                    tempHexagonDictionary[g0] = 0;
                    tempHexagonDictionary[g3] = 0;
                    tempHexagonDictionary[g4] = 0;
                }

                if (CheckHexagonsColors(g0, g4, g5))
                {
                    tempHexagonDictionary[g0] = 0;
                    tempHexagonDictionary[g4] = 0;
                    tempHexagonDictionary[g5] = 0;
                }

                if (CheckHexagonsColors(g0, g5, g6))
                {
                    tempHexagonDictionary[g0] = 0;
                    tempHexagonDictionary[g5] = 0;
                    tempHexagonDictionary[g6] = 0;
                }

                if (CheckHexagonsColors(g0, g6, g1))
                {
                    tempHexagonDictionary[g0] = 0;
                    tempHexagonDictionary[g6] = 0;
                    tempHexagonDictionary[g1] = 0;
                }
            }
        }

        explosionList = tempHexagonDictionary.Keys.ToList();
        if (explosionList.Count > 0)
        {
            yield return new WaitForSeconds(Constants.WAIT_TIME);
            StartCoroutine(ExplodeHexagons(overridePointMechanic));
        }
        else
        {
            if (overridePointMechanic)
            {
                yield return new WaitForSeconds(Constants.WAIT_TIME);
                menuController.fadeOutLoadingCanvas();
            }
        }
    }

    /// <summary>
    /// Patlaması gereken altıgenleri patlatır.
    /// overridePointMechanic puan verilmemesi gereken durumlarda true gönderilmelidir.
    /// </summary>
    /// <param name="overridePointMechanic"></param>
    /// <returns></returns>
    private IEnumerator ExplodeHexagons(bool overridePointMechanic)
    {
        if (!overridePointMechanic)
        {
            int lastBombCount = bombCount;
            point += (5 * explosionList.Count);
            menuController.setPointText(point.ToString());
            bombCount = (int) Math.Floor((double)point / bombThresholdPoint);

            if (bombCount != lastBombCount)
            {
                shouldInstantiateBomb = true;
            }
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

            if (explosionList[i].GetComponent<BombHexagon>() != null)
            {
                BombHexagon bombTemp = explosionList[i].GetComponent<BombHexagon>();
                bombTemp.bombExplosion -= SetGameOver;
                movementEmitter -= bombTemp.decreaseBombCounter;
                if (bombTemp.bombCounter == 0)
                {
                    isGameOver = false;
                }
            }
            Destroy(explosionList[i]);
        }

        if (!isGameOver)
        {
            explosionColumnList = tempColumnDictionary.Keys.ToList();
            yield return new WaitForSeconds(Constants.WAIT_TIME);
            StartCoroutine(RefillHexagons(overridePointMechanic));
        }
        else
        {
            menuController.setGameOverPointText(point.ToString() + " Puan");
            menuController.setGameOverMovementText(movement.ToString() + " Hareket");
            menuController.fadeInGameOverCanvas();
        }
    }

    /// <summary>
    /// Patlamış olan altıgenlerin yerine üst tarafta bulunan altıgeneleri kaydırır ve
    /// boşluklara yeniden altıgen oluşturur.
    /// overridePointMechanic puan verilmemesi gereken durumlarda true gönderilmelidir.
    /// </summary>
    /// <param name="overridePointMechanic"></param>
    /// <returns></returns>
    private IEnumerator RefillHexagons(bool overridePointMechanic)
    {
        bool isRefilled = false;
        isRefilling = true;
        explosionColumnList.Sort();
        for (int i = 0; i < explosionColumnList.Count; i++)
        {
            int counter = 0;
            for (int j = 0; j < rowSize; j++)
            {
                if (hexagons[j, explosionColumnList[i]] != null)
                {
                    GameObject movingObject = hexagons[j, explosionColumnList[i]];
                    movingObject.transform.position = new Vector3(explosionColumnList[i] * Constants.XCOLLOFFSET, counter * Constants.YROWOFFSET + (explosionColumnList[i] % 2 == 0 ? 0 : (Constants.YROWOFFSET / 2.0f)), 0);
                    movingObject.GetComponent<Hexagon>().Row = counter;
                    movingObject.GetComponent<Hexagon>().Col = explosionColumnList[i];
                    hexagons[counter, explosionColumnList[i]] = movingObject;
                    counter++;
                    yield return new WaitForSeconds(Constants.WAIT_TIME);
                }
            }

            for (int j = counter; j < rowSize; j++)
            {
                yield return new WaitForSeconds(Constants.HEX_INSTANTIATE_TIME);
                isRefilled = true;
                GenerateHexagon(j, explosionColumnList[i]);
            }
        }
        isRefilling = false;
        if (isRefilled)
        {
            yield return new WaitForSeconds(Constants.WAIT_TIME);
            StartCoroutine(CheckHexagonExplosion(overridePointMechanic));
        }
        else
        {
            yield return new WaitForSeconds(Constants.WAIT_TIME);
        }
    }

    /// <summary>
    /// Oyun bitti mi değişkenini ayarlar.
    /// </summary>
    private void SetGameOver()
    {
        isGameOver = true;
    }

    /// <summary>
    /// Gönderilen altgenlerin renk eşitliliğini kontrol eder.
    /// g1, g2, g3 -> altıgen objesi
    /// </summary>
    /// <param name="g1"></param>
    /// <param name="g2"></param>
    /// <param name="g3"></param>
    /// <returns></returns>
    private bool CheckHexagonsColors(GameObject g1, GameObject g2, GameObject g3)
    {
        if (g1 == null || g2 == null || g3 == null) return false;

        Hexagon h1 = g1.GetComponent<Hexagon>();
        Hexagon h2 = g2.GetComponent<Hexagon>();
        Hexagon h3 = g3.GetComponent<Hexagon>();

        int c1 = h1.ColorIndex;
        int c2 = h2.ColorIndex;
        int c3 = h3.ColorIndex;

        return (c1 == c2 && c1 == c3);
    }

    /// <summary>
    /// Altıgen objelerinin tutulduğu diziden satır ve sütuna bağlı olarak altıgen objesi döndürür.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public GameObject GetHexagon(int row, int column)
    {
        try
        {
            return hexagons[row, column];
        }
        catch (Exception e)
        {
            return null;
        }
    }

    /// <summary>
    /// Seçili altıgenin komşu altıgenlerini ayarlar.
    /// </summary>
    /// <param name="hexagon"></param>
    public GameObject[] GetNeighborHexagons(GameObject hexagon)
    {
        int row = hexagon.GetComponent<Hexagon>().Row;
        int col = hexagon.GetComponent<Hexagon>().Col;

        GameObject[] temp = new GameObject[]
        {
            GetHexagon(col % 2 == 0 ? row - 1 : row, col + 1),
            GetHexagon(col % 2 == 0 ? row : row + 1, col + 1),
            GetHexagon(row + 1, col),
            GetHexagon(col % 2 == 0  ? row : row + 1, col - 1),
            GetHexagon(col % 2 == 0  ? row - 1 : row, col - 1),
            GetHexagon(row - 1, col),
            GetHexagon(col % 2 == 0 ? row - 1 : row, col + 1),
        };

        return temp;
    }

    /// <summary>
    /// Varolan altıgenleri silip oyunu yeniden başlatır.
    /// </summary>
    /// <returns></returns>
    public IEnumerator DestroyAllAndSetup()
    {
        menuController.fadeInLoadingCanvas();
        menuController.fadeOutGameOverCanvas();
        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < columnSize; j++)
            {
                Destroy(hexagons[i,j]);
            }
        }

        yield return new WaitForSeconds(Constants.WAIT_TIME);
        StartCoroutine(SetupGame());
    }

    public void RestartGame()
    {
        StartCoroutine(DestroyAllAndSetup());
    }
}
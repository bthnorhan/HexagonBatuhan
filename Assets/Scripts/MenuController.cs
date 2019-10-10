using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Doozy.Engine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    //Puan yazısı
    public TextMeshProUGUI pointText;

    //Hareket sayısı yazısı
    public TextMeshProUGUI movementText;

    //Yükleniyor görseli
    public UICanvas loadingCanvas;
    
    //Yükleniyor indikatörü
    public UIView loadingView;
    private RectTransform loadingViewRect;
    
    public UICanvas gameOverCanvas;
    public TextMeshProUGUI gameOverPoint;
    public TextMeshProUGUI gameOverMovement;
    
    // İndikatöörün dönüş hızı
    private float rotateSpeed = 200f;
    private void Start()
    {
        loadingViewRect = loadingView.GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        // Eğer yükleme görseli açıksa indikatörün dönmesini sağlar.
        if (loadingCanvas.IsActive())
        {
            loadingViewRect.Rotate(0f, 0f, rotateSpeed * Time.deltaTime * -1);
        }
    }

    /// <summary>
    /// Puanı günceller.
    /// point -> Güncellenecek yazı 
    /// </summary>
    /// <param name="point"></param>
    public void setPointText(string point)
    {
        pointText.text = point;
    }

    /// <summary>
    /// Yapılmış hareket sayısını günceller.
    /// movement -> Güncellenecek yazı 
    /// </summary>
    /// <param name="movement"></param>
    public void setMovementText(string movement)
    {
        movementText.text = movement;
    }

    /// <summary>
    /// Sahne değiştirir.
    /// sceneIndex -> Değiştirilecek sahnenin build sırasındaki indeksi
    /// </summary>
    /// <param name="sceneIndex"></param>
    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Yükleniyor görselini devre dışı bırakır.
    /// </summary>
    public void fadeOutLoadingCanvas()
    {
        loadingCanvas.transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// Yükleniyor görselini aktif hale getirir.
    /// </summary>
    public void fadeInLoadingCanvas()
    {
        loadingCanvas.transform.gameObject.SetActive(true);
    }

    /// <summary>
    /// Oyun bitti görselini devre dışı bırakır.
    /// </summary>
    public void fadeOutGameOverCanvas()
    {
        gameOverCanvas.transform.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Oyun bitti görselini aktif hale getirir.
    /// </summary>
    public void fadeInGameOverCanvas()
    {
        gameOverCanvas.transform.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Oyun bitti ekranındaki puan yazısını yazdırır.
    /// point -> puan yazısı
    /// </summary>
    /// <param name="point"></param>
    public void setGameOverPointText(String point)
    {
        gameOverPoint.SetText(point);
    }

    /// <summary>
    /// Oyun bitti ekranındaki harektet yazısını yazdırır.
    /// movement -> hareket yazısı
    /// </summary>
    public void setGameOverMovementText(String movement)
    {
        gameOverMovement.SetText(movement);
    }

    /// <summary>
    /// Oyunu yeniden başlatır.
    /// </summary>
    public void restartGame()
    {
        GetComponent<GameController>().RestartGame();
    }
}

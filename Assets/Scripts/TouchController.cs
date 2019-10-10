using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchController : MonoBehaviour
{
    private GameController gameController;

    private Vector2 touchStarted, touchEnded;

    //Aynı yere dokunmadığını anlamak için gerekli eşik değeri.
    public float touchThreshold = 50f;

    private void Start()
    {
        gameController = GetComponent<GameController>();
    }

    /// <summary>
    /// Ekranın ilk dokunulan ve son kaldırıldığı noktaların koordinatlarını karşılaştırır.
    /// </summary>
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            Vector3 touchIn3D = Camera.main.ScreenToWorldPoint(t.position);
            Vector2 touchIn2D = new Vector2(touchIn3D.x, touchIn3D.y);
            RaycastHit2D hit = Physics2D.Raycast(touchIn2D, Vector2.zero);

            if (t.phase == TouchPhase.Began)
            {
                touchStarted = t.position;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                touchEnded = t.position;

                float diffX = touchStarted.x - touchEnded.x;
                float diffY = touchStarted.y - touchEnded.y;

                if (Mathf.Abs(diffX) < touchThreshold && Mathf.Abs(diffY) < touchThreshold)
                {
                    //Seçme
                    if (hit.collider != null)
                    {
                        gameController.SetSelectedHexagon(hit.collider.gameObject);
                    }
                    else
                    {
                        gameController.SetSelectedHexagon(null);
                    }
                }
                else if (Mathf.Abs(diffX) > 0 && Mathf.Abs(diffY) < touchThreshold)
                {
                    //Sağa sola kaydırma
                    gameController.RotateSelectedHexagonsContainer((int)Mathf.Sign(diffX) < 0);

                }
                else
                {
                    //Yukarı aşağı kaydırma
                    gameController.RotateSelectedHexagonsContainer((int)Mathf.Sign(diffY) < 0);
                }
            }
        }
    }
}

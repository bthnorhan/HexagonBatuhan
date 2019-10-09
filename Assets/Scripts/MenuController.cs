using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public TextMeshProUGUI pointText;

    public TextMeshProUGUI movementText;


    public void setPointText(string point)
    {
        pointText.text = point;
    }

    public void setMovementText(string movement)
    {
        movementText.text = movement;
    }

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MaxScore : MonoBehaviour
{
    void Start()
    {
        int score = GameManager.instance.GetHiScore();
        gameObject.GetComponent<Text>().text = score.ToString();
    }
}

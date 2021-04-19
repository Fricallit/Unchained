using UnityEngine;
using UnityEngine.UI;

public class MaxLvl : MonoBehaviour
{
    void Start()
    {
        int lvl = GameManager.instance.GetHighestLevel();
        gameObject.GetComponent<Text>().text = lvl.ToString();
    }
}

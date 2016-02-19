using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuFunctions : MonoBehaviour {

    public static int hiScore;

	public void StartTheGame()
    {
        SceneManager.LoadScene("main");
    }

    void OnLevelWasLoaded(int level)
    {
        if(level == 0)
        {
            Text HiScore = GameObject.FindWithTag("HiScore").GetComponent<Text>();
            HiScore.text = "HI SCORE\n" + hiScore.ToString();
        }
    }
}

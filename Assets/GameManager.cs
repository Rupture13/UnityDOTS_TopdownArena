using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Singleton for convenience
    public static GameManager main;

    private int playerScore;

    [SerializeField]
    private GameObject gameOverPanel = default;

    [SerializeField]
    private UnityString playerScored = default;

    [SerializeField]
    private UnityString playerHealthUpdated = default;
    [SerializeField]
    private UnityFloatEvent playerHealthUpdated2 = default;

    private void Awake()
    {
        //Singleton initialisation
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }
        main = this;

        playerScore = 0;
    }

    public void IncreasePlayerScore(int amount)
    {
        playerScore += amount;
        playerScored.Invoke(playerScore.ToString());
    }

    public void UpdatePlayerHealth(int newHealth)
    {
        playerHealthUpdated.Invoke(newHealth.ToString());
        playerHealthUpdated2.Invoke(newHealth / 10f);

        if (newHealth <= 0)
        {
            StartCoroutine(RevealUI(0.75f));
        }
    }

    public void EndGame()
    {
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    private IEnumerator RevealUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameOverPanel.SetActive(true);
    }
}

[System.Serializable]
public class UnityString : UnityEvent<string>
{
}

[System.Serializable]
public class UnityFloatEvent : UnityEvent<float>
{
}

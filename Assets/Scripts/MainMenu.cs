using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button proceedButton;

    void Start()
    {
        proceedButton.onClick.AddListener(ProceedToDifficulty);
    }

    void ProceedToDifficulty()
    {
        Debug.Log("Proceeding to Difficulty Selection...");
        SceneManager.LoadScene("DiffSelectScene");
    }
}
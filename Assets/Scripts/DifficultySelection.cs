using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DifficultySelection : MonoBehaviour
{
    [Header("UI References")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button proceedButton;
    public TextMeshProUGUI selectedDifficultyText;

    private GameManager.Difficulty? selectedDifficulty = null;

    void Start()
    {
        if (easyButton == null)
        {
            Debug.LogError("DifficultySelection: Easy Button NOT assigned in Inspector!");
            return;
        }
        if (normalButton == null)
        {
            Debug.LogError("DifficultySelection: Normal Button NOT assigned in Inspector!");
            return;
        }
        if (hardButton == null)
        {
            Debug.LogError("DifficultySelection: Hard Button NOT assigned in Inspector!");
            return;
        }
        if (proceedButton == null)
        {
            Debug.LogError("DifficultySelection: Proceed Button NOT assigned in Inspector!");
            return;
        }
        if (selectedDifficultyText == null)
        {
            Debug.LogError("DifficultySelection: Selected Difficulty Text NOT assigned in Inspector!");
            return;
        }
        
        GameManager.FindOrCreate();
        
        easyButton.onClick.AddListener(() => SelectDifficulty(GameManager.Difficulty.Easy));
        normalButton.onClick.AddListener(() => SelectDifficulty(GameManager.Difficulty.Normal));
        hardButton.onClick.AddListener(() => SelectDifficulty(GameManager.Difficulty.Hard));
        proceedButton.onClick.AddListener(ProceedToHatSelection);

        proceedButton.interactable = false;
        selectedDifficultyText.text = "Select a difficulty";
    }

    void SelectDifficulty(GameManager.Difficulty difficulty)
    {
        selectedDifficulty = difficulty;
        GameManager.Instance.SelectedDifficulty = difficulty;

        Debug.Log($"Selected difficulty: {difficulty}");

        // Update UI
        selectedDifficultyText.text = $"Selected: {difficulty}";
        proceedButton.interactable = true;

        // Visual feedback - highlight selected button
        ResetButtonColors();
        switch (difficulty)
        {
            case GameManager.Difficulty.Easy:
                easyButton.GetComponent<Image>().color = Color.green;
                break;
            case GameManager.Difficulty.Normal:
                normalButton.GetComponent<Image>().color = Color.yellow;
                break;
            case GameManager.Difficulty.Hard:
                hardButton.GetComponent<Image>().color = Color.red;
                break;
        }
    }

    void ResetButtonColors()
    {
        easyButton.GetComponent<Image>().color = Color.white;
        normalButton.GetComponent<Image>().color = Color.white;
        hardButton.GetComponent<Image>().color = Color.white;
    }

    void ProceedToHatSelection()
    {
        if (selectedDifficulty.HasValue)
        {
            SceneManager.LoadScene("HatSelectScene");
        }
    }
}

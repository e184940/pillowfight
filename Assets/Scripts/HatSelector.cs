using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HatSelector : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private Button[] hatButtons;

    [SerializeField] private Button startButton;
    [SerializeField] private RawImage previewImage; // Viser 3D preview

    [Header("Hat Settings")] [SerializeField]
    private GameObject[] hatPrefabs; // Samme som HatManager

    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Preview Camera")] [SerializeField]
    private Camera previewCamera;

    [SerializeField] private Transform previewSpawnPoint;
    [SerializeField] private float rotationSpeed = 50f;
    private GameObject currentPreviewHat;
    private readonly Color normalColor = Color.white;
    private readonly Color selectedColor = new(0.3f, 1f, 0.3f);

    private int selectedHatIndex;

    private void Start()
    {
        SetupPreviewCamera();
        SetupButtons();
        SelectHat(0);
    }

    private void Update()
    {
        // Roter preview-hatten
        if (currentPreviewHat != null) currentPreviewHat.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        // Cleanup preview
        if (currentPreviewHat != null)
            Destroy(currentPreviewHat);
    }

    private void SetupPreviewCamera()
    {
        if (previewCamera != null && previewImage != null)
        {
            // Lag RenderTexture for preview
            var renderTexture = new RenderTexture(512, 512, 16);
            previewCamera.targetTexture = renderTexture;
            previewImage.texture = renderTexture;
        }
    }

    private void SetupButtons()
    {
        for (var i = 0; i < hatButtons.Length; i++)
        {
            var index = i;
            hatButtons[i].onClick.AddListener(() => SelectHat(index));
        }

        startButton.onClick.AddListener(StartGame);
    }

    private void SelectHat(int hatIndex)
    {
        if (hatIndex < 0 || hatIndex >= hatButtons.Length)
            return;

        selectedHatIndex = hatIndex;
        UpdateButtonVisuals();
        UpdatePreview(hatIndex);

        Debug.Log($"Selected hat: {hatIndex}");
    }

    private void UpdatePreview(int hatIndex)
    {
        // Fjern gammel preview
        if (currentPreviewHat != null)
            Destroy(currentPreviewHat);

        // Valider index
        if (hatIndex < 0 || hatIndex >= hatPrefabs.Length || hatPrefabs[hatIndex] == null)
            return;

        // Spawn ny preview
        currentPreviewHat = Instantiate(hatPrefabs[hatIndex], previewSpawnPoint);
        currentPreviewHat.transform.localPosition = Vector3.zero;
        currentPreviewHat.transform.localRotation = Quaternion.identity;

        // Fjern physics/colliders fra preview
        var colliders = currentPreviewHat.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
            col.enabled = false;
    }

    private void UpdateButtonVisuals()
    {
        for (var i = 0; i < hatButtons.Length; i++)
        {
            var buttonImage = hatButtons[i].GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = i == selectedHatIndex ? selectedColor : normalColor;

            var buttonText = hatButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.fontSize = i == selectedHatIndex ? 28 : 24;
        }
    }

    private void StartGame()
    {
        PlayerPrefs.SetInt("SelectedHat", selectedHatIndex);
        PlayerPrefs.Save();

        Debug.Log($"Starting game with hat: {selectedHatIndex}");
        SceneManager.LoadScene(gameSceneName);
    }
}
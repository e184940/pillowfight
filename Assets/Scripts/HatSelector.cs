using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HatSelector : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private Button[] hatButtons;

    [SerializeField] private Button startButton;
    [SerializeField] private RawImage previewImage;

    [Header("Hat Settings")] [SerializeField]
    private GameObject[] hatPrefabs;

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
        if (currentPreviewHat != null) 
            currentPreviewHat.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (currentPreviewHat != null)
            Destroy(currentPreviewHat);
    }

    private void SetupPreviewCamera()
    {
        if (previewCamera != null && previewImage != null)
        {
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.depth = -100;
            previewCamera.enabled = true;
            
            Light[] lights = previewCamera.GetComponentsInChildren<Light>();
            if (lights.Length == 0)
            {
                GameObject lightObj = new GameObject("PreviewLight");
                lightObj.transform.SetParent(previewCamera.transform);
                lightObj.transform.localPosition = new Vector3(2, 3, 0);
                lightObj.transform.localRotation = Quaternion.Euler(50, -30, 0);
                
                Light previewLight = lightObj.AddComponent<Light>();
                previewLight.type = LightType.Directional;
                previewLight.intensity = 1.5f;
            }
            
            var renderTexture = new RenderTexture(512, 512, 24);
            renderTexture.Create();
            previewCamera.targetTexture = renderTexture;
            previewImage.texture = renderTexture;
        }
        else
        {
            if (previewCamera == null)
                Debug.LogError("Preview Camera NOT assigned!");
            if (previewImage == null)
                Debug.LogError("Preview Image NOT assigned!");
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
    }

    private void UpdatePreview(int hatIndex)
    {
        if (currentPreviewHat != null)
            Destroy(currentPreviewHat);

        if (hatIndex < 0 || hatIndex >= hatPrefabs.Length || hatPrefabs[hatIndex] == null)
        {
            Debug.LogWarning($"HatSelector: Invalid hat prefab at index {hatIndex}");
            return;
        }

        currentPreviewHat = Instantiate(hatPrefabs[hatIndex], previewSpawnPoint);
        currentPreviewHat.transform.localPosition = Vector3.zero;
        currentPreviewHat.transform.localRotation = Quaternion.identity;

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
        var gameManager = GameManager.FindOrCreate();
        gameManager.SelectedHatIndex = selectedHatIndex;
        
        PlayerPrefs.SetInt("SelectedHat", selectedHatIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
}
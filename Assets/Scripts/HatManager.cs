using UnityEngine;

public class HatManager : MonoBehaviour
{
    [Header("Hat Setup")] [SerializeField] private Transform hatSocket; // Parent for hatten (på hodet)

    [SerializeField] private GameObject[] hatPrefabs; // SAMME prefabs som HatSelector!

    private GameObject currentHat;

    private void Start()
    {
        var selectedHat = PlayerPrefs.GetInt("SelectedHat", 0);
        Debug.Log($"HatManager: Loading hat index {selectedHat}");
        EquipHat(selectedHat);
    }

    private void LateUpdate()
    {
        // Frys hatten på plass (forhindrer bugging)
        if (currentHat != null)
        {
            currentHat.transform.localPosition = Vector3.zero;
            currentHat.transform.localRotation = Quaternion.identity;
        }
    }

    private void EquipHat(int hatIndex)
    {
        // Fjern gammel hatt
        if (currentHat != null)
            Destroy(currentHat);

        // Valider index
        if (hatIndex < 0 || hatIndex >= hatPrefabs.Length)
        {
            Debug.LogError($"HatManager: Invalid hat index {hatIndex}! (Array length: {hatPrefabs.Length})");
            return;
        }

        if (hatPrefabs[hatIndex] == null)
        {
            Debug.LogError($"HatManager: Hat prefab at index {hatIndex} is null!");
            return;
        }

        // Spawn hatt
        var hatPrefab = hatPrefabs[hatIndex];
        currentHat = Instantiate(hatPrefab, hatSocket);

        // Nullstill transform
        currentHat.transform.localPosition = Vector3.zero;
        currentHat.transform.localRotation = Quaternion.identity;
        currentHat.transform.localScale = Vector3.one;

        Debug.Log($"HatManager: Equipped hat '{hatPrefab.name}' at index {hatIndex}");
    }
}
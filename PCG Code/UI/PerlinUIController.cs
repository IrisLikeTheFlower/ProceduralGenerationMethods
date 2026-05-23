using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class PerlinUIController : MonoBehaviour
{
    [Header("Canvas")]
    public RawImage displayImage;
    public Button backButton;
    public Button saveButton;
    public Button generateButton;

    [Header("Parameters")]
    public InputField seedInput;
    public InputField widthInput;
    public InputField heightInput;
    public InputField xMinInput, xMaxInput, yMinInput, yMaxInput;
    public Toggle useOctavesToggle;
    public Button addOctaveButton;

    [Header("Octaves List")]
    public Transform octaveContainer;
    public GameObject octaveEntryPrefab;

    private List<PerlinNoise.Octave> octaves = new List<PerlinNoise.Octave>();
    private Texture2D currentTexture;
    private MainMenuController mainMenuController;

    void Start()
    {
        // Шукаємо MainMenuController на всій сцені (включно з неактивними об'єктами)
        mainMenuController = FindObjectOfType<MainMenuController>(true);
        if (mainMenuController == null)
            Debug.LogError("MainMenuController not found! Please add it to a persistent GameObject.");

        // Прив'язка кнопок
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveImage);
        if (generateButton != null)
            generateButton.onClick.AddListener(Generate);
        if (addOctaveButton != null)
            addOctaveButton.onClick.AddListener(AddDefaultOctave);

        // Початкові значення для полів, якщо вони порожні
        SetDefaultInputValues();

        // Додаємо базову октаву
        octaves.Clear();
        AddOctave(0.01f, 1f);
        RefreshOctaveUI();
    }

    void SetDefaultInputValues()
    {
        if (string.IsNullOrEmpty(seedInput.text)) seedInput.text = "0";
        if (string.IsNullOrEmpty(widthInput.text)) widthInput.text = "512";
        if (string.IsNullOrEmpty(heightInput.text)) heightInput.text = "512";
        if (string.IsNullOrEmpty(xMinInput.text)) xMinInput.text = "0";
        if (string.IsNullOrEmpty(xMaxInput.text)) xMaxInput.text = "5";
        if (string.IsNullOrEmpty(yMinInput.text)) yMinInput.text = "0";
        if (string.IsNullOrEmpty(yMaxInput.text)) yMaxInput.text = "5";
    }

    void OnBackClicked()
    {
        if (mainMenuController != null)
            mainMenuController.BackToMainMenu(gameObject);
        else
            Debug.LogError("Cannot go back: MainMenuController missing");
    }

    void AddDefaultOctave()
    {
        AddOctave(0.02f, 0.5f);
        RefreshOctaveUI();
    }

    void AddOctave(float scale, float amplitude)
    {
        octaves.Add(new PerlinNoise.Octave(scale, amplitude));
    }

    void RefreshOctaveUI()
    {
        if (octaveContainer == null) return;
        foreach (Transform child in octaveContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < octaves.Count; i++)
        {
            int index = i;
            GameObject entry = Instantiate(octaveEntryPrefab, octaveContainer);
            InputField[] inputs = entry.GetComponentsInChildren<InputField>();
            if (inputs.Length >= 2)
            {
                InputField scaleField = inputs[0];
                InputField ampField = inputs[1];
                scaleField.text = octaves[index].scale.ToString(CultureInfo.InvariantCulture);
                ampField.text = octaves[index].amplitude.ToString(CultureInfo.InvariantCulture);

                scaleField.onEndEdit.AddListener((val) => {
                    if (float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                        octaves[index].scale = f;
                });
                ampField.onEndEdit.AddListener((val) => {
                    if (float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                        octaves[index].amplitude = f;
                });
            }
            Button delBtn = entry.GetComponentInChildren<Button>();
            if (delBtn != null)
                delBtn.onClick.AddListener(() => {
                    octaves.RemoveAt(index);
                    RefreshOctaveUI();
                });
        }
    }

    void Generate()
    {
        // Безпечний парсинг
        float ParseFloat(string input, float defaultValue)
        {
            if (string.IsNullOrEmpty(input)) return defaultValue;
            if (float.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
                return result;
            return defaultValue;
        }

        int ParseInt(string input, int defaultValue)
        {
            if (string.IsNullOrEmpty(input)) return defaultValue;
            if (int.TryParse(input, out int result))
                return result;
            return defaultValue;
        }

        int seed = ParseInt(seedInput.text, 0);
        int width = ParseInt(widthInput.text, 512);
        int height = ParseInt(heightInput.text, 512);
        float xMin = ParseFloat(xMinInput.text, 0f);
        float xMax = ParseFloat(xMaxInput.text, 5f);
        float yMin = ParseFloat(yMinInput.text, 0f);
        float yMax = ParseFloat(yMaxInput.text, 5f);
        bool useOctaves = useOctavesToggle.isOn;

        if (width <= 0) width = 512;
        if (height <= 0) height = 512;

        // Якщо список октав порожній, додаємо базову
        if (octaves.Count == 0)
            octaves.Add(new PerlinNoise.Octave(0.01f, 1f));

        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(width, height, xMin, xMax, yMin, yMax, octaves, useOctaves, seed);
        currentTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float val = noiseMap[x, y];
                pixels[y * width + x] = new Color(val, val, val, 1);
            }
        }
        currentTexture.SetPixels(pixels);
        currentTexture.Apply();
        if (displayImage != null)
            displayImage.texture = currentTexture;
    }

    void SaveImage()
    {
        if (currentTexture != null)
            SaveHelper.SaveTextureAsPNG(currentTexture, "PerlinNoise");
    }
}
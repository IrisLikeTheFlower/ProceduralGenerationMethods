using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class PoissonUIController : MonoBehaviour
{
    [Header("Canvas")]
    public RawImage displayImage;
    public Button backButton;
    public Button saveButton;
    public Button generateButton;

    [Header("Parameters")]
    public InputField seedInput;
    public InputField imageWidthInput;
    public InputField imageHeightInput;
    public InputField fieldWidthInput;
    public InputField fieldHeightInput;
    public InputField radiusMinInput;
    public InputField radiusMaxInput;
    public InputField kInput;

    private Texture2D currentTexture;
    private MainMenuController mainMenuController;

    void Start()
    {
        mainMenuController = FindObjectOfType<MainMenuController>(true);
        if (mainMenuController == null)
            Debug.LogError("MainMenuController not found! Please add it to a persistent GameObject.");

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveImage);
        if (generateButton != null)
            generateButton.onClick.AddListener(Generate);

        SetDefaultInputValues();
    }

    void SetDefaultInputValues()
    {
        if (string.IsNullOrEmpty(seedInput.text)) seedInput.text = "0";
        if (string.IsNullOrEmpty(imageWidthInput.text)) imageWidthInput.text = "512";
        if (string.IsNullOrEmpty(imageHeightInput.text)) imageHeightInput.text = "512";
        if (string.IsNullOrEmpty(fieldWidthInput.text)) fieldWidthInput.text = "10";
        if (string.IsNullOrEmpty(fieldHeightInput.text)) fieldHeightInput.text = "10";
        if (string.IsNullOrEmpty(radiusMinInput.text)) radiusMinInput.text = "0.5";
        if (string.IsNullOrEmpty(radiusMaxInput.text)) radiusMaxInput.text = "1.0";
        if (string.IsNullOrEmpty(kInput.text)) kInput.text = "30";
    }

    void OnBackClicked()
    {
        if (mainMenuController != null)
            mainMenuController.BackToMainMenu(gameObject);
        else
            Debug.LogError("Cannot go back: MainMenuController missing");
    }

    void Generate()
    {
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
        int texW = ParseInt(imageWidthInput.text, 512);
        int texH = ParseInt(imageHeightInput.text, 512);
        float fieldW = ParseFloat(fieldWidthInput.text, 10f);
        float fieldH = ParseFloat(fieldHeightInput.text, 10f);
        float rMin = ParseFloat(radiusMinInput.text, 0.5f);
        float rMax = ParseFloat(radiusMaxInput.text, 1f);
        int k = ParseInt(kInput.text, 30);

        if (texW <= 0) texW = 512;
        if (texH <= 0) texH = 512;
        if (fieldW <= 0) fieldW = 10;
        if (fieldH <= 0) fieldH = 10;
        if (rMin <= 0) rMin = 0.1f;
        if (rMax < rMin) rMax = rMin * 2;

        List<Vector2> points = PoissonDiscSampling.GeneratePoints(fieldW, fieldH, rMin, rMax, k, seed);

        currentTexture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        Color bg = Color.white;
        Color pointColor = Color.black;
        for (int y = 0; y < texH; y++)
            for (int x = 0; x < texW; x++)
                currentTexture.SetPixel(x, y, bg);

        foreach (var p in points)
        {
            int px = Mathf.RoundToInt((p.x / fieldW) * (texW - 1));
            int py = Mathf.RoundToInt((p.y / fieldH) * (texH - 1));
            if (px >= 0 && px < texW && py >= 0 && py < texH)
                currentTexture.SetPixel(px, py, pointColor);
        }
        currentTexture.Apply();
        if (displayImage != null)
            displayImage.texture = currentTexture;
    }

    void SaveImage()
    {
        if (currentTexture != null)
            SaveHelper.SaveTextureAsPNG(currentTexture, "PoissonDisc");
    }
}
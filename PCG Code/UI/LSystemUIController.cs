using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class LSystemUIController : MonoBehaviour
{
    [Header("Canvas")]
    public RawImage displayImage;
    public Button backButton;
    public Button saveButton;
    public Button generateButton;

    [Header("Parameters")]
    public InputField imageWidthInput;
    public InputField imageHeightInput;
    public InputField axiomInput;
    public InputField iterationsInput;
    public InputField initialStepInput;
    public InputField initialAngleStepInput;
    public InputField stepDecayInput;
    public InputField angleDecayInput;

    [Header("Rules List")]
    public Transform rulesContainer;
    public GameObject ruleEntryPrefab;
    public Button addRuleButton;
    public Dropdown presetDropdown;

    private List<LSystem.Rule> rules = new List<LSystem.Rule>();
    private Texture2D currentTexture;
    private MainMenuController mainMenuController;

    // Вбудовані пресети – без прив'язки до інспектора
    private List<Preset> presets = new List<Preset>();

    [System.Serializable]
    public class Preset
    {
        public string name;
        public string axiom;
        public List<LSystem.Rule> rules;
    }

    void Start()
    {
        mainMenuController = FindObjectOfType<MainMenuController>(true);
        if (mainMenuController == null)
            Debug.LogError("MainMenuController not found!");

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveImage);
        if (generateButton != null)
            generateButton.onClick.AddListener(Generate);
        if (addRuleButton != null)
            addRuleButton.onClick.AddListener(AddDefaultRule);
        if (presetDropdown != null)
            presetDropdown.onValueChanged.AddListener(OnPresetSelected);

        SetDefaultInputValues();

        // Початкове правило, якщо список пустий
        if (rules.Count == 0)
            AddRule('F', "F[+F][-F]");
        RefreshRulesUI();

        // Ініціалізація пресетів
        InitPresets();
        RefreshPresetDropdown();
    }

    void InitPresets()
    {
        presets.Clear();

        Preset tree = new Preset()
        {
            name = "Tree",
            axiom = "A",
            rules = new List<LSystem.Rule> { new LSystem.Rule('A', "F[+A][-A]") }
        };
        Preset koch = new Preset()
        {
            name = "Koch Snowflake",
            axiom = "F--F--F",
            rules = new List<LSystem.Rule> { new LSystem.Rule('F', "F+F--F+F") }
        };
        Preset dragon = new Preset()
        {
            name = "Dragon Curve",
            axiom = "FX",
            rules = new List<LSystem.Rule> { new LSystem.Rule('X', "X+YF+"), new LSystem.Rule('Y', "-FX-Y") }
        };
        presets.Add(tree);
        presets.Add(koch);
        presets.Add(dragon);
    }

    void RefreshPresetDropdown()
    {
        if (presetDropdown == null) return;
        presetDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (var p in presets)
        {
            if (p != null && !string.IsNullOrEmpty(p.name))
                options.Add(p.name);
            else
                options.Add("Unknown");
        }
        presetDropdown.AddOptions(options);
        if (options.Count > 0)
        {
            presetDropdown.value = 0;
            OnPresetSelected(0);
        }
    }

    void OnPresetSelected(int index)
    {
        if (index < 0 || index >= presets.Count) return;
        Preset p = presets[index];
        if (p == null) return;

        if (!string.IsNullOrEmpty(p.axiom))
            axiomInput.text = p.axiom;

        rules.Clear();
        if (p.rules != null)
        {
            foreach (var r in p.rules)
            {
                if (r != null)
                    rules.Add(new LSystem.Rule(r.symbol, r.replacement));
            }
        }
        RefreshRulesUI();
    }

    void SetDefaultInputValues()
    {
        if (string.IsNullOrEmpty(imageWidthInput.text)) imageWidthInput.text = "800";
        if (string.IsNullOrEmpty(imageHeightInput.text)) imageHeightInput.text = "600";
        if (string.IsNullOrEmpty(axiomInput.text)) axiomInput.text = "F";
        if (string.IsNullOrEmpty(iterationsInput.text)) iterationsInput.text = "4";
        if (string.IsNullOrEmpty(initialStepInput.text)) initialStepInput.text = "10";
        if (string.IsNullOrEmpty(initialAngleStepInput.text)) initialAngleStepInput.text = "25";
        if (string.IsNullOrEmpty(stepDecayInput.text)) stepDecayInput.text = "0.9";
        if (string.IsNullOrEmpty(angleDecayInput.text)) angleDecayInput.text = "0.9";
    }

    void OnBackClicked()
    {
        if (mainMenuController != null)
            mainMenuController.BackToMainMenu(gameObject);
        else
            Debug.LogError("Cannot go back: MainMenuController missing");
    }

    void AddDefaultRule()
    {
        AddRule('X', "F+X");
        RefreshRulesUI();
    }

    void AddRule(char sym, string repl)
    {
        rules.Add(new LSystem.Rule(sym, repl));
    }

    void RefreshRulesUI()
    {
        if (rulesContainer == null) return;
        foreach (Transform child in rulesContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < rules.Count; i++)
        {
            int idx = i;
            GameObject entry = Instantiate(ruleEntryPrefab, rulesContainer);
            InputField[] inputs = entry.GetComponentsInChildren<InputField>();
            if (inputs.Length >= 2)
            {
                InputField symField = inputs[0];
                InputField replField = inputs[1];
                symField.text = rules[idx].symbol.ToString();
                replField.text = rules[idx].replacement;

                symField.onEndEdit.AddListener((val) => {
                    if (val.Length > 0) rules[idx].symbol = val[0];
                });
                replField.onEndEdit.AddListener((val) => {
                    rules[idx].replacement = val;
                });
            }
            Button delBtn = entry.GetComponentInChildren<Button>();
            if (delBtn != null)
                delBtn.onClick.AddListener(() => {
                    rules.RemoveAt(idx);
                    RefreshRulesUI();
                });
        }
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

        int width = ParseInt(imageWidthInput.text, 800);
        int height = ParseInt(imageHeightInput.text, 600);
        string axiom = axiomInput.text;
        int iterations = ParseInt(iterationsInput.text, 4);
        float initStep = ParseFloat(initialStepInput.text, 10f);
        float initAngleStep = ParseFloat(initialAngleStepInput.text, 25f);
        float stepDecay = ParseFloat(stepDecayInput.text, 0.9f);
        float angleDecay = ParseFloat(angleDecayInput.text, 0.9f);

        if (width <= 0) width = 800;
        if (height <= 0) height = 600;
        if (iterations < 0) iterations = 0;

        string commands = LSystem.GenerateString(axiom, rules, iterations);
        Debug.Log($"Generated commands length: {commands.Length}");

        List<LSystem.Segment> segments = LSystem.Interpret(commands, initStep, initAngleStep, stepDecay, angleDecay);
        if (segments == null || segments.Count == 0)
        {
            Debug.LogWarning("No segments generated. Check step length, angle, and that commands contain 'F'.");
            return;
        }

        currentTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color bg = Color.white;
        Color lineColor = Color.black;
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                currentTexture.SetPixel(i, j, bg);

        Vector2 min = segments[0].start;
        Vector2 max = segments[0].start;
        foreach (var seg in segments)
        {
            min = Vector2.Min(min, seg.start);
            min = Vector2.Min(min, seg.end);
            max = Vector2.Max(max, seg.start);
            max = Vector2.Max(max, seg.end);
        }
        Vector2 size = max - min;
        if (size.x == 0 || size.y == 0)
        {
            Debug.LogWarning("Segments have zero size. Check coordinates.");
            return;
        }

        float scaleX = (width - 1) / size.x;
        float scaleY = (height - 1) / size.y;
        float offsetX = -min.x;
        float offsetY = -min.y;

        foreach (var seg in segments)
        {
            Vector2 p1 = new Vector2((seg.start.x + offsetX) * scaleX, (seg.start.y + offsetY) * scaleY);
            Vector2 p2 = new Vector2((seg.end.x + offsetX) * scaleX, (seg.end.y + offsetY) * scaleY);
            DrawLine(currentTexture, (int)p1.x, (int)p1.y, (int)p2.x, (int)p2.y, lineColor);
        }
        currentTexture.Apply();
        if (displayImage != null)
            displayImage.texture = currentTexture;
    }

    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            if (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height)
                tex.SetPixel(x0, y0, col);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    void SaveImage()
    {
        if (currentTexture != null)
            SaveHelper.SaveTextureAsPNG(currentTexture, "LSystem");
    }
}
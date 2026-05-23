using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject perlinPanel;
    public GameObject poissonPanel;
    public GameObject lSystemPanel;

    [Header("Main Menu UI")]
    public InputField searchInput;
    public Transform cardContainer;
    public GameObject cardPrefab;
    public Button githubButton;
    public Button exitButton;

    [Header("Method Data")]
    public List<MethodInfo> methods = new List<MethodInfo>();

    [System.Serializable]
    public class MethodInfo
    {
        public string name;
        public Sprite previewImage;
        public GameObject targetPanel;
    }

    void Start()
    {
        PopulateCards();
        searchInput.onValueChanged.AddListener(FilterCards);
        githubButton.onClick.AddListener(OpenGitHub);
        exitButton.onClick.AddListener(ExitApplication);
    }

    void PopulateCards()
    {
        foreach (var method in methods)
        {
            GameObject card = Instantiate(cardPrefab, cardContainer);
            card.GetComponentInChildren<Text>().text = method.name;

            // Виправлено: знаходимо Image і встановлюємо спрайт без ?. в лівій частині
            Transform imageTransform = card.transform.Find("Image");
            if (imageTransform != null)
            {
                Image img = imageTransform.GetComponent<Image>();
                if (img != null && method.previewImage != null)
                    img.sprite = method.previewImage;
            }

            Button btn = card.GetComponent<Button>();
            btn.onClick.AddListener(() => OpenMethod(method.targetPanel));
        }
    }

    void FilterCards(string query)
    {
        query = query.ToLower();
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform card = cardContainer.GetChild(i);
            string name = card.GetComponentInChildren<Text>().text.ToLower();
            card.gameObject.SetActive(name.Contains(query));
        }
    }

    void OpenMethod(GameObject panel)
    {
        mainMenuPanel.SetActive(false);
        panel.SetActive(true);
    }

    void OpenGitHub()
    {
        Application.OpenURL("https://github.com/IrisLikeTheFlower/ProceduralGenerationMethods");
    }

    void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToMainMenu(GameObject currentPanel)
    {
        currentPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
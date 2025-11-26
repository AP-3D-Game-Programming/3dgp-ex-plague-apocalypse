using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardSelectionUI : MonoBehaviour
{
    public static CardSelectionUI Instance;

    [Header("UI References")]
    public GameObject panel;
    public Button[] cardButtons; // 3 buttons
    public TMP_Text[] cardTitles; // card name texts
    public TMP_Text[] cardDescriptions; // card description texts

    [HideInInspector] public bool cardChosen = false;
    private Card chosenCard;
    private List<Card> currentCards;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        panel.SetActive(false);
    }

    // Show 3 card options
    public void ShowOptions(List<Card> cards)
    {
        currentCards = cards;
        cardChosen = false;
        panel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < cards.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                cardTitles[i].text = cards[i].cardName;
                cardDescriptions[i].text = cards[i].description;

                int index = i; // capture index for the lambda
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => PickCard(index));
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void PickCard(int index)
    {
        chosenCard = currentCards[index];
        cardChosen = true;
        panel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Card GetChosenCard()
    {
        return chosenCard;
    }
}

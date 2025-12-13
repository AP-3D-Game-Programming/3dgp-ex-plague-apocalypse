using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CardSelectionUI : MonoBehaviour
{
    public static CardSelectionUI Instance;

    [Header("UI References")]
    public GameObject panel;
    public Button[] cardButtons;
    public TMP_Text[] cardTitles;
    public TMP_Text[] rarityTexts;
    public TMP_Text[] cardDescriptions;
    public Image[] backgrounds;
    public TMP_Text currentLuckText;
    // Make this public so we can check it, but modify it only internally
    public bool cardChosen { get; private set; }
    private Card chosenCard;
    private List<Card> currentCards;
    [Header("Reroll Settings")]
    public Button refreshButton;
    public TMP_Text refreshCostText;
    private int currentRefreshCost = 1000;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panel.SetActive(false);
        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnRefreshClicked);
        }
    }

    public void ShowOptions(List<Card> cards)
    {
        currentCards = cards;
        cardChosen = false; // Reset flag
        panel.SetActive(true);
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<FirstPersonLook>();
            if (controller != null) controller.enabled = false;
        }
        UpdateRefreshButtonUI();
        RoundManager rm = FindObjectOfType<RoundManager>();
        if (rm != null && currentLuckText != null)
        {
            currentLuckText.text = $" {rm.playerLuck:F1}";
        }

        // 1. Pause the Game
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < cards.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                cardButtons[i].interactable = true; // clickable
                cardTitles[i].text = cards[i].cardName;
                cardDescriptions[i].text = cards[i].description;
                if (rarityTexts != null && i < rarityTexts.Length)
                {
                    rarityTexts[i].text = cards[i].rarity.ToString();
                    rarityTexts[i].color = cards[i].GetRarityColor();
                }

                // --- ANIMATION SETUP ---
                if (backgrounds != null && i < backgrounds.Length)
                {
                    // Add components if missing
                    RainbowCard rainbow = backgrounds[i].GetComponent<RainbowCard>();
                    if (rainbow == null) rainbow = backgrounds[i].gameObject.AddComponent<RainbowCard>();

                    ExoticHoloGradient exotic = backgrounds[i].GetComponent<ExoticHoloGradient>();
                    if (exotic == null) exotic = backgrounds[i].gameObject.AddComponent<ExoticHoloGradient>();

                    // Configure Speeds
                    rainbow.speed = 0.2f;
                    exotic.speed = 0.5f;

                    // Disable both first to clear state
                    rainbow.enabled = false;
                    exotic.enabled = false;

                    // Logic based on rarity
                    if (cards[i].rarity == CardRarity.Mythical)
                    {
                        rainbow.enabled = true;
                    }
                    else if (cards[i].rarity == CardRarity.Exotic)
                    {
                        exotic.enabled = true;
                    }
                    else
                    {
                        // Standard card: Set static color
                        backgrounds[i].color = cards[i].GetRarityColor();
                    }
                }

                int index = i;
                cardButtons[i].onClick.RemoveAllListeners();
                cardButtons[i].onClick.AddListener(() => PickCard(index));
            }
            else
            {
                cardButtons[i].gameObject.SetActive(false);
            }
        }
    }
    public void OnRefreshClicked()
    {
        // Check if player has enough points
        if (PlayerStats.Instance != null && PlayerStats.Instance.points >= currentRefreshCost)
        {
            // Pay the cost
            PlayerStats.Instance.RemovePoints(currentRefreshCost);

            // Increase cost for next time THIS round
            currentRefreshCost += 1000;

            // Ask RoundManager to generate new cards
            RoundManager rm = FindObjectOfType<RoundManager>();
            if (rm != null)
            {
                rm.RerollCards();
            }


            UpdateRefreshButtonUI();
        }
        else
        {
            Debug.Log("Not enough points to refresh!");
        }
    }
    private void UpdateRefreshButtonUI()
    {
        if (refreshButton != null && refreshCostText != null)
        {
            refreshCostText.text = $"Reroll: {currentRefreshCost} points";
            if (PlayerStats.Instance != null)
            {
                bool canAfford = PlayerStats.Instance.points >= currentRefreshCost;
                refreshButton.interactable = canAfford;
            }
        }
    }
    public void UpdateCardOptions(List<Card> newCards)
    {


        currentCards = newCards;

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i < newCards.Count)
            {
                cardButtons[i].gameObject.SetActive(true);
                cardButtons[i].interactable = true;
                cardTitles[i].text = newCards[i].cardName;
                cardDescriptions[i].text = newCards[i].description;

                if (rarityTexts != null && i < rarityTexts.Length)
                {
                    rarityTexts[i].text = newCards[i].rarity.ToString();
                    rarityTexts[i].color = newCards[i].GetRarityColor();
                }

                if (backgrounds != null && i < backgrounds.Length)
                {
                    RainbowCard rainbow = backgrounds[i].GetComponent<RainbowCard>();
                    ExoticHoloGradient exotic = backgrounds[i].GetComponent<ExoticHoloGradient>();
                    if (rainbow) rainbow.enabled = false;
                    if (exotic) exotic.enabled = false;

                    if (newCards[i].rarity == CardRarity.Mythical && rainbow) rainbow.enabled = true;
                    else if (newCards[i].rarity == CardRarity.Exotic && exotic) exotic.enabled = true;
                    else backgrounds[i].color = newCards[i].GetRarityColor();
                }

                int index = i;
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
        // If we already picked a card, ignore this click!
        if (cardChosen) return;

        chosenCard = currentCards[index];
        cardChosen = true;

        // Disable buttons immediately to prevent visual double-clicks
        foreach (var btn in cardButtons) btn.interactable = false;

        panel.SetActive(false);

        Time.timeScale = 1f;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<FirstPersonLook>();
            if (controller != null) controller.enabled = true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public Card GetChosenCard()
    {
        return chosenCard;
    }
}
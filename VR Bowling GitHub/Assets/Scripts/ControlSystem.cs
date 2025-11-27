using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;

public class ControlSystem : MonoBehaviour
{
   [SerializeField] private GameObject controllableAlley;

   [SerializeField] private TextMeshProUGUI playerCountTextBox;
   [SerializeField] private TextMeshProUGUI botCountTextBox;
   [SerializeField] private TextMeshProUGUI botLevelTextBox;

   [SerializeField] private Button NormalButton;
   [SerializeField] private Button FunButton;

   [SerializeField] private Slider playerCountSlider;
   [SerializeField] private Slider botCountSlider;
   [SerializeField] private Slider botLevelSlider;

   [SerializeField] private int maxBots;
   [SerializeField] private int maxPlayers;
   
   private AlleyHandler alleyHandler;

   private int selectedPlayerCount = 0;
   private int selectedBotCount = 0;
   private BotVariables.BotLevel selectedBotLevel = BotVariables.BotLevel.Low;
   private GameVariables.GameModes selectedMode = GameVariables.GameModes.Normal;
   
   private void Awake()
   {
      alleyHandler = controllableAlley.GetComponent<AlleyHandler>();
      EventSystem.current.SetSelectedGameObject(NormalButton.gameObject);
   }

   public void OnGameModeNormalClicked()
   {
      EventSystem.current.SetSelectedGameObject(null);
      EventSystem.current.SetSelectedGameObject(NormalButton.gameObject);
      
      selectedMode = GameVariables.GameModes.Normal;
   }
   
   public void OnGameModeFunClicked()
   {
      EventSystem.current.SetSelectedGameObject(null);
      EventSystem.current.SetSelectedGameObject(FunButton.gameObject);
      
      selectedMode = GameVariables.GameModes.Fun;
   }

   public void OnPlayerCountChanged()
   {
      selectedPlayerCount = (int)playerCountSlider.value;
      playerCountTextBox.text = $"{playerCountSlider.value}/{maxPlayers}";
   }
   
   public void OnBotCountChanged()
   {
      selectedBotCount = (int) botCountSlider.value;
      botCountTextBox.text = $"{botCountSlider.value}/{maxBots}";
   }
   
   public void OnBotLevelChanged()
   {
      switch (botLevelSlider.value)
      {
         case 0:
            selectedBotLevel = BotVariables.BotLevel.Low;
            botLevelTextBox.text = "Low";
            break;
         case 1:
            selectedBotLevel = BotVariables.BotLevel.Mid;
            botLevelTextBox.text = "Mid";
            break;
         case 2:
            selectedBotLevel = BotVariables.BotLevel.High;
            botLevelTextBox.text = "High";
            break;
      }
   }


   public void OnStartGameClicked()
   {
      Debug.Log($"Alley: {alleyHandler.alleyIndex} starts new game with #players: {selectedPlayerCount}, #bots: {selectedBotCount}, level: {selectedBotLevel}, mode: {selectedMode}");
      
      alleyHandler.StartGame(selectedPlayerCount, selectedBotCount, selectedBotLevel, selectedMode);
   }
}

using System;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class SketchUIController : MonoBehaviour
{
    public static event Action<SceneUIState> OnUIStateChanged;
    
    private static SketchUIController instance;
    public static SketchUIController Instance
    {
        get => instance;
        private set => instance = value;
    }

    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField] 
    private SketchUIContextButton[] contextButtonTexts;
    private SketchVolumeOverrider overrider;
    private SketchLightningController lightningController;
    
    void Awake()
    {
        if(Instance == null) 
            Instance = this;
        else if(Instance != this)
            Destroy(gameObject);

        OnUIStateChanged += UpdateCanvasVisibility;
    }

    private void OnDestroy()
    {
        OnUIStateChanged -= UpdateCanvasVisibility;
        if(Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        if(overrider == null)
            overrider = FindObjectOfType<SketchVolumeOverrider>();
        if(lightningController == null)
            lightningController = FindObjectOfType<SketchLightningController>();
        
        ConfigureButtons();
        UpdateToCurrentState();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            ToggleUIState();
    }

    private void ConfigureButtons()
    {
        for (int i = 0; i < contextButtonTexts.Length; i++)
        {
            contextButtonTexts[i].SetText(overrider.GetPresetName(i));
        }
    }

    private void UpdateCanvasVisibility(SceneUIState state)
    {
        bool interaction = state == SceneUIState.Interaction;
        canvasGroup.interactable = interaction;
        canvasGroup.blocksRaycasts = interaction;
        canvasGroup.alpha = interaction ? 1 : 0;
    }

    public void OnContextButtonClicked(int i)
    {
        overrider.ApplyPreset(i);
    }

    public void OnLightingSpeedChanged(float value)
    {
        lightningController.Speed = value;
    }
    
    #region State Machine
    
    public enum SceneUIState { Interaction, Movement}

    private SceneUIState currentSceneUIState = SceneUIState.Interaction;
    
    private void ToggleUIState()
    {
        if(currentSceneUIState == SceneUIState.Interaction)
            currentSceneUIState = SceneUIState.Movement;
        else
            currentSceneUIState = SceneUIState.Interaction;
        
        UpdateToCurrentState();
    }

    private void UpdateToCurrentState()
    {
        bool isInteraction = currentSceneUIState == SceneUIState.Interaction;
        Cursor.visible =  isInteraction;
        Cursor.lockState = isInteraction ? CursorLockMode.None : CursorLockMode.Locked;
        OnUIStateChanged?.Invoke(currentSceneUIState);
    }
    
    #endregion
}

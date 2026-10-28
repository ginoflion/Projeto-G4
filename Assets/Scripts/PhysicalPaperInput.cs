using UnityEngine;
using UnityEngine.InputSystem;
using TMPro; 
using SUPERCharacter;
using System.Collections.Generic;

public class PhysicalPaperInput : MonoBehaviour, IUsable
{
    [Header("Posição de Visualização")]
    public Vector3 normalPosition = Vector3.zero;
    public Vector3 normalRotation = Vector3.zero;
    public Vector3 zoomedPosition = new Vector3(-0.65f, -0.8f, -0.6f); 
    public Vector3 zoomedRotation = new Vector3(-90f, 0f, 0f);

    [System.Serializable]
    public class Simbolo
    {
        public TextMeshPro writtenText;
        [HideInInspector] public string currentInput = "";
        public Collider symbolCollider; 
    }

    [Header("Os Símbolos (Palavras livres)")]
    public List<Simbolo> simbolos;
    public int maxCharactersPerWord = 10;
    
    [Header("Referências")]
    public Camera playerCamera; 
    public LayerMask paperLayer; 
    
    private int currentSymbolIndex = 0; 
    private bool isReading = false;
    private float readStartTime = 0f; 
    
    private SUPERCharacterAIO playerController;
    private InteractionSystem interactionSystem;

    void Start()
    {
        playerController = FindObjectsByType<SUPERCharacterAIO>(FindObjectsSortMode.None)[0];
        interactionSystem = FindObjectsByType<InteractionSystem>(FindObjectsSortMode.None)[0];

        foreach (var sim in simbolos)
        {
            if (sim.writtenText != null) sim.writtenText.text = "";
        }
    }
    
    public void Use()
    {
        if (!isReading)
        {
            EnterZoom();
        }
    }

    private void EnterZoom()
    {
        isReading = true;
        readStartTime = Time.time; 

        transform.localPosition = zoomedPosition;
        transform.localRotation = Quaternion.Euler(zoomedRotation);

        if (playerController != null) playerController.PausePlayer(PauseModes.FreezeInPlace);
        if (interactionSystem != null) interactionSystem.enabled = false;
        
        Keyboard.current.onTextInput += OnTextInput;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        SelectFirstAvailableSymbol();
    }

    private void ExitZoom()
    {
        isReading = false;

        transform.localPosition = normalPosition;
        transform.localRotation = Quaternion.Euler(normalRotation);

        if (playerController != null) playerController.UnpausePlayer();
        if (interactionSystem != null) interactionSystem.enabled = true;
        
        Keyboard.current.onTextInput -= OnTextInput;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        RemoveCursorFromAll();
    }

    void Update()
    {
        if (!isReading) return;
        if (Time.time - readStartTime < 0.2f) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            ExitZoom();
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10f, paperLayer))
            {
                bool clickedOnSymbol = false;
                for (int i = 0; i < simbolos.Count; i++)
                {
                    if (hit.collider == simbolos[i].symbolCollider)
                    {
                        currentSymbolIndex = i;
                        HighlightCurrentLine();
                        clickedOnSymbol = true;
                        break;
                    }
                }
                if (!clickedOnSymbol)
                {
                    ExitZoom();
                }
            }
            else
            {
                ExitZoom(); 
            }
        }
    }

    private void SelectFirstAvailableSymbol()
    {
        if (simbolos.Count > 0)
        {
            currentSymbolIndex = 0;
            HighlightCurrentLine();
        }
    }

    private void OnTextInput(char c)
    {
        if (c == '\u001b' || c == '\u0000') return;

        if (currentSymbolIndex >= simbolos.Count) return;

        Simbolo currentSim = simbolos[currentSymbolIndex];

        if (c == '\b')
        {
            if (currentSim.currentInput.Length > 0)
            {
                currentSim.currentInput = currentSim.currentInput.Substring(0, currentSim.currentInput.Length - 1);
            }
        }
        else if (c == ' ')
        {
            currentSymbolIndex = (currentSymbolIndex + 1) % simbolos.Count;
            HighlightCurrentLine();
            return;
        }
        else if (c == '\n' || c == '\r')
        {
            ExitZoom();
            return;
        }
        else if (currentSim.currentInput.Length < maxCharactersPerWord)
        {
            string caracterStr = c.ToString().ToLower();
            currentSim.currentInput += caracterStr;
        }

        HighlightCurrentLine();
    }

    private void HighlightCurrentLine()
    {
        RemoveCursorFromAll(); 
        
        if (currentSymbolIndex < simbolos.Count)
        {
            simbolos[currentSymbolIndex].writtenText.text = simbolos[currentSymbolIndex].currentInput + "_";
        }
    }

    private void RemoveCursorFromAll()
    {
        foreach (var sim in simbolos)
        {
            sim.writtenText.text = sim.currentInput;
        }
    }

    void OnDestroy()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput;
    }
}
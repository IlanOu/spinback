using UnityEngine;
using MidiJack;
using System;
using System.Collections.Generic;

/// <summary>
/// Façade pour simplifier l'accès aux contrôles d'une table de mixage DJ
/// Spécifiquement adapté pour Hercules DJControl Inpulse 200
/// </summary>
public class DJMapper : MonoBehaviour
{
    #region Singleton
    private static DJMapper _instance;
    public static DJMapper Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DJMapper>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DJMapper");
                    _instance = obj.AddComponent<DJMapper>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Structures
    [Serializable]
    public struct DeckControls
    {
        public float Volume;       // Fader de volume (0-1)
        public float Jog;          // Valeur du jog wheel
        public bool PlayPause;     // État du bouton play/pause
        public bool Cue;           // État du bouton cue
        public bool Sync;          // État du bouton sync
        public float BassEQ;       // EQ des basses (0-1)
        public float MidEQ;        // EQ des médiums (0-1)
        public float HighEQ;       // EQ des aigus (0-1)
        public float Filter;       // Filtre (0-1)
        public bool[] PadButtons;  // État des pads (4 ou 8 selon le modèle)
    }
    #endregion

    #region Événements
    // Événements pour les changements significatifs
    public event Action<int, float> OnVolumeChanged;
    public event Action<int, float> OnJogChanged;
    public event Action<int, bool> OnPlayPauseChanged;
    public event Action<int, bool> OnCueChanged;
    public event Action<int, bool> OnSyncChanged;
    public event Action<int, float> OnEQChanged;
    public event Action<int, int, bool> OnPadChanged;
    public event Action<float> OnCrossfaderChanged;
    public event Action<int, float> OnFilterChanged;
    #endregion

    #region Variables publiques
    
    // Champs privés au lieu de propriétés
    private DeckControls _deckA;
    private DeckControls _deckB;
    private float _crossfader = 0.5f;
    
    // Accesseurs publics pour les decks (lecture seule)
    public DeckControls DeckA => _deckA;
    public DeckControls DeckB => _deckB;
    public float Crossfader => _crossfader;

    
    // Configuration des CC MIDI pour Hercules DJControl Inpulse 200
    [Header("Configuration MIDI")]
    [Tooltip("Numéros CC pour le Deck A")]
    public DJMidiConfig deckAConfig = new DJMidiConfig
    {
        volumeFader = 22,
        jogWheel = 20,
        playPauseNote = 0,
        cueNote = 1,
        syncNote = 2,
        bassEQ = 24,
        midEQ = 25,
        highEQ = 26,
        filter = 27,
        padBaseNote = 36 // Généralement les pads commencent à 36
    };
    
    [Tooltip("Numéros CC pour le Deck B")]
    public DJMidiConfig deckBConfig = new DJMidiConfig
    {
        volumeFader = 23,
        jogWheel = 21,
        playPauseNote = 4,
        cueNote = 5,
        syncNote = 6,
        bassEQ = 28,
        midEQ = 29,
        highEQ = 30,
        filter = 31,
        padBaseNote = 44 // Généralement les pads du deck B commencent plus haut
    };
    
    [Tooltip("Numéro CC pour le crossfader")]
    public int crossfaderCC = 8;
    
    [Tooltip("Seuil de changement pour déclencher les événements")]
    [Range(0.01f, 0.1f)]
    public float changeThreshold = 0.02f;
    
    [Tooltip("Nombre de pads par deck")]
    [Range(4, 8)]
    public int padsPerDeck = 4;
    #endregion

    #region Variables privées
    private Dictionary<int, float> lastCCValues = new Dictionary<int, float>();
    private Dictionary<int, bool> lastNoteStates = new Dictionary<int, bool>();
    #endregion

    [Serializable]
    public struct DJMidiConfig
    {
        public int volumeFader;
        public int jogWheel;
        public int playPauseNote;
        public int cueNote;
        public int syncNote;
        public int bassEQ;
        public int midEQ;
        public int highEQ;
        public int filter;
        public int padBaseNote;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
    
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    
        // Initialiser les tableaux de pads
        _deckA.PadButtons = new bool[padsPerDeck];
        _deckB.PadButtons = new bool[padsPerDeck];
    }

    private void Start()
    {
        // S'abonner aux événements MIDI
        MidiMaster.noteOnDelegate += HandleNoteOn;
        MidiMaster.noteOffDelegate += HandleNoteOff;
        MidiMaster.knobDelegate += HandleKnob;
        
        Debug.Log("DJMapper initialisé - Prêt à recevoir les contrôles MIDI");
    }

    private void OnDestroy()
    {
        // Nettoyer les abonnements
        MidiMaster.noteOnDelegate -= HandleNoteOn;
        MidiMaster.noteOffDelegate -= HandleNoteOff;
        MidiMaster.knobDelegate -= HandleKnob;
    }

    private void Update()
    {
        // Vérification alternative des contrôleurs pour plus de fiabilité
        CheckCC(deckAConfig.volumeFader, value => UpdateDeckVolume(0, value));
        CheckCC(deckBConfig.volumeFader, value => UpdateDeckVolume(1, value));
        CheckCC(deckAConfig.jogWheel, value => UpdateJog(0, value));
        CheckCC(deckBConfig.jogWheel, value => UpdateJog(1, value));
        CheckCC(crossfaderCC, UpdateCrossfader);
        
        // EQs
        CheckCC(deckAConfig.bassEQ, value => UpdateEQ(0, 0, value));
        CheckCC(deckAConfig.midEQ, value => UpdateEQ(0, 1, value));
        CheckCC(deckAConfig.highEQ, value => UpdateEQ(0, 2, value));
        CheckCC(deckBConfig.bassEQ, value => UpdateEQ(1, 0, value));
        CheckCC(deckBConfig.midEQ, value => UpdateEQ(1, 1, value));
        CheckCC(deckBConfig.highEQ, value => UpdateEQ(1, 2, value));
        
        // Filtres
        CheckCC(deckAConfig.filter, value => UpdateFilter(0, value));
        CheckCC(deckBConfig.filter, value => UpdateFilter(1, value));
        
        // Vérification des boutons via polling (alternative aux délégués)
        CheckNote(deckAConfig.playPauseNote, pressed => UpdatePlayPause(0, pressed));
        CheckNote(deckAConfig.cueNote, pressed => UpdateCue(0, pressed));
        CheckNote(deckAConfig.syncNote, pressed => UpdateSync(0, pressed));
        CheckNote(deckBConfig.playPauseNote, pressed => UpdatePlayPause(1, pressed));
        CheckNote(deckBConfig.cueNote, pressed => UpdateCue(1, pressed));
        CheckNote(deckBConfig.syncNote, pressed => UpdateSync(1, pressed));
        
        // Vérification des pads
        for (int i = 0; i < padsPerDeck; i++)
        {
            CheckNote(deckAConfig.padBaseNote + i, pressed => UpdatePad(0, i, pressed));
            CheckNote(deckBConfig.padBaseNote + i, pressed => UpdatePad(1, i, pressed));
        }
    }
    
    #region Méthodes d'aide pour la vérification des contrôles
    private void CheckCC(int ccNumber, Action<float> updateAction)
    {
        float currentValue = MidiMaster.GetKnob(ccNumber);
        
        // Si la valeur existe dans le dictionnaire et a changé significativement
        if (lastCCValues.ContainsKey(ccNumber))
        {
            if (Mathf.Abs(currentValue - lastCCValues[ccNumber]) > changeThreshold)
            {
                lastCCValues[ccNumber] = currentValue;
                updateAction(currentValue);
            }
        }
        else
        {
            // Première détection de cette valeur
            lastCCValues.Add(ccNumber, currentValue);
            if (currentValue > 0)
            {
                updateAction(currentValue);
            }
        }
    }
    
    private void CheckNote(int noteNumber, Action<bool> updateAction)
    {
        bool isPressed = MidiMaster.GetKey(noteNumber) > 0;
        
        if (lastNoteStates.ContainsKey(noteNumber))
        {
            if (lastNoteStates[noteNumber] != isPressed)
            {
                lastNoteStates[noteNumber] = isPressed;
                updateAction(isPressed);
            }
        }
        else
        {
            lastNoteStates.Add(noteNumber, isPressed);
            if (isPressed)
            {
                updateAction(isPressed);
            }
        }
    }
    #endregion
    
    #region Gestionnaires d'événements MIDI
    private void HandleNoteOn(MidiChannel channel, int note, float velocity)
    {
        // Deck A - Play/Pause
        if (note == deckAConfig.playPauseNote)
            UpdatePlayPause(0, true);
        // Deck A - Cue
        else if (note == deckAConfig.cueNote)
            UpdateCue(0, true);
        // Deck A - Sync
        else if (note == deckAConfig.syncNote)
            UpdateSync(0, true);
        // Deck B - Play/Pause
        else if (note == deckBConfig.playPauseNote)
            UpdatePlayPause(1, true);
        // Deck B - Cue
        else if (note == deckBConfig.cueNote)
            UpdateCue(1, true);
        // Deck B - Sync
        else if (note == deckBConfig.syncNote)
            UpdateSync(1, true);
        
        // Pads Deck A
        for (int i = 0; i < padsPerDeck; i++)
        {
            if (note == deckAConfig.padBaseNote + i)
                UpdatePad(0, i, true);
        }
        
        // Pads Deck B
        for (int i = 0; i < padsPerDeck; i++)
        {
            if (note == deckBConfig.padBaseNote + i)
                UpdatePad(1, i, true);
        }
    }
    
    private void HandleNoteOff(MidiChannel channel, int note)
    {
        // Deck A - Play/Pause
        if (note == deckAConfig.playPauseNote)
            UpdatePlayPause(0, false);
        // Deck A - Cue
        else if (note == deckAConfig.cueNote)
            UpdateCue(0, false);
        // Deck A - Sync
        else if (note == deckAConfig.syncNote)
            UpdateSync(0, false);
        // Deck B - Play/Pause
        else if (note == deckBConfig.playPauseNote)
            UpdatePlayPause(1, false);
        // Deck B - Cue
        else if (note == deckBConfig.cueNote)
            UpdateCue(1, false);
        // Deck B - Sync
        else if (note == deckBConfig.syncNote)
            UpdateSync(1, false);
        
        // Pads Deck A
        for (int i = 0; i < padsPerDeck; i++)
        {
            if (note == deckAConfig.padBaseNote + i)
                UpdatePad(0, i, false);
        }
        
        // Pads Deck B
        for (int i = 0; i < padsPerDeck; i++)
        {
            if (note == deckBConfig.padBaseNote + i)
                UpdatePad(1, i, false);
        }
    }
    
    private void HandleKnob(MidiChannel channel, int knobNumber, float knobValue)
{
    // Volumes
    if (knobNumber == deckAConfig.volumeFader)
        UpdateDeckVolume(0, knobValue);
    else if (knobNumber == deckBConfig.volumeFader)
        UpdateDeckVolume(1, knobValue);
    // Jog wheels
    else if (knobNumber == deckAConfig.jogWheel)
        UpdateJog(0, knobValue);
    else if (knobNumber == deckBConfig.jogWheel)
        UpdateJog(1, knobValue);
    // Crossfader
    else if (knobNumber == crossfaderCC)
        UpdateCrossfader(knobValue);
    // EQs Deck A
    else if (knobNumber == deckAConfig.bassEQ)
        UpdateEQ(0, 0, knobValue);
    else if (knobNumber == deckAConfig.midEQ)
        UpdateEQ(0, 1, knobValue);
    else if (knobNumber == deckAConfig.highEQ)
        UpdateEQ(0, 2, knobValue);
    // EQs Deck B
    else if (knobNumber == deckBConfig.bassEQ)
        UpdateEQ(1, 0, knobValue);
    else if (knobNumber == deckBConfig.midEQ)
        UpdateEQ(1, 1, knobValue);
    else if (knobNumber == deckBConfig.highEQ)
        UpdateEQ(1, 2, knobValue);
    // Filtres
    else if (knobNumber == deckAConfig.filter)
        UpdateFilter(0, knobValue);
    else if (knobNumber == deckBConfig.filter)
        UpdateFilter(1, knobValue);
}
#endregion

#region Méthodes de mise à jour des contrôles
private void UpdateDeckVolume(int deck, float value)
{
    if (deck == 0)
    {
        _deckA.Volume = value;
        OnVolumeChanged?.Invoke(0, value);
    }
    else
    {
        _deckB.Volume = value;
        OnVolumeChanged?.Invoke(1, value);
    }
}

private void UpdateJog(int deck, float value)
{
    // Les jog wheels envoient généralement des valeurs relatives
    // Nous gardons simplement la dernière valeur pour l'utilisation
    if (deck == 0)
    {
        _deckA.Jog = value;
        OnJogChanged?.Invoke(0, value);
    }
    else
    {
        _deckB.Jog = value;
        OnJogChanged?.Invoke(1, value);
    }
}

private void UpdatePlayPause(int deck, bool pressed)
{
    if (deck == 0)
    {
        _deckA.PlayPause = pressed;
        OnPlayPauseChanged?.Invoke(0, pressed);
    }
    else
    {
        _deckB.PlayPause = pressed;
        OnPlayPauseChanged?.Invoke(1, pressed);
    }
}

private void UpdateCue(int deck, bool pressed)
{
    if (deck == 0)
    {
        _deckA.Cue = pressed;
        OnCueChanged?.Invoke(0, pressed);
    }
    else
    {
        _deckB.Cue = pressed;
        OnCueChanged?.Invoke(1, pressed);
    }
}

private void UpdateSync(int deck, bool pressed)
{
    if (deck == 0)
    {
        _deckA.Sync = pressed;
        OnSyncChanged?.Invoke(0, pressed);
    }
    else
    {
        _deckB.Sync = pressed;
        OnSyncChanged?.Invoke(1, pressed);
    }
}

private void UpdateEQ(int deck, int eqType, float value)
{
    if (deck == 0)
    {
        switch (eqType)
        {
            case 0: // Bass
                _deckA.BassEQ = value;
                break;
            case 1: // Mid
                _deckA.MidEQ = value;
                break;
            case 2: // High
                _deckA.HighEQ = value;
                break;
        }
    }
    else
    {
        switch (eqType)
        {
            case 0: // Bass
                _deckB.BassEQ = value;
                break;
            case 1: // Mid
                _deckB.MidEQ = value;
                break;
            case 2: // High
                _deckB.HighEQ = value;
                break;
        }
    }
    
    OnEQChanged?.Invoke(deck * 3 + eqType, value);
}

private void UpdateCrossfader(float value)
{
    _crossfader = value;
    OnCrossfaderChanged?.Invoke(value);
}

private void UpdateFilter(int deck, float value)
{
    if (deck == 0)
    {
        _deckA.Filter = value;
        OnFilterChanged?.Invoke(0, value);
    }
    else
    {
        _deckB.Filter = value;
        OnFilterChanged?.Invoke(1, value);
    }
}

private void UpdatePad(int deck, int padIndex, bool pressed)
{
    if (padIndex >= padsPerDeck)
        return;
        
    if (deck == 0)
    {
        _deckA.PadButtons[padIndex] = pressed;
    }
    else
    {
        _deckB.PadButtons[padIndex] = pressed;
    }
    
    OnPadChanged?.Invoke(deck, padIndex, pressed);
}
#endregion

#region Méthodes publiques d'accès simplifié
/// <summary>
/// Obtient la valeur du volume pour un deck spécifique (0-1)
/// </summary>
public float GetVolume(int deck)
{
    return deck == 0 ? _deckA.Volume : _deckB.Volume;
}

/// <summary>
/// Vérifie si un bouton spécifique est actuellement pressé
/// </summary>
public bool IsButtonPressed(int deck, DJButton button)
{
    if (deck == 0)
    {
        switch (button)
        {
            case DJButton.PlayPause: return _deckA.PlayPause;
            case DJButton.Cue: return _deckA.Cue;
            case DJButton.Sync: return _deckA.Sync;
            default: return false;
        }
    }
    else
    {
        switch (button)
        {
            case DJButton.PlayPause: return _deckB.PlayPause;
            case DJButton.Cue: return _deckB.Cue;
            case DJButton.Sync: return _deckB.Sync;
            default: return false;
        }
    }
}

/// <summary>
/// Vérifie si un pad spécifique est actuellement pressé
/// </summary>
public bool IsPadPressed(int deck, int padIndex)
{
    if (padIndex >= padsPerDeck)
        return false;
        
    return deck == 0 ? _deckA.PadButtons[padIndex] : _deckB.PadButtons[padIndex];
}

/// <summary>
/// Obtient la valeur d'un EQ spécifique (0-1)
/// </summary>
public float GetEQ(int deck, EQBand band)
{
    if (deck == 0)
    {
        switch (band)
        {
            case EQBand.Bass: return _deckA.BassEQ;
            case EQBand.Mid: return _deckA.MidEQ;
            case EQBand.High: return _deckA.HighEQ;
            default: return 0.5f;
        }
    }
    else
    {
        switch (band)
        {
            case EQBand.Bass: return _deckB.BassEQ;
            case EQBand.Mid: return _deckB.MidEQ;
            case EQBand.High: return _deckB.HighEQ;
            default: return 0.5f;
        }
    }
}

/// <summary>
/// Obtient la valeur du filtre pour un deck spécifique (0-1)
/// </summary>
public float GetFilter(int deck)
{
    return deck == 0 ? _deckA.Filter : _deckB.Filter;
}

/// <summary>
/// Obtient la valeur de jog wheel pour un deck spécifique (-1 à 1)
/// La valeur est centrée sur 0 pour faciliter l'utilisation
/// </summary>
public float GetJogValue(int deck)
{
    float rawValue = deck == 0 ? _deckA.Jog : _deckB.Jog;
    // Convertir de 0-1 à -1 à 1 (avec 0.5 comme point neutre)
    return (rawValue - 0.5f) * 2f;
}

/// <summary>
/// Obtient la position actuelle du crossfader (0 = gauche, 1 = droite)
/// </summary>
public float GetCrossfader()
{
    return Crossfader;
}

/// <summary>
/// S'abonne à un événement de changement de bouton
/// </summary>
public void SubscribeToButton(int deck, DJButton button, Action<bool> callback)
{
    switch (button)
    {
        case DJButton.PlayPause:
            OnPlayPauseChanged += (d, pressed) => { if (d == deck) callback(pressed); };
            break;
        case DJButton.Cue:
            OnCueChanged += (d, pressed) => { if (d == deck) callback(pressed); };
            break;
        case DJButton.Sync:
            OnSyncChanged += (d, pressed) => { if (d == deck) callback(pressed); };
            break;
    }
}

/// <summary>
/// S'abonne à un événement de changement de pad
/// </summary>
public void SubscribeToPad(int deck, int padIndex, Action<bool> callback)
{
    OnPadChanged += (d, p, pressed) => { if (d == deck && p == padIndex) callback(pressed); };
}

/// <summary>
/// S'abonne à un événement de changement de volume
/// </summary>
public void SubscribeToVolume(int deck, Action<float> callback)
{
    OnVolumeChanged += (d, value) => { if (d == deck) callback(value); };
}

/// <summary>
/// S'abonne à un événement de changement de crossfader
/// </summary>
public void SubscribeToCrossfader(Action<float> callback)
{
    OnCrossfaderChanged += callback;
}

/// <summary>
/// S'abonne à un événement de changement d'EQ
/// </summary>
public void SubscribeToEQ(int deck, EQBand band, Action<float> callback)
{
    int eqIndex = deck * 3 + (int)band;
    OnEQChanged += (idx, value) => { if (idx == eqIndex) callback(value); };
}

/// <summary>
/// S'abonne à un événement de changement de filtre
/// </summary>
public void SubscribeToFilter(int deck, Action<float> callback)
{
    OnFilterChanged += (d, value) => { if (d == deck) callback(value); };
}

/// <summary>
/// S'abonne à un événement de changement de jog wheel
/// </summary>
public void SubscribeToJog(int deck, Action<float> callback)
{
    OnJogChanged += (d, value) => { if (d == deck) callback(value); };
}
#endregion

#region Types énumérés
public enum DJButton
{
    PlayPause,
    Cue,
    Sync
}

public enum EQBand
{
    Bass,
    Mid,
    High
}
#endregion

#region Méthodes de débogage
/// <summary>
/// Affiche l'état actuel des contrôleurs dans la console
/// </summary>
public void DebugControls()
{
    Debug.Log("=== ÉTAT DES CONTRÔLEURS DJ ===");
    Debug.Log($"DECK A: Volume={_deckA.Volume:F2}, Play={_deckA.PlayPause}, Cue={_deckA.Cue}, Sync={_deckA.Sync}");
    Debug.Log($"EQs A: Bass={_deckA.BassEQ:F2}, Mid={_deckA.MidEQ:F2}, High={_deckA.HighEQ:F2}, Filter={_deckA.Filter:F2}");
    
    Debug.Log($"DECK B: Volume={_deckB.Volume:F2}, Play={_deckB.PlayPause}, Cue={_deckB.Cue}, Sync={_deckB.Sync}");
    Debug.Log($"EQs B: Bass={_deckB.BassEQ:F2}, Mid={_deckB.MidEQ:F2}, High={_deckB.HighEQ:F2}, Filter={_deckB.Filter:F2}");
    
    Debug.Log($"CROSSFADER: {Crossfader:F2}");
    
    string padsA = "PADS A: ";
    for (int i = 0; i < padsPerDeck; i++)
    {
        padsA += $"{i}={_deckA.PadButtons[i]} ";
    }
    Debug.Log(padsA);
    
    string padsB = "PADS B: ";
    for (int i = 0; i < padsPerDeck; i++)
    {
        padsB += $"{i}={_deckB.PadButtons[i]} ";
    }
    Debug.Log(padsB);
}

/// <summary>
/// Affiche une interface simple dans l'éditeur pour visualiser l'état des contrôles
/// </summary>
/// <summary>
    /// Affiche une interface simple dans l'éditeur pour visualiser l'état des contrôles
    /// </summary>
    void OnGUI()
    {
        // Afficher uniquement en mode débogage
        if (!Debug.isDebugBuild)
            return;
            
        int width = 400;
        int height = 300;
        int x = Screen.width - width - 10;
        int y = 10;
        
        GUI.Box(new Rect(x, y, width, height), "DJ Controller Status");
        
        // Deck A
        GUI.Label(new Rect(x + 10, y + 25, 100, 20), "DECK A");
        GUI.Label(new Rect(x + 10, y + 45, 100, 20), $"Volume: {_deckA.Volume:F2}");
        GUI.Label(new Rect(x + 10, y + 65, 100, 20), $"Play: {(_deckA.PlayPause ? "ON" : "OFF")}");
        GUI.Label(new Rect(x + 10, y + 85, 100, 20), $"Cue: {(_deckA.Cue ? "ON" : "OFF")}");
        GUI.Label(new Rect(x + 10, y + 105, 100, 20), $"Sync: {(_deckA.Sync ? "ON" : "OFF")}");
        
        GUI.Label(new Rect(x + 10, y + 125, 100, 20), $"Bass: {_deckA.BassEQ:F2}");
        GUI.Label(new Rect(x + 10, y + 145, 100, 20), $"Mid: {_deckA.MidEQ:F2}");
        GUI.Label(new Rect(x + 10, y + 165, 100, 20), $"High: {_deckA.HighEQ:F2}");
        GUI.Label(new Rect(x + 10, y + 185, 100, 20), $"Filter: {_deckA.Filter:F2}");
        
        // Pads A
        string padsA = "Pads: ";
        for (int i = 0; i < padsPerDeck; i++)
        {
            padsA += _deckA.PadButtons[i] ? $"{i}✓ " : $"{i}✗ ";
        }
        GUI.Label(new Rect(x + 10, y + 205, 200, 20), padsA);
        
        // Deck B
        GUI.Label(new Rect(x + 200, y + 25, 100, 20), "DECK B");
        GUI.Label(new Rect(x + 200, y + 45, 100, 20), $"Volume: {_deckB.Volume:F2}");
        GUI.Label(new Rect(x + 200, y + 65, 100, 20), $"Play: {(_deckB.PlayPause ? "ON" : "OFF")}");
        GUI.Label(new Rect(x + 200, y + 85, 100, 20), $"Cue: {(_deckB.Cue ? "ON" : "OFF")}");
        GUI.Label(new Rect(x + 200, y + 105, 100, 20), $"Sync: {(_deckB.Sync ? "ON" : "OFF")}");
        
        GUI.Label(new Rect(x + 200, y + 125, 100, 20), $"Bass: {_deckB.BassEQ:F2}");
        GUI.Label(new Rect(x + 200, y + 145, 100, 20), $"Mid: {_deckB.MidEQ:F2}");
        GUI.Label(new Rect(x + 200, y + 165, 100, 20), $"High: {_deckB.HighEQ:F2}");
        GUI.Label(new Rect(x + 200, y + 185, 100, 20), $"Filter: {_deckB.Filter:F2}");
        
        // Pads B
        string padsB = "Pads: ";
        for (int i = 0; i < padsPerDeck; i++)
        {
            padsB += _deckB.PadButtons[i] ? $"{i}✓ " : $"{i}✗ ";
        }
        GUI.Label(new Rect(x + 200, y + 205, 200, 20), padsB);
        
        // Crossfader
        GUI.Label(new Rect(x + 10, y + 235, 100, 20), "CROSSFADER:");
        GUI.HorizontalSlider(new Rect(x + 110, y + 235, 200, 20), Crossfader, 0f, 1f);
        GUI.Label(new Rect(x + 320, y + 235, 50, 20), $"{Crossfader:F2}");
        
        // Jog wheels
        GUI.Label(new Rect(x + 10, y + 265, 100, 20), $"JOG A: {GetJogValue(0):F2}");
        GUI.Label(new Rect(x + 200, y + 265, 100, 20), $"JOG B: {GetJogValue(1):F2}");
    }
    #endregion
    
    /// <summary>
    /// Définit manuellement les numéros MIDI pour la configuration
    /// Utile pour ajuster les mappings à la volée
    /// </summary>
    public void SetMidiConfig(int deck, string controlName, int midiNumber)
    {
        if (deck == 0)
        {
            switch (controlName.ToLower())
            {
                case "volume":
                    deckAConfig.volumeFader = midiNumber;
                    break;
                case "jog":
                    deckAConfig.jogWheel = midiNumber;
                    break;
                case "play":
                    deckAConfig.playPauseNote = midiNumber;
                    break;
                case "cue":
                    deckAConfig.cueNote = midiNumber;
                    break;
                case "sync":
                    deckAConfig.syncNote = midiNumber;
                    break;
                case "bass":
                    deckAConfig.bassEQ = midiNumber;
                    break;
                case "mid":
                    deckAConfig.midEQ = midiNumber;
                    break;
                case "high":
                    deckAConfig.highEQ = midiNumber;
                    break;
                case "filter":
                    deckAConfig.filter = midiNumber;
                    break;
                case "padbase":
                    deckAConfig.padBaseNote = midiNumber;
                    break;
            }
        }
        else
        {
            switch (controlName.ToLower())
            {
                case "volume":
                    deckBConfig.volumeFader = midiNumber;
                    break;
                case "jog":
                    deckBConfig.jogWheel = midiNumber;
                    break;
                case "play":
                    deckBConfig.playPauseNote = midiNumber;
                    break;
                case "cue":
                    deckBConfig.cueNote = midiNumber;
                    break;
                case "sync":
                    deckBConfig.syncNote = midiNumber;
                    break;
                case "bass":
                    deckBConfig.bassEQ = midiNumber;
                    break;
                case "mid":
                    deckBConfig.midEQ = midiNumber;
                    break;
                case "high":
                    deckBConfig.highEQ = midiNumber;
                    break;
                case "filter":
                    deckBConfig.filter = midiNumber;
                    break;
                case "padbase":
                    deckBConfig.padBaseNote = midiNumber;
                    break;
            }
        }
        
        if (controlName.ToLower() == "crossfader")
        {
            crossfaderCC = midiNumber;
        }
        
        Debug.Log($"Configuration MIDI mise à jour: Deck {deck}, Contrôle {controlName}, Numéro MIDI {midiNumber}");
    }
}
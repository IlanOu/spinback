using UnityEngine;
using MidiJack;

public class SimpleDJMapper : MonoBehaviour
{
    #region Configuration MIDI

    [Header("Configuration MIDI")]
    [Tooltip("Numéro CC du crossfader")]
    public int crossfaderCC = 20;
    [Tooltip("Canal du crossfader (ex: Ch1)")]
    public MidiChannel crossfaderChannel = MidiChannel.Ch1;

    [Tooltip("Note pour Play/Pause")]
    public int playPauseNote = 7;
    [Tooltip("Note de base pour les pads")]
    public int padBaseNote = 36;
    [Tooltip("Nombre de pads par deck")]
    public int padsCount = 4;

    [Tooltip("Canal pour le deck A (ex: Ch11)")]
    public MidiChannel deckChannelA = MidiChannel.Ch11;
    [Tooltip("Canal pour le deck B (ex: Ch12)")]
    public MidiChannel deckChannelB = MidiChannel.Ch12;

    #endregion

    #region État des contrôles

    private float crossfaderValue = 0.5f;
    private bool playPauseA = false;
    private bool playPauseB = false;
    private bool[] padsA;
    private bool[] padsB;

    #endregion

    private void Awake()
    {
        // Initialisation des tableaux de pads
        padsA = new bool[padsCount];
        padsB = new bool[padsCount];
    }

    private void Start()
    {
        // Abonnement aux délégués MidiJack
        MidiMaster.noteOnDelegate += OnNoteOn;
        MidiMaster.noteOffDelegate += OnNoteOff;
        MidiMaster.knobDelegate += OnKnob;

        Debug.Log("SimpleDJMapper initialisé.");
    }

    private void OnDestroy()
    {
        // Désabonnement
        MidiMaster.noteOnDelegate -= OnNoteOn;
        MidiMaster.noteOffDelegate -= OnNoteOff;
        MidiMaster.knobDelegate -= OnKnob;
    }

    #region Gestion des événements MIDI

    // Pour le crossfader (CC)
    private void OnKnob(MidiChannel channel, int knobNumber, float knobValue)
    {
        Debug.Log("<color=blue>KNOB</color> - Canal: " + channel + ", Numéro CC: " + knobNumber + ", Valeur: " + knobValue);
        if (channel == crossfaderChannel && knobNumber == crossfaderCC)
        {
            crossfaderValue = knobValue;
            Debug.Log("Crossfader : " + crossfaderValue);
        }
    }

    // Pour les notes (Play/Pause et pads)
    private void OnNoteOn(MidiChannel channel, int note, float velocity)
    {
        Debug.Log("<color=green>NOTE ON</color> - Canal: " + channel + ", Note: " + note + ", Vélocité: " + velocity);
        if (channel == deckChannelA)
        {
            // Play/Pause Deck A
            if (note == playPauseNote)
            {
                playPauseA = true;
                Debug.Log("Deck A : Play/Pause ON");
            }

            // Pads Deck A
            for (int i = 0; i < padsCount; i++)
            {
                if (note == padBaseNote + i)
                {
                    padsA[i] = true;
                    Debug.Log("Deck A : Pad " + i + " appuyé");
                }
            }
        }
        else if (channel == deckChannelB)
        {
            // Play/Pause Deck B
            if (note == playPauseNote)
            {
                playPauseB = true;
                Debug.Log("Deck B : Play/Pause ON");
            }

            // Pads Deck B
            for (int i = 0; i < padsCount; i++)
            {
                if (note == padBaseNote + i)
                {
                    padsB[i] = true;
                    Debug.Log("Deck B : Pad " + i + " appuyé");
                }
            }
        }
    }

    private void OnNoteOff(MidiChannel channel, int note)
    {
        Debug.Log("<color=red>NOTE OFF</color> - Canal: " + channel + ", Note: " + note);
        if (channel == deckChannelA)
        {
            // Play/Pause Deck A
            if (note == playPauseNote)
            {
                playPauseA = false;
                Debug.Log("Deck A : Play/Pause OFF");
            }

            // Pads Deck A
            for (int i = 0; i < padsCount; i++)
            {
                if (note == padBaseNote + i)
                {
                    padsA[i] = false;
                    Debug.Log("Deck A : Pad " + i + " relâché");
                }
            }
        }
        else if (channel == deckChannelB)
        {
            // Play/Pause Deck B
            if (note == playPauseNote)
            {
                playPauseB = false;
                Debug.Log("Deck B : Play/Pause OFF");
            }

            // Pads Deck B
            for (int i = 0; i < padsCount; i++)
            {
                if (note == padBaseNote + i)
                {
                    padsB[i] = false;
                    Debug.Log("Deck B : Pad " + i + " relâché");
                }
            }
        }
    }

    #endregion

    #region Méthode de Debug (optionnel)

    // Affiche l'état courant dans la console (pour tests)
    private void Update()
    {
        // Par exemple, appuie sur la touche "D" pour afficher l'état
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("=== État SimpleDJMapper ===");
            Debug.Log("Crossfader : " + crossfaderValue);
            Debug.Log("Deck A - Play/Pause : " + playPauseA);
            for (int i = 0; i < padsCount; i++)
            {
                Debug.Log("Deck A - Pad " + i + " : " + padsA[i]);
            }
            Debug.Log("Deck B - Play/Pause : " + playPauseB);
            for (int i = 0; i < padsCount; i++)
            {
                Debug.Log("Deck B - Pad " + i + " : " + padsB[i]);
            }
        }
    }

    #endregion
}

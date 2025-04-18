using UnityEngine;
using MidiJack;

public class MidiJackDetector : MonoBehaviour
{
    void Update()
    {
        // Vérifie toutes les notes MIDI (0 à 127)
        for (int note = 0; note < 128; note++)
        {
            float value = MidiMaster.GetKey(note);
            if (value > 0)
            {
                Debug.Log("Note MIDI activée : " + note + " - valeur : " + value);
            }
        }

        // Exemple : lire le knob ou fader sur channel 0, control 7
        float knobValue = MidiMaster.GetKnob(7, 0);
        if (knobValue > 0)
        {
            Debug.Log("Fader ou Knob détecté : " + knobValue);
        }
    }
}
using UnityEngine;
using MidiJack;
using System.Collections.Generic;

public class MidiInputDebugger : MonoBehaviour
{
    // Dictionnaire pour stocker les dernières valeurs des contrôleurs
    private Dictionary<int, float> lastKnobValues = new Dictionary<int, float>();
    // Pour limiter l'affichage des messages dans la console
    private float logCooldown = 0;
    
    void Start()
    {
        Debug.Log("=== Démarrage du débogueur MIDI avec MidiJack ===");
        
        // S'abonner aux événements MIDI
        MidiMaster.noteOnDelegate += NoteOnHandler;
        MidiMaster.noteOffDelegate += NoteOffHandler;
        MidiMaster.knobDelegate += KnobHandler;
    }
    
    void OnDestroy()
    {
        // Nettoyer les abonnements
        MidiMaster.noteOnDelegate -= NoteOnHandler;
        MidiMaster.noteOffDelegate -= NoteOffHandler;
        MidiMaster.knobDelegate -= KnobHandler;
    }
    
    void NoteOnHandler(MidiChannel channel, int note, float velocity)
    {
        Debug.Log($"<color=green>NOTE ON</color> - Canal: {channel}, Note: {note}, Vélocité: {velocity}");
    }
    
    void NoteOffHandler(MidiChannel channel, int note)
    {
        Debug.Log($"<color=red>NOTE OFF</color> - Canal: {channel}, Note: {note}");
    }
    
    void KnobHandler(MidiChannel channel, int knobNumber, float knobValue)
    {
        // Formater la valeur pour meilleure lisibilité (0-127 en MIDI)
        int displayValue = Mathf.RoundToInt(knobValue * 127);
        
        // Afficher le type de contrôle selon les numéros standards
        string controlType = GetControlType(knobNumber);
        
        Debug.Log($"<color=yellow>CONTRÔLEUR</color> - Canal: {channel}, N°: {knobNumber} ({controlType}), Valeur: {displayValue}/127");
    }
    
    void Update()
    {
        // Vérification alternative des contrôleurs pour capter ceux qui pourraient être manqués par les délégués
        if (logCooldown <= 0)
        {
            for (int i = 0; i < 128; i++)
            {
                float currentValue = MidiMaster.GetKnob(i);
                
                // Si la valeur existe dans le dictionnaire et a changé significativement
                if (lastKnobValues.ContainsKey(i))
                {
                    if (Mathf.Abs(currentValue - lastKnobValues[i]) > 0.01f)
                    {
                        lastKnobValues[i] = currentValue;
                        // Ne pas afficher car déjà géré par le délégué KnobHandler
                    }
                }
                else if (currentValue > 0)
                {
                    // Nouvelle valeur détectée
                    lastKnobValues.Add(i, currentValue);
                }
            }
            
            logCooldown = 0.1f; // Limiter la fréquence de vérification
        }
        else
        {
            logCooldown -= Time.deltaTime;
        }
    }
    
    // Fonction pour identifier les types de contrôleurs courants
    string GetControlType(int ccNumber)
    {
        switch (ccNumber)
        {
            case 1: return "Modulation";
            case 7: return "Volume";
            case 10: return "Pan";
            case 11: return "Expression";
            case 64: return "Sustain";
            case 74: return "Brightness";
            // Ajoutez ici les contrôles spécifiques à votre DJControl Inpulse 200
            case 20: return "Possible jog wheel";
            case 21: return "Possible crossfader";
            case 22:
            case 23: return "Possible fader de canal";
            case 24:
            case 25:
            case 26:
            case 27: return "Possible EQ";
            default: return "Contrôleur inconnu";
        }
    }
    
    // Fonction pour dessiner une interface simple dans l'éditeur
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), "Débogueur MIDI - Vérifiez la console pour les détails");
        
        int y = 40;
        foreach (var knob in lastKnobValues)
        {
            if (knob.Value > 0.01f)
            {
                string controlType = GetControlType(knob.Key);
                GUI.Label(new Rect(10, y, 400, 20), 
                    $"CC {knob.Key} ({controlType}): {Mathf.RoundToInt(knob.Value * 127)}/127");
                
                // Dessiner une barre pour visualiser la valeur
                GUI.Box(new Rect(250, y, 100 * knob.Value, 15), "");
                
                y += 25;
            }
        }
    }
}
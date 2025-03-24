using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        // S'abonner aux événements
        DJMapper.Instance.SubscribeToButton(0, DJMapper.DJButton.PlayPause, OnPlayPause);
        DJMapper.Instance.SubscribeToVolume(0, OnVolumeChange);
        DJMapper.Instance.SubscribeToCrossfader(OnCrossfaderChange);
    }
    
    void Update()
    {
        // Vérifier l'état actuel
        if (DJMapper.Instance.IsButtonPressed(0, DJMapper.DJButton.Cue))
        {
            // Faire quelque chose quand le bouton Cue est pressé
        }
        
        // Obtenir des valeurs
        float volume = DJMapper.Instance.GetVolume(0);
        float jogValue = DJMapper.Instance.GetJogValue(1);
    }
    
    void OnPlayPause(bool isPressed)
    {
        Debug.Log($"Bouton Play/Pause: {(isPressed ? "Pressé" : "Relâché")}");
    }
    
    void OnVolumeChange(float value)
    {
        Debug.Log($"Volume changé: {value}");
    }
    
    void OnCrossfaderChange(float value)
    {
        Debug.Log($"Crossfader changé: {value}");
    }
}
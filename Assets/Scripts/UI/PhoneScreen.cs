using System;
using DefaultNamespace;
using Minis;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PhoneScreen: MonoBehaviour
    {
        [SerializeField] private Image[] screens;
        
        private enum ScreenIndex
        {
            First = 0,
            Second = 1,
            Third = 2
        }
        
        private void Start()
        {
            if (screens == null || screens.Length < 3)
            {
                Debug.LogError("Les trois écrans ne sont pas assignés dans l'inspecteur !");
                return;
            }
            
            MidiBindingRegistry.Instance.Bind(ActionEnum.FirstScreen, (input) => SwitchScreen(ScreenIndex.First));
            MidiBindingRegistry.Instance.Bind(ActionEnum.SecondScreen, (input) => SwitchScreen(ScreenIndex.Second));
            MidiBindingRegistry.Instance.Bind(ActionEnum.ThirdScreen, (input) => SwitchScreen(ScreenIndex.Third));
            
            SwitchScreen(ScreenIndex.First);
        }
        
        private void SwitchScreen(ScreenIndex activeScreen)
        {
            Debug.Log($"{activeScreen} Screen");
            
            foreach (var screen in screens)
            {
                screen.enabled = false;
            }
            screens[(int)activeScreen].enabled = true;
        }
    }
}
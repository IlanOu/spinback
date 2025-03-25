using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Minis
{
    public class MidiBindingRegistry : MonoBehaviour
    {
        [SerializeField]
        private List<MidiNoteBinding> noteBindings = new ();
        [SerializeField]
        private List<MidiControlBinding> controlBindings = new ();
        
        private Dictionary<ActionEnum, Action<MidiInput>> _namedListeners = new();
        
        private static MidiBindingRegistry _instance;
        public static MidiBindingRegistry Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (MinisEventManager.Instance != null)
            {
                MinisEventManager.Instance.OnNoteOn += OnNoteOn;
                MinisEventManager.Instance.OnNoteOff += OnNoteOff;
                MinisEventManager.Instance.OnControlChange += OnControlChange;
            }
        }

        private void OnDestroy()
        {
            if (MinisEventManager.Instance != null)
            {
                MinisEventManager.Instance.OnNoteOn -= OnNoteOn;
                MinisEventManager.Instance.OnNoteOff -= OnNoteOff;
                MinisEventManager.Instance.OnControlChange -= OnControlChange;
            }
        }

        public void Bind(ActionEnum action, Action<MidiInput> listener)
        {
            if (!_namedListeners.ContainsKey(action))
            {
                _namedListeners.Add(action, listener);
            }
            else
            {
                _namedListeners[action] += listener;
            }
        }

        public void Unbind(ActionEnum action, Action<MidiInput> listener)
        {
            if (_namedListeners.ContainsKey(action))
            {
                _namedListeners[action] -= listener;
            }
        }

        private void OnNoteOn(MidiInput input)
        {
            foreach (var binding in noteBindings)
            {
                if (binding != null && binding.isPressed && binding.number == input.Number && binding.channel == input.Channel)
                {
                    if (_namedListeners.TryGetValue(binding.action, out var listener))
                    {
                        listener?.Invoke(input);
                    }
                }
            }
        }
        
        private void OnNoteOff(MidiInput input)
        {
            foreach (var binding in noteBindings)
            {
                if (binding != null && !binding.isPressed && binding.number == input.Number && binding.channel == input.Channel)
                {
                    if (_namedListeners.TryGetValue(binding.action, out var listener))
                    {
                        listener?.Invoke(input);
                    }
                }
            }
        }
        
        private void OnControlChange(MidiInput input)
        {
            foreach (var binding in controlBindings)
            {
                if (binding != null && binding.number == input.Number && binding.channel == input.Channel)
                {
                    if (_namedListeners.TryGetValue(binding.action, out var listener))
                    {
                        listener?.Invoke(input);
                    }
                }
            }
        }
    }
}
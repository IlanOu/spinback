using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minis
{
    public class MidiBindingRegistry : MonoBehaviour
    {
        [SerializeField]
        private List<NamedMidiBinding> bindings = new ();
        private Dictionary<string, Action<MidiInput>> _namedListeners = new();
        
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

        public void Bind(string name, Action<MidiInput> listener)
        {
            if (!_namedListeners.ContainsKey(name))
            {
                _namedListeners.Add(name, listener);
            }
            else
            {
                _namedListeners[name] += listener;
            }
        }

        public void Unbind(string name, Action<MidiInput> listener)
        {
            if (_namedListeners.ContainsKey(name))
            {
                _namedListeners[name] -= listener;
            }
        }

        private void OnNoteOn(MidiInput input)
        {
            foreach (var binding in bindings)
            {
                if (!binding.isControl && binding.number == input.Number && binding.channel == input.Channel)
                {
                    if (_namedListeners.TryGetValue(binding.name, out var listener))
                    {
                        listener?.Invoke(input);
                    }
                }
            }
        }
        
        private void OnNoteOff(MidiInput input)
        {
            foreach (var binding in bindings)
            {
                if (!binding.isControl && binding.number == input.Number && binding.channel == input.Channel)
                {
                    if (_namedListeners.TryGetValue(binding.name, out var listener))
                    {
                        listener?.Invoke(input);
                    }
                }
            }
        }
        
        private void OnControlChange(MidiInput input)
        {
            foreach (var binding in bindings)
            {
                if (binding.isControl && binding.number == input.Number && binding.channel == input.Channel)
                {
                    if (_namedListeners.TryGetValue(binding.name, out var listener))
                    {
                        listener?.Invoke(input);
                    }
                }
            }
        }
    }
}
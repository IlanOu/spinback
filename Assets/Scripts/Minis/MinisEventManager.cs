using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minis
{
    public class MinisEventManager : MonoBehaviour
    {
        private static MinisEventManager _instance;
        public static MinisEventManager Instance => _instance;

        public event Action<MidiInput> OnNoteOn;
        public event Action<MidiInput> OnNoteOff;
        public event Action<MidiInput> OnControlChange;
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
            InputSystem.onDeviceChange += (device, change) =>
            {
                if (change != InputDeviceChange.Added) return;

                var midiDevice = device as MidiDevice;
                if (midiDevice == null) return;

                SetupNoteCallbacks(midiDevice);
                SetupControlCallback(midiDevice);
            };
        }

        private void SetupNoteCallbacks(MidiDevice midiDevice)
        {
            // ON
            midiDevice.onWillNoteOn += (note, velocity) => {
                OnNoteOn?.Invoke(MidiInput.FromNote(note,  velocity));
            };

            // OFF
            midiDevice.onWillNoteOff += (note) => {
                OnNoteOff?.Invoke(MidiInput.FromNote(note, 0f));
            };
        }
    
        private void SetupControlCallback(MidiDevice midiDevice)
        {
            // ON SLIDE
            midiDevice.onWillControlChange += (cc, value) => {
                OnControlChange?.Invoke(MidiInput.FromControl(cc, value));
            };
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minis
{
    public class MinisEventManager : MonoBehaviour
    {
        private static MinisEventManager _instance;
        public static MinisEventManager Instance => _instance;

        public event Action<MinisInput> NoteOn;
        public event Action<MinisInput> NoteOff;
        public event Action<MinisInput> ControlChange;
        private void Awake()
        {
            if (_instance != null && _instance != this) {
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
                NoteOn?.Invoke(MinisInput.FromNote(note,  velocity));
            };

            // OFF
            midiDevice.onWillNoteOff += (note) => {
                NoteOff?.Invoke(MinisInput.FromNote(note, 0f));
            };
        }
    
        private void SetupControlCallback(MidiDevice midiDevice)
        {
            // ON SLIDE
            midiDevice.onWillControlChange += (cc, value) => {
                ControlChange?.Invoke(MinisInput.FromControl(cc, value));
            };
        }
    }
}

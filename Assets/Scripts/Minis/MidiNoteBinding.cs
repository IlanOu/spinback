using System;
using DefaultNamespace;

namespace Minis
{
    [Serializable]
    public class MidiNoteBinding
    {
        public ActionEnum action;
        public int number;
        public int channel;
        public bool isPressed;
    }
}
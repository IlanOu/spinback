using System;
using DefaultNamespace;

namespace Minis
{
    [Serializable]
    public class MidiControlBinding
    {
        public ActionEnum action;
        public int number;
        public int channel;
    }
}
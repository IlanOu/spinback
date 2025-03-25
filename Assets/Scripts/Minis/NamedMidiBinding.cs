namespace Minis
{
    [System.Serializable]
    public class NamedMidiBinding
    {
        public string name;
        public int number;
        public int channel;
        public bool isControl;
    }
}
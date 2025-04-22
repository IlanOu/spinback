namespace Minis
{
    public class MidiInput
    {
        public int Number;
        public string ShortName;
        public int Channel;
        public float Velocity;
        public float Value;

        public static MidiInput FromNote(MidiNoteControl note, float velocity)
        {
            var minisInput = new MidiInput
            {
                Number = note.noteNumber,
                ShortName = note.shortDisplayName,
                Channel = (note.device as MidiDevice)?.channel ?? 0,
                Velocity = velocity,
                Value = 0
            };
            return minisInput;
        }
    
        public static MidiInput FromControl(MidiValueControl control, float value)
        {
            var minisInput = new MidiInput
            {
                Number = control.controlNumber,
                ShortName = control.shortDisplayName,
                Channel = (control.device as MidiDevice)?.channel ?? 0,
                Velocity = 0,
                Value = value
            };
            return minisInput;
        }
    }
}
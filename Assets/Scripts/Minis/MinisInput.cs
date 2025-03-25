namespace Minis
{
    public class MinisInput
    {
        public int Number;
        public string ShortName;
        public int Channel;
        public float Velocity;
        public float Value;

        public static MinisInput FromNote(MidiNoteControl note, float velocity)
        {
            var minisInput = new MinisInput
            {
                Number = note.noteNumber,
                ShortName = note.shortDisplayName,
                Channel = (note.device as MidiDevice)?.channel ?? 0,
                Velocity = velocity,
                Value = 0
            };
            return minisInput;
        }
    
        public static MinisInput FromControl(MidiValueControl control, float value)
        {
            var minisInput = new MinisInput
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
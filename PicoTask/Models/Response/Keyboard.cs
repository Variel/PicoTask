using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PicoTask.Models.Response
{
    public class Keyboard
    {
        public KeyboardType type { get; set; }
        public string[] buttons { get; set; }
        
        public static readonly Keyboard TextKeyboard = new Keyboard { type = KeyboardType.Text };
    }

    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum KeyboardType
    {
        Text,
        Buttons
    }
}

using System.Reflection;

namespace TelegramAudioBot.Core.Constants;

public abstract class Constants
{
    public static class Command
    {
        public const string HELP        = "/help";
        public const string ADD_VOICE   = "/addvoice";
        public const string UPDATE      = "/update";
        public const string CHANGE      = "/change";
        public const string GET_STORAGE = "/getstorage";
        public const string CLEAN       = "/clean";
        private static string[] AllCommands => GetAllValues();

        public static string[] GetAllCommands()
        {
            return AllCommands;
        }
        
        private static string[] GetAllValues()
        {
            var type = typeof(Command);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            string[] values = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                values[i] = fields[i].GetValue(null)?.ToString() ?? string.Empty;
            }
            return values;
        }
    }

    public static class Text
    {
        public const string ADD_NEW_VOICE = "Add new voice record in format fileId:title:keywords";
        public const string SEND_NEW_FILE = "Send new storage file";

    }

    
}
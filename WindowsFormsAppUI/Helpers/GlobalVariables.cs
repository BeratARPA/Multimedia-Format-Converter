using System.IO;

namespace WindowsFormsAppUI.Helpers
{
    public class GlobalVariables
    {
        public static ShellForm ShellForm { get; set; }
        public static IniFile iniFile = new IniFile(Path.Combine(FolderLocations.winFormUIFolderPath, "Settings.ini"));
    }
}

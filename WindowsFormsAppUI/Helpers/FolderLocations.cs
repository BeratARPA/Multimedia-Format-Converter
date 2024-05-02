using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WindowsFormsAppUI.Helpers
{
    public class FolderLocations
    {
        public static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string mgsFolderPath = Path.Combine(documentsPath, "MGS");
        public static string winFormUIFolderPath = Path.Combine(mgsFolderPath, "WinFormUI");

        public static void CreateFolders()
        {
            if (!Directory.Exists(mgsFolderPath))
            {
                Directory.CreateDirectory(mgsFolderPath);
                GrantAccess(mgsFolderPath);
            }

            if (!Directory.Exists(winFormUIFolderPath))
            {
                Directory.CreateDirectory(winFormUIFolderPath);
                GrantAccess(winFormUIFolderPath);
            }
        }

        public static void GrantAccess(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();
            directorySecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            directoryInfo.SetAccessControl(directorySecurity);

        }
    }
}

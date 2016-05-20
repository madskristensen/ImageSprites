using System.IO;

namespace ImageSprites
{
    static class FileEvents
    {
        internal static void OnSaving(string fileName)
        {
            if (Saving != null)
            {
                var type = File.Exists(fileName) ? WatcherChangeTypes.Changed : WatcherChangeTypes.Created;
                var dir = Path.GetDirectoryName(fileName);
                var name = Path.GetFileName(fileName);

                Saving(null, new FileSystemEventArgs(type, dir, name));
            }
        }

        internal static void OnSaved(string fileName)
        {
            if (Saved != null)
            {
                var type = File.Exists(fileName) ? WatcherChangeTypes.Changed : WatcherChangeTypes.Created;
                var dir = Path.GetDirectoryName(fileName);
                var name = Path.GetFileName(fileName);

                Saved(null, new FileSystemEventArgs(type, dir, name));
            }
        }

        public static event FileSystemEventHandler Saving;
        public static event FileSystemEventHandler Saved;
    }
}

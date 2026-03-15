using System.Runtime.InteropServices;

namespace FatSorter;

public static class FileSystemSync
{
    public static void Flush()
    {
        if (OperatingSystem.IsLinux())
        {
            LinuxSync();
            Thread.Sleep(100);
        }
        else if (OperatingSystem.IsMacOS())
        {
            MacSync();
            Thread.Sleep(100);
        }
        // Windows uses write-through caching for removable drives by default,
        // so explicit sync is not required for the typical FAT volume use case.
    }

    [DllImport("libc", EntryPoint = "sync")]
    private static extern void LinuxSync();

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "sync")]
    private static extern void MacSync();
}

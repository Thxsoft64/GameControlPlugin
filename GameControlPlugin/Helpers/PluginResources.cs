using System.Reflection;

namespace Loupedeck.GameControlPlugin.Helpers
{
    // A helper class for managing plugin resources.
    // Note that the resource files handled by this class must be embedded in the plugin assembly at compile time.
    // That is, the Build Action of the files must be "Embedded Resource" in the plugin project.

    internal static class PluginResources
    {
        private static Assembly _assembly;

        public static void Init(Assembly assembly)
        {
            assembly.CheckNullArgument(nameof(assembly));
            _assembly = assembly;
        }

        // Retrieves the names of all the resource files in the specified folder.
        // The parameter `folderName` must be specified as a full path, for example, `Loupedeck.ExamplePlugin.Resources`.
        // Returns the full names of the resource files, for example, `Loupedeck.ExamplePlugin.Resources.Resource.txt`.
        public static string[] GetFilesInFolder(string folderName) => _assembly.GetFilesInFolder(folderName);

        // Finds the first resource file with the specified file name.
        // Returns the full name of the found resource file.
        // Throws `FileNotFoundException` if the resource file is not found.
        public static string FindFile(string fileName) => _assembly.FindFileOrThrow(fileName);

        // Finds all the resource files that match the specified regular expression pattern.
        // Returns the full names of the found resource files.
        // Example:
        //     `PluginResources.FindFiles(@"\w+\.txt$")` returns all the resource files with the extension `.txt`.
        public static string[] FindFiles(string regexPattern) => _assembly.FindFiles(regexPattern);

        // Finds the first resource file with the specified file name, and returns the file as a stream.
        // Throws `FileNotFoundException` if the resource file is not found.
        public static Stream GetStream(string resourceName) => _assembly.GetStream(FindFile(resourceName));

        // Reads content of the specified text file, and returns the file content as a string.
        // Throws `FileNotFoundException` if the resource file is not found.
        public static string ReadTextFile(string resourceName) => _assembly.ReadTextFile(FindFile(resourceName));

        // Reads content of the specified binary file, and returns the file content as bytes.
        // Throws `FileNotFoundException` if the resource file is not found.
        public static byte[] ReadBinaryFile(string resourceName) => _assembly.ReadBinaryFile(FindFile(resourceName));

        // Reads content of the specified image file, and returns the file content as a bitmap image.
        // Throws `FileNotFoundException` if the resource file is not found.
        public static BitmapImage ReadImage(string resourceName) => _assembly.ReadImage(FindFile(resourceName));

        // Extracts the specified resource file to the given file path in the file system.
        // Throws `FileNotFoundException` if the resource file is not found, or a system exception if the output file cannot be written.
        public static void ExtractFile(string resourceName, string filePathName)
            => _assembly.ExtractFile(FindFile(resourceName), filePathName);
    }
}

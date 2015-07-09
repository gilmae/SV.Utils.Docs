using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV.Utils.Docs
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("USAGE: docs input_path output_path theme_path");
                return;
            }

            string root = args[0];
            string generatedRoot = args[1];
            string themeRoot = args[2];

            if (Directory.Exists(Path.Combine(themeRoot, "assets")))
            {
                CopyDirectory(Path.Combine(themeRoot, "assets"), Path.Combine(generatedRoot, "assets"), true);
            }

            if (!Directory.Exists(generatedRoot))
            {
                Directory.CreateDirectory(generatedRoot);
            }

            // Get Page Template
            string pageTemplatePath = Path.Combine(themeRoot, "page.html");
            if (!File.Exists(pageTemplatePath))
            {
                Console.WriteLine("Template missing.");
                return;
            }

            string pageTemplate = File.ReadAllText(pageTemplatePath);

            ProcessDirectory(root, generatedRoot, pageTemplate);
        }

        private static void ProcessDirectory(string root, string generatedRoot, string pageTemplate)
        {
            if (!Directory.Exists(generatedRoot))
            {
                Directory.CreateDirectory(generatedRoot);
            }
            // Generate Index page
            if (File.Exists(Path.Combine(root, "index.markdown")))
            {
                CreatePage(Path.Combine(root, "index.markdown"), Path.Combine(generatedRoot, "index.html"), pageTemplate);
            }

            string pagesRoot = Path.Combine(root, "_pages");

            if (Directory.Exists(pagesRoot))
            {
                string[] files = Directory.GetFiles(pagesRoot, "*.markdown");

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    string fileRoot = Path.Combine(generatedRoot, fileInfo.Name.Replace(fileInfo.Extension, ""));
                    if (!Directory.Exists(fileRoot))
                    {
                        Directory.CreateDirectory(fileRoot);
                    }

                    string outputFile = Path.Combine(fileRoot, "index.html");

                    CreatePage(file, outputFile, pageTemplate);
                }
            }

            foreach (string dir in Directory.GetDirectories(root))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                string name = dirInfo.Name;
                if (!name.StartsWith("_"))
                {
                    ProcessDirectory(Path.Combine(root, name), Path.Combine(generatedRoot, name), pageTemplate);
                }
            }
        }

        private static void CreatePage(string source, string destination, string pageTemplate)
        {
            using (var reader = new System.IO.StreamReader(source))
            {
                StringBuilder markdown = new StringBuilder();
                StringWriter stringwriter = new StringWriter(markdown);
                CommonMark.CommonMarkConverter.Convert(reader, stringwriter);

                using (var writer = new System.IO.StreamWriter(destination))
                {
                    writer.Write(pageTemplate.Replace("{{content}}", markdown.ToString()));
                }
            }

        }

        private static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {


            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}

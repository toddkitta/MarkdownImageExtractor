using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownImageExtractor
{
    public class Program
    {
        private const string IN_FILE = @"C:\Users\tkitta\OneDrive - Microsoft\Docs\cortanamanual.md";
        private const string OUT_FILE = @"C:\temp\out.md";
        private const string OUT_IMAGES_FOLDER = @"C:\temp\mdimages";
        private const string IMAGE_URL_PREFIX = @"images/";

        public static void Main(string[] args)
        {
            if (!Init())
                return;

            using (StreamWriter output = new StreamWriter(OUT_FILE))
            {
                string currentTaskName = null;
                int imageInTaskCount = 0;

                foreach (string line in File.ReadLines(IN_FILE))
                {
                    string outLine = null;
                    
                    // see if this line is a task
                    if(line.StartsWith("### Task"))
                    {
                        currentTaskName = GetTaskName(line);
                        imageInTaskCount = 0;
                        outLine = line;
                    }
                    // see if this line is an image
                    else if(line.StartsWith(" ![](data:image"))
                    {
                        outLine = SaveImageAndGetMarkdownLine(line, currentTaskName, imageInTaskCount++);
                    }
                    else
                    {
                        outLine = line;
                    }

                    output.WriteLine(outLine);
                }
            }
        }

        private static string SaveImageAndGetMarkdownLine(string line, string currentTaskName, int imageCount)
        {
            string base64 = line.Substring(line.IndexOf(',') + 1);
            base64 = base64.Remove(base64.Length - 1);

            byte[] data = Convert.FromBase64String(base64);

            string fileName = $"{currentTaskName}_{imageCount}.png";
            string filePath = Path.Combine(OUT_IMAGES_FOLDER, fileName);

            File.WriteAllBytes(filePath, data);

            string markdownOutput = $"![Screenshot]({IMAGE_URL_PREFIX}{fileName})";

            return markdownOutput;
        }

        private static string GetTaskName(string line)
        {
            string output = line.Substring(line.IndexOf(':') + 1);
            return output.Trim().ToLower().Replace(' ', '_');
        }

        private static bool Init()
        {
            if(!File.Exists(IN_FILE))
            {
                Console.WriteLine("No input file...");
                return false;
            }

            string outFolder = Path.GetDirectoryName(OUT_FILE);

            if (!Directory.Exists(outFolder))
                Directory.CreateDirectory(outFolder);

            if (File.Exists(OUT_FILE))
                File.Delete(OUT_FILE);

            if (Directory.Exists(OUT_IMAGES_FOLDER))
            {
                foreach(string file in Directory.EnumerateFiles(OUT_IMAGES_FOLDER))
                    File.Delete(file);

                Directory.Delete(OUT_IMAGES_FOLDER);
            }

            Directory.CreateDirectory(OUT_IMAGES_FOLDER);

            return true;
        }
    }
}

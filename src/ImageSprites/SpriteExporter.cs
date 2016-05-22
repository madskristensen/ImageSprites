using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageSprites
{
    internal class SpriteExporter
    {
        public async static Task ExportStylesheet(IEnumerable<SpriteFragment> fragments, SpriteDocument doc)
        {
            foreach (var format in doc.Exports)
            {
                string outputFile = Path.ChangeExtension(doc.FileName, "." + format.ToString().ToLowerInvariant());
                var outputDirectory = Path.GetDirectoryName(outputFile);
                var bgUrl = MakeRelative(outputFile, Path.ChangeExtension(doc.FileName, doc.OutputExtension));
                StringBuilder sb = new StringBuilder(GetDescription(format));

                foreach (SpriteFragment fragment in fragments)
                {
                    sb.AppendLine(GetSelector(fragment.FileName, format) + " {");
                    sb.AppendLine("/* You may have to set 'display: block' */");
                    sb.AppendLine("\twidth: " + fragment.Width + "px;");
                    sb.AppendLine("\theight: " + fragment.Height + "px;");
                    sb.AppendLine("\tbackground: url('" + bgUrl + "') -" + fragment.X + "px -" + fragment.Y + "px;");
                    sb.AppendLine("}");
                }

                if (File.Exists(outputFile) && File.ReadAllText(outputFile) == sb.ToString())
                    return;

                Directory.CreateDirectory(outputDirectory);

                using (var writer = new StreamWriter(outputFile))
                {
                    await writer.WriteAsync(sb.ToString().Replace("-0px", "0"));
                }
            }
        }

        private static string MakeRelative(string fileBase, string file)
        {
            Uri address1 = new Uri(fileBase);// Create a new Uri from a string.
            Uri address2 = new Uri(file);

            return address1.MakeRelativeUri(address2).ToString();
        }

        private static string GetDescription(ExportFormat format)
        {
            string text = "This is an example of how to use the image sprite in your own CSS files";

            if (format != ExportFormat.Css)
                text = "@import this file directly into your existing " + format + " files to use these mixins";

            return "/*" + Environment.NewLine + text + Environment.NewLine + "*/" + Environment.NewLine;
        }

        private static string GetSelector(string fileName, ExportFormat format)
        {
            string className = Path.GetFileNameWithoutExtension(fileName);

            if (format == ExportFormat.Less)
                return ".sprite-" + className + "()";
            else if (format == ExportFormat.Scss)
                return "@mixin sprite-" + className + "()";

            return "." + className;
        }
    }
}
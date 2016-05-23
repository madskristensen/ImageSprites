using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageSprites
{
    internal class SpriteExporter
    {
        public async static Task ExportStylesheet(IEnumerable<SpriteFragment> fragments, SpriteDocument doc, SpriteGenerator generator)
        {
            if (doc?.Stylesheets == null)
                return;

            var mainClass = Path.GetFileNameWithoutExtension(doc.FileName).ToLowerInvariant().Replace(" ", "");

            foreach (var format in doc.Stylesheets.Formats)
            {
                string outputFile = doc.FileName + "." + format.ToString().ToLowerInvariant();
                var outputDirectory = Path.GetDirectoryName(outputFile);
                var bgUrl = MakeRelative(outputFile, doc.FileName + doc.OutputExtension);

                StringBuilder sb = new StringBuilder(GetDescription(format));

                if (format == ExportFormat.Css)
                {
                    sb.AppendLine($".{mainClass} {{");
                    sb.AppendLine($"\tbackground-image: url('{doc.Stylesheets.Root + bgUrl}');");
                    sb.AppendLine($"\tbackground-repeat: no-repeat;");
                    sb.AppendLine($"\tdisplay: block;");
                    sb.AppendLine("}");
                }

                foreach (SpriteFragment fragment in fragments)
                {
                    if (format == ExportFormat.Css)
                    {
                        sb.AppendLine($"{GetSelector(fragment.ID, mainClass, format)} {{");
                        sb.AppendLine($"\twidth: {fragment.Width}px;");
                        sb.AppendLine($"\theight: {fragment.Height}px;");
                        sb.AppendLine($"\tbackground-position: -{fragment.X}px -{fragment.Y}px;");
                        sb.AppendLine("}");
                    }
                    else
                    {
                        sb.AppendLine(GetSelector(fragment.ID, mainClass, format) + " {");
                        sb.AppendLine($"\twidth: {fragment.Width}px;");
                        sb.AppendLine($"\theight: {fragment.Height}px;");
                        sb.AppendLine($"\tdisplay: block;");
                        sb.AppendLine($"\tbackground: url('{doc.Stylesheets.Root + bgUrl}') -{fragment.X}px -{fragment.Y}px no-repeat;");
                        sb.AppendLine("}");
                    }
                }

                if (File.Exists(outputFile) && File.ReadAllText(outputFile) == sb.ToString())
                    continue;

                Directory.CreateDirectory(outputDirectory);

                using (var writer = new StreamWriter(outputFile))
                {
                    generator.OnSaving(outputFile, doc);
                    await writer.WriteAsync(sb.ToString().Replace("-0px", "0"));
                    generator.OnSaved(outputFile, doc);
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

        private static string GetSelector(string ident, string mainClass, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Less:
                    return $".{mainClass}-{ident}()";
                case ExportFormat.Scss:
                    return $"@mixin {mainClass}-{ident}()";
                default: // CSS
                    return $".{mainClass}.{ident}";
            }
        }
    }
}
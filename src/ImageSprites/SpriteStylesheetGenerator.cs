using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageSprites
{
    internal class SpriteStylesheetGenerator
    {
        public async static Task ExportStylesheet(IEnumerable<SpriteFragment> fragments, SpriteDocument doc, SpriteImageGenerator generator)
        {
            if (doc.Stylesheet == Stylesheet.None)
                return;

            var mainClass = SpriteHelpers.GetIdentifier(doc.FileName);

            string outputFile = doc.FileName + "." + doc.Stylesheet.ToString().ToLowerInvariant();
            var outputDirectory = Path.GetDirectoryName(outputFile);
            var bgUrl = doc.PathPrefix + SpriteHelpers.MakeRelative(outputFile, doc.FileName + doc.OutputExtension);

            StringBuilder sb = new StringBuilder(GetDescription(doc.Stylesheet));

            if (doc.Stylesheet == Stylesheet.Css)
            {
                sb.AppendLine($".{mainClass} {{");
                sb.AppendLine($"\tbackground-image: url('{bgUrl}');");
                sb.AppendLine($"\tbackground-repeat: no-repeat;");
                AddCustomStyles(doc.CustomStyles, sb);
                sb.AppendLine("}");
            }

            foreach (SpriteFragment fragment in fragments)
            {
                if (doc.Stylesheet == Stylesheet.Css)
                {
                    sb.AppendLine($"{GetSelector(fragment.ID, mainClass, doc.Stylesheet)} {{");
                    sb.AppendLine($"\twidth: {fragment.Width}px;");
                    sb.AppendLine($"\theight: {fragment.Height}px;");
                    sb.AppendLine($"\tbackground-position: -{fragment.X}px -{fragment.Y}px;");
                    sb.AppendLine("}");
                }
                else if (doc.Stylesheet == Stylesheet.Styl)
                {
                    sb.AppendLine(GetSelector(fragment.ID, mainClass, doc.Stylesheet));
                    sb.AppendLine($"\twidth {fragment.Width}px");
                    sb.AppendLine($"\theigh {fragment.Height}px");
                    sb.AppendLine($"\tbackground-position -{fragment.X}px -{fragment.Y}px");
                }
                else
                {
                    sb.AppendLine(GetSelector(fragment.ID, mainClass, doc.Stylesheet) + " {");
                    sb.AppendLine($"\twidth: {fragment.Width}px;");
                    sb.AppendLine($"\theight: {fragment.Height}px;");
                    AddCustomStyles(doc.CustomStyles, sb);
                    sb.AppendLine($"\tbackground: url('{bgUrl}') -{fragment.X}px -{fragment.Y}px no-repeat;");
                    sb.AppendLine("}");
                }
            }

            if (File.Exists(outputFile) && File.ReadAllText(outputFile) == sb.ToString())
                return;

            Directory.CreateDirectory(outputDirectory);

            using (var writer = new StreamWriter(outputFile))
            {
                generator.OnSaving(outputFile, doc);
                await writer.WriteAsync(sb.ToString().Replace("-0px", "0"));
                generator.OnSaved(outputFile, doc);
            }
        }

        private static void AddCustomStyles(IDictionary<string, object> customStyles, StringBuilder sb)
        {
            foreach (string property in customStyles.Keys)
            {
                sb.AppendLine($"\t{property}: {customStyles[property]};");
            }
        }

        private static string GetDescription(Stylesheet format)
        {
            string text = "This is an example of how to use the image sprite in your own CSS files";

            if (format != Stylesheet.Css)
                text = "@import this file directly into your existing " + format + " files to use these mixins";

            return "/*" + Environment.NewLine + text + Environment.NewLine + "*/" + Environment.NewLine;
        }

        private static string GetSelector(string ident, string mainClass, Stylesheet format)
        {
            switch (format)
            {
                case Stylesheet.Less:
                    return $".{mainClass}-{ident}()";
                case Stylesheet.Scss:
                    return $"@mixin {mainClass}-{ident}()";
                case Stylesheet.Styl:
                    return $"{mainClass}-{ident}()";
                default: // CSS
                    return $".{mainClass}.{ident}";
            }
        }
    }
}
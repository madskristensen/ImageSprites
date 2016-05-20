using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageSprites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageSpritesTest
{
    [TestClass]
    public class GenerateTest
    {
        private string _artifacts;

        [TestInitialize]
        public void Setup()
        {
            _artifacts = new DirectoryInfo(@"..\..\Artifacts").FullName;

        }

        [TestMethod]
        public async Task Png96Dpi()
        {
            var fileName = Path.Combine(_artifacts, "png96.sprite");
            var outFile = Path.ChangeExtension(fileName, ".jpg");

            try
            {
                var doc = await SpriteDocument.FromFile(fileName);
                var fragments = SpriteGenerator.Generate(doc);

                using (var image = Image.FromFile(outFile))
                {
                    Assert.AreEqual(56, image.Height); // 16 + padding
                    Assert.AreEqual(236, image.Width); // 16 * 6 + padding
                }
            }
            finally
            {
                File.Delete(outFile);
            }
        }
    }
}

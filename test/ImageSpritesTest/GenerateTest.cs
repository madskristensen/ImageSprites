using System;
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
        private SpriteGenerator _generator;

        [TestInitialize]
        public void Setup()
        {
            _artifacts = new DirectoryInfo(@"..\..\Artifacts").FullName;
            _generator = new SpriteGenerator();
        }

        [TestMethod]
        public async Task Png96Dpi()
        {
            var fileName = Path.Combine(_artifacts, "png96.sprite");
            var imgFile = Path.ChangeExtension(fileName, ".jpg");
            var lessFile = Path.ChangeExtension(fileName, ".less");

            try
            {
                var doc = await SpriteDocument.FromFile(fileName);
                await _generator.Generate(doc);

                using (var image = Image.FromFile(imgFile))
                {
                    Assert.AreEqual(56, image.Height); // 16 + padding
                    Assert.AreEqual(236, image.Width); // 16 * 6 + padding
                }

                string less = File.ReadAllText(lessFile);
                Assert.IsTrue(less.Contains(".sprite-a()"), "Sprite \"a.png\" not generated");
                Assert.IsTrue(less.Contains("url('png96.jpg')"), "Incorrect url value");
            }
            finally
            {
                File.Delete(imgFile);
                File.Delete(lessFile);
            }
        }

        [TestMethod]
        public async Task Png384Dpi()
        {
            var fileName = Path.Combine(_artifacts, "png384.sprite");
            var imgFile = Path.ChangeExtension(fileName, ".png");

            try
            {
                var doc = await SpriteDocument.FromFile(fileName);
                await _generator.Generate(doc);

                using (var image = Image.FromFile(imgFile))
                {
                    Assert.AreEqual(384, Math.Round(image.HorizontalResolution), "Not 384 DPI");
                    Assert.AreEqual(166, image.Height); // 16 + padding
                    Assert.AreEqual(36, image.Width); // 16 * 6 + padding
                }
            }
            finally
            {
                File.Delete(imgFile);
            }
        }
    }
}

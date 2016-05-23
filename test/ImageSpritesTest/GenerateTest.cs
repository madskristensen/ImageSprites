using System;
using System.Linq;
using System.Drawing;
using System.IO;
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
            _generator.Saving += SavingEventHandler;
            _generator.Saved += SavingEventHandler;
        }

        private void SavingEventHandler(object sender, SpriteImageGenerationEventArgs e)
        {
            Assert.IsTrue(e.Document.Images.Any());
            Assert.IsTrue(e.FileName.StartsWith(e.Document.FileName));
        }

        [TestMethod]
        public async Task Png96Dpi()
        {
            var fileName = Path.Combine(_artifacts, "png96.sprite");
            var imgFile = fileName + ".jpg";
            var lessFile = fileName + ".less";

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
                Assert.IsTrue(less.Contains(".png96-a()"), "Sprite \"a.png\" not generated");
                Assert.IsTrue(less.Contains("url('png96.sprite.jpg')"), "Incorrect url value");
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
            var imgFile = fileName + ".png";
            var cssFile = fileName + ".css";

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

                string css = File.ReadAllText(cssFile);
                Assert.IsTrue(css.Contains(".png384.a"), "Sprite \"a.png\" not generated");
                Assert.IsTrue(css.Contains("url('png384.sprite.png')"), "Incorrect url value");
            }
            finally
            {
                File.Delete(imgFile);
                File.Delete(cssFile);
            }
        }
    }
}

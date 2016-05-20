using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageSprites;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageSpritesTest
{
    [TestClass]
    public class SerializationTest
    {
        private string _fileName;
        private string _artifacts;

        [TestInitialize]
        public void Setup()
        {
            _artifacts = new DirectoryInfo(@"..\..\Artifacts").FullName;
            _fileName = Path.Combine(_artifacts, "file.sprite");
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(_fileName);
        }

        [TestMethod]
        public async Task CreateDocument()
        {
            var original = new SpriteDocument(_fileName);
            original.Images = new[] { "img/a.png", "img/b.png" };
            original.Format = ImageType.Jpg;
            await original.Save();

            var read = await SpriteDocument.FromFile(_fileName);

            Assert.AreEqual(original.Direction, read.Direction);
            Assert.AreEqual(original.FileName, read.FileName);
            Assert.AreEqual(original.Format, read.Format);
            Assert.AreEqual(original.Images.Count(), read.Images.Count());

            var input = new FileInfo(Path.Combine(_artifacts, "img/a.png")).FullName;
            Assert.AreEqual(input, read.ToAbsoluteImages().First());
        }
    }
}

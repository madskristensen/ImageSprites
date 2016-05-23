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

            SpriteDocument.Saving += SavingEventHandler;
            SpriteDocument.Saved += SavingEventHandler;
        }

        private void SavingEventHandler(object sender, FileSystemEventArgs e)
        {
            Assert.IsTrue(e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created);
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(_fileName);
        }

        [TestMethod]
        public async Task CreateDocument()
        {
            var img = Path.Combine(_artifacts, "images/96dpi/a.png");
            var original = new SpriteDocument(_fileName, new[] { img });
            original.Output = ImageType.Jpg;
            await original.Save();

            var read = await SpriteDocument.FromFile(_fileName);

            Assert.AreEqual(original.Orientation, read.Orientation);
            Assert.AreEqual(original.FileName, read.FileName);
            Assert.AreEqual(original.Output, read.Output);
            Assert.AreEqual(original.Images.Count(), read.Images.Count());

            var input = new FileInfo(Path.Combine(_artifacts, "images/96dpi/a.png")).FullName;
            Assert.AreEqual(input, read.ToAbsoluteImages().First().Value);
        }

        [TestMethod]
        public void FromJsonFail()
        {
            try
            {
                SpriteDocument.FromJSON("\"images\": {\"a\": \"dontexist.png\"}}");
            }
            catch (SpriteParseException ex)
            {
                Assert.IsNull(ex.FileName);
            }
        }

        [TestMethod, ExpectedException(typeof(FileNotFoundException))]
        public async Task FromFileFail()
        {
            await SpriteDocument.FromFile("doesntexist.json");
        }
    }
}

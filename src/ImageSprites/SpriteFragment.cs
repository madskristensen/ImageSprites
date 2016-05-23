
namespace ImageSprites
{
    internal class SpriteFragment
    {
        public SpriteFragment(string id, string fileName, int width, int height, int x, int y)
        {
            ID = id;
            FileName = fileName;
            Width = width;
            Height = height;
            X = x;
            Y = y;
        }

        public string ID { get; set; }
        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
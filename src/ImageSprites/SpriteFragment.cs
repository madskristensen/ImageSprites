
namespace ImageSprites
{
    internal class SpriteFragment
    {
        public SpriteFragment(string id, int width, int height, int x, int y)
        {
            ID = id;
            Width = width;
            Height = height;
            X = x;
            Y = y;
        }

        public string ID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
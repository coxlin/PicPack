using System.Collections.Generic;

namespace BitmapJSONLib
{
    public class BitmapJSONElement
    {
        public string Name;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public bool Rotate;
    }

    public class BitmapJSON
    {
        public string Name;
        public List<BitmapJSONElement> Images;
    }
}

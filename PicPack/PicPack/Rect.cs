/** Based on file Rect.h
	@author Jukka Jylänki
	This work is released to Public Domain, do whatever you want with it.
*/

namespace PicPack
{
    public class RectSize
    {
        public int Width;
        public int Height;
    }

    public class Rect
    {
        public int Width;
        public int Height;
        public int X;
        public int Y;
    }

    public static class RectUtils
    {
        public static bool IsContainedIn(Rect a, Rect b)
        {
            return a.X >= b.X && a.Y >= b.Y && a.X + a.Width <= b.X + b.Width && a.Y + a.Height <= b.Y + b.Height;
        }
    }
}

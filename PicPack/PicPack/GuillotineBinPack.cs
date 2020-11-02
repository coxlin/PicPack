using System;
using System.Collections.Generic;

/** Based on file GuillotineBinPack.cpp
	@author Jukka Jylänki
	@brief Implements different bin packer algorithms that use the GUILLOTINE data structure.
	This work is released to Public Domain, do whatever you want with it.
https://github.com/juj/RectangleBinPack/blob/master/GuillotineBinPack.cpp
*/

namespace PicPack
{
    public sealed class GuillotineBinPack
    {
        public readonly List<Rect> FreeRectangles = new List<Rect>();
        public readonly List<Rect> UsedRectangles = new List<Rect>();

        private int _binWidth, _binHeight;

        public void Init(int width, int height)
        {
            _binWidth = width;
            _binHeight = height;
            UsedRectangles.Clear();

            Rect n = new Rect();
            n.X = n.Y = 0;
            n.Width = width;
            n.Height = height;

            FreeRectangles.Clear();
        }

        #region INSERTION
        public Rect Insert(
            int width,
            int height,
            bool merge,
            FreeRectChoiceHeuristic rectChoice,
            GuillotineSplitHeuristic splitMethod)
        {
            // Find where to put the new rectangle.
            int freeNodeIndex = 0;
            Rect newRect = FindPositionForNewNode(width, height, rectChoice, ref freeNodeIndex);

            // Abort if we didn't have enough space in the bin.
            if (newRect.Height == 0)
                return newRect;

            // Remove the space that was just consumed by the new rectangle.
            SplitFreeRectByHeuristic(FreeRectangles[freeNodeIndex], newRect, splitMethod);
            FreeRectangles.RemoveAt(freeNodeIndex);

            // Perform a Rectangle Merge step if desired.
            if (merge)
            {
                MergeFreeList();
            }

            // Remember the new used rectangle.
            UsedRectangles.Add(newRect);

            return newRect;
        }

        public void Insert(
            List<RectSize> rects,
            bool merge,
            FreeRectChoiceHeuristic rectChoice,
            GuillotineSplitHeuristic splitMethod)
        {
            int bestFreeRect = 0;
            int bestRect = 0;
            bool bestFlipped = false;
            while (rects.Count > 0)
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < FreeRectangles.Count; ++i)
                {
                    for (int j = 0; j < rects.Count; ++j)
                    {
                        // If this rectangle is a perfect match, we pick it instantly.
                        if (rects[j].Width == FreeRectangles[i].Width && rects[j].Height == FreeRectangles[i].Height)
                        {
                            bestFreeRect = i;
                            bestRect = j;
                            bestFlipped = false;
                            bestScore = int.MinValue;
                            i = FreeRectangles.Count; // Force a jump out of the outer loop as well - we got an instant fit.
                            break;
                        }
                        // If flipping this rectangle is a perfect match, pick that then.
                        else if (rects[j].Height == FreeRectangles[i].Width && rects[j].Width == FreeRectangles[i].Height)
                        {
                            bestFreeRect = i;
                            bestRect = j;
                            bestFlipped = true;
                            bestScore = int.MinValue;
                            i = FreeRectangles.Count; // Force a jump out of the outer loop as well - we got an instant fit.
                            break;
                        }
                        // Try if we can fit the rectangle upright.
                        else if (rects[j].Width <= FreeRectangles[i].Width && rects[j].Height <= FreeRectangles[i].Height)
                        {
                            int score = ScoreByHeuristic(rects[j].Width, rects[j].Height, FreeRectangles[i], rectChoice);
                            if (score < bestScore)
                            {
                                bestFreeRect = i;
                                bestRect = j;
                                bestFlipped = false;
                                bestScore = score;
                            }
                        }
                        // If not, then perhaps flipping sideways will make it fit?
                        else if (rects[j].Height <= FreeRectangles[i].Width && rects[j].Width <= FreeRectangles[i].Height)
                        {
                            int score = ScoreByHeuristic(rects[j].Height, rects[j].Width, FreeRectangles[i], rectChoice);
                            if (score < bestScore)
                            {
                                bestFreeRect = i;
                                bestRect = j;
                                bestFlipped = true;
                                bestScore = score;
                            }
                        }
                    }
                }

                // If we didn't manage to find any rectangle to pack, abort.
                if (bestScore == int.MaxValue)
                    return;

                // Otherwise, we're good to go and do the actual packing.
                Rect newNode = new Rect();
                newNode.X = FreeRectangles[bestFreeRect].X;
                newNode.Y = FreeRectangles[bestFreeRect].Y;
                newNode.Width = rects[bestRect].Width;
                newNode.Height = rects[bestRect].Height;

                if (bestFlipped)
                {
                    Swap(ref newNode.Width, ref newNode.Height);
                }

                // Remove the free space we lost in the bin.
                SplitFreeRectByHeuristic(FreeRectangles[bestFreeRect], newNode, splitMethod);
                FreeRectangles.RemoveAt(bestFreeRect);

                // Remove the rectangle we just packed from the input list.
                rects.RemoveAt(bestRect);

                // Perform a Rectangle Merge step if desired.
                if (merge)
                {
                    MergeFreeList();
                }

                // Remember the new used rectangle.
                UsedRectangles.Add(newNode);

            }
        }
        #endregion

        #region FIT
        public bool Fits(RectSize r, Rect freeRect)
        {
            return (r.Width <= freeRect.Width && r.Height <= freeRect.Height) ||
                (r.Height <= freeRect.Width && r.Width <= freeRect.Height);
        }

        /// @return True if r fits perfectly inside freeRect, i.e. the leftover area is 0.
        public bool FitsPerfectly(RectSize r, Rect freeRect)
        {
            return (r.Width == freeRect.Width && r.Height == freeRect.Height) ||
                (r.Height == freeRect.Width && r.Width == freeRect.Height);
        }
        #endregion

        #region OCCUPANCY
        private float Occupancy()
        {
            long usedSurfaceArea = 0;
            for (int i = 0; i < UsedRectangles.Count; ++i)
            {
                usedSurfaceArea += (long)(UsedRectangles[i].Width * UsedRectangles[i].Height);
            }

            return (float)usedSurfaceArea / (_binWidth * _binHeight);
        }
        #endregion

        #region SCORING

        private static int ScoreByHeuristic(int width, int height, Rect freeRect, FreeRectChoiceHeuristic rectChoice)
        {
            switch (rectChoice)
            {
                case FreeRectChoiceHeuristic.RectBestAreaFit: return ScoreBestAreaFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectBestShortSideFit: return ScoreBestShortSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectBestLongSideFit: return ScoreBestLongSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstAreaFit: return ScoreWorstAreaFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstShortSideFit: return ScoreWorstShortSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstLongSideFit: return ScoreWorstLongSideFit(width, height, freeRect);
                default: throw new Exception();
            }
        }

        private static int ScoreBestAreaFit(int width, int height, Rect freeRect)
        {
            return freeRect.Width * freeRect.Height - width * height;
        }

        private static int ScoreBestShortSideFit(int width, int height, Rect freeRect)
        {
            int leftoverHoriz = Math.Abs(freeRect.Width - width);
            int leftoverVert = Math.Abs(freeRect.Height - height);
            int leftover = Math.Min(leftoverHoriz, leftoverVert);
            return leftover;
        }


        private static int ScoreBestLongSideFit(int width, int height, Rect freeRect)
        {
            int leftoverHoriz = Math.Abs(freeRect.Width - width);
            int leftoverVert = Math.Abs(freeRect.Height - height);
            int leftover = Math.Max(leftoverHoriz, leftoverVert);
            return leftover;
        }

        private static int ScoreWorstAreaFit(int width, int height, Rect freeRect)
        {
            return -ScoreBestAreaFit(width, height, freeRect);
        }

        private static int ScoreWorstShortSideFit(int width, int height, Rect freeRect)
        {
            return -ScoreBestShortSideFit(width, height, freeRect);
        }

        private static int ScoreWorstLongSideFit(int width, int height, Rect freeRect)
        {
            return -ScoreBestLongSideFit(width, height, freeRect);
        }
        #endregion

        #region NODE POS
        private Rect FindPositionForNewNode(int width, int height, FreeRectChoiceHeuristic rectChoice, ref int nodeIndex)
        {
            Rect bestNode = new Rect();
            int bestScore = int.MaxValue;

            /// Try each free rectangle to find the best one for placement.
            for (int i = 0; i < FreeRectangles.Count; ++i)
            {
                // If this is a perfect fit upright, choose it immediately.
                if (width == FreeRectangles[i].Width && height == FreeRectangles[i].Height)
                {
                    bestNode.X = FreeRectangles[i].X;
                    bestNode.Y = FreeRectangles[i].Y;
                    bestNode.Width = width;
                    bestNode.Height = height;
                    bestScore = int.MinValue;
                    nodeIndex = i;
                    break;
                }
                // If this is a perfect fit sideways, choose it.
                else if (height == FreeRectangles[i].Width && width == FreeRectangles[i].Height)
                {
                    bestNode.X = FreeRectangles[i].X;
                    bestNode.Y = FreeRectangles[i].Y;
                    bestNode.Width = height;
                    bestNode.Height = width;
                    bestScore = int.MinValue;
                    nodeIndex = i;
                    break;
                }
                // Does the rectangle fit upright?
                else if (width <= FreeRectangles[i].Width && height <= FreeRectangles[i].Height)
                {
                    int score = ScoreByHeuristic(width, height, FreeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestNode.X = FreeRectangles[i].X;
                        bestNode.Y = FreeRectangles[i].Y;
                        bestNode.Width = width;
                        bestNode.Height = height;
                        bestScore = score;
                        nodeIndex = i;
                    }
                }
                // Does the rectangle fit sideways?
                else if (height <= FreeRectangles[i].Width && width <= FreeRectangles[i].Height)
                {
                    int score = ScoreByHeuristic(height, width, FreeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestNode.X = FreeRectangles[i].X;
                        bestNode.Y = FreeRectangles[i].Y;
                        bestNode.Width = height;
                        bestNode.Height = width;
                        bestScore = score;
                        nodeIndex = i;
                    }
                }
            }
            return bestNode;
        }
        #endregion

        #region SPLITTING
        private void SplitFreeRectByHeuristic(Rect freeRect, Rect placedRect, GuillotineSplitHeuristic method)
        {
            // Compute the lengths of the leftover area.
            int w = freeRect.Width - placedRect.Width;
            int h = freeRect.Height - placedRect.Height;

            // Placing placedRect into freeRect results in an L-shaped free area, which must be split into
            // two disjoint rectangles. This can be achieved with by splitting the L-shape using a single line.
            // We have two choices: horizontal or vertical.	

            // Use the given heuristic to decide which choice to make.

            bool splitHorizontal;
            switch (method)
            {
                case GuillotineSplitHeuristic.SplitShorterLeftoverAxis:
                    // Split along the shorter leftover axis.
                    splitHorizontal = (w <= h);
                    break;
                case GuillotineSplitHeuristic.SplitLongerLeftoverAxis:
                    // Split along the longer leftover axis.
                    splitHorizontal = (w > h);
                    break;
                case GuillotineSplitHeuristic.SplitMinimizeArea:
                    // Maximize the larger area == minimize the smaller area.
                    // Tries to make the single bigger rectangle.
                    splitHorizontal = (placedRect.Width * h > w * placedRect.Height);
                    break;
                case GuillotineSplitHeuristic.SplitMaximizeArea:
                    // Maximize the smaller area == minimize the larger area.
                    // Tries to make the rectangles more even-sized.
                    splitHorizontal = (placedRect.Width * h <= w * placedRect.Height);
                    break;
                case GuillotineSplitHeuristic.SplitShorterAxis:
                    // Split along the shorter total axis.
                    splitHorizontal = (freeRect.Width <= freeRect.Height);
                    break;
                case GuillotineSplitHeuristic.SplitLongerAxis:
                    // Split along the longer total axis.
                    splitHorizontal = (freeRect.Width > freeRect.Height);
                    break;
                default:
                    splitHorizontal = true;
                    throw new Exception();
            }

            // Perform the actual split.
            SplitFreeRectAlongAxis(freeRect, placedRect, splitHorizontal);
        }

        private void SplitFreeRectAlongAxis(Rect freeRect, Rect placedRect, bool splitHorizontal)
        {
            // Form the two new rectangles.
            Rect bottom = new Rect();
            bottom.X = freeRect.X;
            bottom.Y = freeRect.Y + placedRect.Height;
            bottom.Height = freeRect.Height - placedRect.Height;

            Rect right = new Rect();
            right.X = freeRect.X + placedRect.Width;
            right.Y = freeRect.Y;
            right.Width = freeRect.Width - placedRect.Width;

            if (splitHorizontal)
            {
                bottom.Width = freeRect.Width;
                right.Height = placedRect.Height;
            }
            else // Split vertically
            {
                bottom.Width = placedRect.Width;
                right.Height = freeRect.Height;
            }

            // Add the new rectangles into the free rectangle pool if they weren't degenerate.
            if (bottom.Width > 0 && bottom.Height > 0)
                FreeRectangles.Add(bottom);
            if (right.Width > 0 && right.Height > 0)
                FreeRectangles.Add(right);
        }
        #endregion

        #region SWAP
        private static void Swap<T>(ref T a, ref T b)
        {
            Console.WriteLine("You sent the Swap() method a {0}",
               typeof(T));
            T temp;
            temp = a;
            a = b;
            b = temp;
        }
        #endregion

        #region MERGE LISTS
        private void MergeFreeList()
        {
            for (int i = 0; i < FreeRectangles.Count; ++i)
                for (int j = i + 1; j < FreeRectangles.Count; ++j)
                {
                    if (FreeRectangles[i].Width == FreeRectangles[j].Width && FreeRectangles[i].X == FreeRectangles[j].X)
                    {
                        if (FreeRectangles[i].Y == FreeRectangles[j].Y + FreeRectangles[j].Height)
                        {
                            FreeRectangles[i].Y -= FreeRectangles[j].Height;
                            FreeRectangles[i].Height += FreeRectangles[j].Height;
                            FreeRectangles.RemoveAt(j);
                            --j;
                        }
                        else if (FreeRectangles[i].Y + FreeRectangles[i].Height == FreeRectangles[j].Y)
                        {
                            FreeRectangles[i].Height += FreeRectangles[j].Height;
                            FreeRectangles.RemoveAt(j);
                            --j;
                        }
                    }
                    else if (FreeRectangles[i].Height == FreeRectangles[j].Height && FreeRectangles[i].Y == FreeRectangles[j].Y)
                    {
                        if (FreeRectangles[i].X == FreeRectangles[j].X + FreeRectangles[j].Width)
                        {
                            FreeRectangles[i].X -= FreeRectangles[j].Width;
                            FreeRectangles[i].Width += FreeRectangles[j].Width;
                            FreeRectangles.RemoveAt(j);
                            --j;
                        }
                        else if (FreeRectangles[i].X + FreeRectangles[i].Width == FreeRectangles[j].X)
                        {
                            FreeRectangles[i].Width += FreeRectangles[j].Width;
                            FreeRectangles.RemoveAt(j);
                            --j;
                        }
                    }
                }
        }
        #endregion

    }
}

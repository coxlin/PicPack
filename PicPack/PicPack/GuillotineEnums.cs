namespace PicPack
{
    public enum FreeRectChoiceHeuristic
	{
		RectBestAreaFit,
		RectBestShortSideFit,
		RectBestLongSideFit,
		RectWorstAreaFit, 
		RectWorstShortSideFit,
		RectWorstLongSideFit
	};


	public enum GuillotineSplitHeuristic
	{
		SplitShorterLeftoverAxis,
		SplitLongerLeftoverAxis,
		SplitMinimizeArea,
		SplitMaximizeArea, 
		SplitShorterAxis, 
		SplitLongerAxis
	};
}

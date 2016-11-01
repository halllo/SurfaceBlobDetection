using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace SurfaceBlobDetection
{
	public class AllTagsVisualizationDefinition : TagVisualizationDefinition
	{
		protected override bool Matches(TagData tag)
		{
			return true;
		}
	}
}

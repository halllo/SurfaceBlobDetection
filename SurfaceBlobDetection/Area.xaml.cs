using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SurfaceBlobDetection
{
	public partial class Area
	{
		public Area()
		{
			InitializeComponent();
		}

		private void TrackingCanvas_StartTracking(TrackingCanvas.ITrackedBlob trackedBlob)
		{
			var axis = trackedBlob.Axis;
			
			trackedBlob.Display(
				!trackedBlob.IsTag && 160 >= axis.MajorAxis && axis.MajorAxis >= 90 ? (FrameworkElement)new Ellipse
				{
					Width = 350,
					Height = 350,
					StrokeThickness = 2,
					Stroke = new SolidColorBrush(Colors.Orange)
				} :
				trackedBlob.IsTag ? (FrameworkElement)new Rectangle
				{
					Width = 60,
					Height = 60,
					StrokeThickness = 2,
					Stroke = new SolidColorBrush(Colors.Lime),
					RenderTransform = new RotateTransform(axis.Orientation, 60/2, 60/2)
				} :
				(FrameworkElement)new Ellipse
				{
					Width = 200,
					Height = 200,
					StrokeThickness = 1,
					Stroke = new SolidColorBrush(Colors.Cyan),
				}
			);
		}
	}
}

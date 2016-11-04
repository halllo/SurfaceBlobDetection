using System.Windows;

namespace SurfaceBlobDetection.Test
{
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			Loaded += (s, e) =>
			{
				area.ScatterViewOverlay.Items.Add(Tag(value: 3));
				area.ScatterViewOverlay.Items.Add(Ding(majorAxis: 100, minorAxis: 1));
				area.ScatterViewOverlay.Items.Add(Ding(majorAxis: 200, minorAxis: 1));
			};
		}
		
		Object Tag(long value)
		{
			return new Object(new ObjectViewModel(area.TrackingCanvasLayer)
			{
				IsTag = true,
				TagValue = value
			});
		}

		Object Ding(double majorAxis, double minorAxis)
		{
			return new Object(new ObjectViewModel(area.TrackingCanvasLayer)
			{
				Size = new Size(majorAxis, minorAxis)
			});
		}
	}
}

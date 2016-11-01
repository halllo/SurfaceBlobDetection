using System;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Input;

namespace SurfaceBlobDetection
{
	public partial class Area
	{
		public Area()
		{
			InitializeComponent();
		}

		private void Canvas_TouchDown(object sender, TouchEventArgs e)
		{
			e.TouchDevice.Capture(VisualizerCanvas);
			e.Handled = true;
			UpdateLog();
		}

		private void Canvas_TouchMove(object sender, TouchEventArgs e)
		{
			UpdateLog();
		}

		private void Canvas_TouchUp(object sender, TouchEventArgs e)
		{
			if (e.TouchDevice.Captured == VisualizerCanvas)
			{
				VisualizerCanvas.ReleaseTouchCapture(e.TouchDevice);
			}
			UpdateLog();
		}

		private void UpdateLog()
		{
			Log.Text = "";
			foreach (TouchDevice device in VisualizerCanvas.GetInputDevicesCaptured())
			{
				var data = device.GetEllipseData(VisualizerCanvas);
				var position = device.GetCenterPosition(this);
				var tagData = device.GetTagData();

				Log.Text = Log.Text + "\n blob : "
					+ "Axis=" + data.MajorAxis + "," + data.MinorAxis + "; "
					+ "Pos=" + position.X + "," + position.Y + "; "
					+ "Tag=" + tagData.Value + ";";
			}
		}
	}
}

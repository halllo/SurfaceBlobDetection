using Microsoft.Surface;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Input;
using System.Windows;

namespace SurfaceBlobDetection
{
	public partial class Window1
	{
		public Window1()
		{
			InitializeComponent();

			ApplicationServices.InactivityTimeoutOccurring += ApplicationServices_InactivityTimeoutOccurring;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			ApplicationServices.InactivityTimeoutOccurring -= ApplicationServices_InactivityTimeoutOccurring;
		}

		private void ApplicationServices_InactivityTimeoutOccurring(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void Canvas_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
		{
			e.TouchDevice.Capture(VisualizerCanvas);
			e.Handled = true;
			UpdateLog();
		}

		private void Canvas_TouchMove(object sender, System.Windows.Input.TouchEventArgs e)
		{
			UpdateLog();
		}

		private void Canvas_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
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
					+ "Tag=" + (tagData != null ? tagData.Value.ToString() : "<no tag>") + ";";
			}
		}
	}
}
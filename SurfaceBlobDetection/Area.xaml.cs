using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Input;

namespace SurfaceBlobDetection
{
	public partial class Area
	{
		public Area()
		{
			InitializeComponent();
		}



		private List<Blob> _Blobs = null;

		private void Canvas_TouchDown(object sender, TouchEventArgs e)
		{
			e.TouchDevice.Capture(VisualizerCanvas);
			e.Handled = true;

			_Blobs = UpdateBlobs(_Blobs);
			UpdateLog();
			UpdateVisuals();
		}

		private void Canvas_TouchMove(object sender, TouchEventArgs e)
		{
			if (_Blobs == null)
			{
				return;
			}

			UpdateLog();
			UpdateVisuals();
		}

		private void Canvas_TouchUp(object sender, TouchEventArgs e)
		{
			if (e.TouchDevice.Captured == VisualizerCanvas)
			{
				VisualizerCanvas.ReleaseTouchCapture(e.TouchDevice);
			}

			_Blobs = UpdateBlobs(_Blobs, removeExpired: true);
			UpdateLog();
			UpdateVisuals();
		}

		List<Blob> UpdateBlobs(List<Blob> oldBlobs, bool removeExpired = false)
		{
			List<TouchDevice> devices = new List<TouchDevice>();

			List<Blob> blobs = oldBlobs == null ? new List<Blob>() : oldBlobs;
			Log.Text = "";

			// Update list detected.
			foreach (TouchDevice device in VisualizerCanvas.GetInputDevicesCaptured())
			{
				if (!device.GetIsFingerRecognized() && !device.GetIsTagRecognized())
				{
					var data = device.GetEllipseData(VisualizerCanvas);
					devices.Add(device);
				}
			}


			// Remove blobs no longer in the screen.
			foreach (var blob in blobs)
			{
				bool foundBlob = false;
				foreach (var device in devices)
				{
					if (device.Id == blob.Device.Id)
					{
						foundBlob = true;
					}
				}
				if (!foundBlob)
				{
					blob.IsExpired = true;
				}
			}


			// Add blobs in the screen, but not already in the list.
			foreach (TouchDevice device in devices)
			{
				// This is a good pair!
				bool isTracked = false;
				foreach (var item in blobs)
				{
					if (item.Device.Id == device.Id)
					{
						isTracked = true;
					}
				}
				if (!isTracked)
				{
					blobs.Add(new Blob(device));
				}
			}


			// Remove expired visuals.
			var expiredBlobs = new List<Blob>();
			foreach (var blob in blobs)
			{
				if (blob.IsExpired)
				{
					expiredBlobs.Add(blob);
				}
			}
			foreach (var expiredBlob in expiredBlobs)
			{
				VisualizerCanvas.Children.Remove(expiredBlob.Visualization);
				blobs.Remove(expiredBlob);
			}
			expiredBlobs.Clear();


			return blobs;
		}

		private void UpdateLog()
		{
			Log.Text = "";
			foreach (TouchDevice device in VisualizerCanvas.GetInputDevicesCaptured())
			{
				var data = device.GetEllipseData(VisualizerCanvas);
				var position = device.GetCenterPosition(this);
				var tagData = device.GetTagData();

				Log.Text = Log.Text + "\n Device(" + device.Id + ") : "
					+ "Axis=" + data.MajorAxis + "," + data.MinorAxis + "; "
					+ "Pos=" + position.X + "," + position.Y + "; "
					+ "Tag=" + tagData.Value + ";";
			}

			foreach (var blob in _Blobs)
			{
				var data = blob.Device.GetEllipseData(VisualizerCanvas);
				var position = blob.Device.GetCenterPosition(this);
				var tagData = blob.Device.GetTagData();

				Log.Text = Log.Text + "\n TrackedBlob(" + blob.Device.Id + ") : "
					+ "Axis=" + data.MajorAxis + "," + data.MinorAxis + "," + data.Orientation + "; "
					+ "Pos=" + position.X + "," + position.Y + "; "
					+ "Tag=" + tagData.Value + ";";
			}
		}

		private void UpdateVisuals()
		{
			foreach (var blob in _Blobs)
			{
				var position = blob.Device.GetCenterPosition(VisualizerCanvas);
				var ellipseData = blob.Device.GetEllipseData(VisualizerCanvas);

				if (blob.Visualization == null)
				{
					var rectangle = new Rectangle();
					rectangle.Width = 300;
					rectangle.Height = 200;
					rectangle.StrokeThickness = 5;
					rectangle.Stroke = new SolidColorBrush(Colors.Orange);

					rectangle.RenderTransform = new RotateTransform(ellipseData.Orientation, rectangle.Width / 2, rectangle.Height / 2);
					rectangle.SetValue(Canvas.LeftProperty, position.X - rectangle.Width / 2);
					rectangle.SetValue(Canvas.TopProperty, position.Y - rectangle.Height / 2);

					blob.Visualization = rectangle;
					VisualizerCanvas.Children.Add(blob.Visualization);
				}
				else
				{
					Rectangle rectangle = blob.Visualization as Rectangle;
					rectangle.RenderTransform = new RotateTransform(ellipseData.Orientation, rectangle.Width / 2, rectangle.Height / 2);
					rectangle.SetValue(Canvas.LeftProperty, position.X - rectangle.Width / 2);
					rectangle.SetValue(Canvas.TopProperty, position.Y - rectangle.Height / 2);
				}
			}
		}
	}

	public class Blob
	{
		public Blob(TouchDevice device)
		{
			Device = device;
		}

		public TouchDevice Device { get; }
		public bool IsExpired { get; set; }
		public UIElement Visualization { get; set; }
	}
}

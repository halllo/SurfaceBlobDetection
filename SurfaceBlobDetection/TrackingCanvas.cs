using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Input;

namespace SurfaceBlobDetection
{
	public class TrackingCanvas : Canvas
	{
		public interface ITrackedBlob
		{
			Point Center { get; }
			EllipseData Axis { get; }
			bool IsTag { get; }
			long TagValue { get; }
			bool HasDisplay { get; }
			void Display(FrameworkElement element);
		}

		private class TrackedBlob : ITrackedBlob
		{
			public TrackedBlob(TrackingCanvas container, TouchDevice touch)
			{
				Container = container;
				Touch = touch;
			}

			public TrackingCanvas Container { get; private set; }
			public TouchDevice Touch { get; private set; }

			public bool IsExpired { get; set; }
			public FrameworkElement Visualization { get; set; }

			public Point Center { get { return Touch.GetCenterPosition(Container); } }
			public EllipseData Axis { get { return Touch.GetEllipseData(Container); } }
			public bool IsTag { get { return Touch.GetIsTagRecognized(); } }
			public long TagValue { get { return Touch.GetTagData().Value; } }
			public bool HasDisplay { get { return Visualization != null; } }
			public void Display(FrameworkElement element)
			{
				if (Visualization != null) throw new NotSupportedException("TrackedBlob already displays.");
				Visualization = element;
				Container.Children.Add(Visualization);
			}
			public void DontDisplay()
			{
				if (Visualization != null)
				{
					Container.Children.Remove(Visualization);
				}
			}
			public void Move()
			{
				if (Visualization != null)
				{
					var center = Center;
					Visualization.SetValue(Canvas.LeftProperty, center.X - Visualization.Width / 2);
					Visualization.SetValue(Canvas.TopProperty, center.Y - Visualization.Height / 2);
				}
			}
		}



		public TrackingCanvas()
		{
			Background = new SolidColorBrush(Colors.Black);
			TouchDown += Canvas_TouchDown;
			TouchMove += Canvas_TouchMove;
			TouchUp += Canvas_TouchUp;
			Children.Add(_Log);
		}

		public event Action<ITrackedBlob> StartTracking;
		private TextBlock _Log = new TextBlock();
		private List<TrackedBlob> _Blobs = null;


		private void Canvas_TouchDown(object sender, TouchEventArgs e)
		{
			e.TouchDevice.Capture(this);
			e.Handled = true;

			UpdateBlobs();
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
			if (e.TouchDevice.Captured == this)
			{
				base.ReleaseTouchCapture(e.TouchDevice);
			}

			UpdateBlobs(removeExpired: true);
			UpdateLog();
			UpdateVisuals();
		}


		private void UpdateBlobs(bool removeExpired = false)
		{
			var currentTouches = this.GetInputDevicesCaptured()
				.Cast<TouchDevice>()
				.Where(touch => !touch.GetIsFingerRecognized())
				.ToList();

			var trackedBlobs = new List<TrackedBlob>(_Blobs ?? Enumerable.Empty<TrackedBlob>());

			// Remove blobs no longer on the screen.
			foreach (var blob in trackedBlobs)
			{
				if (!currentTouches.Any(touch => touch.Id == blob.Touch.Id))
				{
					blob.IsExpired = true;
				}
			}

			// Add blobs on the screen, but not already tracked.
			foreach (TouchDevice touch in currentTouches)
			{
				if (!trackedBlobs.Any(blob => blob.Touch.Id == touch.Id))
				{
					trackedBlobs.Add(new TrackedBlob(this, touch));
				}
			}

			// Remove expired visuals.
			var expiredBlobs = trackedBlobs.Where(blob => blob.IsExpired).ToList();
			foreach (var expiredBlob in expiredBlobs)
			{
				expiredBlob.DontDisplay();
				trackedBlobs.Remove(expiredBlob);
			}

			_Blobs = trackedBlobs;
		}

		private void UpdateVisuals()
		{
			foreach (var blob in _Blobs)
			{
				if (blob.HasDisplay)
				{
					blob.Move();
				}
				else
				{
					var e = StartTracking;
					if (e != null)
					{
						e.Invoke(blob);
					}
					blob.Move();
				}
			}
		}

		private void UpdateLog()
		{
			_Log.Text = "";
			foreach (var blob in _Blobs)
			{
				var axis = blob.Axis;
				var center = blob.Center;
				var tag = blob.TagValue;

				_Log.Text = _Log.Text + "\nTrackedBlob(" + blob.Touch.Id + ") : "
					+ "Axis(" + axis.MajorAxis + "; " + axis.MinorAxis + "; " + axis.Orientation + "); "
					+ "Pos(" + center.X + "; " + center.Y + "); "
					+ "Tag(" + tag + ");";
			}
		}
	}
}

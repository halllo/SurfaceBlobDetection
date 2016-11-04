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

			event Action<ITrackedBlob> Moved;
			event Action<ITrackedBlob> Removed;
		}
		private abstract class TrackedBlob : ITrackedBlob
		{
			public TrackedBlob(TrackingCanvas container)
			{
				Container = container;
			}

			public abstract int Id { get; }
			public abstract Point Center { get; }
			public abstract EllipseData Axis { get; }
			public abstract bool IsTag { get; }
			public abstract long TagValue { get; }

			public TrackingCanvas Container { get; private set; }
			public bool IsExpired { get; set; }
			public FrameworkElement Visualization { get; set; }
			public bool HasDisplay { get { return Visualization != null; } }

			public event Action<ITrackedBlob> Moved;
			public event Action<ITrackedBlob> Removed;

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

				var e = Removed;
				if (e != null) e.Invoke(this);
			}
			public void Move()
			{
				if (Visualization != null)
				{
					var center = Center;
					Visualization.SetValue(Canvas.LeftProperty, center.X - Visualization.Width / 2);
					Visualization.SetValue(Canvas.TopProperty, center.Y - Visualization.Height / 2);

					if (IsTag)
					{
						var rotate = Visualization.RenderTransform as RotateTransform;
						if (rotate != null) rotate.Angle = Axis.Orientation;
					}

					var e = Moved;
					if (e != null) e.Invoke(this);
				}
			}
		}
		private class TrackedTouchBlob : TrackedBlob
		{
			public TrackedTouchBlob(TrackingCanvas container, TouchDevice touch) : base(container)
			{
				Touch = touch;
			}

			public TouchDevice Touch { get; private set; }

			public override int Id { get { return Touch.Id; } }
			public override Point Center { get { return Touch.GetCenterPosition(Container); } }
			public override EllipseData Axis { get { return Touch.GetEllipseData(Container); } }
			public override bool IsTag { get { return Touch.GetIsTagRecognized(); } }
			public override long TagValue { get { return Touch.GetTagData().Value; } }
		}













		public TrackingCanvas()
		{
			Background = Brushes.Black;
			TouchDown += Canvas_TouchDown;
			TouchMove += Canvas_TouchMove;
			TouchUp += Canvas_TouchUp;
			Children.Add(_Log);
		}

		public event Action<ITrackedBlob> StartTracking;
		private TextBlock _Log = new TextBlock { Foreground = Brushes.White };
		private List<TrackedBlob> _Blobs = null;



		private void Canvas_TouchDown(object sender, TouchEventArgs e)
		{
			e.TouchDevice.Capture(this);
			e.Handled = true;

			UpdateBlobs(this.GetInputDevicesCaptured());
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

			UpdateBlobs(this.GetInputDevicesCaptured(), removeExpired: true);
			UpdateLog();
			UpdateVisuals();
		}




		private void UpdateBlobs(IEnumerable<InputDevice> inputDevicesCaptured, bool removeExpired = false)
		{
			var currentTouches = inputDevicesCaptured
				.Cast<TouchDevice>()
				.Where(touch => !touch.GetIsFingerRecognized())
				.ToList();

			var trackedBlobs = new List<TrackedBlob>(_Blobs ?? Enumerable.Empty<TrackedBlob>());

			// Remove blobs no longer on the screen.
			foreach (var blob in trackedBlobs)
			{
				if (!currentTouches.Any(touch => touch.Id == blob.Id))
				{
					blob.IsExpired = true;
				}
			}

			// Add blobs on the screen, but not already tracked.
			foreach (TouchDevice touch in currentTouches)
			{
				if (!trackedBlobs.Any(blob => blob.Id == touch.Id))
				{
					trackedBlobs.Add(new TrackedTouchBlob(this, touch));
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

				_Log.Text = _Log.Text + "\n\tTrackedBlob(" + blob.Id + ") : "
					+ "Axis(" + axis.MajorAxis + "; " + axis.MinorAxis + "; " + axis.Orientation + "); "
					+ "Pos(" + center.X + "; " + center.Y + "); "
					+ "Tag(" + tag + ");";
			}
		}








































		internal ITrackedBlob ForTestingPurposes_StartTracking(SimulatedTouch simulatedTouch)
		{
			if (_Blobs == null) _Blobs = new List<TrackedBlob>();
			var trackedBlob = new ForTestingPurposes_TrackedBlob(this, simulatedTouch);
			_Blobs.Add(trackedBlob);

			UpdateLog();
			UpdateVisuals();

			return trackedBlob;
		}
		internal void ForTestingPurposes_StopTracking(ITrackedBlob trackedBlob)
		{
			var blob = (TrackedBlob)trackedBlob;
			blob.DontDisplay();
			_Blobs.Remove(blob);

			UpdateLog();
			UpdateVisuals();
		}
		internal void ForTestingPurposes_UpdateTracking()
		{
			UpdateLog();
			UpdateVisuals();
		}
		internal class SimulatedTouch
		{
			public int Id { get; set; }
			public Point Center { get; set; }
			public EllipseData Axis { get; set; }
			public bool IsTag { get; set; }
			public long TagValue { get; set; }
		}
		private class ForTestingPurposes_TrackedBlob : TrackedBlob
		{
			public ForTestingPurposes_TrackedBlob(TrackingCanvas container, SimulatedTouch touch) : base(container)
			{
				Touch = touch;
			}

			public SimulatedTouch Touch { get; set; }

			public override int Id { get { return Touch.Id; } }
			public override Point Center { get { return Touch.Center; } }
			public override EllipseData Axis { get { return Touch.Axis; } }
			public override bool IsTag { get { return Touch.IsTag; } }
			public override long TagValue { get { return Touch.TagValue; } }
		}
	}
}

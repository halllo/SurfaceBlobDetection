using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using static SurfaceBlobDetection.TrackingCanvas;

namespace SurfaceBlobDetection.Test
{
	public partial class Object
	{
		ObjectViewModel _ViewModel;
		public Object(ObjectViewModel viewModel)
		{
			InitializeComponent();
			DataContext = _ViewModel = viewModel;

			ContainerManipulationDelta += (s, e) =>
			{
				_ViewModel.Position = Center;
				_ViewModel.Orientation = Orientation;
				_ViewModel.Update();
			};
		}
	}

	public class ObjectViewModel : ViewModel
	{
		TrackingCanvas _TrackingCanvas;

		public ObjectViewModel(TrackingCanvas trackingCanvas)
		{
			_TrackingCanvas = trackingCanvas;
			PlacedCommmand = new Command(Placed);
		}

		public string Info { get { return IsTag ? "Tag-ID: " + TagValue : "majorAxis=" + (int)Size.Width + "; minorAxis=" + (int)Size.Height + ""; } }
		public bool IsTag { get; set; }
		public long TagValue { get; set; }

		Size _Size; public Size Size
		{
			get { return _Size; }
			set { _Size = value; NotifyChanged("Size"); if (_Touch != null) _Touch.Axis = new Microsoft.Surface.Presentation.Input.EllipseData(Size.Width, Size.Height, Orientation); }
		}
		Point _Position; public Point Position
		{
			get { return _Position; }
			set { _Position = value; NotifyChanged("Position"); if (_Touch != null) _Touch.Center = Position; }
		}
		double _Orientation; public double Orientation
		{
			get { return _Orientation; }
			set { _Orientation = value; NotifyChanged("Orientation"); if (_Touch != null) _Touch.Axis = new Microsoft.Surface.Presentation.Input.EllipseData(Size.Width, Size.Height, Orientation); }
		}

		public Command PlacedCommmand { get; set; }
		static int _DeviceIdSequence = 0;
		SimulatedTouch _Touch;
		ITrackedBlob _TrackedBlob;
		private void Placed(object sender)
		{
			var isPlaced = ((SurfaceCheckBox)sender).IsChecked == true;

			if (isPlaced)
			{
				_Touch = new SimulatedTouch
				{
					Id = _DeviceIdSequence++,
					IsTag = IsTag,
					TagValue = TagValue,
					Center = Position,
					Axis = new Microsoft.Surface.Presentation.Input.EllipseData(Size.Width, Size.Height, Orientation)
				};
				_TrackedBlob = _TrackingCanvas.ForTestingPurposes_StartTracking(_Touch);
			}
			else
			{
				_TrackingCanvas.ForTestingPurposes_StopTracking(_TrackedBlob);
				_Touch = null;
				_TrackedBlob = null;
			}
		}

		public void Update()
		{
			if (_TrackedBlob != null)
			{
				_TrackingCanvas.ForTestingPurposes_UpdateTracking();
			}
		}
	}
}

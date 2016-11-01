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
	}
}
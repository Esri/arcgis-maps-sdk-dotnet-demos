using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using SharpDX.XInput;
using System;
using Windows.UI.Xaml;

namespace XInputHelper
{
	// Usage:
	// Add namespace using
	//		xmlns:xinput="using:XInputHelper"
	//
	// Add inside SceneView:
	//		<xinput:XInputSceneController.Controller>
	//			<xinput:XInputSceneController />
	//		</xinput:XInputSceneController.Controller>
	//
	//
	//
	//
	//
	//
	//
	public class XInputSceneController
	{
		private SceneView sceneView;
		private DispatcherTimer timer;
		private SharpDX.XInput.Controller controller;
		private bool relativeToDirection;
		private DateTime time;

		public XInputSceneController()
		{
			controller = new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.One);
		}

		private void SetSceneView(SceneView sceneView)
		{
			if(this.sceneView != null)
			{
				//Clean up
				this.sceneView.Loaded -= _sceneView_Loaded;
				this.sceneView.Unloaded -= _sceneView_Unloaded;
				Stop();
			}
			this.sceneView = sceneView;
			if(sceneView != null)
			{
				this.sceneView.Loaded += _sceneView_Loaded;
				this.sceneView.Unloaded += _sceneView_Unloaded;
				Start(); //We should only start if already loaded, but no way to detect that currently
			}
		}
		private void Start()
		{
			if(timer == null)
			{
				timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1.0 / 200) };
				timer.Tick += timer_Tick;
				time = DateTime.Now;
				timer.Start();
			}
		}
		private void Stop()
		{
			if(timer != null)
			{
				timer.Tick -= timer_Tick;
				timer.Stop();
				timer = null;
			}
		}

		private void timer_Tick(object sender, object e)
		{
			UpdateView();
		}

		private void UpdateView()
		{
			if (sceneView == null)
				return;
			if (controller == null || !controller.IsConnected)
				return;
			//The Camera might change whil doing all the calculations...
			var originalCamera = sceneView.Camera;
			if (originalCamera == null)
				return;
			var newCamera = originalCamera;
			var state = controller.GetState();
			var time2 = DateTime.Now;
			bool isRightShoulderPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
			//Choose if staying at the same alitude or not
			if (isRightShoulderPressed)
			{
				var elapsed = time2 - time;
				if (elapsed != TimeSpan.Zero && elapsed > TimeSpan.FromSeconds(0.25))
				{
					timer.Stop();
					timer.Start();
					relativeToDirection = !relativeToDirection;
					time = time2;
				}
			}
			else
				time = time2;

			double LeftThumbStickX = 0;
			double LeftThumbStickY = 0;
			double RightThumbStickX = 0;
			double RightThumbStickY = 0;
			var gamePad = new NormalizedGamepad(state.Gamepad);
			LeftThumbStickX = gamePad.LeftThumbXNormalized;
			LeftThumbStickY = gamePad.LeftThumbYNormalized;
			RightThumbStickX = gamePad.RightThumbXNormalized;
			RightThumbStickY = gamePad.RightThumbYNormalized;

			if ((LeftThumbStickX != 0) || (LeftThumbStickY != 0) || (RightThumbStickX != 0) || (RightThumbStickY != 0))
			{
				//Move sideways (crab move), so calling move forward after modifying heading and tilt, then resetting heading and tilt 
				if (LeftThumbStickX != 0)
				{
					//Distance depends on the altitude, but must have minimum values (one way and the other) to not be almost nothing when altitude gets to 0)
					var distance = (double)LeftThumbStickX * newCamera.Location.Z / 40;
					if (distance > 0 && distance < 0.75)
						distance = 0.75;
					if (distance < 0 && distance > -0.75)
						distance = -0.75;
					newCamera = newCamera.SetPitch(90).Rotate(90, 0).MoveForward(distance).Rotate(-90, 0).SetPitch(newCamera.Pitch);
				}

				var distance2 = (double)LeftThumbStickY * newCamera.Location.Z / 40;
				if (distance2 > 0 && distance2 < 1)
					distance2 = 1;
				if (distance2 < 0 && distance2 > -1)
					distance2 = -1;
				//Move in the direction of the camera (modify the elevation)
				newCamera = newCamera.MoveForward(distance2).Rotate((double)RightThumbStickX, newCamera.Pitch > 2 ? -(double)RightThumbStickY : RightThumbStickY < 0 ? -(double)RightThumbStickY : 0);

				//If move in the direction of the camera but staying at the same elevation.
				if (!relativeToDirection)
					newCamera = newCamera.SetLocation(new MapPoint(newCamera.Location.X, newCamera.Location.Y, originalCamera.Location.Z, newCamera.Location.SpatialReference));
			}

			//Triggers are just elevating the camera

			double elevationDistance = (double)(state.Gamepad.RightTrigger - state.Gamepad.LeftTrigger);
			if ((elevationDistance != 0) && (newCamera.Location.Z != double.NaN))
			{
				var distance = elevationDistance * Math.Abs(newCamera.Location.Z) / 10000;
				newCamera = newCamera.Elevate(distance > 1 ? distance : distance < 1 ? distance : distance < 0 ? -1 : 1);
			}


			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
				newCamera = newCamera.SetHeading(0);

			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
				newCamera = newCamera.SetHeading(180);

			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
				newCamera = newCamera.SetHeading(270);

			if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
				newCamera = newCamera.SetHeading(90);

			if (!newCamera.IsEqual(originalCamera))
				sceneView.SetView(newCamera);

		}

		private void _sceneView_Unloaded(object sender, RoutedEventArgs e)
		{
			Stop();
		}

		private void _sceneView_Loaded(object sender, RoutedEventArgs e)
		{
			Start();
		}

		public static XInputSceneController GetController(DependencyObject obj)
		{
			return (XInputSceneController)obj.GetValue(ControllerProperty);
		}

		public static void SetController(DependencyObject obj, XInputSceneController value)
		{
			obj.SetValue(ControllerProperty, value);
		}

		public static readonly DependencyProperty ControllerProperty =
			DependencyProperty.RegisterAttached("Controller", typeof(XInputSceneController), typeof(XInputSceneController), new PropertyMetadata(null, OnControllerPropertyChanged));

		private static void OnControllerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var oldCtrl = e.OldValue as XInputSceneController;
			var newCtrl = e.NewValue as XInputSceneController;
			if(oldCtrl != null)
				oldCtrl.SetSceneView(null);
			if (newCtrl != null)
				newCtrl.SetSceneView(d as Esri.ArcGISRuntime.Controls.SceneView);
		}
	}
}

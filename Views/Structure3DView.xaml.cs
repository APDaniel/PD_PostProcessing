using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace PD_ScriptTemplate.Views
{
    /// <summary>
    /// Interaction logic for Structure3DView.xaml
    /// </summary>
    public partial class Structure3DView : UserControl
    {
        //Constructor
        public Structure3DView()
        {
            InitializeComponent();
        }
        //Used to define if the mouse is captured
        private bool isMouseCaptured = false;

        private Point3D initialCameraPosition;
        private Vector3D initialCameraLookDirection;
        private Vector3D initialCameraUpDirection;


        //Used for rotation moves
        private Point lastMousePos;

        //Used for pan moves
        private Point lastRightMousePos;

        //Used for scene translation
        private TranslateTransform3D sceneTranslation = new TranslateTransform3D();

        /// <summary>
        /// Method used to define what button is pressed in the ViewPort
        /// </summary>
        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isMouseCaptured = true;
                lastMousePos = e.GetPosition(Viewport3D);
                Viewport3D.CaptureMouse();
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                isMouseCaptured = true;
                lastRightMousePos = e.GetPosition(Viewport3D);
                Viewport3D.CaptureMouse();
            }
        }

        /// <summary>
        /// Method used to detect when the mouse butto is released
        /// </summary>
        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                isMouseCaptured = false;
                Viewport3D.ReleaseMouseCapture();
            }
            if (e.RightButton == MouseButtonState.Released)
            {
                isMouseCaptured = false;
                Viewport3D.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Method used to apply moves to the displaying structure in the ViewPort3D
        /// </summary>
        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseCaptured == true&&e.LeftButton==MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(Viewport3D); // Use Viewport3D here

                // Calculate the rotation and update the camera's orientation
                UpdateCameraRotation(mousePos, lastMousePos);

                lastMousePos = mousePos;

            }
            if (isMouseCaptured ==true&& e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(Viewport3D); // Use Viewport3D here
                // Calculate the rotation and update the camera's orientation
                UpdateCameraPan(mousePos, lastMousePos);
                lastRightMousePos = mousePos;
            }
        }

        /// <summary>
        /// Method used to rotate camera when the left mouse button is pressed
        /// </summary>
        private void UpdateCameraRotation(Point currentMousePos, Point lastMousePos)
        {

            double width = Viewport3D.ActualWidth;
            double height = Viewport3D.ActualHeight;

            // Scale bounds to [0,0] - [2,2]
            double x1 = lastMousePos.X / (width / 2);
            double y1 = lastMousePos.Y / (height / 2);

            double x2 = currentMousePos.X / (width / 2);
            double y2 = currentMousePos.Y / (height / 2);

            //Translate 0,0 to the center
            x1 = x1 - 1;
            y1 = 1 - y1;
            x2 = x2 - 1;
            y2 = 1 - y2;

            // Compute the Z coordinate of the points on the sphere beneath the mouse pointer
            double z1Squared = 1 - x1 * x1 - y1 * y1;
            double z1 = z1Squared > 0 ? Math.Sqrt(z1Squared) : 0;

            double z2Squared = 1 - x2 * x2 - y2 * y2;
            double z2 = z2Squared > 0 ? Math.Sqrt(z2Squared) : 0;

            // Create vectors for the two points on the sphere
            Vector3D v1 = new Vector3D(x1, y1, z1);
            Vector3D v2 = new Vector3D(x2, y2, z2);

            // Compute the axis and angle of rotation
            Vector3D axis = Vector3D.CrossProduct(v1, v2);
            double angle = Vector3D.AngleBetween(v1, v2);

            // Update the camera's orientation
            Transform3DGroup cameraTransform = mainCamera.Transform as Transform3DGroup;
            if (cameraTransform != null)
            {
                // Check if the first transform in the Transform3DGroup is a RotateTransform3D
                if (cameraTransform.Children[0] is RotateTransform3D rt)
                {
                    AxisAngleRotation3D r = rt.Rotation as AxisAngleRotation3D;
                    if (r != null)
                    {
                        // Update the camera's orientation
                        Quaternion q = new Quaternion(r.Axis, r.Angle);

                        // We negate the angle because we are rotating the camera.
                        // Do not do this if you are rotating the scene instead.
                        Quaternion delta = new Quaternion(axis, -angle);
                        q *= delta;

                        r.Axis = q.Axis;
                        r.Angle = q.Angle;
                    }
                }
            }
        }

        /// <summary>
        /// Called method to translate the scene
        /// </summary>
        private void UpdateCameraPan(Point currentMousePos, Point lastRightMousePos)
        {
            double panSensivity = 2;

            initialCameraPosition = mainCamera.Position;
            initialCameraLookDirection = mainCamera.LookDirection;
            initialCameraUpDirection = mainCamera.UpDirection;


            double width = Viewport3D.ActualWidth;
            double height = Viewport3D.ActualHeight;

            // Scale bounds to [0,0] - [2,2]
            double x1 = lastMousePos.X / (width / 2);
            double y1 = lastMousePos.Y / (height / 2);

            double x2 = currentMousePos.X / (width / 2);
            double y2 = currentMousePos.Y / (height / 2);

            // Translate 0,0 to the center
            x1 = x1 - 1;
            y1 = 1 - y1;
            x2 = x2 - 1;
            y2 = 1 - y2;

            // Compute the Z coordinate of the points on the sphere beneath the mouse pointer
            double z1Squared = 1 - x1 * x1 - y1 * y1;
            double z1 = z1Squared > 0 ? Math.Sqrt(z1Squared) : 0;

            double z2Squared = 1 - x2 * x2 - y2 * y2;
            double z2 = z2Squared > 0 ? Math.Sqrt(z2Squared) : 0;

            // Create vectors for the two points on the sphere
            Vector3D v1 = new Vector3D(x1, y1, z1);
            Vector3D v2 = new Vector3D(x2, y2, z2);

            // Compute the camera's right and up directions
            Vector3D cameraRight = Vector3D.CrossProduct(initialCameraLookDirection, initialCameraUpDirection);
            cameraRight.Normalize();
            Vector3D cameraUp = initialCameraUpDirection;
            cameraUp.Normalize();

            // Calculate the translation in the X and Y direction
            double translationX = Vector3D.DotProduct(v2 - v1, cameraRight) * panSensivity;
            double translationZ = Vector3D.DotProduct(v2 - v1, cameraUp) * panSensivity;

            // Apply the translation to the scene translation
            sceneTranslation = new TranslateTransform3D();
            sceneTranslation.OffsetX += translationX;
            sceneTranslation.OffsetZ += translationZ;

            // Apply the translation to the Model3DGroup
            //structure3Dmodel.Transform = sceneTranslation;

            mainCamera.Position = new Point3D(initialCameraPosition.X - translationX, initialCameraPosition.Y-translationZ, initialCameraPosition.Z);

            initialCameraPosition = mainCamera.Position;
            initialCameraLookDirection = mainCamera.LookDirection;
            initialCameraUpDirection = mainCamera.UpDirection;

            lastMousePos = currentMousePos;
        }


        /// <summary>
        /// Called to zoom with a mouse wheel
        /// </summary>
        private void Viewport3D_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Adjust the zoom sensitivity here (you can change the value as needed)
            double zoomSensitivity = 0.001;

            double zoomFactor = e.Delta * zoomSensitivity;

            // Calculate the new camera position based on the zoom factor
            Vector3D lookDirection = mainCamera.LookDirection;
            double lookDirectionLength = lookDirection.Length;
            lookDirection.Normalize();
            lookDirection *= zoomFactor;

            var fallbackValue = mainCamera.Position;
            if (fallbackValue.X <= 2 && fallbackValue.Y <= 2 && fallbackValue.Z <= 2) fallbackValue = new Point3D(2, 2, 2);
            mainCamera.Position = mainCamera.Position + (lookDirectionLength - lookDirection.Length) * lookDirection;
            
            //Limit maximumm and miinimum zoom
            if (mainCamera.Position.X<=2&& mainCamera.Position.Y <= 2&& mainCamera.Position.Z <= 2) 
            {
                mainCamera.Position = fallbackValue;
            }
            if (mainCamera.Position.X >= 50 || mainCamera.Position.Y >= 50 || mainCamera.Position.Z >= 50)
            {
                mainCamera.Position = fallbackValue;
            }
        }

    }
}

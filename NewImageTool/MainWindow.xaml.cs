using BetterTogether.Device;
using BetterTogether.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewImageTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPairedDevice _device;
        ICamera _camera;
        System.IO.MemoryStream stream;

        public MainWindow()
        {
            InitializeComponent();
            InitBetterTogether();
        }
        private void InitBetterTogether()
        {
            // Initializes the device discovery service. By default NFC pairing is disabled, and WiFi broadcast pairing is enabled.
            DeviceFinder.Initialize("Robot 01");

            // Subscribe to an event that indicates that a connection request has arrived.
            DeviceFinder.DeviceConnectionAccepting += DeviceFinder_DeviceConnectionAccepting;

            // Subscribe to an event that indicates that connection status has changed.
            DeviceFinder.ConnectionStatusChanged += DeviceFinder_ConnectionStatusChanged;

            try
            {
                // Start device discovery through NFC pairing. The connection will be established using Wi-Fi.
                DeviceFinder.Start(ConnectionActionType.WIFI);
            }
            catch (Exception exp)
            {
                //MessageBox.Show(exp.Message);
            }
        }

        void DeviceFinder_DeviceConnectionAccepting(object sender, ConnectionAcceptingEventArgs e)
        {
            e.ConnectionDeferral.AcceptAlways();
        }

        void DeviceFinder_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            switch (e.ConnectionStatus)
            {
                case ConnectionStatus.NFC_TAPPED:
                    // User performed an NFC tap with the local device.
                    break;
                case ConnectionStatus.CONNECTED:
                    // Connection succeeded.

                    OnDeviceConnected(e.Device);
                    break;
                case ConnectionStatus.FAILED:

                    // Connection failed.
                    break;
            }
        }

        private async void OnDeviceConnected(IPairedDevice pairedDevice)
        {
            //StatusMessage.Visibility = Visibility.Collapsed;

            _device = pairedDevice;

            // Tell the camera object the 
            // resolution we want for the incoming video.
            // Here we use the 1st available resolution
            _camera = await _device.CameraManager.OpenAsync(
                    CameraLocation.Back,
                    _device.CameraManager.GetAvailableCaptureResolutions(
                    CameraLocation.Back)[0]
                    );
            ell_flag.Fill = Brushes.Green;
            ell_flag.Stroke = Brushes.Green;
            // Please notice the preview resolution is different to capture resolution
            await _camera.SetPreviewResolutionAsync(new System.Windows.Size(800, 448));
            _camera.PreviewFrameAvailable += _camera_PreviewFrameAvailable;
        }

        private async void _camera_PreviewFrameAvailable(object sender, PreviewArrivedEventArgs e)
        {
            try
            {
                stream = new System.IO.MemoryStream(e.Frame.ImageStream);

                if (null == stream)

                    return;

                await Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                    {
                        try
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = stream;   // Copy stream to local
                            bitmapImage.EndInit();

                            cam.Source = bitmapImage;
                            cam_right.Source = bitmapImage;
                        }
                        catch (Exception ex) { };
                    }));
            }
            catch (Exception ex) { }
        }
    }
}

using Microsoft.Maps.MapControl.WPF;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Map
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point center;
        double maxDistanceToCenter;
        public MainWindow()
        {
            InitializeComponent();
            this.map.SizeChanged += Map_SizeChanged;
        }

        private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            center = new Point(e.NewSize.Width / 2, e.NewSize.Height / 2);
            maxDistanceToCenter = GetLength(center.X, center.Y);
        }

        private async void map_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var location = new Microsoft.Maps.MapControl.WPF.Location();
            if (map.TryViewportPointToLocation(e.GetPosition(map), out location))
            {

                var req = new BingMapsRESTToolkit.ReverseGeocodeRequest()
                {
                    BingMapsKey = bingmapskey.Text.Trim(),
                    Point = new BingMapsRESTToolkit.Coordinate(location.Latitude, location.Longitude)
                };

                var response = await BingMapsRESTToolkit.ServiceManager.GetResponseAsync(req);

                if (response != null && response.ResourceSets != null && response.ResourceSets.Length > 0 && response.ResourceSets[0].Resources != null && response.ResourceSets[0].Resources.Length > 0)
                {
                    var searchLocation = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;

                    string result = $"{searchLocation.Address.FormattedAddress}, - {searchLocation.Address.AddressLine}, {searchLocation.Address.AdminDistrict}, {searchLocation.Address.AdminDistrict2}, {searchLocation.Address.CountryRegion}";

                    resultField.Text = result;
                }
            }
        }

        double GetDistance(Point currentPosition)
        {
            double deltaX = Convert.ToDouble(Math.Abs(currentPosition.X - center.X));
            double deltaY = Convert.ToDouble(Math.Abs(currentPosition.Y - center.Y));
            var distance = GetLength(deltaX, deltaY);

            return distance;
        }

        private double GetLength(double x, double y)
        {
            double x2 = Math.Pow(x, 2);
            double y2 = Math.Pow(y, 2);

            double length = (int)Math.Round(Math.Sqrt(x2 + y2));
            return length;
        }

        private void map_MouseMove(object sender, MouseEventArgs e)
        {
            var distance = GetDistance(e.GetPosition(map));
            var frequency = GetFrequency(distance);
            Console.Beep(frequency, 250);
        }

        int GetFrequency(double distanceToCenter)
        {

            var factor = 1 - (distanceToCenter / maxDistanceToCenter);
            var range = (5000 - 37);
            var frequency = range * factor + 37;

            return Convert.ToInt32(frequency);

        }

        private async void searchfield_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            await search();
            e.Handled = true;
        }

        async Task search()
        {
            var req = new BingMapsRESTToolkit.GeocodeRequest()
            {
                BingMapsKey = bingmapskey.Text.Trim(),
                Query = searchfield.Text.Trim()
            };

            var response = await BingMapsRESTToolkit.ServiceManager.GetResponseAsync(req);

            if (response != null && response.ResourceSets != null && response.ResourceSets.Length > 0 && response.ResourceSets[0].Resources != null && response.ResourceSets[0].Resources.Length > 0)
            {
                var result = response.ResourceSets[0].Resources[0] as BingMapsRESTToolkit.Location;

                var boundingBox = new Microsoft.Maps.MapControl.WPF.LocationRect()
                {
                    South = result.BoundingBox[0],
                    West = result.BoundingBox[1],
                    North = result.BoundingBox[2],
                    East = result.BoundingBox[3]
                };
                map.SetView(boundingBox);
            }
        }

        private void bingmapskey_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                return;
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(bingmapskey.Text.Trim());
        }
    }
}

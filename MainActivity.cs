using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Location;
using Android.Gms.Common.Apis;
using Android.Locations;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using System;
using Android.Gms.Common;
using static Android.Gms.Common.Apis.GoogleApiClient;
using Android.Runtime;

namespace XamarinGoogleLocationApi
{
    [Activity(Label = "XamarinGoogleLocationApi", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity,IConnectionCallbacks,IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        private const int MY_PERMISSION_REQUEST_CODE = 7171;
        private const int PLAY_SERVICES_RESOLUTION_REQUEST = 7172;
        private TextView txtCoordinates;
        private Button btnGetCoordinates, btnTracking;
        private bool mRequestingLocationUpdates = false;
        private LocationRequest mLocationRequest;
        private GoogleApiClient mGoogleApiClient;
        private Location mLastLocation;

        private static int UPDATE_INTERVAL = 5000; // SEC
        private static int FATEST_INTERVAL = 3000; // SEC
        private static int DISPLACEMENT = 10; // METERS


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch(requestCode)
            {
                case MY_PERMISSION_REQUEST_CODE:
                    if(grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if(CheckPlayServices())
                        {
                            BuildGoogleApiClient();
                            CreateLocationRequest();
                        }
                    }
                    break;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            txtCoordinates = FindViewById<TextView>(Resource.Id.txtCoordinates);
            btnGetCoordinates = FindViewById<Button>(Resource.Id.btnGetCoord);
            btnTracking = FindViewById<Button>(Resource.Id.btnTrackingLocation);

            if(ActivityCompat.CheckSelfPermission(this,Manifest.Permission.AccessFineLocation) != Permission.Granted
                && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] {
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation
                },MY_PERMISSION_REQUEST_CODE);
            }
            else
            {
                if(CheckPlayServices())
                {
                    BuildGoogleApiClient();
                    CreateLocationRequest();
                }
            }

            btnGetCoordinates.Click += delegate {
                DisplayLocation();
            };

            btnTracking.Click += delegate {
                TogglePeriodicLocationUpdates();
            };

        }

        private void TogglePeriodicLocationUpdates()
        {
            if(!mRequestingLocationUpdates)
            {
                btnTracking.Text = "Stop location update";
                mRequestingLocationUpdates = true;
                StartLocationUpdates();
            }
            else
            {
                btnTracking.Text = "Start location update";
                mRequestingLocationUpdates = false;
                StopLocationUpdates();
            }
        }

        private void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL);
            mLocationRequest.SetFastestInterval(FATEST_INTERVAL);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetSmallestDisplacement(DISPLACEMENT);
        }

        private void BuildGoogleApiClient()
        {
            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.API).Build();
            mGoogleApiClient.Connect();
        }

        private bool CheckPlayServices()
        {
            int resultCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);
            if(resultCode != ConnectionResult.Success)
            {
                if(GooglePlayServicesUtil.IsUserRecoverableError(resultCode))
                {
                    GooglePlayServicesUtil.GetErrorDialog(resultCode, this, PLAY_SERVICES_RESOLUTION_REQUEST).Show();
                }
                else
                {
                    Toast.MakeText(ApplicationContext, "This device is not support Google Play Services", ToastLength.Long).Show();
                    Finish();
                }
                return false;
            }
            return true;
        }

        private void DisplayLocation()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
              && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            mLastLocation = LocationServices.FusedLocationApi.GetLastLocation(mGoogleApiClient);
            if (mLastLocation != null)
            {
                double lat = mLastLocation.Latitude;
                double lng = mLastLocation.Longitude;
                txtCoordinates.Text = $"{lat} / {lng}";
            }
            else
                txtCoordinates.Text = "Couldn't get the location.";
        }

        private void StartLocationUpdates()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Permission.Granted
              && ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
            {
                return;
            }
            LocationServices.FusedLocationApi.RequestLocationUpdates(mGoogleApiClient, mLocationRequest, this);
       }

        private void StopLocationUpdates()
        {
            LocationServices.FusedLocationApi.RemoveLocationUpdates(mGoogleApiClient, this);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            
        }

        public void OnConnected(Bundle connectionHint)
        {
            DisplayLocation();
            if (mRequestingLocationUpdates)
                StartLocationUpdates();
        }

      

        public void OnConnectionSuspended(int cause)
        {
            mGoogleApiClient.Connect();
        }

        public void OnLocationChanged(Location location)
        {
            mLastLocation = location;
            DisplayLocation();
        }
    }
}


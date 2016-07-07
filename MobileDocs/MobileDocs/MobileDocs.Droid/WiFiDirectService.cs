using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Net.Wifi.P2p;
using Android.Content.PM;
using Android.Net;
using Android.Net.Wifi;
using System.Threading.Tasks;

namespace MobileDocs.Droid
{
    class WiFiDirectService : Java.Lang.Object,
        WifiP2pManager.IActionListener,
        WifiP2pManager.IPeerListListener,
        WifiP2pManager.IConnectionInfoListener
    {
        static WifiP2pManager wifiManager;
        static WifiP2pManager.Channel channel;
        IntentFilter filter = new IntentFilter();
        private Receiver receiver;
        bool isConnected = false;
        WebService web = new WebService();

        Activity mActivity;

        string currentDevice = "";

        const string NAME = "VOSTRO";

        public WiFiDirectService(Activity activity)
        {
            mActivity = activity;
            if (wifiManager == null)
            {
                wifiManager = (WifiP2pManager)activity.GetSystemService(Context.WifiP2pService);
            }

            if (channel == null)
            {
                channel = wifiManager.Initialize(activity, activity.MainLooper, null);
            }

            if (receiver == null)
            {
                filter.AddAction(WifiP2pManager.WifiP2pPeersChangedAction);
                filter.AddAction(WifiP2pManager.WifiP2pStateChangedAction);
                filter.AddAction(WifiP2pManager.WifiP2pConnectionChangedAction);
                filter.AddAction(WifiP2pManager.WifiP2pThisDeviceChangedAction);

                receiver = new Receiver(wifiManager, channel, this);
                activity.RegisterReceiver(receiver, filter);
            }

            PackageManager pm = activity.PackageManager;
            FeatureInfo[] features = pm.GetSystemAvailableFeatures();
            foreach (FeatureInfo info in features)
            {
                if (info != null && info.Name != null && info.Name.Equals("android.hardware.wifi.direct"))
                {
                    Console.WriteLine("wifi p2p is supported");
                }
            }

            //wifiManager.StopPeerDiscovery(channel, this);
            wifiManager.DiscoverPeers(channel, this);
            Console.WriteLine("start discovering");
        }


        public class Receiver : BroadcastReceiver
        {
            WifiP2pManager manager;
            WifiP2pManager.Channel channel;
            WiFiDirectService service;

            public Receiver(WifiP2pManager manager, WifiP2pManager.Channel channel, WiFiDirectService service)
            {
                this.manager = manager;
                this.channel = channel;
                this.service = service;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;

                Console.WriteLine(action);
                if (action == WifiP2pManager.WifiP2pPeersChangedAction)
                {
                    manager.RequestPeers(channel, service);
                    Console.WriteLine("request for peers list");
                }
                else if (action == WifiP2pManager.WifiP2pConnectionChangedAction)
                {
                    NetworkInfo networkInfo = (NetworkInfo)intent.GetParcelableExtra(WifiP2pManager.ExtraNetworkInfo);

                    if (networkInfo.IsConnected)
                    {
                        Console.WriteLine("connected to " + networkInfo.ToString());
                    }
                    // We are connected with the other device, request connection
                    // info to find group owner IP
                    manager.RequestConnectionInfo(channel, service);
                    Console.WriteLine("request for connection info");
                }
            }
        }

        //WifiP2pManager.IActionListener
        public void OnFailure([GeneratedEnum] WifiP2pFailureReason reason)
        {
            Console.WriteLine("fail: " + reason.ToString());
        }

        public void OnSuccess()
        {
            Console.WriteLine("success");
        }

        //WifiP2pManager.IPeerListListener
        public void OnPeersAvailable(WifiP2pDeviceList peers)
        {
            if (currentDevice == "")
            {
                foreach (var device in peers.DeviceList)
                {
                    Console.WriteLine(device.DeviceName);
                    if (device.DeviceName == NAME)
                    {
                        WifiP2pConfig config = new WifiP2pConfig();
                        config.DeviceAddress = device.DeviceAddress;
                        config.Wps.Setup = WpsInfo.Pbc;

                        if (isConnected)
                        {
                            wifiManager.CancelConnect(channel, this);
                            isConnected = false;
                        }
                        wifiManager.Connect(channel, config, this);
                        currentDevice = device.DeviceName;
                    }
                }
            }
        }

        //WifiP2pManager.IConnectionInfoListener
        public void OnConnectionInfoAvailable(WifiP2pInfo info)
        {
            Console.WriteLine("\t" + info.ToString());
            //mText.Text += "\n" + info.ToString();

            if (info.GroupOwnerAddress != null)
            {
                isConnected = true;
                Task.Run(() =>
                {
                    //await Task.Delay(2000);
                    //Socket socket = new Socket();
                    //await socket.ConnectAsync(new InetSocketAddress(info.GroupOwnerAddress.HostAddress, 50001));
                    //Java.IO.DataOutputStream stream = new Java.IO.DataOutputStream(socket.OutputStream);
                    //byte[] data = new byte[1];
                    //data[0] = 0;
                    //await stream.WriteAsync(data);
                    //stream.Close();
                    //socket.Close();
                    //Console.WriteLine("closed");
                    Console.WriteLine(info.GroupOwnerAddress.HostAddress);
                    web.initStream(info.GroupOwnerAddress.HostAddress, 50001);
                    web.SetOnDataReceivedListener((MainActivity)mActivity);
                });
            }
        }
    }
}
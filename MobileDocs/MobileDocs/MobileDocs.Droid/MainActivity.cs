using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Bluetooth.LE;

namespace MobileDocs.Droid
{
	[Activity (Label = "MobileDocs.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity,
        BluetoothAdapter.ILeScanCallback
	{
		int count = 1;

        BluetoothAdapter mBluetoothAdapter;
        static BluetoothGatt mBluetoothGatt;
        static BluetoothGattCharacteristic mCharacteristic;
        static BluetoothGattDescriptor mNotificationDescriptor;

        static bool isReceivingNotifications = false;

        static TextView mText;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
                if (mNotificationDescriptor != null && mBluetoothGatt != null)
                {
                    if (isReceivingNotifications)
                    {
                        mNotificationDescriptor.SetValue(new byte[] { 0, 0 });
                    } else
                    {
                        mNotificationDescriptor.SetValue(new byte[] { 0, 1 });
                    }
                    if (mBluetoothGatt.WriteDescriptor(mNotificationDescriptor))
                    {
                        isReceivingNotifications = !isReceivingNotifications;
                    }
                    Console.WriteLine("notifications " + isReceivingNotifications);
                } 
			};

            mText = FindViewById<TextView>(Resource.Id.text);

            BluetoothManager bluetoothManager = (BluetoothManager)GetSystemService(Context.BluetoothService);
            mBluetoothAdapter = bluetoothManager.Adapter;

            if (mBluetoothAdapter == null || !mBluetoothAdapter.IsEnabled)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableBtIntent, 1);
            }
            else
            {
                mBluetoothAdapter.StartLeScan(this);
            }

        }


        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            //mText.Text += device.GetUuids().ToString() + "\n";
            mBluetoothGatt = device.ConnectGatt(this, true, new GattCallback());
        }

        private class GattCallback : BluetoothGattCallback
        {
            public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
            {
                Console.WriteLine("status " + status);
                mBluetoothGatt.DiscoverServices();
                
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
            {
                byte[] characteristicValue = characteristic.GetValue();
                if (characteristicValue != null && characteristicValue.Length > 0)
                {
                    Console.WriteLine("cvalue " + BitConverter.ToString(characteristicValue));
                }

                Console.WriteLine("enable notification " + mBluetoothGatt.SetCharacteristicNotification(characteristic, true));

                foreach (var descriptor in characteristic.Descriptors)
                {
                    Console.WriteLine("descriptor " + descriptor.Uuid.ToString());
                    //if (descriptor.Uuid.ToString().Equals("00001234-0000-1000-8000-00805f9b34fb"))
                    //{
                    //    Console.WriteLine("read descriptor " + mBluetoothGatt.ReadDescriptor(descriptor));
                    //}
                    if (descriptor.Uuid.ToString().Equals("00002902-0000-1000-8000-00805f9b34fb"))
                    {
                        mNotificationDescriptor = descriptor;
                        //Console.WriteLine("read descriptor " + mBluetoothGatt.ReadDescriptor(descriptor));
                        descriptor.SetValue(new byte[] { 0, 1});
                        isReceivingNotifications = mBluetoothGatt.WriteDescriptor(descriptor);
                        Console.WriteLine("descriptor write " + isReceivingNotifications);
                    }
                }
            }

            public override void OnDescriptorRead(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
            {
                Console.WriteLine("descriptor " + descriptor.Uuid.ToString());
                if (descriptor.Uuid.ToString().Equals("00001234-0000-1000-8000-00805f9b34fb"))
                {
                    byte[] descriptorValue = descriptor.GetValue();
                    if (descriptorValue != null && descriptorValue.Length > 0)
                    {
                        Console.WriteLine("value " + BitConverter.ToString(descriptorValue));
                    }
                }
            }

            public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
            {
                Console.WriteLine("descriptor write " + descriptor.Uuid.ToString() + " " + status);
                byte[] descriptorValue = descriptor.GetValue();
                if (descriptorValue != null && descriptorValue.Length > 0)
                {
                    Console.WriteLine("value " + BitConverter.ToString(descriptorValue));
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                Console.WriteLine("characteristic changed " + characteristic.Uuid.ToString());
                byte[] characteristicValue = characteristic.GetValue();
                if (characteristicValue != null && characteristicValue.Length > 0)
                {
                    Console.WriteLine("cvalue " + BitConverter.ToString(characteristicValue));
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
            {
                Console.WriteLine("service " + status);
                foreach(var service in gatt.Services)
                {
                    Console.WriteLine("service " + service.Uuid.ToString());
                    foreach(var characteristic in service.Characteristics)
                    {
                        Console.WriteLine("characterstic " + characteristic.Uuid.ToString());
                        if (characteristic.Uuid.ToString().Equals("00000000-0000-1000-8000-00805f9b34fb"))
                        {
                            mBluetoothGatt.ReadCharacteristic(characteristic);
                        }                        
                    }
                }
            }
        }
    }
}



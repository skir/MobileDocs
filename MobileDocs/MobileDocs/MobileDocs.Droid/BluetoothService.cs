using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Bluetooth;
using System.Diagnostics;

namespace MobileDocs.Droid
{
    class BluetoothService : Java.Lang.Object,
        BluetoothAdapter.ILeScanCallback
    {

        BluetoothAdapter mBluetoothAdapter;
        static BluetoothGatt mBluetoothGatt;
        static BluetoothGattCharacteristic mCharacteristic;
        static BluetoothGattDescriptor mNotificationDescriptor;

        Activity mActivity;

        static bool isReceivingNotifications = false;

        public BluetoothService(BluetoothManager manager, Activity activity)
        {
            mBluetoothAdapter = manager.Adapter;
            mActivity = activity;

            if (mBluetoothAdapter == null || !mBluetoothAdapter.IsEnabled)
            {
                Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                activity.StartActivityForResult(enableBtIntent, 1);
            }
            else
            {
                mBluetoothAdapter.StartLeScan(this);
            }
        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            //mText.Text += device.GetUuids().ToString() + "\n";
            GattCallback callback = new GattCallback();
            mBluetoothGatt = device.ConnectGatt(mActivity, true, callback);
        }

        private class GattCallback : BluetoothGattCallback
        {
            int bytesReceived = 0;
            Stopwatch stopWatch = new Stopwatch();
            string bytesString = "";

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
                        descriptor.SetValue(new byte[] { 0, 1 });
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

            private bool isLenthPrinted = false;
            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                //Console.WriteLine("characteristic changed " + characteristic.Uuid.ToString());
                byte[] characteristicValue = characteristic.GetValue();
                if (characteristicValue != null && characteristicValue.Length > 0)
                {
                    //Console.WriteLine("cvalue " + BitConverter.ToString(characteristicValue));

                    //Java.IO.File tempMp3 = Java.IO.File.CreateTempFile("temp", "mp3", mContext.CacheDir);
                    ////tempMp3.DeleteOnExit();
                    //Java.IO.FileOutputStream fos = new Java.IO.FileOutputStream(tempMp3);
                    //fos.Write(characteristicValue);
                    //fos.Close();

                    //Java.IO.FileInputStream fis = new Java.IO.FileInputStream(tempMp3);
                    //mPlayer.Reset();
                    //mPlayer.SetDataSource(fis.FD);

                    //mPlayer.Prepare();
                    //mPlayer.Start();
                    if (!isLenthPrinted)
                    {
                        Console.WriteLine(characteristicValue.Length);
                        isLenthPrinted = !isLenthPrinted;
                    }
                    bytesString += "-" + BitConverter.ToString(characteristicValue);
                    //bytesString = "";
                    //for (int j = 0; j < characteristicValue.Length; ++j)
                    //{
                    //    byte tmp = 128;
                    //    for (int i = 0; i < 8; ++i)
                    //    {
                    //        bytesString += ((tmp & characteristicValue[j]) > 0) ? 1 : 0;
                    //        tmp >>= 1;
                    //    }
                    //}
                    //mAudioTrack.Write(characteristicValue, 0, characteristicValue.Length);
                    //activity.DecodePCMtoAAC(characteristicValue);
                    if (bytesReceived == 0)
                    {
                        stopWatch.Start();
                        Console.WriteLine("started");
                    }
                    bytesReceived += characteristicValue.Length;
                    if (stopWatch.ElapsedMilliseconds > 5000 && stopWatch.IsRunning)
                    {
                        stopWatch.Stop();
                        Console.WriteLine("SPEED " + bytesReceived / (stopWatch.ElapsedMilliseconds / 1000));
                    }
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
            {
                Console.WriteLine("service " + status);
                foreach (var service in gatt.Services)
                {
                    Console.WriteLine("service " + service.Uuid.ToString());
                    if (service.Uuid.ToString().Equals("0003a150-0000-1000-8000-00805f9b0131"))
                    {
                        foreach (var characteristic in service.Characteristics)
                        {
                            Console.WriteLine("characterstic " + characteristic.Uuid.ToString());
                            if (characteristic.Uuid.ToString().Equals("00000000-0000-1000-8000-00805f9b34fb"))
                            {
                                mBluetoothGatt.ReadCharacteristic(characteristic);
                                Console.WriteLine("enable notification " + mBluetoothGatt.SetCharacteristicNotification(characteristic, true));
                            }
                        }
                    }
                }
            }
        }
    }
}
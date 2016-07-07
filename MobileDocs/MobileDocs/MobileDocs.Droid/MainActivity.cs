using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Media;
using System.Diagnostics;
using Java.Nio;

namespace MobileDocs.Droid
{
	[Activity (Label = "MobileDocs.Droid", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity,
        WebService.OnDataReceived
    {
		int count = 1;
        
        static TextView mText;
        static EditText mPort;

        static Context mContext;

        static AudioTrack mAudioTrack;
        static MediaPlayer mPlayer = null;
        private MediaCodec decoder;
        
        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
            mContext = this;
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.Click += delegate {
				button.Text = string.Format ("{0} clicks!", count++);
                //mText.Text = bytesReceived.ToString();
                //if (mNotificationDescriptor != null && mBluetoothGatt != null)
                //{
                //    if (isReceivingNotifications)
                //    {
                //        mNotificationDescriptor.SetValue(new byte[] { 0, 0 });
                //    }
                //    else
                //    {
                //        mNotificationDescriptor.SetValue(new byte[] { 0, 1 });
                //    }
                //    if (mBluetoothGatt.WriteDescriptor(mNotificationDescriptor))
                //    {
                //        isReceivingNotifications = !isReceivingNotifications;
                //    }
                //    Console.WriteLine("notifications " + isReceivingNotifications);
                //}
                //new WiFiDirectService(this);
            };

            FindViewById<Button>(Resource.Id.toggle).Click += delegate
            {
                //if (mAudioTrack != null)
                //{
                //    TogglePlayingState();
                //}

                //try
                //{
                //    Java.IO.OutputStreamWriter outputStreamWriter = new Java.IO.OutputStreamWriter(OpenFileOutput("config.txt", FileCreationMode.WorldReadable));
                //    outputStreamWriter.Write(bytesString);
                //    outputStreamWriter.Close();
                //}
                //catch (Java.IO.IOException e)
                //{
                //    Console.WriteLine("File write failed: " + e.ToString());
                //}
                //Console.WriteLine(bytesString);
            };

            mText = FindViewById<TextView>(Resource.Id.text);
            mPort = FindViewById<EditText>(Resource.Id.port);

            new BluetoothService((BluetoothManager)GetSystemService(Context.BluetoothService), this);       

            mAudioTrack = new AudioTrack(Stream.Music, 44100, ChannelOut.Stereo, Encoding.Pcm16bit, 100000, AudioTrackMode.Stream);

            decoder = MediaCodec.CreateDecoderByType("audio/mp4a-latm");
            MediaFormat format = new MediaFormat();
            format.SetString(MediaFormat.KeyMime, "audio/mp4a-latm");
            format.SetInteger(MediaFormat.KeyChannelCount, 1);
            format.SetInteger(MediaFormat.KeySampleRate, 44100);
            format.SetInteger(MediaFormat.KeyBitRate, 64 * 1024);//AAC-HE 64kbps
            format.SetInteger(MediaFormat.KeyAacProfile, (int) Android.Media.MediaCodecProfileType.Aacobjecthe);

            decoder.Configure(format, null, null, 0);

            decoder.Start();
            mAudioTrack.Play();

            mPlayer = new MediaPlayer();
        }

        public void OnReceived(byte[] data)
        {
            mAudioTrack.Write(data, 1, data.Length - 1);
        }

        public void TogglePlayingState()
        {
            //Console.WriteLine(mAudioTrack.PlayState);
            if (mAudioTrack.PlayState == PlayState.Playing)
            {
                mAudioTrack.Pause();
            }
            else if (mAudioTrack.PlayState == PlayState.Paused)
            {
                mAudioTrack.Play();
            }
            Console.WriteLine("toggled");
            //mText.Text = mAudioTrack.PlayState.ToString();
        }

        private void DecodePCMtoAAC(byte[] data)
        {
            ByteBuffer[] inputBuffers;
            ByteBuffer[] outputBuffers;

            ByteBuffer inputBuffer;
            ByteBuffer outputBuffer;

            MediaCodec.BufferInfo bufferInfo;
            int inputBufferIndex;
            int outputBufferIndex;
            byte[] outData;

            inputBuffers = decoder.GetInputBuffers();
            outputBuffers = decoder.GetOutputBuffers();
            inputBufferIndex = decoder.DequeueInputBuffer(-1);
            if (inputBufferIndex >= 0)
            {
                inputBuffer = inputBuffers[inputBufferIndex];
                inputBuffer.Clear();

                inputBuffer.Put(data);

                decoder.QueueInputBuffer(inputBufferIndex, 0, data.Length, 0, 0);
            }

            bufferInfo = new MediaCodec.BufferInfo();
            outputBufferIndex = decoder.DequeueOutputBuffer(bufferInfo, 0);

            while (outputBufferIndex >= 0)
            {
                outputBuffer = outputBuffers[outputBufferIndex];

                outputBuffer.Position(bufferInfo.Offset);
                outputBuffer.Limit(bufferInfo.Offset + bufferInfo.Size);

                outData = new byte[bufferInfo.Size];
                outputBuffer.Get(outData);

                //  Log.d("AudioDecoder", outData.length + " bytes decoded");

                mAudioTrack.Write(outData, 0, outData.Length);

                decoder.ReleaseOutputBuffer(outputBufferIndex, false);
                outputBufferIndex = decoder.DequeueOutputBuffer(bufferInfo, 0);

            }
        }
    }
}



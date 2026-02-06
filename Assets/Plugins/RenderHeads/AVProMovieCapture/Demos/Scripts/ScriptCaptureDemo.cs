using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

//-----------------------------------------------------------------------------
// Copyright 2012-2025 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture.Demos
{

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    /// <summary>
    /// Demo code to create and write frames manually into a movie using the low-level API via scripting
    /// </summary>
    public class ScriptCaptureDemo : MonoBehaviour
	{
		[Header("Options")]
		[SerializeField] private uint _videoWidth = 1920;
		[SerializeField] private uint _videoHeight = 1080;
		[SerializeField] private int _frameRate = 30;
		[SerializeField] private string _filePath = "c:/Temp/Test.mp4";
		[SerializeField] private float _videoLength = 5; // seconds
		[Space]
		[Header("GUI Options")]
		[SerializeField] private bool _drawGUI = true;
		[SerializeField] private bool _runOnStart = false;
		[SerializeField] [Range(1, 10)] private float _GUIScale = 1.5f;
		// GUI
		[Header("Debug")]
        [SerializeField] private float labelWidth = 100;
        [SerializeField] private float itemHeight = 25;
        [SerializeField] private float inputWidth = 55;
        [SerializeField] private float WindowWidth = 370;
        [SerializeField] private float WindowHeight = 185;

        // Codec
        private const string X264CodecName = "H264";
        private const string FallbackCodecName = "Uncompressed";
        // State
        private Codec _videoCodec;
		private int _encoderHandle;

		private void Start()
		{
			if (NativePlugin.Init())
			{
				// Find the index for the video codec
				_videoCodec = CodecManager.FindCodec(CodecType.Video, X264CodecName);
				if (_videoCodec == null)
				{
					_videoCodec = CodecManager.FindCodec(CodecType.Video, FallbackCodecName);
				}
			}
			else
			{
				this.enabled = false;
			}

			if (_runOnStart)
			{
				CreateVideoFromByteArray(_filePath, _videoWidth, _videoHeight, _frameRate);
			}
        }

		private void OnDestroy()
		{
			NativePlugin.Deinit();
		}

		public void CreateVideoFromByteArray(string filePath, uint width, uint height, int frameRate)
		{
            Debug.Log("[ScriptCatpureDemo] Started Video Creation");

            byte[] frameData = new byte[width * height * 4];
			GCHandle frameHandle = GCHandle.Alloc(frameData, GCHandleType.Pinned);

            // Start the recording session
            int encoderHandle = NativePlugin.CreateRecorderVideo(filePath, width, height, frameRate, (int)NativePlugin.PixelFormat.RGBA32, false, false, _videoCodec.Index, AudioCaptureSource.None, 0, 0, -1, -1, true, null);
			if (encoderHandle >= 0)
			{
				NativePlugin.Start(encoderHandle);

				int numFrames = (int)(_videoLength * _frameRate);
				for (int i = 0; i < numFrames; i++)
				{
                    // TODO: fill the byte array with your own data :)
                    FillFrameWithColorFade(frameData, width, height, i, numFrames);

                    // Wait for the encoder to be ready for the next frame
                    int numAttempts = 32;
					while (numAttempts > 0)
					{
						if (NativePlugin.IsNewFrameDue(encoderHandle))
						{
							// Encode the new frame
							NativePlugin.EncodeFrame(encoderHandle, frameHandle.AddrOfPinnedObject());
							break;
						}
						System.Threading.Thread.Sleep(1);
						numAttempts--;
					}
				}

				// End the session
				NativePlugin.Stop(encoderHandle, false);
				NativePlugin.FreeRecorder(encoderHandle);
			}

			if (frameHandle.IsAllocated)
			{
				frameHandle.Free();
			}
			Debug.Log("[ScriptCatpureDemo] Finished Video Creation");
		}

        private void FillFrameWithColorFade(
			byte[] frameData,
			uint width,
			uint height,
			int frameIndex,
			int totalFrames)
        {
            // Progress from 0 → 1
            float t = (float)frameIndex / (totalFrames - 1);

            // Hue goes from 0 → 360 → back to red
            float hue = t * 360f;

            Color rgb = Color.HSVToRGB(hue / 360f, 1f, 1f);

            byte r = (byte)(rgb.r * 255);
            byte g = (byte)(rgb.g * 255);
            byte b = (byte)(rgb.b * 255);
            byte a = 255;

            int index = 0;
            int pixelCount = (int)(width * height);

            for (int i = 0; i < pixelCount; i++)
            {
                frameData[index++] = r; // R
                frameData[index++] = g; // G
                frameData[index++] = b; // B
                frameData[index++] = a; // A
            }
        }

        public void OnGUI()
        {
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
            GUI.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_GUIScale, _GUIScale, 1f));

			using (new GUILayout.AreaScope(new Rect(10, 10, WindowWidth, WindowHeight), "", new GUIStyle(GUI.skin.box)))
			{
                GUILayout.Label($"<b>Script Capture Demo</b>", new GUIStyle(GUI.skin.label) { fontSize = 16 });
				using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(itemHeight)))
				{
					GUILayout.Label("File Path: ", GUILayout.MaxWidth(labelWidth));
                    _filePath = GUILayout.TextField(_filePath);
                }
                using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(itemHeight)))
                {
                    GUILayout.Label("Width: ", GUILayout.MaxWidth(labelWidth));
					uint.TryParse(GUILayout.TextField(_videoWidth.ToString(), GUILayout.MaxWidth(inputWidth)), out _videoWidth);
                    _videoWidth = (uint)GUILayout.HorizontalSlider(_videoWidth, 1, 3840);
                }
                using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(itemHeight)))
                {
                    GUILayout.Label("Height: ", GUILayout.MaxWidth(labelWidth));
                    uint.TryParse(GUILayout.TextField(_videoHeight.ToString(), GUILayout.MaxWidth(inputWidth)), out _videoHeight);
                    _videoHeight = (uint)GUILayout.HorizontalSlider(_videoHeight, 1, 2160);
                }
                using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(itemHeight)))
                {
                    GUILayout.Label("Frame Rate: ", GUILayout.MaxWidth(labelWidth));
                    int.TryParse(GUILayout.TextField(_frameRate.ToString(), GUILayout.MaxWidth(inputWidth)), out _frameRate);
                    _frameRate = (int)GUILayout.HorizontalSlider(_frameRate, 1, 120);
                }
                using (new GUILayout.HorizontalScope(GUILayout.MaxHeight(itemHeight)))
                {
                    GUILayout.Label("Video Length: ", GUILayout.MaxWidth(labelWidth));
                    float.TryParse(GUILayout.TextField(_videoLength.ToString(), GUILayout.MaxWidth(inputWidth)), out _videoLength);
                    _videoLength = (int)GUILayout.HorizontalSlider(_videoLength, 1, 20);
                }
                GUILayout.Space(10);
				if (GUILayout.Button("Create Video"))
				{
                    CreateVideoFromByteArray(_filePath, _videoWidth, _videoHeight, _frameRate);
                }
            }

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        }
    }
#endif
}
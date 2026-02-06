#if UNITY_2017_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
#endif
#if UNITY_5_6_OR_NEWER && UNITY_2018_3_OR_NEWER
	#define AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
#endif
#if UNITY_2017_1_OR_NEWER
	#define AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
#endif
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using static RenderHeads.Media.AVProMovieCapture.CaptureBase;

//-----------------------------------------------------------------------------
// Copyright 2012-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(CaptureBase), true)]
	public class CaptureBaseEditor : UnityEditor.Editor
	{
		private const string SettingsPrefix = "AVProMovieCapture-BaseEditor-";
		internal const string UnityAssetStore_FullVersionUrl = "https://assetstore.unity.com/packages/tools/video/avpro-movie-capture-151061?aid=1101lcNgx";

		public readonly static string[] ResolutionStrings = { "8192x8192 (1:1)", "8192x4096 (2:1)", "4096x4096 (1:1)", "4096x2048 (2:1)", "2048x4096 (1:2)", "3840x2160 (16:9)", "3840x2048 (15:8)", "3840x1920 (2:1)", "2560x1440 (16:9)", "2048x2048 (1:1)", "2048x1024 (2:1)", "1920x1080 (16:9)", "1280x720 (16:9)", "1024x768 (4:3)", "800x600 (4:3)", "800x450 (16:9)", "640x480 (4:3)", "640x360 (16:9)", "320x240 (4:3)", "Original", "Custom" };

		private readonly static GUIContent _guiBlankSpace = new GUIContent(" ");
		private readonly static GUIContent _guiContentMotionBlurSamples = new GUIContent("Samples");
		private readonly static GUIContent _guiContentMotionBlurCameras = new GUIContent("Cameras");
		private readonly static GUIContent _guiContentFolder = new GUIContent("Folder");
		private readonly static GUIContent _guiContentPath = new GUIContent("Path");
		private readonly static GUIContent _guiContentSubfolders = new GUIContent("Subfolder(s)");
		private readonly static GUIContent _guiContentPrefix = new GUIContent("Prefix");
		//private readonly static GUIContent _guiContentAppendTimestamp = new GUIContent("Append Timestamp");
		private readonly static GUIContent _guiContentManualExtension = new GUIContent("Manual Extension");
		private readonly static GUIContent _guiContentExtension = new GUIContent("Extension");
		private readonly static GUIContent _guiContentStartFrame = new GUIContent("Start Frame");
		private readonly static GUIContent _guiContentZeroDigits = new GUIContent("Zero Digits");
		private readonly static GUIContent _guiContentPipePath = new GUIContent("Pipe Path");
		private readonly static GUIContent _guiContentToggleKey = new GUIContent("Toggle Key");
		private readonly static GUIContent _guiContentStartMode = new GUIContent("Start Mode");
		private readonly static GUIContent _guiContentStartDelay = new GUIContent("Start Delay");
		private readonly static GUIContent _guiContentSeconds = new GUIContent("Seconds");
		private readonly static GUIContent _guiContentStopMode = new GUIContent("Stop Mode");
		private readonly static GUIContent _guiContentFrames = new GUIContent("Frames");
		private readonly static GUIContent _guiContentCodecSearchOrder = new GUIContent("Codec Search Order");
		private readonly static GUIContent _guiContentSupportTextureRecreate = new GUIContent("Support Texture Recreate", "Using this option will slow rendering (forces GPU sync), but is needed to handle cases where texture resources are recreated, due to alt-tab or window resizing.");
		private readonly static GUIContent _guiStreamableMP4 = new GUIContent("Streamable MP4");
		private readonly static GUIContent _guiStereoPacking = new GUIContent("Stereo Packing");
		private readonly static GUIContent _guiSphericalLayout = new GUIContent("Spherical Layout");
		private readonly static GUIContent _guiAndroidUpdateMediaGallery = new GUIContent("Update Media Gallery");
		private readonly static GUIContent _guiAndroidNoCaptureRotation = new GUIContent("No Capture Rotation");
		private readonly static GUIContent _gui_iOSSaveCaptureWhenAppLosesFocus = new GUIContent("Save capture when app loses focus", "When enabled the current capture will be saved when the app loses focus, when disabled the capture will be cancelled");
		private readonly static GUIContent _guiContentFramePTSMode = new GUIContent("Frame Presentation Timestamp", "Realtime captures only, affects how each captured frame's timestamp is generated");
		private readonly static GUIContent _guiContentWriteOrientationMetadata = new GUIContent("Write Orientation Metadata", "Writes the camera's current orientation to the captured video track");
		private readonly static GUIContent _guiContentFilenameComponents = new GUIContent("", "Select which custom prefixes you would like to append to the file name");
		private readonly static GUIContent _guiContentFilenameComponentCustomString = new GUIContent("Custom String", "Write a custom string that will be appended to the end of the file name");

		private static bool _isTrialVersion = false;
		private SerializedProperty _propCaptureKey;
		private SerializedProperty _propMinimumDiskSpaceMB;
		private SerializedProperty _propPersistAcrossSceneLoads;

		private SerializedProperty _propIsRealtime;

		private SerializedProperty _propOutputTarget;
		private SerializedProperty _propImageSequenceFormatWindows;
		private SerializedProperty _propImageSequenceFormatMacOS;
		private SerializedProperty _propImageSequenceFormatIOS;
		private SerializedProperty _propImageSequenceFormatAndroid;
		private SerializedProperty _propImageSequenceStartFrame;
		private SerializedProperty _propImageSequenceZeroDigits;
		private SerializedProperty _propOutputFolderType;
		private SerializedProperty _propOutputFolderPath;
		//private SerializedProperty _propAppendFilenameTimestamp;
		private SerializedProperty _propFileNamePrefix;
		private SerializedProperty _propAllowManualFileExtension;
		private SerializedProperty _propFileNameExtension;
		private SerializedProperty _propForceFileName;
		private SerializedProperty _propNamedPipePath;
        private SerializedProperty _propFileNameComponents;
        private SerializedProperty _propFilenameComponentCustomText;
        private SerializedProperty _propFilenameComponentSeperator;

        private SerializedProperty _propVideoCodecPriorityWindows;
		private SerializedProperty _propVideoCodecPriorityMacOS;
		//private SerializedProperty _propVideoCodecPriorityAndroid;
		private SerializedProperty _propForceVideoCodecIndexWindows;
		private SerializedProperty _propForceVideoCodecIndexMacOS;
		private SerializedProperty _propForceVideoCodecIndexIOS;
		private SerializedProperty _propForceVideoCodecIndexAndroid;

		private SerializedProperty _propAudioCaptureSource;
		private SerializedProperty _propAudioCodecPriorityWindows;
		private SerializedProperty _propAudioCodecPriorityMacOS;
		//private SerializedProperty _propAudioCodecPriorityAndroid;
		private SerializedProperty _propForceAudioCodecIndexWindows;
		private SerializedProperty _propForceAudioCodecIndexMacOS;
		private SerializedProperty _propForceAudioCodecIndexIOS;
		private SerializedProperty _propForceAudioCodecIndexAndroid;
		private SerializedProperty _propForceAudioDeviceIndex;
		private SerializedProperty _propUnityAudioCapture;
		private SerializedProperty _propManualAudioSampleRate;
		private SerializedProperty _propManualAudioChannelCount;

		private SerializedProperty _propStartTrigger;
		private SerializedProperty _propStartDelay;
		private SerializedProperty _propStartDelaySeconds;

		private SerializedProperty _propStopMode;
		private SerializedProperty _propStopFrames;
		private SerializedProperty _propStopSeconds;
//		private SerializedProperty _propPauseCaptureOnAppPause;

		private class PropVideoHints
		{
			public SerializedProperty propAverageBitrate;
			public SerializedProperty propMaximumBitrate;
			public SerializedProperty propQuality;
			public SerializedProperty propKeyframeInterval;
			public SerializedProperty propTransparency;
			public SerializedProperty propHardwareEncoding;
			public SerializedProperty propFastStart;
			public SerializedProperty propInjectStereoPacking;
			public SerializedProperty propStereoPacking;
			public SerializedProperty propInjectSphericalVideoLayout;
			public SerializedProperty propSphericalVideoLayout;
			public SerializedProperty propEnableConstantQuality;
			public SerializedProperty propEnableFragmentedWriting;
			public SerializedProperty propMovieFragmentInterval;
			public SerializedProperty propColourRange;
			public SerializedProperty propFramePTSMode;
		}

		private class PropImageHints
		{
			public SerializedProperty propQuality;
			public SerializedProperty propTransparency;
		}

		private PropVideoHints[] _propVideoHints;
		private PropImageHints[] _propImageHints;

		private SerializedProperty _propDownScale;
		private SerializedProperty _propMaxVideoSize;
		private SerializedProperty _propFrameRate;
		private SerializedProperty _propTimelapseScale;
		private SerializedProperty _propFrameUpdateMode;
		private SerializedProperty _propFlipVertically;
		private SerializedProperty _propForceGpuFlush;
		private SerializedProperty _propWaitForEndOfFrame;
		private SerializedProperty _propAndroidUpdateMediaGallery;
		private SerializedProperty _propAndroidNoCaptureRotation;
		private SerializedProperty _prop_iOSSaveCaptureWhenAppLosesFocus;
		private SerializedProperty _propWriteOrientationMetadata;

		private SerializedProperty _propUseMotionBlur;
		private SerializedProperty _propMotionBlurSamples;
		private SerializedProperty _propMotionBlurCameras;

		private SerializedProperty _propLogCaptureStartStop;
		private SerializedProperty _propAllowVsyncDisable;
		private SerializedProperty _propSupportTextureRecreate;
		#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
		private SerializedProperty _propTimelineController;
		#endif
		#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
		private SerializedProperty _propVideoPlayerController;
		#endif

		private static bool _isExpandedStartStop = false;
		private static bool _isExpandedOutput = false;
		private static bool _isExpandedVisual = false;
		private static bool _isExpandedAudio = false;
		private static bool _isExpandedPost = false;
		private static bool _isExpandedMisc = false;
		private static bool _isExpandedTrial = true;
		private static bool _isExpandedAbout = false;
		private static NativePlugin.Platform _selectedPlatform = NativePlugin.Platform.Windows;
		private static GUIStyle _stylePlatformBox = null;

		protected CaptureBase _baseCapture;


		// TODO


		private AnimBool _aboutSeciton = new();
		private AnimBool _startStopSection = new();
		private AnimBool _outputFilePathSection = new();
		private AnimBool _visualSection = new();
		private AnimBool _audioSection = new();
		private AnimBool _postSection = new();
		private AnimBool _miscSection = new();

		// Start Stop (ss)
		private AnimBool _ss_StartDelay = new();
		private AnimBool _ss_StopMode_FramesEncoded = new();
		private AnimBool _ss_StopMode_Other = new();

		// Output (o)
		private AnimBool _o_OutputTarget_Video = new();
		private AnimBool _o_OutputTarget_Image = new();
		private AnimBool _o_OutputTarget_Pipe = new();
		private AnimBool _o_ManualExtension = new();
		private AnimBool _o_CustomFilenameComponent = new();
		private AnimBool _o_FilenameComponent = new();

		// Visual (v)
		private AnimBool _v_DownScale = new();
		private AnimBool _v_OutputTarget_Video = new();
		private AnimBool _v_OutputTarget_Image = new();
		// codecs (c) + encoderhints (e)
		private AnimBool _vce_SelectedPlatform_Windows = new();
		private AnimBool _vce_SelectedPlatform_macOS = new();
		private AnimBool _vce_SelectedPlatform_iOS = new();
		private AnimBool _vce_SelectedPlatform_Android = new();
		private AnimBool _ve_EnableFragmentedWriting = new();
		// motionblur (mb)
		private AnimBool _vmb_MotionBlur = new();

		// Audio (a) + post (p)
		private AnimBool _ap_OutputTarget = new();
		private AnimBool _a_CaptureSourceNone = new();
		private AnimBool _a_OfflineInvalidOption = new();
		private AnimBool _a_RealtimeWwise = new();
		private AnimBool _a_OfflineWwise = new();
		private AnimBool _a_ShowAudioOptions = new();
		private AnimBool _a_CaptureSourceMicrophone = new();
		private AnimBool _a_CaptureSourceUnityWwise = new();
		private AnimBool _a_CaptureSourceManual = new();
		private AnimBool _p_StereoPackingCustom = new();
		private AnimBool _p_SphericalLayoutCustom = new();

		// Misc (m)
		private AnimBool _m_SelectedPlatform_Windows = new();
		private AnimBool _m_SelectedPlatform_Android = new();
		private AnimBool _m_SelectedPlatform_macOS = new();
		private AnimBool _m_SelectedPlatform_iOS = new();


        public override void OnInspectorGUI()
		{
			// Warning if the base component is used
			if (this.target.GetType() == typeof(CaptureBase))
			{
				GUI.color = Color.yellow;
				GUILayout.BeginVertical("box");
				GUILayout.TextArea("Error: This is not a component, this is the base class.\n\nPlease add one of the components\n(eg:CaptureFromScene / CaptureFromCamera etc)");
				GUILayout.EndVertical();
				return;
			}

			if (_stylePlatformBox == null)
			{
				_stylePlatformBox = new GUIStyle(GUI.skin.box);
				_stylePlatformBox.padding.top = 0;
				_stylePlatformBox.padding.bottom = 0;
			}

			GUI_Header();
			GUI_BaseOptions();
		}

		protected virtual void GUI_User()
		{

		}

		protected void GUI_Header()
		{
			// Describe the watermark for trial version
			if (_isTrialVersion)
			{
				EditorUtils.DrawSectionColored("- AVPRO MOVIE CAPTURE -\nTRIAL VERSION", ref _isExpandedTrial, DrawTrialMessage, Color.magenta, Color.magenta, Color.magenta);
			}

			// Button to launch the capture window
			{
				GUI.backgroundColor = new Color(0.96f, 0.25f, 0.47f);
				if (GUILayout.Button("\n◄ Open Movie Capture Window ►\n"))
				{
					CaptureEditorWindow.Init();
				}
				GUI.backgroundColor = Color.white;
			}
		}

		protected void DrawTrialMessage()
		{
			string message = "The free trial version is watermarked.  Upgrade to the full package to remove the watermark.";

			GUI.backgroundColor = Color.yellow;
			EditorGUILayout.BeginVertical(GUI.skin.box);
			//GUI.color = Color.yellow;
			//GUILayout.Label("AVPRO MOVIE CAPTURE - FREE TRIAL VERSION", EditorStyles.boldLabel);
			GUI.color = Color.white;
			GUILayout.Label(message, EditorStyles.wordWrappedLabel);
			if (GUILayout.Button("Upgrade Now"))
			{
				Application.OpenURL(UnityAssetStore_FullVersionUrl);
			}
			EditorGUILayout.EndVertical();
			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;
		}

		protected void GUI_BaseOptions()
		{
			serializedObject.Update();

			if (_baseCapture == null)
			{
				return;
			}

			//DrawDefaultInspector();

			if (!_baseCapture.IsCapturing())
			{
				GUILayout.Space(8f);
				EditorUtils.BoolAsDropdown("Capture Mode", _propIsRealtime, "Realtime Capture", "Offline Render");
				GUILayout.Space(8f);

				if (serializedObject.ApplyModifiedProperties())
				{
					EditorUtility.SetDirty(target);
				}

				GUI_User();

				// After the user mode we must update the serialised object again
				serializedObject.Update();

				EditorUtils.DrawSection("Start / Stop", ref _isExpandedStartStop, GUI_StartStop, ref _startStopSection);
				EditorUtils.DrawSection("Output", ref _isExpandedOutput, GUI_OutputFilePath, ref _outputFilePathSection);
				EditorUtils.DrawSection("Visual", ref _isExpandedVisual, GUI_Visual, ref _visualSection);
				_ap_OutputTarget.target = _propOutputTarget.enumValueIndex == (int)OutputTarget.VideoFile;
                if (EditorGUILayout.BeginFadeGroup(_ap_OutputTarget.faded))
				{
					EditorUtils.DrawSection("Audio", ref _isExpandedAudio, GUI_Audio, ref _audioSection);
					EditorUtils.DrawSection("Post", ref _isExpandedPost, GUI_Post, ref _postSection);
				}
				EditorGUILayout.EndFadeGroup();

				EditorUtils.DrawSection("Misc", ref _isExpandedMisc, GUI_Misc, ref _miscSection);
				//EditorUtils.DrawSection("Platform Specific", ref _isExpandedMisc, GUI_PlatformSpecific);

				EditorUtils.DrawSection("Help", ref _isExpandedAbout, GUI_About, ref _aboutSeciton);

				if (serializedObject.ApplyModifiedProperties())
				{
					EditorUtility.SetDirty(target);
				}

				GUI_Controls();
			}
			else
			{
				GUI_Stats();
				GUI_Progress();
				GUI_Controls();
			}
		}

		protected void GUI_Progress()
		{
			if (_baseCapture == null)
			{
				return;
			}

			if (_propStopMode.enumValueIndex != (int)StopMode.None)
			{
				Rect r = GUILayoutUtility.GetRect(128f, EditorStyles.label.CalcHeight(GUIContent.none, 32f), GUILayout.ExpandWidth(true));
				float progress = _baseCapture.GetProgress();
				EditorGUI.ProgressBar(r, progress, (progress * 100f).ToString("F1") + "%");
			}
		}

		protected void GUI_Stats()
		{
			if (_baseCapture == null)
			{
				return;
			}

			if (Application.isPlaying && _baseCapture.IsCapturing())
			{
				CaptureEditorWindow.DrawBaseCapturingGUI(_baseCapture);

				{
					EditorGUILayout.BeginVertical("box");
					EditorGUI.indentLevel++;
					{
						float lastEncodedSeconds = (float)Mathf.FloorToInt((float)_baseCapture.CaptureStats.NumEncodedFrames / _baseCapture.FrameRate);
						if (_baseCapture.IsRealTime)
						{
							lastEncodedSeconds = _baseCapture.CaptureStats.TotalEncodedSeconds;
						}
						float lastEncodedMinutes = Mathf.FloorToInt(lastEncodedSeconds / 60f);
						lastEncodedSeconds = lastEncodedSeconds % 60;
						uint lastEncodedFrame = _baseCapture.CaptureStats.NumEncodedFrames % (uint)_baseCapture.FrameRate;

						string lengthText = string.Format("{0:00}:{1:00}.{2:000}", lastEncodedMinutes, lastEncodedSeconds, lastEncodedFrame);
						EditorGUILayout.LabelField("Video Length", lengthText);

						if (!_baseCapture.IsRealTime)
						{
							long lastFileSize = _baseCapture.GetCaptureFileSize();
							EditorGUILayout.LabelField("File Size", ((float)lastFileSize / (1024f * 1024f)).ToString("F1") + "MB");
							EditorGUILayout.LabelField("Avg Bitrate", (8f * ((float)lastFileSize / (1024f * 1024f)) / (float)((lastEncodedMinutes * 60) + lastEncodedSeconds)).ToString("F2") + "Mb/s");
						}
					}
					EditorGUI.indentLevel--;
					EditorGUILayout.EndVertical();
				}
			}
		}

		protected void GUI_Controls()
		{
			if (_baseCapture == null)
			{
				return;
			}

			GUILayout.Space(8.0f);

			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			{
				if (!_baseCapture.IsCapturing())
				{
					GUI.backgroundColor = Color.green;
					string startString = "Start Capture";
					if (!_baseCapture.IsRealTime)
					{
						startString = "Start Render";
					}
					if (GUILayout.Button(startString, GUILayout.Height(32f)))
					{
						_baseCapture.SelectVideoCodec();
						_baseCapture.SelectAudioCodec();
						_baseCapture.SelectAudioInputDevice();
						// We have to queue the start capture otherwise Screen.width and height aren't correct
						_baseCapture.QueueStartCapture();
					}
					GUI.backgroundColor = Color.white;
				}
				else
				{
					GUILayout.BeginHorizontal();
					if (!_baseCapture.IsPaused())
					{
						GUI.backgroundColor = Color.yellow;
						if (GUILayout.Button("Pause", GUILayout.Height(32f)))
						{
							_baseCapture.PauseCapture();
						}
					}
					else
					{
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("Resume", GUILayout.Height(32f)))
						{
							_baseCapture.ResumeCapture();
						}
					}
					GUI.backgroundColor = Color.cyan;
					if (GUILayout.Button("Cancel", GUILayout.Height(32f)))
					{
						_baseCapture.CancelCapture();
					}
					GUI.backgroundColor = Color.red;
					if (GUILayout.Button("Stop", GUILayout.Height(32f)))
					{
						_baseCapture.StopCapture();
					}
					GUI.backgroundColor = Color.white;
					GUILayout.EndHorizontal();
				}
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(CaptureBase.LastFileSaved));
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Browse Last"))
			{
				if (!string.IsNullOrEmpty(CaptureBase.LastFileSaved))
				{
					Utils.ShowInExplorer(CaptureBase.LastFileSaved);
				}
			}
			{
				Color prevColor = GUI.color;
				GUI.color = Color.cyan;
				if (GUILayout.Button("View Last Capture"))
				{
					if (!string.IsNullOrEmpty(CaptureBase.LastFileSaved))
					{
						Utils.OpenInDefaultApp(CaptureBase.LastFileSaved);
					}
				}
				GUI.color = prevColor;
			}
			GUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
		}

		protected void GUI_OutputFilePath()
		{
			EditorUtils.EnumAsDropdown("Output Target", _propOutputTarget, EditorUtils.OutputTargetNames);

			_o_OutputTarget_Video.target = _propOutputTarget.enumValueIndex == (int)OutputTarget.VideoFile;
			_o_OutputTarget_Image.target = _propOutputTarget.enumValueIndex == (int)OutputTarget.ImageSequence;
			_o_OutputTarget_Pipe.target = _propOutputTarget.enumValueIndex == (int)OutputTarget.NamedPipe;

			if (EditorGUILayout.BeginFadeGroup(_o_OutputTarget_Video.faded))
			{
				GUILayout.Space(10);
				GUILayout.Label(_guiContentFolder, EditorStyles.boldLabel);
#if UNITY_EDITOR_OSX
				// Photo Library is only for the video output type so grab the current folder type in case we need to reset it
				int outputFolderTypePrevValue = _propOutputFolderType.enumValueIndex;
				if (outputFolderTypePrevValue == (int)CaptureBase.OutputPath.PhotoLibrary)
					// Already the Photo Library type so reset to the default option
					outputFolderTypePrevValue = (int)CaptureBase.DefaultOutputFolderType;
#endif
				EditorGUILayout.PropertyField(_propOutputFolderType, _guiContentFolder);
				
				//Debug.Log($"Current Output Method: {_propOutputFolderType.enumValueIndex}, Wanted Output method: {(int)CaptureBase.OutputPath.Absolute}");
				//RBN Note: because their is an obsolte value that overloads 1 and then becomes 2 Absolute is moved to 3 and the rest are moved up by 1 as
				//          well, thus the + 1
                if (_propOutputFolderType.enumValueIndex == (int)CaptureBase.OutputPath.Absolute + 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(_propOutputFolderPath, _guiContentPath);
					if (GUILayout.Button(">", GUILayout.Width(22)))
					{
						// RBN Note: EndHorizontal / EndFadeGroup dont like to be delay, which this does so we put in delay call so its done after
						//			 everything else, does mean we have to apply the property to make sure it saves tho.
						EditorApplication.delayCall += () =>
						{
							_propOutputFolderPath.stringValue = EditorUtility.SaveFolderPanel("Select Folder To Store Video Captures", System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../")), "");
							serializedObject.ApplyModifiedProperties();
						};
					}
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.PropertyField(_propOutputFolderPath, _guiContentSubfolders);
				}
				GUILayout.Space(10);
				GUILayout.Label("File Name", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(_propFileNamePrefix, _guiContentPrefix);
                EditorGUILayout.PropertyField(_propFileNameComponents, _guiContentFilenameComponents, GUILayout.Width(100f), GUILayout.MaxWidth(100f), GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();

				_o_FilenameComponent.target = _propFileNameComponents.enumValueFlag != 0;
                if (EditorGUILayout.BeginFadeGroup(_o_FilenameComponent.faded))
				{
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_propFilenameComponentSeperator);
                    EditorGUI.indentLevel--;
                }
				EditorGUILayout.EndFadeGroup();

                _o_CustomFilenameComponent.target = (_propFileNameComponents.enumValueFlag & (int)FilenameComponents.CustomText) != 0;
                if (EditorGUILayout.BeginFadeGroup(_o_CustomFilenameComponent.faded))
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(_propFilenameComponentCustomText, _guiContentFilenameComponentCustomString);
                    EditorGUI.indentLevel--;
                }
				EditorGUILayout.EndFadeGroup();

				//EditorGUILayout.PropertyField(_propAppendFilenameTimestamp, _guiContentAppendTimestamp);
				EditorGUILayout.PropertyField(_propAllowManualFileExtension, _guiContentManualExtension);

				_o_ManualExtension.target = _propAllowManualFileExtension.boolValue;
                if (EditorGUILayout.BeginFadeGroup(_o_ManualExtension.faded))
				{
					EditorGUILayout.PropertyField(_propFileNameExtension, _guiContentExtension);
				}
				EditorGUILayout.EndFadeGroup();

				GUILayout.Space(10);
				GUILayout.Label("Current Path:");
                EditorGUILayout.LabelField(_baseCapture.GenerateFilePathPreview(), new GUIStyle(EditorStyles.label) { wordWrap = true });
			}
			EditorGUILayout.EndFadeGroup();
			if (EditorGUILayout.BeginFadeGroup(_o_OutputTarget_Image.faded))
			{
				BeginPlatformSelection();
				if (_selectedPlatform == NativePlugin.Platform.Windows)
				{
					EditorUtils.EnumAsDropdown("Format", _propImageSequenceFormatWindows, Utils.WindowsImageSequenceFormatNames);
				}
				else if (_selectedPlatform == NativePlugin.Platform.macOS)
				{
					EditorUtils.EnumAsDropdown("Format", _propImageSequenceFormatMacOS, Utils.MacOSImageSequenceFormatNames);
				}
				else if (_selectedPlatform == NativePlugin.Platform.iOS)
				{
					EditorUtils.EnumAsDropdown("Format", _propImageSequenceFormatIOS, Utils.IOSImageSequenceFormatNames);
				}
				else if (_selectedPlatform == NativePlugin.Platform.Android)
				{
					EditorUtils.EnumAsDropdown("Format", _propImageSequenceFormatAndroid, Utils.AndroidImageSequenceFormatNames);
				}
				EndPlatformSelection();
				GUILayout.Space(8f);
				GUILayout.Label(_guiContentFolder, EditorStyles.boldLabel);
#if UNITY_EDITOR_OSX
				// Photo Library is only for the video output type so grab the current folder type in case we need to reset it
				int outputFolderTypePrevValue = _propOutputFolderType.enumValueIndex;
				if (outputFolderTypePrevValue == (int)CaptureBase.OutputPath.PhotoLibrary)
					// Already the Photo Library type so reset to the default option
					outputFolderTypePrevValue = (int)CaptureBase.DefaultOutputFolderType;
#endif
				EditorGUILayout.PropertyField(_propOutputFolderType, _guiContentFolder);

#if UNITY_EDITOR_OSX
				bool isImageSequence = _propOutputTarget.enumValueIndex == (int)OutputTarget.ImageSequence;
				if (isImageSequence && _propOutputFolderType.enumValueIndex == (int)CaptureBase.OutputPath.PhotoLibrary)
				{
					Debug.LogWarning("Photo Library is unavailable for the Image Sequence output type");
					_propOutputFolderType.enumValueIndex = (int)outputFolderTypePrevValue;
				}
#endif
				// RBN Note: SEE ln 493
				if (_propOutputFolderType.enumValueIndex == (int)CaptureBase.OutputPath.Absolute + 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(_propOutputFolderPath, _guiContentPath);
					if (GUILayout.Button(">", GUILayout.Width(22)))
					{
                        // RBN Note: SEE ln 501
                        EditorApplication.delayCall += () =>
                        {
                            _propOutputFolderPath.stringValue = EditorUtility.SaveFolderPanel("Select Folder To Store Video Captures", System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../")), "");
                            serializedObject.ApplyModifiedProperties();
                        };
                    }
					EditorGUILayout.EndHorizontal();
				}
				else
				{
					EditorGUILayout.PropertyField(_propOutputFolderPath, _guiContentSubfolders);
				}

				GUILayout.Label("File Name", EditorStyles.boldLabel);
				
				GUILayout.BeginHorizontal();
				
				EditorGUILayout.PropertyField(_propFileNamePrefix, _guiContentPrefix);
                EditorGUILayout.PropertyField(_propFileNameComponents, label: new GUIContent(""), GUILayout.Width(100f), GUILayout.MaxWidth(100f), GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();

                _o_FilenameComponent.target = _propFileNameComponents.enumValueFlag != 0;
                if (EditorGUILayout.BeginFadeGroup(_o_FilenameComponent.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_propFilenameComponentSeperator);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();

                _o_CustomFilenameComponent.target = (_propFileNameComponents.enumValueFlag & (int)FilenameComponents.CustomText) != 0;
                if (EditorGUILayout.BeginFadeGroup(_o_CustomFilenameComponent.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_propFilenameComponentCustomText, _guiContentFilenameComponentCustomString);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUILayout.PropertyField(_propImageSequenceStartFrame, _guiContentStartFrame);
				EditorGUILayout.PropertyField(_propImageSequenceZeroDigits, _guiContentZeroDigits);

                GUILayout.Space(10);
                GUILayout.Label("Current Path:");
                EditorGUILayout.LabelField(_baseCapture.GenerateFilePathPreview(), new GUIStyle(EditorStyles.label) { wordWrap = true });
            }
            EditorGUILayout.EndFadeGroup();

			if (EditorGUILayout.BeginFadeGroup(_o_OutputTarget_Pipe.faded))
			{
				EditorGUILayout.PropertyField(_propNamedPipePath, _guiContentPipePath);

                GUILayout.Space(10);
            }
			EditorGUILayout.EndFadeGroup();
		}

		protected void GUI_StartStop()
		{
			EditorGUILayout.PropertyField(_propCaptureKey, _guiContentToggleKey);

			EditorGUILayout.Separator();

			EditorGUILayout.PropertyField(_propStartTrigger, _guiContentStartMode);

			EditorGUILayout.PropertyField(_propStartDelay, _guiContentStartDelay);

			_ss_StartDelay.target = (StartDelayMode)_propStartDelay.enumValueIndex == StartDelayMode.RealSeconds ||
				(StartDelayMode)_propStartDelay.enumValueIndex == StartDelayMode.GameSeconds;
			if (EditorGUILayout.BeginFadeGroup(_ss_StartDelay.faded))
			{
				EditorGUILayout.PropertyField(_propStartDelaySeconds, _guiContentSeconds);
			}
			EditorGUILayout.EndFadeGroup();

			EditorGUILayout.Separator();


			EditorGUILayout.PropertyField(_propStopMode, _guiContentStopMode);

			_ss_StopMode_FramesEncoded.target = (StopMode)_propStopMode.enumValueIndex == StopMode.FramesEncoded;
			_ss_StopMode_Other.target = (StopMode)_propStopMode.enumValueIndex == StopMode.SecondsElapsed || (StopMode)_propStopMode.enumValueIndex == StopMode.SecondsEncoded;

			if (EditorGUILayout.BeginFadeGroup(_ss_StopMode_FramesEncoded.faded))
			{
				EditorGUILayout.PropertyField(_propStopFrames, _guiContentFrames);
			}
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_ss_StopMode_Other.faded))
			{
				EditorGUILayout.PropertyField(_propStopSeconds, _guiContentSeconds);
			}
			EditorGUILayout.EndFadeGroup();

			//EditorGUILayout.PropertyField(_propPauseCaptureOnAppPause);
		}

        private void BeginPlatformSelection(string title = null)
		{
			GUILayout.BeginVertical(_stylePlatformBox);
			if (!string.IsNullOrEmpty(title))
			{
				GUILayout.Label(title, EditorStyles.boldLabel);
			}
			int rowCount = 0;
			int platformIndex = (int)_selectedPlatform;
			for (int i = 0; i < NativePlugin.PlatformNames.Length; i++)
			{
				if (i % 3 == 0)
				{
					GUILayout.BeginHorizontal();
					rowCount++;
				}

				Color hilight = Color.yellow;

				if (i == platformIndex)
				{
				}
				else
				{
					// Unselected, unmodified
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = Color.grey;
						GUI.color = new Color(0.65f, 0.66f, 0.65f);// Color.grey;
					}
				}

				if (i == platformIndex)
				{
					if (!GUILayout.Toggle(true, NativePlugin.PlatformNames[i], GUI.skin.button))
					{
						platformIndex = -1;
					}
				}
				else
				{
					if (GUILayout.Button(NativePlugin.PlatformNames[i]))
					{
						platformIndex = i;
					}
				}
				if ((i+1) % 3 == 0)
				{
					rowCount--;
					GUILayout.EndHorizontal();
				}
				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.white;
				GUI.color = Color.white;
			}

			if (rowCount > 0)
			{
				GUILayout.EndHorizontal();
			}

			if (platformIndex != (int)_selectedPlatform)
			{
				_selectedPlatform = (NativePlugin.Platform)platformIndex;

				// We do this to clear the focus, otherwise a focused text field will not change when the Toolbar index changes
				EditorGUI.FocusTextInControl("ClearFocus");
			}
		}

		private void EndPlatformSelection()
		{
			GUILayout.EndVertical();
		}

		protected virtual void GUI_Misc()
		{
			EditorGUILayout.PropertyField(_propLogCaptureStartStop);
			EditorGUILayout.PropertyField(_propAllowVsyncDisable);
			EditorGUILayout.PropertyField(_propWaitForEndOfFrame);
			EditorGUILayout.PropertyField(_propSupportTextureRecreate, _guiContentSupportTextureRecreate);
			EditorGUILayout.PropertyField(_propPersistAcrossSceneLoads);
			#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
			EditorGUILayout.PropertyField(_propTimelineController);
			#endif
			#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
			EditorGUILayout.PropertyField(_propVideoPlayerController);
			#endif

			BeginPlatformSelection();

			_m_SelectedPlatform_Windows.target = _selectedPlatform == NativePlugin.Platform.Windows;
			_m_SelectedPlatform_Android.target = _selectedPlatform == NativePlugin.Platform.Android;
			_m_SelectedPlatform_macOS.target = _selectedPlatform == NativePlugin.Platform.macOS || _selectedPlatform == NativePlugin.Platform.iOS;
			_m_SelectedPlatform_iOS.target = _selectedPlatform == NativePlugin.Platform.iOS;

            if (EditorGUILayout.BeginFadeGroup(_m_SelectedPlatform_Windows.faded))
			{
				EditorGUILayout.PropertyField(_propForceGpuFlush);
				EditorGUILayout.PropertyField(_propMinimumDiskSpaceMB);
			}
			EditorGUILayout.EndFadeGroup();
			
			if (EditorGUILayout.BeginFadeGroup(_m_SelectedPlatform_Android.faded))
			{
				EditorGUILayout.PropertyField(_propAndroidUpdateMediaGallery, _guiAndroidUpdateMediaGallery);
				EditorGUILayout.PropertyField(_propAndroidNoCaptureRotation, _guiAndroidNoCaptureRotation);
				EditorGUILayout.PropertyField(_propWriteOrientationMetadata, _guiContentWriteOrientationMetadata);
			}
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(_m_SelectedPlatform_macOS.faded))
			{
				EditorGUILayout.PropertyField(_propMinimumDiskSpaceMB);
			}
            EditorGUILayout.EndFadeGroup();

            if (EditorGUILayout.BeginFadeGroup(_m_SelectedPlatform_iOS.faded))
			{
				EditorGUILayout.PropertyField(_propWriteOrientationMetadata, _guiContentWriteOrientationMetadata);
				EditorGUILayout.PropertyField(_prop_iOSSaveCaptureWhenAppLosesFocus, _gui_iOSSaveCaptureWhenAppLosesFocus);
			}
            EditorGUILayout.EndFadeGroup();

            EndPlatformSelection();
		}

		protected virtual void GUI_About()
		{
			CaptureEditorWindow.DrawConfigGUI_About();
		}

		protected void GUI_Visual()
		{
			EditorGUILayout.PropertyField(_propDownScale);
			_v_DownScale.target = _propDownScale.enumValueIndex == 5;

            if (EditorGUILayout.BeginFadeGroup(_v_DownScale.faded))		// 5 is DownScale.Custom
			{
				EditorGUILayout.PropertyField(_propMaxVideoSize, new GUIContent("Size"));
				_propMaxVideoSize.vector2Value = new Vector2(Mathf.Clamp((int)_propMaxVideoSize.vector2Value.x, 1, NativePlugin.MaxRenderWidth), Mathf.Clamp((int)_propMaxVideoSize.vector2Value.y, 1, NativePlugin.MaxRenderHeight));
			}
			EditorGUILayout.EndFadeGroup();
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(_propFrameRate, GUILayout.ExpandWidth(false));
			_propFrameRate.floatValue = Mathf.Clamp(_propFrameRate.floatValue, 0.01f, 240f);
			EditorUtils.FloatAsPopup("▶", "Common Frame Rates", this.serializedObject, _propFrameRate, EditorUtils.CommonFrameRateNames, EditorUtils.CommonFrameRateValues);
			GUILayout.EndHorizontal();

			EditorGUI.BeginDisabledGroup(!_propIsRealtime.boolValue);
			EditorGUILayout.PropertyField(_propTimelapseScale);
			_propTimelapseScale.intValue = Mathf.Max(1, _propTimelapseScale.intValue);
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.PropertyField(_propFrameUpdateMode);

			EditorGUILayout.PropertyField(_propFlipVertically);

			EditorGUILayout.Space();

			_v_OutputTarget_Video.target = _propOutputTarget.enumValueIndex == (int)OutputTarget.VideoFile;
			_v_OutputTarget_Image.target = _propOutputTarget.enumValueIndex == (int)OutputTarget.ImageSequence;


            if (EditorGUILayout.BeginFadeGroup(_v_OutputTarget_Video.faded))
			{
				GUI_VisualCodecs();
				GUI_VideoHints();
			}
			EditorGUILayout.EndFadeGroup();
			if (EditorGUILayout.BeginFadeGroup(_v_OutputTarget_Image.faded))
            {
				GUI_ImageHints();
			}
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(_propIsRealtime.boolValue);
			GUILayout.Label("Motion Blur", EditorStyles.boldLabel);
			if (_propIsRealtime.boolValue)
			{
				GUI.color = Color.yellow;
				GUILayout.TextArea("Motion Blur only available in Offline Render mode");
				GUI.color = Color.white;
			}
			else
			{
				GUI_MotionBlur();
			}
			EditorGUI.EndDisabledGroup();
		}

		protected void GUI_VisualCodecs_Windows()
		{
			bool searchByName = (_propForceVideoCodecIndexWindows.intValue < 0);
			bool newSearchByName = EditorGUILayout.Toggle("Search by name", searchByName);
			if (searchByName != newSearchByName)
			{
				if (newSearchByName)
				{
					_propForceVideoCodecIndexWindows.intValue = -1;
				}
				else
				{
					_propForceVideoCodecIndexWindows.intValue = 0;
				}
			}

			if (_propForceVideoCodecIndexWindows.intValue < 0)
			{
				EditorGUILayout.PropertyField(_propVideoCodecPriorityWindows, _guiContentCodecSearchOrder, true);
			}
			else
			{
				EditorGUILayout.PropertyField(_propForceVideoCodecIndexWindows);
			}
		}

		protected void GUI_VisualCodecs_Android()
		{
#if UNITY_2022_1_OR_NEWER
			if (this is CaptureFromScreenEditor)
			{
				var graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
				if (graphicsAPIs[0] == UnityEngine.Rendering.GraphicsDeviceType.Vulkan && PlayerSettings.vulkanEnablePreTransform)
				{
					GUI.color = Color.red;
					GUILayout.TextArea("CaptureFromScreen will not generate correct output when capturing in the non-default orienation when using the Vulkan API and PlayerSettings.vulkanEnablePreTransform is enabled.");
					GUI.color = Color.white;
				}
			}
#endif
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Toggle("Search by name", false);
			EditorGUI.EndDisabledGroup();
			_propForceVideoCodecIndexAndroid.intValue = EditorGUILayout.Popup(_propForceVideoCodecIndexAndroid.intValue, NativePlugin.VideoCodecNamesAndroid);
		}

		protected void GUI_VisualCodecs_MacOS()
		{
			bool searchByName = (_propForceVideoCodecIndexMacOS.intValue < 0);
			bool newSearchByName = EditorGUILayout.Toggle("Search by name", searchByName);
			if (searchByName != newSearchByName)
			{
				if (newSearchByName)
				{
					_propForceVideoCodecIndexMacOS.intValue = -1;
				}
				else
				{
					_propForceVideoCodecIndexMacOS.intValue = 0;
				}
			}

			if (_propForceVideoCodecIndexMacOS.intValue < 0)
			{
				EditorGUILayout.PropertyField(_propVideoCodecPriorityMacOS, _guiContentCodecSearchOrder, true);
			}
			else
			{
				_propForceVideoCodecIndexMacOS.intValue = EditorGUILayout.Popup(_propForceVideoCodecIndexMacOS.intValue, NativePlugin.VideoCodecNamesMacOS);
			}
		}

		protected void GUI_VisualCodecs_IOS()
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.Toggle("Search by name", false);
			EditorGUI.EndDisabledGroup();
			_propForceVideoCodecIndexIOS.intValue = EditorGUILayout.Popup(_propForceVideoCodecIndexIOS.intValue, NativePlugin.VideoCodecNamesIOS);
		}

		protected void GUI_VisualCodecs()
		{
			BeginPlatformSelection("Video Codec");
            _vce_SelectedPlatform_Windows.target = _selectedPlatform == NativePlugin.Platform.Windows;
            _vce_SelectedPlatform_macOS.target	= _selectedPlatform == NativePlugin.Platform.macOS;
            _vce_SelectedPlatform_iOS.target		= _selectedPlatform == NativePlugin.Platform.iOS;
			_vce_SelectedPlatform_Android.target = _selectedPlatform == NativePlugin.Platform.Android;

            if (EditorGUILayout.BeginFadeGroup(_vce_SelectedPlatform_Windows.faded))
			{
				GUI_VisualCodecs_Windows();
			}
			EditorGUILayout.EndFadeGroup();
			if (EditorGUILayout.BeginFadeGroup(_vce_SelectedPlatform_macOS.faded))
			{
				GUI_VisualCodecs_MacOS();
			}
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_vce_SelectedPlatform_iOS.faded))
			{
				GUI_VisualCodecs_IOS();
			}
            EditorGUILayout.EndFadeGroup();
            if (EditorGUILayout.BeginFadeGroup(_vce_SelectedPlatform_Android.faded))
			{
				GUI_VisualCodecs_Android();
			}
            EditorGUILayout.EndFadeGroup();
            EndPlatformSelection();
		}

		protected void GUI_AudioCodecs()
		{
			BeginPlatformSelection("Audio Codec");
			if (_selectedPlatform == NativePlugin.Platform.Windows)
			{
				bool searchByName = (_propForceAudioCodecIndexWindows.intValue < 0);
				bool newSearchByName = EditorGUILayout.Toggle("Search by name", searchByName);
				if (searchByName != newSearchByName)
				{
					if (newSearchByName)
					{
						_propForceAudioCodecIndexWindows.intValue = -1;
					}
					else
					{
						_propForceAudioCodecIndexWindows.intValue = 0;
					}
				}

				if (_propForceAudioCodecIndexWindows.intValue < 0)
				{
					EditorGUILayout.PropertyField(_propAudioCodecPriorityWindows, _guiContentCodecSearchOrder, true);
				}
				else
				{
					EditorGUILayout.PropertyField(_propForceAudioCodecIndexWindows);
				}
			}
			else if (_selectedPlatform == NativePlugin.Platform.Android)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Toggle("Search by name", false);
				EditorGUI.EndDisabledGroup();
				_propForceAudioCodecIndexAndroid.intValue = EditorGUILayout.Popup(_propForceAudioCodecIndexAndroid.intValue, NativePlugin.AudioCodecNamesAndroid);
			}
			else if (_selectedPlatform == NativePlugin.Platform.macOS)
			{
				bool searchByName = (_propForceAudioCodecIndexMacOS.intValue < 0);
				bool newSearchByName = EditorGUILayout.Toggle("Search by name", searchByName);
				if (searchByName != newSearchByName)
				{
					if (newSearchByName)
					{
						_propForceAudioCodecIndexMacOS.intValue = -1;
					}
					else
					{
						_propForceAudioCodecIndexMacOS.intValue = 0;
					}
				}

				if (_propForceAudioCodecIndexMacOS.intValue < 0)
				{
					EditorGUILayout.PropertyField(_propAudioCodecPriorityMacOS, _guiContentCodecSearchOrder, true);
				}
				else
				{
					_propForceAudioCodecIndexMacOS.intValue = EditorGUILayout.Popup(_propForceAudioCodecIndexMacOS.intValue, NativePlugin.AudioCodecNamesMacOS);
				}
			}
			else if (_selectedPlatform == NativePlugin.Platform.iOS)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.Toggle("Search by name", false);
				EditorGUI.EndDisabledGroup();
				_propForceAudioCodecIndexIOS.intValue = EditorGUILayout.Popup(_propForceAudioCodecIndexIOS.intValue, NativePlugin.AudioCodecNamesIOS);
			}
			EndPlatformSelection();
		}

		protected void GUI_Audio()
		{
			EditorUtils.EnumAsDropdown("Audio Source", _propAudioCaptureSource, EditorUtils.AudioCaptureSourceNames);

			_a_CaptureSourceNone.target = _propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.None;
            
            if (EditorGUILayout.BeginFadeGroup(_a_CaptureSourceNone.faded))
			{
				bool showAudioOptions = true;
#if AVPRO_MOVIECAPTURE_OFFLINE_AUDIOCAPTURE
				_a_OfflineInvalidOption.target = !_propIsRealtime.boolValue &&
					_propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.Manual &&
					_propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.Unity &&
					_propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.Wwise;

                if (EditorGUILayout.BeginFadeGroup(_a_OfflineInvalidOption.faded))
				{
					GUI.color = Color.yellow;
					GUILayout.TextArea("Only Manual, Unity and Wwise Audio Sources are available in offline capture mode");
					GUI.color = Color.white;
					showAudioOptions = false;
				}
				EditorGUILayout.EndFadeGroup();
#else
				_a_OfflineInvalidOption.target = !_propIsRealtime.boolValue &&
									_propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.Manual &&
									_propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.Unity &&
									_propAudioCaptureSource.enumValueIndex != (int)AudioCaptureSource.Wwise;

					if (EditorGUILayout.BeginFadeGroup(_a_OfflineInvalidOption.faded))
					{
						GUI.color = Color.yellow;
						GUILayout.TextArea("Only Manual and Wwise Audio Source is available in offline capture mode");
						GUI.color = Color.white;
						showAudioOptions = false;
					}
					EditorGUILayout.EndFadeGroup();
#endif
				_a_RealtimeWwise.target = _propIsRealtime.boolValue && _propAudioCaptureSource.enumValueIndex == (int)AudioCaptureSource.Wwise;
                if (EditorGUILayout.BeginFadeGroup(_a_RealtimeWwise.faded))
				{
					GUI.color = Color.yellow;
					GUILayout.TextArea("Wwise Audio Source is not available in realtime capture mode");
					GUI.color = Color.white;
					showAudioOptions = false;
				}
				EditorGUILayout.EndFadeGroup();
#if !AVPRO_MOVIECAPTURE_WWISE_SUPPORT
				_a_OfflineWwise.target = !_propIsRealtime.boolValue && _propAudioCaptureSource.enumValueIndex == (int)AudioCaptureSource.Wwise;
                if (EditorGUILayout.BeginFadeGroup(_a_OfflineWwise.faded))
				{
					GUI.color = Color.red;
					GUILayout.TextArea("To support Wwise audio capture: add AVPRO_MOVIECAPTURE_WWISE_SUPPORT to script defines in Player Settings");
					GUI.color = Color.white;
					showAudioOptions = false;
				}
				EditorGUILayout.EndFadeGroup();
#endif
				_a_ShowAudioOptions.target = showAudioOptions;
                if (EditorGUILayout.BeginFadeGroup(_a_ShowAudioOptions.faded))
				{
					_a_CaptureSourceMicrophone.target = _propAudioCaptureSource.enumValueIndex == (int)AudioCaptureSource.Microphone;
					_a_CaptureSourceUnityWwise.target = _propAudioCaptureSource.enumValueIndex == (int)AudioCaptureSource.Unity ||
						_propAudioCaptureSource.enumValueIndex == (int)AudioCaptureSource.Wwise;
					_a_CaptureSourceManual.target = _propAudioCaptureSource.enumValueIndex == (int)AudioCaptureSource.Manual;


                    if (EditorGUILayout.BeginFadeGroup(_a_CaptureSourceMicrophone.faded))
					{
						// TODO: change this into platform specific........
						// TODO: add search by name support................
						EditorGUILayout.PropertyField(_propForceAudioDeviceIndex);
					}
					EditorGUILayout.EndFadeGroup();
					if (EditorGUILayout.BeginFadeGroup(_a_CaptureSourceUnityWwise.faded))
					{
						EditorGUILayout.PropertyField(_propUnityAudioCapture);
					}
                    EditorGUILayout.EndFadeGroup();
                    if (EditorGUILayout.BeginFadeGroup(_a_CaptureSourceManual.faded))
					{
						EditorUtils.IntAsDropdown("Sample Rate", _propManualAudioSampleRate, EditorUtils.CommonAudioSampleRateNames, EditorUtils.CommonAudioSampleRateValues);
						EditorGUILayout.PropertyField(_propManualAudioChannelCount, new GUIContent("Channels"));
					}
                    EditorGUILayout.EndFadeGroup();

                    EditorGUILayout.Space();
					GUI_AudioCodecs();
					EditorGUILayout.Space();
				}
				EditorGUILayout.EndFadeGroup();
			}
			EditorGUILayout.EndFadeGroup();
			EditorGUI.EndDisabledGroup();
		}

		protected void GUI_VideoHints()
		{
			BeginPlatformSelection("Encoder Hints");
			if (_selectedPlatform >= NativePlugin.Platform.First && _selectedPlatform < NativePlugin.Platform.Count)
			{
				PropVideoHints props = _propVideoHints[(int)_selectedPlatform];
				EditorUtils.BitrateField("Average Bitrate", props.propAverageBitrate);
				EditorGUI.BeginDisabledGroup(_selectedPlatform != NativePlugin.Platform.Windows);
				EditorUtils.BitrateField("Maxiumum Bitrate", props.propMaximumBitrate);
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.PropertyField(props.propQuality);
				EditorGUILayout.PropertyField(props.propKeyframeInterval);
				EditorGUILayout.PropertyField(props.propTransparency);

				// Choose between Limited and Full colour ranges, currently Android only				
				if (_selectedPlatform == NativePlugin.Platform.Android)
				{
					EditorGUILayout.PropertyField(props.propColourRange);
				}
				else
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.PropertyField(props.propColourRange);
					EditorGUI.EndDisabledGroup();
				}

				if (_selectedPlatform == NativePlugin.Platform.Windows)
				{
					EditorGUILayout.PropertyField(props.propHardwareEncoding);
				}
				else
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.Toggle("Use Hardware Encoding", true);
					EditorGUI.EndDisabledGroup();
				}

				_vce_SelectedPlatform_macOS.target = _selectedPlatform == NativePlugin.Platform.macOS;
				_vce_SelectedPlatform_iOS.target = _selectedPlatform == NativePlugin.Platform.iOS;
				if (EditorGUILayout.BeginFadeGroup(_vce_SelectedPlatform_macOS.faded))
				{
                    EditorGUILayout.PropertyField(props.propEnableConstantQuality);
                    EditorGUILayout.PropertyField(props.propEnableFragmentedWriting);
                    _ve_EnableFragmentedWriting.target = props.propEnableFragmentedWriting.boolValue;
                    if (EditorGUILayout.BeginFadeGroup(_ve_EnableFragmentedWriting.faded))
                    {
                        EditorGUILayout.PropertyField(props.propMovieFragmentInterval);
                    }
                    EditorGUILayout.EndFadeGroup();
                    EditorGUILayout.PropertyField(props.propFramePTSMode, _guiContentFramePTSMode);
                }
				EditorGUILayout.EndFadeGroup();
                if (EditorGUILayout.BeginFadeGroup(_vce_SelectedPlatform_iOS.faded))
                {
                    EditorGUILayout.PropertyField(props.propEnableFragmentedWriting);
					_ve_EnableFragmentedWriting.target = props.propEnableFragmentedWriting.boolValue;
                    if (EditorGUILayout.BeginFadeGroup(_ve_EnableFragmentedWriting.faded))
                    {
                        EditorGUILayout.PropertyField(props.propMovieFragmentInterval);
                    }
					EditorGUILayout.EndFadeGroup();
                    EditorGUILayout.PropertyField(props.propFramePTSMode, _guiContentFramePTSMode);
                }
                EditorGUILayout.EndFadeGroup();
			}
			EndPlatformSelection();
		}

		protected void GUI_ImageHints()
		{
			BeginPlatformSelection("Encoder Hints");
			if (_selectedPlatform >= NativePlugin.Platform.First && _selectedPlatform < NativePlugin.Platform.Count)
			{
				PropImageHints props = _propImageHints[(int)_selectedPlatform];
				if (_selectedPlatform != NativePlugin.Platform.Windows)
				{
					EditorGUILayout.PropertyField(props.propQuality);
                    EditorGUILayout.PropertyField(props.propTransparency);
                }
                else // WIN
				{
					// custom UI for windows Encoder Hints
					if (_propImageSequenceFormatWindows.enumValueIndex == 0) // PNG 
					{
						// looks nicer with this but want to turn it into a toggle
						//EditorGUILayout.PropertyField(props.propQuality);
                        EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Compressed");
						bool val = _baseCapture.GetEncoderHints().imageHints.quality >= 1 ? true : false;
                        val = EditorGUILayout.ToggleLeft("", val);
                        _baseCapture.GetEncoderHints().imageHints.quality = val ? 1 : 0;
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.PropertyField(props.propTransparency);
                    }
                    else // JPEG
                    {
						if (_baseCapture.GetEncoderHints().imageHints.quality > 1)
							_baseCapture.GetEncoderHints().imageHints.quality = 1;
						EditorGUILayout.PropertyField(props.propQuality);
                    }
                }
			}
			EndPlatformSelection();
		}

		protected void GUI_PlatformSpecific()
		{
			BeginPlatformSelection();
			if (_selectedPlatform >= NativePlugin.Platform.First && _selectedPlatform < NativePlugin.Platform.Count)
			{
				GUILayout.Label("Video Codecs", EditorStyles.boldLabel);

				if (_selectedPlatform == NativePlugin.Platform.Windows)
				{
					GUI_VisualCodecs_Windows();
				}
				else if (_selectedPlatform == NativePlugin.Platform.macOS)
				{
					GUI_VisualCodecs_MacOS();
				}
				else if (_selectedPlatform == NativePlugin.Platform.iOS)
				{
					GUI_VisualCodecs_IOS();
				}
				else if (_selectedPlatform == NativePlugin.Platform.Android)
				{
					GUI_VisualCodecs_Android();
				}

				GUILayout.Label("Encoder Hints", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_propVideoHints[(int)_selectedPlatform].propAverageBitrate);
				if (_selectedPlatform == NativePlugin.Platform.Windows)
				{
					EditorGUILayout.PropertyField(_propVideoHints[(int)_selectedPlatform].propMaximumBitrate);
				}
				EditorGUILayout.PropertyField(_propVideoHints[(int)_selectedPlatform].propQuality);
				EditorGUILayout.PropertyField(_propVideoHints[(int)_selectedPlatform].propKeyframeInterval);
				EditorGUILayout.PropertyField(_propVideoHints[(int)_selectedPlatform].propTransparency);
				if (_selectedPlatform == NativePlugin.Platform.Windows)
				{
					EditorGUILayout.PropertyField(_propVideoHints[(int)_selectedPlatform].propHardwareEncoding);
				}
				else
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.Toggle("Use Hardware Encoding", true);
					EditorGUI.EndDisabledGroup();
				}
			}
			EndPlatformSelection();
		}

		protected void GUI_Post()
		{
			BeginPlatformSelection();
			if (_selectedPlatform >= NativePlugin.Platform.First && _selectedPlatform < NativePlugin.Platform.Count)
			{
				PropVideoHints propHints = _propVideoHints[(int)_selectedPlatform];
				EditorGUILayout.PropertyField(propHints.propFastStart, _guiStreamableMP4);
				EditorGUILayout.PropertyField(propHints.propInjectStereoPacking, _guiStereoPacking);
				_p_StereoPackingCustom.target = propHints.propInjectStereoPacking.enumValueIndex == (int)NoneAutoCustom.Custom;
                if (EditorGUILayout.BeginFadeGroup(_p_StereoPackingCustom.faded))
				{
					EditorGUILayout.PropertyField(propHints.propStereoPacking, _guiBlankSpace);
				}
				EditorGUILayout.EndFadeGroup();
				EditorGUILayout.PropertyField(propHints.propInjectSphericalVideoLayout, _guiSphericalLayout);
				_p_SphericalLayoutCustom.target = propHints.propInjectSphericalVideoLayout.enumValueIndex == (int)NoneAutoCustom.Custom;
                if (EditorGUILayout.BeginFadeGroup(_p_SphericalLayoutCustom.faded))
				{
					EditorGUILayout.PropertyField(propHints.propSphericalVideoLayout, _guiBlankSpace);
				} 
				EditorGUILayout.EndFadeGroup();
			}
			EndPlatformSelection();
		}

		protected void GUI_MotionBlur()
		{
			EditorGUILayout.PropertyField(_propUseMotionBlur);
			_vmb_MotionBlur.target = _propUseMotionBlur.boolValue;

            if (EditorGUILayout.BeginFadeGroup(_vmb_MotionBlur.faded))
			{
				EditorGUILayout.PropertyField(_propMotionBlurSamples, _guiContentMotionBlurSamples);
				EditorGUILayout.PropertyField(_propMotionBlurCameras, _guiContentMotionBlurCameras, true);
			}
			EditorGUILayout.EndFadeGroup();

        }

		private void LoadSettings()
		{
			_isExpandedStartStop = EditorPrefs.GetBool(SettingsPrefix + "ExpandStartStop", _isExpandedStartStop);
			_isExpandedOutput = EditorPrefs.GetBool(SettingsPrefix + "ExpandOutput", _isExpandedOutput);
			_isExpandedVisual = EditorPrefs.GetBool(SettingsPrefix + "ExpandVisual", _isExpandedVisual);
			_isExpandedAudio = EditorPrefs.GetBool(SettingsPrefix + "ExpandAudio", _isExpandedAudio);
			_isExpandedPost = EditorPrefs.GetBool(SettingsPrefix + "ExpandPost", _isExpandedPost);
			_isExpandedMisc = EditorPrefs.GetBool(SettingsPrefix + "ExpandMisc", _isExpandedMisc);
			_selectedPlatform = (NativePlugin.Platform)EditorPrefs.GetInt(SettingsPrefix + "SelectedPlatform", (int)_selectedPlatform);

            // AnimBools
            _aboutSeciton.value = EditorPrefs.GetBool("_aboutSeciton", false);
            _startStopSection.value = EditorPrefs.GetBool("_startStopSection", false);
            _outputFilePathSection.value = EditorPrefs.GetBool("_outputFilePathSection", false);
            _visualSection.value = EditorPrefs.GetBool("_visualSection", false);
            _audioSection.value = EditorPrefs.GetBool("_audioSection", false);
            _postSection.value = EditorPrefs.GetBool("_postSection", false);
            _miscSection.value = EditorPrefs.GetBool("_miscSection", false);
            _ss_StartDelay.value = EditorPrefs.GetBool("_ss_StartDelay", false);
            _ss_StopMode_FramesEncoded.value = EditorPrefs.GetBool("_ss_StopMode_FramesEncoded", false);
            _ss_StopMode_Other.value = EditorPrefs.GetBool("_ss_StopMode_Other", false);
            _o_OutputTarget_Video.value = EditorPrefs.GetBool("_o_OutputTarget_Video", false);
            _o_OutputTarget_Image.value = EditorPrefs.GetBool("_o_OutputTarget_Image", false);
            _o_OutputTarget_Pipe.value = EditorPrefs.GetBool("_o_OutputTarget_Pipe", false);
            _o_ManualExtension.value = EditorPrefs.GetBool("_o_ManualExtension", false);
            _o_CustomFilenameComponent.value = EditorPrefs.GetBool("_o_CustomFilenameComponent", false);
            _o_FilenameComponent.value = EditorPrefs.GetBool("_o_FilenameComponent", false);
            _v_DownScale.value = EditorPrefs.GetBool("_v_DownScale", false);
            _v_OutputTarget_Video.value = EditorPrefs.GetBool("_v_OutputTarget_Video", false);
            _v_OutputTarget_Image.value = EditorPrefs.GetBool("_v_OutputTarget_Image", false);
            _vce_SelectedPlatform_Windows.value = EditorPrefs.GetBool("_vce_SelectedPlatform_Windows", false);
            _vce_SelectedPlatform_macOS.value = EditorPrefs.GetBool("_vce_SelectedPlatform_macOS", false);
            _vce_SelectedPlatform_iOS.value = EditorPrefs.GetBool("_vce_SelectedPlatform_iOS", false);
            _vce_SelectedPlatform_Android.value = EditorPrefs.GetBool("_vce_SelectedPlatform_Android", false);
            _ve_EnableFragmentedWriting.value = EditorPrefs.GetBool("_ve_EnableFragmentedWriting", false);
            _vmb_MotionBlur.value = EditorPrefs.GetBool("_vmb_MotionBlur", false);
            _ap_OutputTarget.value = EditorPrefs.GetBool("_ap_OutputTarget", false);
            _a_OfflineInvalidOption.value = EditorPrefs.GetBool("_a_OfflineInvalidOption", false);
            _a_RealtimeWwise.value = EditorPrefs.GetBool("_a_RealtimeWwise", false);
            _a_OfflineWwise.value = EditorPrefs.GetBool("_a_OfflineWwise", false);
            _a_ShowAudioOptions.value = EditorPrefs.GetBool("_a_ShowAudioOptions", false);
            _a_CaptureSourceMicrophone.value = EditorPrefs.GetBool("_a_CaptureSourceMicrophone", false);
            _a_CaptureSourceUnityWwise.value = EditorPrefs.GetBool("_a_CaptureSourceUnityWwise", false);
            _a_CaptureSourceManual.value = EditorPrefs.GetBool("_a_CaptureSourceManual", false);
            _a_CaptureSourceNone.value = EditorPrefs.GetBool("_a_CaptureSourceNone", false);
            _p_StereoPackingCustom.value = EditorPrefs.GetBool("_p_StereoPackingCustom", false);
            _p_SphericalLayoutCustom.value = EditorPrefs.GetBool("_p_SphericalLayoutCustom", false);
            _m_SelectedPlatform_Windows.value = EditorPrefs.GetBool("_m_SelectedPlatform_Windows", false);
            _m_SelectedPlatform_Android.value = EditorPrefs.GetBool("_m_SelectedPlatform_Android", false);
            _m_SelectedPlatform_macOS.value = EditorPrefs.GetBool("_m_SelectedPlatform_macOS", false);
            _m_SelectedPlatform_iOS.value = EditorPrefs.GetBool("_m_SelectedPlatform_iOS", false);
        }

		private void SaveSettings()
		{
			EditorPrefs.SetBool(SettingsPrefix + "ExpandStartStop", _isExpandedStartStop);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandOutput", _isExpandedOutput);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandVisual", _isExpandedVisual);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandAudio", _isExpandedAudio);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandPost", _isExpandedPost);
			EditorPrefs.SetBool(SettingsPrefix + "ExpandMisc", _isExpandedMisc);
			EditorPrefs.SetInt(SettingsPrefix + "SelectedPlatform", (int)_selectedPlatform);

            // AnimBools
            EditorPrefs.SetBool("_aboutSeciton", _aboutSeciton.target);
            EditorPrefs.SetBool("_startStopSection", _startStopSection.target);
            EditorPrefs.SetBool("_outputFilePathSection", _outputFilePathSection.target);
            EditorPrefs.SetBool("_visualSection", _visualSection.target);
            EditorPrefs.SetBool("_audioSection", _audioSection.target);
            EditorPrefs.SetBool("_postSection", _postSection.target);
            EditorPrefs.SetBool("_miscSection", _miscSection.target);
            EditorPrefs.SetBool("_ss_StartDelay", _ss_StartDelay.target);
            EditorPrefs.SetBool("_ss_StopMode_FramesEncoded", _ss_StopMode_FramesEncoded.target);
            EditorPrefs.SetBool("_ss_StopMode_Other", _ss_StopMode_Other.target);
            EditorPrefs.SetBool("_o_OutputTarget_Video", _o_OutputTarget_Video.target);
            EditorPrefs.SetBool("_o_OutputTarget_Image", _o_OutputTarget_Image.target);
            EditorPrefs.SetBool("_o_OutputTarget_Pipe", _o_OutputTarget_Pipe.target);
            EditorPrefs.SetBool("_o_ManualExtension", _o_ManualExtension.target);
            EditorPrefs.SetBool("_o_CustomFilenameComponent", _o_CustomFilenameComponent.target);
            EditorPrefs.SetBool("_o_FilenameComponent", _o_FilenameComponent.target);
            EditorPrefs.SetBool("_v_DownScale", _v_DownScale.target);
            EditorPrefs.SetBool("_v_OutputTarget_Video", _v_OutputTarget_Video.target);
            EditorPrefs.SetBool("_v_OutputTarget_Image", _v_OutputTarget_Image.target);
            EditorPrefs.SetBool("_vce_SelectedPlatform_Windows", _vce_SelectedPlatform_Windows.target);
            EditorPrefs.SetBool("_vce_SelectedPlatform_macOS", _vce_SelectedPlatform_macOS.target);
            EditorPrefs.SetBool("_vce_SelectedPlatform_iOS", _vce_SelectedPlatform_iOS.target);
            EditorPrefs.SetBool("_vce_SelectedPlatform_Android", _vce_SelectedPlatform_Android.target);
            EditorPrefs.SetBool("_ve_EnableFragmentedWriting", _ve_EnableFragmentedWriting.target);
            EditorPrefs.SetBool("_vmb_MotionBlur", _vmb_MotionBlur.target);
            EditorPrefs.SetBool("_ap_OutputTarget", _ap_OutputTarget.target);
            EditorPrefs.SetBool("_a_OfflineInvalidOption", _a_OfflineInvalidOption.target);
            EditorPrefs.SetBool("_a_RealtimeWwise", _a_RealtimeWwise.target);
            EditorPrefs.SetBool("_a_OfflineWwise", _a_OfflineWwise.target);
            EditorPrefs.SetBool("_a_ShowAudioOptions", _a_ShowAudioOptions.target);
            EditorPrefs.SetBool("_a_CaptureSourceMicrophone", _a_CaptureSourceMicrophone.target);
            EditorPrefs.SetBool("_a_CaptureSourceUnityWwise", _a_CaptureSourceUnityWwise.target);
            EditorPrefs.SetBool("_a_CaptureSourceManual", _a_CaptureSourceManual.target);
            EditorPrefs.SetBool("_a_CaptureSourceNone", _a_CaptureSourceNone.target);
            EditorPrefs.SetBool("_p_StereoPackingCustom", _p_StereoPackingCustom.target);
            EditorPrefs.SetBool("_p_SphericalLayoutCustom", _p_SphericalLayoutCustom.target);
            EditorPrefs.SetBool("_m_SelectedPlatform_Windows", _m_SelectedPlatform_Windows.target);
            EditorPrefs.SetBool("_m_SelectedPlatform_Android", _m_SelectedPlatform_Android.target);
            EditorPrefs.SetBool("_m_SelectedPlatform_macOS", _m_SelectedPlatform_macOS.target);
            EditorPrefs.SetBool("_m_SelectedPlatform_iOS", _m_SelectedPlatform_iOS.target);
        }

		protected virtual void OnEnable()
		{
			#if UNITY_EDITOR_WIN
			_selectedPlatform = NativePlugin.Platform.Windows;
			#elif UNITY_EDITOR_OSX
			_selectedPlatform = NativePlugin.Platform.macOS;
			#endif

			LoadSettings();

			_baseCapture = (CaptureBase)this.target;

			_propCaptureKey = serializedObject.AssertFindProperty("_captureKey");
			_propPersistAcrossSceneLoads = serializedObject.AssertFindProperty("_persistAcrossSceneLoads");
			_propIsRealtime = serializedObject.AssertFindProperty("_isRealTime");
			_propMinimumDiskSpaceMB = serializedObject.AssertFindProperty("_minimumDiskSpaceMB");

			_propOutputTarget = serializedObject.AssertFindProperty("_outputTarget");
			_propImageSequenceFormatWindows = serializedObject.AssertFindProperty("_imageSequenceFormatWindows");
			_propImageSequenceFormatMacOS = serializedObject.AssertFindProperty("_imageSequenceFormatMacOS");
			_propImageSequenceFormatIOS = serializedObject.AssertFindProperty("_imageSequenceFormatIOS");
			_propImageSequenceFormatAndroid = serializedObject.AssertFindProperty("_imageSequenceFormatAndroid");
			_propImageSequenceStartFrame = serializedObject.AssertFindProperty("_imageSequenceStartFrame");
			_propImageSequenceZeroDigits = serializedObject.AssertFindProperty("_imageSequenceZeroDigits");
			_propOutputFolderType = serializedObject.AssertFindProperty("_outputFolderType");
			_propOutputFolderPath = serializedObject.AssertFindProperty("_outputFolderPath");
			//_propAppendFilenameTimestamp = serializedObject.AssertFindProperty("_appendFilenameTimestamp"); // replaced in favour of _filenameComponents
			_propFileNamePrefix = serializedObject.AssertFindProperty("_filenamePrefix");
			_propAllowManualFileExtension = serializedObject.AssertFindProperty("_allowManualFileExtension");
			_propFileNameExtension = serializedObject.AssertFindProperty("_filenameExtension");
			_propNamedPipePath = serializedObject.AssertFindProperty("_namedPipePath");
			_propFileNameComponents = serializedObject.AssertFindProperty("_filenameComponents");
			_propFilenameComponentCustomText = serializedObject.AssertFindProperty("_filenameComponentCustomText");
			_propFilenameComponentSeperator = serializedObject.AssertFindProperty("_filenameComponentSeperator");

            _propVideoCodecPriorityWindows = serializedObject.AssertFindProperty("_videoCodecPriorityWindows");
			_propVideoCodecPriorityMacOS = serializedObject.AssertFindProperty("_videoCodecPriorityMacOS");
			//_propVideoCodecPriorityAndroid = serializedObject.AssertFindProperty("_videoCodecPriorityAndroid");
			_propForceVideoCodecIndexWindows = serializedObject.AssertFindProperty("_forceVideoCodecIndexWindows");
			_propForceVideoCodecIndexMacOS = serializedObject.AssertFindProperty("_forceVideoCodecIndexMacOS");
			_propForceVideoCodecIndexIOS = serializedObject.AssertFindProperty("_forceVideoCodecIndexIOS");
			_propForceVideoCodecIndexAndroid = serializedObject.AssertFindProperty("_forceVideoCodecIndexAndroid");

			_propAudioCodecPriorityWindows = serializedObject.AssertFindProperty("_audioCodecPriorityWindows");
			_propAudioCodecPriorityMacOS = serializedObject.AssertFindProperty("_audioCodecPriorityMacOS");
			//_propAudioCodecPriorityIOS = serializedObject.AssertFindProperty("_audioCodecPriorityIOS");
			//_propAudioCodecPriorityAndroid = serializedObject.AssertFindProperty("_audioCodecPriorityAndroid");
			_propForceAudioCodecIndexWindows = serializedObject.AssertFindProperty("_forceAudioCodecIndexWindows");
			_propForceAudioCodecIndexMacOS = serializedObject.AssertFindProperty("_forceAudioCodecIndexMacOS");
			_propForceAudioCodecIndexIOS = serializedObject.AssertFindProperty("_forceAudioCodecIndexIOS");
			_propForceAudioCodecIndexAndroid = serializedObject.AssertFindProperty("_forceAudioCodecIndexAndroid");

			_propAudioCaptureSource = serializedObject.AssertFindProperty("_audioCaptureSource");
			_propUnityAudioCapture = serializedObject.AssertFindProperty("_unityAudioCapture");
			_propForceAudioDeviceIndex = serializedObject.AssertFindProperty("_forceAudioInputDeviceIndex");
			_propManualAudioSampleRate = serializedObject.AssertFindProperty("_manualAudioSampleRate");
			_propManualAudioChannelCount = serializedObject.AssertFindProperty("_manualAudioChannelCount");

			_propDownScale = serializedObject.AssertFindProperty("_downScale");
			_propMaxVideoSize = serializedObject.AssertFindProperty("_maxVideoSize");
			_propFrameRate = serializedObject.AssertFindProperty("_frameRate");
			_propTimelapseScale = serializedObject.AssertFindProperty("_timelapseScale");
			_propFrameUpdateMode = serializedObject.AssertFindProperty("_frameUpdateMode");
			_propFlipVertically = serializedObject.AssertFindProperty("_flipVertically");
			_propForceGpuFlush = serializedObject.AssertFindProperty("_forceGpuFlush");
			_propWaitForEndOfFrame = serializedObject.AssertFindProperty("_useWaitForEndOfFrame");
			_propAndroidUpdateMediaGallery = serializedObject.AssertFindProperty("_androidUpdateMediaGallery");
			_propAndroidNoCaptureRotation = serializedObject.AssertFindProperty("_androidNoCaptureRotation");
			_prop_iOSSaveCaptureWhenAppLosesFocus = serializedObject.AssertFindProperty("_iOSSaveCaptureWhenAppLosesFocus");
			_propWriteOrientationMetadata = serializedObject.AssertFindProperty("_writeOrientationMetadata");

			_propUseMotionBlur = serializedObject.AssertFindProperty("_useMotionBlur");
			_propMotionBlurSamples = serializedObject.AssertFindProperty("_motionBlurSamples");
			_propMotionBlurCameras = serializedObject.AssertFindProperty("_motionBlurCameras");

			_propStartTrigger = serializedObject.AssertFindProperty("_startTrigger");
			_propStartDelay = serializedObject.AssertFindProperty("_startDelay");
			_propStartDelaySeconds = serializedObject.AssertFindProperty("_startDelaySeconds");

			_propStopMode = serializedObject.AssertFindProperty("_stopMode");
			_propStopFrames = serializedObject.AssertFindProperty("_stopFrames");
			_propStopSeconds = serializedObject.AssertFindProperty("_stopSeconds");

//			_propPauseCaptureOnAppPause = serializedObject.AssertFindProperty("_pauseCaptureOnAppPause");

			_propVideoHints = new PropVideoHints[(int)NativePlugin.Platform.Count];
			_propVideoHints[(int)NativePlugin.Platform.Windows] = GetProperties_VideoHints(serializedObject, "_encoderHintsWindows.videoHints");
			_propVideoHints[(int)NativePlugin.Platform.macOS] = GetProperties_VideoHints(serializedObject, "_encoderHintsMacOS.videoHints");
			_propVideoHints[(int)NativePlugin.Platform.iOS] = GetProperties_VideoHints(serializedObject, "_encoderHintsIOS.videoHints");
			_propVideoHints[(int)NativePlugin.Platform.Android] = GetProperties_VideoHints(serializedObject, "_encoderHintsAndroid.videoHints");

			_propImageHints = new PropImageHints[(int)NativePlugin.Platform.Count];
			_propImageHints[(int)NativePlugin.Platform.Windows] = GetProperties_ImageHints(serializedObject, "_encoderHintsWindows.imageHints");
			_propImageHints[(int)NativePlugin.Platform.macOS] = GetProperties_ImageHints(serializedObject, "_encoderHintsMacOS.imageHints");
			_propImageHints[(int)NativePlugin.Platform.iOS] = GetProperties_ImageHints(serializedObject, "_encoderHintsIOS.imageHints");
			_propImageHints[(int)NativePlugin.Platform.Android] = GetProperties_ImageHints(serializedObject, "_encoderHintsAndroid.imageHints");

			_propLogCaptureStartStop = serializedObject.AssertFindProperty("_logCaptureStartStop");
			_propAllowVsyncDisable = serializedObject.AssertFindProperty("_allowVSyncDisable");
			_propSupportTextureRecreate = serializedObject.AssertFindProperty("_supportTextureRecreate");

			#if AVPRO_MOVIECAPTURE_PLAYABLES_SUPPORT
			_propTimelineController = serializedObject.AssertFindProperty("_timelineController");
			#endif
			#if AVPRO_MOVIECAPTURE_VIDEOPLAYER_SUPPORT
			_propVideoPlayerController = serializedObject.AssertFindProperty("_videoPlayerController");
			#endif

			_isTrialVersion = false;
			if (Application.isPlaying)
			{
				_isTrialVersion = IsTrialVersion();
			}


			_aboutSeciton.valueChanged.AddListener(Repaint);
            _startStopSection.valueChanged.AddListener(Repaint);
            _outputFilePathSection.valueChanged.AddListener(Repaint);
            _visualSection.valueChanged.AddListener(Repaint);
            _audioSection.valueChanged.AddListener(Repaint);
            _postSection.valueChanged.AddListener(Repaint);
			_miscSection.valueChanged.AddListener(Repaint);
            _ss_StartDelay.valueChanged.AddListener(Repaint);
            _ss_StopMode_FramesEncoded.valueChanged.AddListener(Repaint);
            _ss_StopMode_Other.valueChanged.AddListener(Repaint);
            _o_OutputTarget_Video.valueChanged.AddListener(Repaint);
            _o_OutputTarget_Image.valueChanged.AddListener(Repaint);
            _o_OutputTarget_Pipe.valueChanged.AddListener(Repaint);
            _o_ManualExtension.valueChanged.AddListener(Repaint);
            _o_CustomFilenameComponent.valueChanged.AddListener(Repaint);
            _o_FilenameComponent.valueChanged.AddListener(Repaint);
            _v_DownScale.valueChanged.AddListener(Repaint);
            _v_OutputTarget_Video.valueChanged.AddListener(Repaint);
            _v_OutputTarget_Image.valueChanged.AddListener(Repaint);
            _vce_SelectedPlatform_Windows.valueChanged.AddListener(Repaint);
            _vce_SelectedPlatform_macOS.valueChanged.AddListener(Repaint);
            _vce_SelectedPlatform_iOS.valueChanged.AddListener(Repaint);
            _vce_SelectedPlatform_Android.valueChanged.AddListener(Repaint);
            _ve_EnableFragmentedWriting.valueChanged.AddListener(Repaint);
            _vmb_MotionBlur.valueChanged.AddListener(Repaint);
            _ap_OutputTarget.valueChanged.AddListener(Repaint);
            _a_OfflineInvalidOption.valueChanged.AddListener(Repaint);
            _a_RealtimeWwise.valueChanged.AddListener(Repaint);
            _a_OfflineWwise.valueChanged.AddListener(Repaint);
            _a_ShowAudioOptions.valueChanged.AddListener(Repaint);
            _a_CaptureSourceMicrophone.valueChanged.AddListener(Repaint);
            _a_CaptureSourceUnityWwise.valueChanged.AddListener(Repaint);
            _a_CaptureSourceManual.valueChanged.AddListener(Repaint);
            _a_CaptureSourceNone.valueChanged.AddListener(Repaint);
            _p_StereoPackingCustom.valueChanged.AddListener(Repaint);
            _p_SphericalLayoutCustom.valueChanged.AddListener(Repaint);
            _m_SelectedPlatform_Windows.valueChanged.AddListener(Repaint);
            _m_SelectedPlatform_Android.valueChanged.AddListener(Repaint);
            _m_SelectedPlatform_macOS.valueChanged.AddListener(Repaint);
            _m_SelectedPlatform_iOS.valueChanged.AddListener(Repaint);
        }

        private void OnDisable()
        {
            SaveSettings();
            _aboutSeciton.valueChanged.RemoveListener(Repaint);
            _startStopSection.valueChanged.RemoveListener(Repaint);
            _outputFilePathSection.valueChanged.RemoveListener(Repaint);
            _visualSection.valueChanged.RemoveListener(Repaint);
            _audioSection.valueChanged.RemoveListener(Repaint);
            _postSection.valueChanged.RemoveListener(Repaint);
            _miscSection.valueChanged.RemoveListener(Repaint);
            _ss_StartDelay.valueChanged.RemoveListener(Repaint);
            _ss_StopMode_FramesEncoded.valueChanged.RemoveListener(Repaint);
            _ss_StopMode_Other.valueChanged.RemoveListener(Repaint);
            _o_OutputTarget_Video.valueChanged.RemoveListener(Repaint);
            _o_OutputTarget_Image.valueChanged.RemoveListener(Repaint);
            _o_OutputTarget_Pipe.valueChanged.RemoveListener(Repaint);
            _o_ManualExtension.valueChanged.RemoveListener(Repaint);
            _o_CustomFilenameComponent.valueChanged.RemoveListener(Repaint);
            _o_FilenameComponent.valueChanged.RemoveListener(Repaint);
            _v_DownScale.valueChanged.RemoveListener(Repaint);
            _v_OutputTarget_Video.valueChanged.RemoveListener(Repaint);
            _v_OutputTarget_Image.valueChanged.RemoveListener(Repaint);
            _vce_SelectedPlatform_Windows.valueChanged.RemoveListener(Repaint);
            _vce_SelectedPlatform_macOS.valueChanged.RemoveListener(Repaint);
            _vce_SelectedPlatform_iOS.valueChanged.RemoveListener(Repaint);
            _vce_SelectedPlatform_Android.valueChanged.RemoveListener(Repaint);
            _ve_EnableFragmentedWriting.valueChanged.RemoveListener(Repaint);
            _vmb_MotionBlur.valueChanged.RemoveListener(Repaint);
            _ap_OutputTarget.valueChanged.RemoveListener(Repaint);
            _a_OfflineInvalidOption.valueChanged.RemoveListener(Repaint);
            _a_RealtimeWwise.valueChanged.RemoveListener(Repaint);
            _a_OfflineWwise.valueChanged.RemoveListener(Repaint);
            _a_ShowAudioOptions.valueChanged.RemoveListener(Repaint);
            _a_CaptureSourceMicrophone.valueChanged.RemoveListener(Repaint);
            _a_CaptureSourceUnityWwise.valueChanged.RemoveListener(Repaint);
            _a_CaptureSourceManual.valueChanged.RemoveListener(Repaint);
            _a_CaptureSourceNone.valueChanged.RemoveListener(Repaint);
            _p_StereoPackingCustom.valueChanged.RemoveListener(Repaint);
            _p_SphericalLayoutCustom.valueChanged.RemoveListener(Repaint);
            _m_SelectedPlatform_Windows.valueChanged.RemoveListener(Repaint);
            _m_SelectedPlatform_Android.valueChanged.RemoveListener(Repaint);
            _m_SelectedPlatform_macOS.valueChanged.RemoveListener(Repaint);
            _m_SelectedPlatform_iOS.valueChanged.RemoveListener(Repaint);
        }

        private static PropVideoHints GetProperties_VideoHints(SerializedObject serializedObject, string prefix)
		{
			PropVideoHints result = new PropVideoHints();
			result.propAverageBitrate = serializedObject.AssertFindProperty(prefix + ".averageBitrate");
			result.propMaximumBitrate = serializedObject.AssertFindProperty(prefix + ".maximumBitrate");
			result.propQuality = serializedObject.AssertFindProperty(prefix + ".quality");
			result.propKeyframeInterval = serializedObject.AssertFindProperty(prefix + ".keyframeInterval");
			result.propFastStart = serializedObject.AssertFindProperty(prefix + ".allowFastStartStreamingPostProcess");
			result.propTransparency = serializedObject.AssertFindProperty(prefix + ".transparency");
			result.propColourRange = serializedObject.AssertFindProperty(prefix + ".colourRange");
			result.propHardwareEncoding = serializedObject.AssertFindProperty(prefix + ".useHardwareEncoding");
			result.propInjectStereoPacking = serializedObject.AssertFindProperty(prefix + ".injectStereoPacking");
			result.propStereoPacking = serializedObject.AssertFindProperty(prefix + ".stereoPacking");
			result.propInjectSphericalVideoLayout = serializedObject.AssertFindProperty(prefix + ".injectSphericalVideoLayout");
			result.propSphericalVideoLayout = serializedObject.AssertFindProperty(prefix + ".sphericalVideoLayout");
			result.propEnableConstantQuality = serializedObject.AssertFindProperty(prefix + ".enableConstantQuality");
			result.propEnableFragmentedWriting = serializedObject.AssertFindProperty(prefix + ".enableFragmentedWriting");
			result.propMovieFragmentInterval = serializedObject.AssertFindProperty(prefix + ".movieFragmentInterval");
			result.propFramePTSMode = serializedObject.AssertFindProperty(prefix + ".realtimeFramePresentationTimestampOptions");
			return result;
		}

		private static PropImageHints GetProperties_ImageHints(SerializedObject serializedObject, string prefix)
		{
			PropImageHints result = new PropImageHints();
			result.propQuality = serializedObject.AssertFindProperty(prefix + ".quality");
			result.propTransparency = serializedObject.AssertFindProperty(prefix + ".transparency");
			return result;
		}

		protected static bool IsTrialVersion()
		{
			bool result = false;
			try
			{
				result = NativePlugin.IsTrialVersion();
			}
			catch (System.DllNotFoundException)
			{
				// Silent catch as we report this error elsewhere
			}
			return result;
		}

		protected static void ShowNoticeBox(MessageType messageType, string message)
		{
			//GUI.backgroundColor = Color.yellow;
			//EditorGUILayout.HelpBox(message, messageType);

			switch (messageType)
			{
				case MessageType.Error:
					GUI.color = Color.red;
					message = "Error: " + message;
					break;
				case MessageType.Warning:
					GUI.color = Color.yellow;
					message = "Warning: " + message;
					break;
			}

			//GUI.color = Color.yellow;
			GUILayout.TextArea(message);
			GUI.color = Color.white;
		}

		public override bool RequiresConstantRepaint()
		{
			CaptureBase capture = (this.target) as CaptureBase;
			return (Application.isPlaying && capture.isActiveAndEnabled && capture.IsCapturing() && !capture.IsPaused());
		}
	}
}
#endif
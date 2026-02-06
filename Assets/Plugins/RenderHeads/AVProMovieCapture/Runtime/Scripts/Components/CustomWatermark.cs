using System;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2025 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProMovieCapture
{

	/* RBN Notes:
		- Capture from Camera                       (Camera Based)
		 - Working
		- Capture from Screen                       (Camera Based)
		 - Working
		- Capture from Camera 360                   (Camera Based - Minor Extra Steps)
		 - Working 
		- Capture from Camera 360 Stereo ODS        (Graphics.Blit - Major Extra Steps)
		  - Does not work with IPD differning from the base 0.064
		- Capture from Texture                      (Graphics.Blit)
		 - Working
		- Capture from WebCamTexture                (Graphics.Blit)
		 - Working 
	 */
	public class CustomWatermark : MonoBehaviour
	{
		// The differernt options for positioning the watermark on the screen
		public enum WatermarkPositionOptions
		{
			AnchorPosition,
			RandomPosition,
			LerpRandomPosition,
			CurveBasedPosition,
			CustomPosition,
			CustomFunction
		}
		// the differernt capture types that can be used
		private enum CaptureType
		{
			Camera,
			Screen,
			Camera360,
			Camera360ODS,
			Texture,
			WebCamTexture
		}

		private enum RenderFaces
		{
			ALL,
			Right,
			Left,
			Bottom,
			Top,
			Front,
			Back
		}

		[Tooltip("The Capture Component that is being used, Watermark will be injected to this one")]
		[SerializeField] private CaptureBase _captureComponent;
		// Camera + Screen + Camera360 + Camera360ODS
		[Tooltip("The Material that is being used to draw the watermark")]
		[SerializeField] private Material _mat;
		// Camera + Screen + Camera360
		[Tooltip("The Camera the is currently being used for rendering")]
		[SerializeField] private Camera _targetCamera;
		// Camera360
		[Tooltip("360 + ODS - Faces 0-5\n-1 = Render to all")]
		[SerializeField] private RenderFaces _renderToFace = RenderFaces.Front;
		// Camera360ODS
		private bool _updatePosition;
		// Texture
		[Tooltip("The material to use when drawing the Watermark to a texture")]
		[SerializeField] private Material _textureBlitMaterial;

		private CaptureType captureType;

		[Tooltip("Texture to use for the watermark")]
		[SerializeField] private Texture2D _watermarkTexture;
		[Tooltip("How to position the watermark on the screen")]
		[SerializeField] private WatermarkPositionOptions _positionOption;
		[Tooltip("The size of the watermark")]
		[SerializeField] private Vector2Int _watermarkSize;

		// Anchor Position Offset Options (9 main anchor points)
		[Tooltip("Set this to achor the watermark to one of the 9 main anchor points")]
		[SerializeField] private int _anchorPosition = -1;

		// Global Options
		[Tooltip("Do you want to limit the watermark to a specific area, and scale it size and movements to match?")]
		[SerializeField] private RectInt _offsetArea;

		// Random Position Offset Options
		[Tooltip("How often the watermark will change position")]
		[SerializeField] private float _rateOfChange = 1f;
		private float _timeOfNextChange;
		private Vector2 _previousRandomPosition;

		// Lerp Random Position Offset Options
		[Tooltip("The Speed at which the watermark moves")]
		[Range(0, 1)]
		[SerializeField] private float _lerpSpeed = 0.1f;
		[Tooltip("Use frame Scaled time (Smoothes movement on varying framerate)")]
		[SerializeField] private bool _useDeltaTime = false;
		private Vector2 _currentPosition = Vector2.zero;
		private Vector2 _targetPosition = Vector2.negativeInfinity;

		// Graph Based Position
		[Tooltip("The Speed at which the watermark moves")]
		[Range(0, 2)]
		[SerializeField] private float _curveTraversalSpeed;
		[Tooltip("The curve for the watermark to follow")]
		[SerializeField] private AnimationCurve _curve;
		[Tooltip("When reaching the end should the watermark teleport to the start or traverse the curve backwards (loop)")]
		[SerializeField] private bool _loop;
		private float _curveCounter = 0;
		private bool _movingRight = true;

		// Custom Position
		[Tooltip("Set a custom position for the watermark to be in")]
		[SerializeField] private Vector2 _position;

		// Custom Function
		[Tooltip("Set a custom function to be called that will handle the movement of the watermark")]
		public delegate Vector2 CustomMovementFunction();
		public CustomMovementFunction CustomMovementFunc;

		private bool _isSetup;
		private Vector2 _screenSize;
		private double scaledTime = 1;
		private double lastCallTime = 1;

		public CaptureBase CaptureComponent
		{
			get { return _captureComponent; }
			set { _captureComponent = value; }
		}

		public Texture2D Texture
		{
			get { return _watermarkTexture; }
		}

		public bool UpdatePosition
		{
			get { return _updatePosition; }
			set { _updatePosition = value; }
		}

		public Vector2 ScreenSize
		{
			get { return _screenSize; }
		}

		public int RenderToFace
		{
			get { return (int)_renderToFace - 1; }
		}


		private void Start()
		{
			Setup();
		}

		public void OnEnable()
		{
			Setup();
		}

		/// <summary>
		/// Setup the watermark component, this method is to allow for captures the happen during play mode, and not nececarrly at the start
		/// </summary>
		public void Setup()
		{
			if (_isSetup) { return; }

			// Always want to do this unless using 360 ODS mode
			_updatePosition = true;

			// we need a camera
			if (_targetCamera == null)
			{
				_targetCamera = Camera.main;
			}

			// Do the processing on the camera
			if (!_mat)
			{
				var shader = Shader.Find("Hidden/AVProMovieCapture/CustomWatermark");
				_mat = new Material(shader);
				_mat.hideFlags = HideFlags.HideAndDontSave;
				_mat.SetTexture("_MainTex", _watermarkTexture);
			}
			else // already have a material so just set the texture
			{
				_mat.hideFlags = HideFlags.HideAndDontSave;
				_mat.SetTexture("_MainTex", _watermarkTexture);
			}

			_screenSize = new Vector2(Screen.width, Screen.height);

			if (_captureComponent == null) { return; }

			// Get the type if capture that is to be used
			switch (_captureComponent.GetType().Name)
			{
				case "CaptureFromCamera":
					captureType = CaptureType.Camera;
					break;
				case "CaptureFromCamera360":
					captureType = CaptureType.Camera360;
					break;
				case "CaptureFromCamera360ODS":
					captureType = CaptureType.Camera360ODS;
					_updatePosition = false;
					((CaptureFromCamera360ODS)_captureComponent).watermark = this;
					break;
				case "CaptureFromScreen":
					captureType = CaptureType.Screen;
					break;
				case "CaptureFromTexture":
					captureType = CaptureType.Texture;
					((CaptureFromTexture)_captureComponent).customWatermark = this;
					break;
				case "CaptureFromWebCamTexture":
					captureType = CaptureType.WebCamTexture;
					((CaptureFromTexture)_captureComponent).customWatermark = this;
					break;
			}

			// if using a capture method that relise on the camera add the on post render callback
			if (captureType == CaptureType.Screen || captureType == CaptureType.Camera || captureType == CaptureType.Camera360)
			{
				Camera.onPostRender += OnPostRenderCallback;
			}

			_isSetup = true;
		}

		/// <summary>
		/// This is used to determine if the 360ODS compoenent has been setup to allow for watermarking.
		/// </summary>
		/// <returns>
		/// True if watermarking is supported for by the current 360ODS capture component; otherwise, false.
		/// </returns>
		public bool DoesODSSupportWatermark()
		{
			bool result = false;
			// Camera360ODS only supports watermarking if Camera360ODS.Setup.SupportWatermark is true
			if (GetCaptureType() == 3)
			{
				result = ((CaptureFromCamera360ODS)_captureComponent).Setup.SupportWatermark;
			}
			return result;
		}

		/// <summary>
		/// checks if the CustomWatermark is using the custom function for positioning
		/// of the watermark
		/// </summary>
		/// <returns>True if using custom function method for positioning; otherwise false</returns>
		public bool IsUsingCustomFunction()
		{
			bool result = false;
			if (_positionOption == WatermarkPositionOptions.CustomFunction)
			{
				result = true;
			}
			return result;
		}

		/// <summary>
		/// Returns the integer value representing the current capture type in use by this watermark component.
		/// - Checks the type name of the assigned _captureComponent and maps it to the corresponding
		///   internal CaptureType enum value (Camera, Camera360, Camera360ODS, Screen, Texture, WebCamTexture).
		/// - If no capture component is assigned, or the type is unrecognized, returns -1.
		/// </summary>
		/// <returns>
		/// Integer value of the current capture type, or -1 if not set or unknown.
		/// </returns>
		public int GetCaptureType()
		{
			if (!_captureComponent) { return -1; }

			// Map the capture component's type name to the internal CaptureType enum
			switch (_captureComponent.GetType().Name)
			{
				case "CaptureFromCamera":                   // 1
					return (int)CaptureType.Camera;
				case "CaptureFromCamera360":                // 2
					return (int)CaptureType.Camera360;
				case "CaptureFromCamera360ODS":             // 3
					return (int)CaptureType.Camera360ODS;
				case "CaptureFromScreen":                   // 4
					return (int)CaptureType.Screen;
				case "CaptureFromTexture":                  // 5
					return (int)CaptureType.Texture;
				case "CaptureFromWebCamTexture":            // 6
					return (int)CaptureType.WebCamTexture;
				default:
					return -1; // Unknown or unsupported type
			}
		}

		/// <summary>
		/// Handles Cleanup of the Watermark component, removing any camera post render actions,
		/// as well as removing the watermark from any Capture components
		/// </summary>
		public void Stop()
		{
			if (!_isSetup) { return; }

			// Unsubscribe the watermark drawing callback from the camera event to prevent leaks
			Camera.onPostRender -= OnPostRenderCallback; // STOP

			// Remove Watermark from capture componenets so they stop trying to apply it
			if (GetCaptureType() == 3)      // ODS
			{
				((CaptureFromCamera360ODS)_captureComponent).watermark = null;
			}
			else if (GetCaptureType() == 5) // Texture
			{
				((CaptureFromTexture)_captureComponent).customWatermark = null;
			}
			else if (GetCaptureType() == 6) // WebcamTexture
			{
				((CaptureFromWebCamTexture)_captureComponent).customWatermark = null;
			}

			_isSetup = false;
		}
		private void OnDestroy() { Stop(); }
		public void OnDisable() { Stop(); }

		/// <summary>
		/// Adds the watermark to the provided RenderTexture by blitting it with a custom material.
		/// - Ensures the internal screenSize matches the texture's dimensions.
		/// - Initializes the watermark blit material if not already set, using a custom shader.
		/// - Sets the watermark and main textures on the material.
		/// - Calculates the normalized size and offset for the watermark using GetOffset().
		/// - Sets the watermark position and size on the material.
		/// - Performs a two-step blit: first draws the watermark onto a temporary texture, then copies it back to the original texture.
		/// - Releases the temporary texture after use.
		/// This function is called by texture-based capture components before the texture is sent to native code.
		/// </summary>
		/// <param name="texture">The RenderTexture to which the watermark will be added.</param>
		/// <returns>Texture with the watermark applied.</returns>
		public Texture AddWatermarkToTexture(RenderTexture texture)
		{
			// Make sure the size is correct for this texture
			if (_screenSize.x != texture.width || _screenSize.y != texture.height)
			{
				_screenSize = new Vector2(texture.width, texture.height);
			}

			// Get the material if there is not one
			if (!_textureBlitMaterial)
			{
				var shader = Shader.Find("Hidden/AVProMovieCapture/CustomWatermarkTexture");
				_textureBlitMaterial = new Material(shader);
				_textureBlitMaterial.hideFlags = HideFlags.HideAndDontSave;
			}

			// Set the watermark and main textures on the material
			_textureBlitMaterial.SetTexture("_WatermarkTex", _watermarkTexture);
			_textureBlitMaterial.SetTexture("_MainTex", texture);

			// Calculate normalized size and offset for the watermark
			var val = GetOffset();
			// Set the watermark rectangle: X, Y, Width, Height
			_textureBlitMaterial.SetVector("_WatermarkRect", new Vector4(val.Item2.x, val.Item2.y, val.Item1.x, val.Item1.y));

			// Blit the watermark onto a temporary texture, then copy back to the original
			RenderTexture tempTexture = RenderTexture.GetTemporary((int)_screenSize.x, (int)_screenSize.y, 0, RenderTextureFormat.Default);
			Graphics.Blit(texture, tempTexture, _textureBlitMaterial);
			Graphics.Blit(tempTexture, texture);
			RenderTexture.ReleaseTemporary(tempTexture);

			// Return the watermarked texture
			return texture;
		}

		// This will current only convert correctly if using _blendOverlapPercent > 0.0f with camera 360
		private static int GetFace360(RenderFaces face) => face switch
		{
			RenderFaces.ALL => -1,
			RenderFaces.Right => 2,
			RenderFaces.Left => 3,
			RenderFaces.Top => 4,
			RenderFaces.Bottom => 5,
			RenderFaces.Front => 0,
			RenderFaces.Back => 1,
			_ => -1,
		};

		/// <summary>
		/// Callback that draws the watermark onto the rendered image after the camera finishes rendering.
		/// - Only draws for the correct camera and capture type (Screen, Camera, or Camera360).
		/// - For Camera360, only draws on the specified cubemap face and flips the watermark vertically.
		/// - Skips drawing for Camera360ODS (handled differently).
		/// - Calculates the normalized position and size for the watermark using GetOffset().
		/// - Uses GL immediate mode to draw a textured quad at the correct screen position.
		/// - Handles flipping the watermark if required (e.g., for cubemap faces).
		/// </summary>
		/// <param name="cam">The camera that has just finished rendering.</param>
		private void OnPostRenderCallback(Camera cam)
		{
			// 360 draws the watermark upside down
			bool needsFlip = false;

			// Only draw for the correct camera and capture type
			if (captureType == CaptureType.Screen || captureType == CaptureType.Camera)
			{
				if (cam != _targetCamera) { return; }

			}
			else if (captureType == CaptureType.Camera360)
			{
				// Skip reflection and preview cameras, and only draw for the target camera
				if (cam.cameraType == CameraType.Reflection || cam.cameraType == CameraType.Preview || cam != _targetCamera) { return; }
				// Only render to the specified cubemap face, unless rendering to all (-1)
				if (GetFace360(_renderToFace) != ((CaptureFromCamera360)_captureComponent).currentRenderingTarget && GetFace360(_renderToFace) != -1) { return; }


				needsFlip = true; // Draws upside down otherwise
			}
			else if (captureType == CaptureType.Camera360ODS)
			{
				// Watermark handled differently for ODS, so skip
				return;
			}

			// Get the normalized size and offset for the watermark
			var val = GetOffset();
			// Calculate quad corners: Left, Right, Top, Bottom
			Vector4 pos = new Vector4(0 + val.Item2.x, val.Item1.x + val.Item2.x, val.Item1.y + val.Item2.y, 0 + val.Item2.y);

			// Draw the watermark quad using GL immediate mode
			GL.PushMatrix();
			_mat.SetPass(0);
			GL.LoadOrtho();
			GL.Begin(GL.QUADS);
			if (needsFlip) // Flip the image in both the X and Y axes
			{
				GL.TexCoord2(1, 1); // Top Right (was Bottom Left)
				GL.Vertex3(pos[0], pos[3], 0);
				GL.TexCoord2(1, 0); // Bottom Right (was Top Left)
				GL.Vertex3(pos[0], pos[2], 0);
				GL.TexCoord2(0, 0); // Bottom Left (was Top Right)
				GL.Vertex3(pos[1], pos[2], 0);
				GL.TexCoord2(0, 1); // Top Left (was Bottom Right)
				GL.Vertex3(pos[1], pos[3], 0);
			}
			else
			{
				GL.TexCoord2(0, 0); // Bottom Left
				GL.Vertex3(pos[0], pos[3], 0);
				GL.TexCoord2(0, 1); // Top Left
				GL.Vertex3(pos[0], pos[2], 0);
				GL.TexCoord2(1, 1); // Top Right
				GL.Vertex3(pos[1], pos[2], 0);
				GL.TexCoord2(1, 0); // Bottom Right
				GL.Vertex3(pos[1], pos[3], 0);
			}
			GL.End();
			GL.PopMatrix();
		}


		/// <summary>
		/// Calculates the normalized size and position offset for the watermark based on the current configuration.
		/// - Determines the watermark's pixel offset using the selected positioning mode (_positionOption),
		///   which can be anchor, random, lerp random, curve-based, or custom.
		/// - Ensures the watermark stays within the screen bounds by clamping the offset.
		/// - Converts both the watermark size and offset from pixel coordinates to normalized [0,1] values,
		///   making them resolution-independent for rendering.
		/// - For 360 ODS capture, updates the screen size to match the recording resolution.
		/// - Updates the last call time for time-based position calculations.
		/// </summary>
		/// <returns>
		/// A tuple containing:
		///   - Vector2: Normalized watermark size (width, height) in [0,1] relative to the screen.
		///   - Vector2: Normalized watermark offset (x, y) in [0,1] relative to the screen.
		/// </returns>
		public (Vector2, Vector2) GetOffset()
		{
			// Update scaled time for time-based position calculations (except for 360ODS, which updates differently)
			if (_updatePosition)
			{
				scaledTime = Time.timeAsDouble - lastCallTime >= 0 ? Time.timeAsDouble - lastCallTime : 0;
			}

			// For 360ODS, update the screen size to match the recording resolution
			if (captureType == CaptureType.Camera360ODS)
			{
				_screenSize = new Vector2(
					((CaptureFromCamera360ODS)_captureComponent).GetRecordingWidth(),
					((CaptureFromCamera360ODS)_captureComponent).GetRecordingHeight()
				);
			}

			// Calculate the pixel offset based on the selected positioning mode
			Vector2 offset = Vector2.zero;
			switch (_positionOption)
			{
				case WatermarkPositionOptions.AnchorPosition:
					offset = AnchorPositionOffset();
					break;
				case WatermarkPositionOptions.RandomPosition:
					offset = RandomPositionOffset();
					break;
				case WatermarkPositionOptions.LerpRandomPosition:
					offset = RandomPositionOffsetLerp();
					break;
				case WatermarkPositionOptions.CurveBasedPosition:
					offset = GraphBasedPositionOffset();
					break;
				case WatermarkPositionOptions.CustomPosition:
					offset = CustomPositionOffset();
					break;
				case WatermarkPositionOptions.CustomFunction:
					if (CustomMovementFunc != null)
						offset = CustomMovementFunc();
					else
						offset = Vector2.zero;
					break;
				default:
					break;
			}

			// Clamp the offset so the watermark does not go off screen
			if (offset.x < 0) 
			{ 
				offset.x = 0; 
			}
			if (offset.y < 0)
			{
				offset.y = 0;
			}
			if (offset.x + _watermarkSize.x > _screenSize.x)
			{
				offset.x = (int)(_screenSize.x - _watermarkSize.x);
			}
			if (offset.y + _watermarkSize.y > _screenSize.y)
			{
				offset.y = (int)(_screenSize.y - _watermarkSize.y);
			}

			// Convert watermark size and offset to normalized [0,1] values
			var normSize = NormalizeValue(_watermarkSize);
			var normOffset = NormalizeValue(offset);

			// Update the last call time for time-based calculations
			lastCallTime = Time.timeAsDouble;

			// Return the normalized size and offset as a tuple
			return (normSize, normOffset);
		}

		/// <summary>
		/// Calculates the custom position for the watermark, ensuring it stays within the screen bounds.
		/// Uses the user-specified _position field as the desired (x, y) location in pixels.
		/// If the position is negative, it is clamped to 0.
		/// If the position would place the watermark outside the screen (right or bottom edge),
		/// it is clamped so the watermark remains fully visible.
		/// </summary>
		/// <returns>A Vector2 containing the (x, y) position in pixels for the watermark.</returns>
		private Vector2 CustomPositionOffset()
		{
			// Clamp X position: if less than 0, set to 0; if greater than screen, clamp to right edge
			var xPos = _position.x < 0 ? 0 : _position.x > _screenSize.x ? (_screenSize.x - _watermarkSize.x) : _position.x;
			// Clamp Y position: if less than 0, set to 0; if greater than screen, clamp to bottom edge
			var yPos = _position.y < 0 ? 0 : _position.y > _screenSize.y ? (_screenSize.y - _watermarkSize.y) : _position.y;
			// Return the clamped position
			return new Vector2(xPos, yPos);
		}

		/// <summary>
		/// Calculates the watermark's position along a user-defined animation curve, enabling graph-based movement.
		/// - Moves the watermark horizontally across the screen at a speed determined by _curveTraversalSpeed and scaledTime.
		/// - The direction (_movingRight) is reversed if looping is enabled and the curve end is reached.
		/// - The X position is determined by _curveCounter (normalized [0,1]), scaled to screen width.
		/// - The Y position is evaluated from the animation curve at _curveCounter, scaled to screen height.
		/// - If not looping, the watermark teleports to the start when reaching the end of the curve.
		/// This allows for custom, smooth, and potentially looping motion paths for the watermark.
		/// </summary>
		/// <returns>A Vector2 containing the (x, y) position in pixels for the watermark along the curve.</returns>
		private Vector2 GraphBasedPositionOffset()
		{
			// Move the curve counter forward or backward depending on direction
			if (_movingRight)
			{
				_curveCounter += 500 * _curveTraversalSpeed * (float)scaledTime / _screenSize.x;
			}
			else
			{
				_curveCounter -= 500 * _curveTraversalSpeed * (float)scaledTime / _screenSize.x;
			}

			// If the counter exceeds the right edge, handle looping or reset
			if (_curveCounter >= (_screenSize.x - _watermarkSize.x) / _screenSize.x)
			{
				if (_loop)
				{
					_movingRight = false; // Reverse direction for looping
				}
				else
				{
					_curveCounter = 0;    // Teleport to start if not looping
				}
			}
			// If the counter exceeds the left edge and looping is enabled, reverse direction
			if (_loop && _curveCounter <= 0)
			{
				_movingRight = true;
			}

			// Evaluate the curve at the current counter for Y, scale X and Y to screen size
			var val = _curve.Evaluate(_curveCounter);
			return new Vector2(_curveCounter * _screenSize.x, val * _screenSize.y);
		}

		/// <summary>
		/// Calculates a random position for the watermark, optionally lerping the movement.
		/// - If this is the first call, initializes the target and current positions.
		/// - If 'lerp' is true, moves the current position towards the target using linear interpolation (Lerp).
		///   The speed is controlled by _lerpSpeed and optionally deltatime.
		/// - When the current position is close enough to the target, a new random target position is chosen.
		/// This function is used for lerped random watermark movement.
		/// </summary>
		/// <returns>The current (x, y) position in pixels for the watermark.</returns>
		private Vector2 RandomPositionOffsetLerp()
		{
			// If this is the first call, set the initial target and current positions
			if (_targetPosition.x == float.NegativeInfinity || _targetPosition.y == float.NegativeInfinity)
			{
				_targetPosition = GetRandomPositionInArea();
				_currentPosition = Vector2.zero;
			}
			Vector3 velocity = Vector3.zero;

			// Use Lerp for linear interpolation
			if (_useDeltaTime)
			{
				_currentPosition = Vector2.Lerp(_currentPosition, _targetPosition, _lerpSpeed / 10);
			}
			else
			{
				_currentPosition = Vector2.Lerp(_currentPosition, _targetPosition, _lerpSpeed * (float)scaledTime);
			}

			// If close enough to the target, pick a new random target position
			if (Vector2.Distance(_currentPosition, _targetPosition) < 5f)
			{
				_targetPosition = GetRandomPositionInArea();
			}

			// Return the current position for the watermark
			return _currentPosition;
		}

		/// <summary>
		/// Returns a random position for the watermark within the allowed area, updating at a fixed interval.
		/// The position is updated only after a specified time (_rateOfChange) has elapsed since the last change.
		/// Until then, the previous random position is reused. This ensures the watermark "jumps" to a new
		/// random location at a regular rate, rather than moving smoothly.
		/// </summary>
		/// <returns>A Vector2 containing the (x, y) position in pixels for the watermark.</returns>
		private Vector2 RandomPositionOffset()
		{
			// If enough time has passed, pick a new random position and update the timer
			if (Time.time > _timeOfNextChange)
			{
				_timeOfNextChange = Time.time + _rateOfChange;
				_previousRandomPosition = GetRandomPositionInArea();
				return _previousRandomPosition;
			}
			else
			{
				// Otherwise, keep using the previous position
				return _previousRandomPosition;
			}
		}

		/// <summary>
		/// Generates a random position within the user-defined offset area, clamped to the screen bounds.
		/// The X and Y coordinates are randomly selected within the _offsetArea rectangle,
		/// but are also clamped so they do not exceed the screen size.
		/// This ensures the watermark appears only within the allowed area and never off-screen.
		/// </summary>
		/// <returns>A Vector2 containing the random (x, y) position in pixels within the valid area.</returns>
		private Vector2 GetRandomPositionInArea()
		{
			// Randomly select X within the allowed area, clamped to [0, screenSize.x]
			float xPos = UnityEngine.Random.Range(
				_offsetArea.xMin < 0 ? 0 : _offsetArea.xMin,
				_offsetArea.xMax > _screenSize.x ? _screenSize.x : _offsetArea.xMax
			);

			// Randomly select Y within the allowed area, clamped to [0, screenSize.y]
			float yPos = UnityEngine.Random.Range(
				_offsetArea.yMin < 0 ? 0 : _offsetArea.yMin,
				_offsetArea.yMax > _screenSize.y ? _screenSize.y : _offsetArea.yMax
			);

			// Return the random position as a Vector2
			return new Vector2(xPos, yPos);
		}

		/// <summary>
		/// Calculates the pixel offset for the watermark based on a selected anchor position.
		/// The anchor position is specified by the _anchorPosition field (0-8), corresponding to:
		/// 
		/// 0 = Top Left    | 3 = Top Center    | 6 = Top Right
		/// ----------------|-------------------|-----------------
		/// 1 = Middle Left | 4 = Middle Center | 7 = Middle Right
		/// ----------------|-------------------|-----------------
		/// 2 = Bottom Left | 5 = Bottom Center | 8 = Bottom Right
		/// 
		/// Returns a Vector2 containing the (x, y) offset in pixels, ensuring the watermark
		/// is placed at the correct anchor point on the screen.
		/// </summary>
		private Vector2 AnchorPositionOffset()
		{
			float offsetx = 0;
			float offsety = 0;

			// Select anchor position based on _anchorPosition index
			switch (_anchorPosition)
			{
				case 0: // Top Left
					offsetx = 0;
					offsety = _screenSize.y - _watermarkSize.y;
					break;
				case 1: // Middle Left
					offsetx = 0;
					offsety = (_screenSize.y - _watermarkSize.y) / 2;
					break;
				case 2: // Bottom Left
					offsetx = 0;
					offsety = 0;
					break;
				case 3: // Top Center
					offsetx = (_screenSize.x - _watermarkSize.x) / 2;
					offsety = _screenSize.y - _watermarkSize.y;
					break;
				case 4: // Middle Center
					offsetx = (_screenSize.x - _watermarkSize.x) / 2;
					offsety = (_screenSize.y - _watermarkSize.y) / 2;
					break;
				case 5: // Bottom Center
					offsetx = (_screenSize.x - _watermarkSize.x) / 2;
					offsety = 0;
					break;
				case 6: // Top Right
					offsetx = _screenSize.x - _watermarkSize.x;
					offsety = _screenSize.y - _watermarkSize.y;
					break;
				case 7: // Middle Right
					offsetx = _screenSize.x - _watermarkSize.x;
					offsety = (_screenSize.y - _watermarkSize.y) / 2;
					break;
				case 8: // Bottom Right
					offsetx = _screenSize.x - _watermarkSize.x;
					offsety = 0;
					break;
				default: // Default to Bottom Left if out of range
					offsetx = 0;
					offsety = 0;
					break;
			}

			// Return the calculated offset in pixels
			return new Vector2(offsetx, offsety);
		}

		/// <summary>
		/// Normalizes a Vector2 value relative to the current screen size.
		/// Each component is divided by the corresponding screen dimension,
		/// clamped to a maximum of 1.0. This is used to convert pixel-based
		/// coordinates or sizes into normalized [0,1] space for consistent
		/// rendering across different resolutions.
		/// </summary>
		/// <param name="values">The Vector2 value (e.g., size or position in pixels) to normalize.</param>
		/// <returns>A Vector2 where each component is in the range [0,1], relative to the screen size.</returns>
		private Vector2 NormalizeValue(Vector2 values)
		{
			// Normalize X: If value exceeds screen width, clamp to 1.0; otherwise, divide by width
			float xN = values.x > _screenSize.x ? 1f : values.x / _screenSize.x;
			// Normalize Y: If value exceeds screen height, clamp to 1.0; otherwise, divide by height
			float yN = values.y > _screenSize.y ? 1f : values.y / _screenSize.y;
			// Return the normalized value
			return new Vector2(xN, yN);
		}
	}
}
using SharpDX.XInput;
using System;

namespace XInputHelper
{
    /// <summary>
    /// Handles normalizing gamepad controller values, and adding a tolerance to readings
    /// </summary>
    internal class NormalizedGamepad
    {
        private const float XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE = 7849;
        private const float XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE = 8689;
        private const float XINPUT_GAMEPAD_TRIGGER_THRESHOLD = 30;

        private Gamepad pad;
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedGamepad"/> class.
        /// </summary>
        /// <param name="gamepad">The gamepad to get normalized values for.</param>
        public NormalizedGamepad(Gamepad gamepad)
        {
            pad = gamepad;
        }

        public float LeftThumbXNormalized
        {
            get
            {
                float LX = (float)pad.LeftThumbX;
                if (LX < 0f)
                {
                    LX = -LX;
                }
                if (LX <= XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE)
                {
                    LX = 0f;
                }
                else
                {
                    LX = LX - XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE;
                    LX = LX / (32767f - XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE);
                }
                if (pad.LeftThumbX >= 0)
                {
                    return LX;
                }
                return -LX;
            }
        }

        public float LeftThumbYNormalized
        {
            get
            {
                float LY = (float)pad.LeftThumbY;
                if (LY < 0f)
                {
                    LY = -LY;
                }
                if (LY <= XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE)
                {
                    LY = 0f;
                }
                else
                {
                    LY = LY - XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE;
                    LY = LY / (32767f - XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE);
                }
                if (pad.LeftThumbY >= 0)
                {
                    return LY;
                }
                return -LY;
            }
        }

        public float LeftTriggerNormalized
        {
            get
            {
                float t = (float)pad.LeftTrigger;
                if (t <= XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
                {
                    t = 0f;
                }
                else
                {
                    t = t - XINPUT_GAMEPAD_TRIGGER_THRESHOLD;
                    t = t / (255f - XINPUT_GAMEPAD_TRIGGER_THRESHOLD);
                }
                return t;
            }
        }

        public float RightThumbXNormalized
        {
            get
            {
                float LX = (float)pad.RightThumbX;
                if (LX < 0f)
                {
                    LX = -LX;
                }
                if (LX <= XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE)
                {
                    LX = 0f;
                }
                else
                {
                    LX = LX - XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE;
                    LX = LX / (32767 - XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE);
                }
                if (pad.RightThumbX >= 0)
                {
                    return LX;
                }
                return -LX;
            }
        }

        public float RightThumbYNormalized
        {
            get
            {
                float LY = (float)pad.RightThumbY;
                if (LY < 0f)
                {
                    LY = -LY;
                }
                if (LY <= XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE)
                {
                    LY = 0f;
                }
                else
                {
                    LY = LY - XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE;
                    LY = LY / (32767 - XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE); // 24078f;
                }
                if (pad.RightThumbY >= 0)
                {
                    return LY;
                }
                return -LY;
            }
        }

        public float RightTriggerNormalized
        {
            get
            {
                float t = (float)pad.RightTrigger;
                if (t <= XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
                {
                    t = 0f;
                }
                else
                {
                    t = t - XINPUT_GAMEPAD_TRIGGER_THRESHOLD;
                    t = t / (255f - XINPUT_GAMEPAD_TRIGGER_THRESHOLD);
                }
                return t;
            }
        }
    }
}
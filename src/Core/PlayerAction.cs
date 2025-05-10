using System;

namespace GameFramework
{
    public class PlayerAction
    {
        public BaseAction Action { get; }
        public int FrameNumber { get; }

        public PlayerAction(BaseAction action, int frameNumber)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "Action cannot be null.");
            }
            if (frameNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameNumber), "Frame number cannot be negative.");
            }
            Action = action;
            FrameNumber = frameNumber;
        }
    }
}

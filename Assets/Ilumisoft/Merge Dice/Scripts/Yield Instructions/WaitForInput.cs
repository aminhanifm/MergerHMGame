using UnityEngine;

namespace Ilumisoft.MergeDice
{
    /// <summary>
    /// Waits until the user clicks the left mouse button or touches the screen
    /// </summary>
    public class WaitForInput : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return !Input.GetMouseButtonDown(0);
            }
        }
    }
}
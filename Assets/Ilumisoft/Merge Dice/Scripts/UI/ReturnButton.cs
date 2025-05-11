using UnityEngine;
using UnityEngine.UI;

namespace Ilumisoft.MergeDice
{
    [RequireComponent(typeof(Button))]
    public class ReturnButton : MonoBehaviour
    {
        Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        void Update()
        {
            // Invokes the button click event when the ESC or Return button (mobile) is clicked
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                button.onClick.Invoke();
            }
        }
    }
}
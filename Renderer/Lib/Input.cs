using System.Windows.Forms;

namespace Renderer.Lib
{
    internal static class NativeMethods
    {
        public static bool IsControlKeyDown()
        {
            return (GetKeyState(VK_CONTROL) & KEY_PRESSED) != 0;
        }

        public static bool KeyState(Keys key)
        {
            return (GetKeyState((int)key) & KEY_PRESSED) != 0;
        }

        private const int KEY_PRESSED = 0x8000;
        private const int VK_CONTROL = 0x11;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int key);
    }

    public class Input
    {
        public bool GetKey(Keys key)
        {
            return NativeMethods.KeyState(key);
        }
    }
}
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Managers
{
    internal static class ControlsManager
    {
        public static bool[] keysPressed = new bool[349];

        public static void OnKeyboardDown(KeyboardKeyEventArgs e) 
        {
            keysPressed[(int)e.Key] = true;
        }
        public static void OnKeyboardUp(KeyboardKeyEventArgs e) 
        {
            keysPressed[(int)e.Key] = false;
        }
    }
}

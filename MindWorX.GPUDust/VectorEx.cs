using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MindWorX.GPUDust
{
    static class VectorEx
    {
        public static Vector2 Floor(this Vector2 vector)
        {
            return new Vector2((Single)Math.Floor(vector.X), (Single)Math.Floor(vector.Y));
        }
    }
}

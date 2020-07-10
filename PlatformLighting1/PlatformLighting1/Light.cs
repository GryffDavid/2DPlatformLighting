using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    public class Light
    {
        public Vector3 Position;
        public Color Color;
        public float Power, Size;
        public bool Active;

        public Light()
        {

        }
    }
}

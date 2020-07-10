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

        public float CurTime, MaxTime;

        public Light()
        {

        }

        public void Update(GameTime gameTime)
        {
            if (MaxTime > 0)
            {
                CurTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurTime > MaxTime && Power > 0)
                {
                    Power -= 0.15f;                    
                    //Power -= 0.01f;
                }

                if (CurTime > MaxTime && Power <= 0)
                {
                    Active = false;
                }
            }
        }
    }
}

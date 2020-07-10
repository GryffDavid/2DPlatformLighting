using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    class Sprite
    {
        public Vector2 Position;
        public Texture2D Texture, Normal, Emissive;
        public float Depth;
        
        public Sprite(Texture2D texture, Vector2 position, Texture2D normal, Texture2D emissive)
        {
            Position = position;
            Texture = texture;
            Normal = normal;
            Emissive = emissive;
            Depth = position.Y / 720f;
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {
            Position.X -= 2* (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Draw(Texture, Position, color);
        }

        public void DrawNormal(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Normal, Position, Color.White);
        }

        public void DrawEmissive(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Emissive, Position, new Color(255, 255, 255, 2));
        }
    }
}

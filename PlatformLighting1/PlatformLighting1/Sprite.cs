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
        Vector2 Position;
        Texture2D Texture;

        public Sprite(Texture2D texture, Vector2 position)
        {
            Position = position;
            Texture = texture;
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {
            Position.X += (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Draw(Texture, Position, color);
        }
    }
}

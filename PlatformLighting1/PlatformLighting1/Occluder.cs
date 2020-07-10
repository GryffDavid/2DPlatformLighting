using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    class Occluder
    {
        Texture2D Texture;
        Vector2 Position, Size;
        Rectangle DestinationRectangle;
        Color Color = Color.Red;

        public Occluder(Vector2 position, Vector2 size, Texture2D texture)
        {

        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, Color);
        }
    }
}

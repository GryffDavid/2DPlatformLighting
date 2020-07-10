using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    class Solid
    {
        public Texture2D Texture;
        Vector2 Position, Size;
        public Rectangle DestinationRectangle;
        BoundingBox BoundingRectangle;
        public VertexPositionColor[] vertices = new VertexPositionColor[4];
        PolygonShadow shadow;
        public float Depth;

        public Solid(Texture2D texture, Vector2 position, Vector2 size)
        {
            Texture = texture;
            Position = position;
            Size = size;
            Depth = Position.Y / 720f;
        }

        public void Update(GameTime gameTime)
        {
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            BoundingRectangle = new BoundingBox(new Vector3(Position.X, Position.Y, 0), new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0));

            vertices[0] = new VertexPositionColor() { Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0) };
            vertices[1] = new VertexPositionColor() { Position = new Vector3(DestinationRectangle.Right, DestinationRectangle.Top, 0) };
            vertices[2] = new VertexPositionColor() { Position = new Vector3(DestinationRectangle.Right, DestinationRectangle.Bottom, 0) };
            vertices[3] = new VertexPositionColor() { Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Bottom, 0) };
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, color);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    class PolygonShadow
    {
        public VertexPositionColor[] Vertices = new VertexPositionColor[6];

        public PolygonShadow()
        {
            Vertices[0] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(100, 100, 0) };
            Vertices[1] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(150, 100, 0) };
            Vertices[2] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(150, 150, 0) };

            Vertices[3] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(150, 150, 0) };
            Vertices[4] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(100, 150, 0) };
            Vertices[5] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(100, 100, 0) };

            //Vertices[6] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(150, 150, 0) };
            //Vertices[7] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(100, 150, 0) };
            //Vertices[8] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(100, 100, 0) };
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GraphicsDevice graphicDevice)
        {
            graphicDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vertices, 0, 2);
        }
    }
}

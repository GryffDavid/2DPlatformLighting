using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PlatformLighting1
{
    public class SolidCircle
    {
        public VertexPositionColor[] vertices = new VertexPositionColor[60];
        int[] indices = new int[60];
        List<Vector2> Points = new List<Vector2>();

        public SolidCircle()
        {
            for (float i = 0; i < Math.PI * 2; i += 0.3f)
            {
                Points.Add(new Vector2(1280/2, 200) + new Vector2((float)Math.Cos(i), (float)Math.Sin(i)) * 100);
            }

            //for (int i = 0; i < Points.Count; i++)
            //{
            //    vertices[i] = new VertexPositionColor() { Color = Color.Black, Position = new Vector3(Points[i].X, Points[i].Y, 0) };
            //}

            vertices[0] = new VertexPositionColor()
            {
                Position = new Vector3(200, 200, 0),
                Color = Color.Red
            };


            for (int i = 0; i < 21; i++)
            {
                vertices[i] = new VertexPositionColor()
                {
                    Position = new Vector3(Points[i].X, Points[i].Y, 0),
                    Color = Color.Red
                };
            }

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            for (int i = 3; i < 57; i += 3)
            {
                indices[i] = 0;
                indices[i + 1] = indices[i - 1];
                indices[i + 2] = indices[i - 1] + 1;
            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void LoadContent(ContentManager content)
        {
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphics, BasicEffect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, indices.Length, indices, 0, indices.Length/3);
            }
        }
    }
}

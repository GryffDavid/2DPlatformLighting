using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    public abstract class Drawable
    {
        public static Random Random = new Random();

        public bool Active;
        public float DrawDepth;
        public float PreviousMaxY;

        private float _MaxY;
        public float MaxY
        {
            get { return _MaxY; }
            set
            {
                if (_MaxY != PreviousMaxY)
                {
                    PreviousMaxY = _MaxY;
                }

                _MaxY = value;
                DrawDepth = MaxY / 1080.0f;
            }
        }

        public BlendState BlendState = BlendState.AlphaBlend;

        public BoundingBox CollisionBox;
        public BoundingBox BoundingBox;
        public BoundingSphere BoundingSphere;

        public float ZDepth = 16f; //The theoretical physical depth of the object in the non-existant Z dimension. Not DrawDepth though.
        public bool Emissive = false;
        public bool Normal = false;
        public bool Shadows = false;

        public Texture2D Texture;
        public Rectangle DestinationRectangle, SourceRectangle;
        public Color Color = Color.White;
        public Vector2 Position;

        public VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        public int[] indices = new int[6];

        //public virtual void Update(GameTime gameTime)
        //{

        //}

        public virtual void Initialize()
        {
            //DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y

            vertices[0] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(0, 0),
                Color = Color
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top, 0),
                TextureCoordinate = new Vector2(1, 0),
                Color = Color
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left + DestinationRectangle.Width, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(1, 1),
                Color = Color
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Position = new Vector3(DestinationRectangle.Left, DestinationRectangle.Top + DestinationRectangle.Height, 0),
                TextureCoordinate = new Vector2(0, 1),
                Color = Color
            };

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        //public virtual void Update()
        //{
        //    DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Texture.Width, (int)Texture.Height);
        //}

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}

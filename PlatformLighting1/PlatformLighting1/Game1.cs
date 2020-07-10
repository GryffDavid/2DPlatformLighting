using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PlatformLighting1
{    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D EmissiveMap, BlurMap, ColorMap, NormalMap, LightMap;

        VertexPositionColorTexture[] Vertices;
        VertexBuffer VertexBuffer;

        #region Sprites
        Texture2D Sprite, Texture, NormalTexture;
        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        #endregion

        List<Light> LightList = new List<Light>();

        Color AmbientLight = new Color(0.3f, 0.3f, 0.3f, 1f);
        private float specularStrength = 1.0f;

        public static BlendState BlendBlack = new BlendState()
        {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,

            AlphaBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One
        };


        Matrix Projection = Matrix.CreateOrthographicOffCenter(0, 1280, 720, 0, -10, 10);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
        }
        
        protected override void Initialize()
        {

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            EmissiveMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            BlurMap = new RenderTarget2D(GraphicsDevice, 1280, 720); 
            ColorMap = new RenderTarget2D(GraphicsDevice, 1280, 720); 
            NormalMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            LightMap = new RenderTarget2D(GraphicsDevice, 1280, 720);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Sprite = Content.Load<Texture2D>("Sprite");

            BlurEffect = Content.Load<Effect>("Blur");
            LightCombined = Content.Load<Effect>("LightCombined");
            LightEffect = Content.Load<Effect>("LightEffect");

            BlurEffect.Parameters["Projection"].SetValue(Projection);

            Vertices = new VertexPositionColorTexture[4];
            Vertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            Vertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
            Vertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            Vertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));
            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), Vertices.Length, BufferUsage.None);
            VertexBuffer.SetData(Vertices);

            //Vertices2 = Vertices1;
            //VertexBuffer2 = VertexBuffer1;

            Texture = Content.Load<Texture2D>("Texture");
            NormalTexture = Content.Load<Texture2D>("NormalTexture");

            LightList.Add(new Light()
            {
                Color = Color.White,
                Active = true,
                LightDecay = 800,
                Power = 0.001f,
                Position = new Vector3(100, 100, 200)
            });
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            Vector3 LightPos;

            LightPos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 100f);
            LightList[0].Position = LightPos;

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            #region Draw to ColorMap
            GraphicsDevice.SetRenderTarget(ColorMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(Texture, Texture.Bounds, Color.White);
            spriteBatch.End();
            #endregion

            #region Draw to NormalMap
            GraphicsDevice.SetRenderTarget(NormalMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(NormalTexture, NormalTexture.Bounds, Color.White);
            spriteBatch.End();
            #endregion
                        
            #region Draw to LightMap
            GraphicsDevice.SetRenderTarget(LightMap);
            GraphicsDevice.Clear(Color.Transparent);

            foreach (Light light in LightList)
            {
                GraphicsDevice.SetVertexBuffer(VertexBuffer);

                LightEffect.Parameters["lightStrength"].SetValue(light.Power);
                LightEffect.Parameters["lightPosition"].SetValue(light.Position);
                LightEffect.Parameters["lightColor"].SetValue(ColorToVector(light.Color));
                LightEffect.Parameters["lightDecay"].SetValue(light.LightDecay);
                LightEffect.Parameters["specularStrength"].SetValue(specularStrength);

                LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];

                LightEffect.Parameters["screenWidth"].SetValue(1280);
                LightEffect.Parameters["screenHeight"].SetValue(720);

                LightEffect.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

                LightEffect.Parameters["NormalMap"].SetValue(NormalMap);
                LightEffect.Parameters["ColorMap"].SetValue(ColorMap);
                LightEffect.CurrentTechnique.Passes[0].Apply();

                GraphicsDevice.BlendState = BlendBlack;

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Vertices, 0, 2);
            }

            #endregion

            //Draw everything to the backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, LightCombined);
            //RaysEffect.CurrentTechnique.Passes[0].Apply();
            //spriteBatch.Draw(LightShadowMap, Vector2.Zero, Color.White);

            #region Draw the lightmap and color map combined
            LightCombined.CurrentTechnique = LightCombined.Techniques["DeferredCombined2"];
            LightCombined.Parameters["ambient"].SetValue(1f);
            LightCombined.Parameters["lightAmbient"].SetValue(4f);
            LightCombined.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

            LightCombined.Parameters["ColorMap"].SetValue(ColorMap);
            LightCombined.Parameters["ShadingMap"].SetValue(LightMap);
            LightCombined.Parameters["NormalMap"].SetValue(NormalMap);

            LightCombined.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(ColorMap, Vector2.Zero, Color.White);
            #endregion
            spriteBatch.End();

            base.Draw(gameTime);
        }


        protected Vector4 ColorToVector(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }
    }
}

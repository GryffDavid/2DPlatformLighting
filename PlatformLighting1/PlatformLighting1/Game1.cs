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

        VertexPositionColorTexture[] Vertices1;
        VertexBuffer VertexBuffer1;

        VertexPositionColorTexture[] Vertices2;
        VertexBuffer VertexBuffer2;

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

            Vertices1 = new VertexPositionColorTexture[6];
            Vertices1[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            Vertices1[1] = new VertexPositionColorTexture(new Vector3(1280, 0, 0), Color.White, new Vector2(1, 0));
            Vertices1[2] = new VertexPositionColorTexture(new Vector3(1280, 720, 0), Color.White, new Vector2(1, 1));
            Vertices1[3] = new VertexPositionColorTexture(new Vector3(1280, 720, 0), Color.White, new Vector2(1, 1));
            Vertices1[4] = new VertexPositionColorTexture(new Vector3(0, 720, 0), Color.White, new Vector2(0, 1));
            Vertices1[5] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            VertexBuffer1 = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), Vertices1.Length, BufferUsage.None);
            VertexBuffer1.SetData(Vertices1);

            Vertices2 = Vertices1;
            VertexBuffer2 = VertexBuffer1;

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
            #region Draw to emissive map
            GraphicsDevice.SetRenderTarget(EmissiveMap);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(Sprite, new Vector2(100, 100), Color.White);
            spriteBatch.End(); 
            #endregion

            #region Blur
            GraphicsDevice.SetRenderTarget(BlurMap);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetVertexBuffer(VertexBuffer1);
            BlurEffect.Parameters["InputTexture"].SetValue(EmissiveMap);
            BlurEffect.CurrentTechnique = BlurEffect.Techniques["Technique1"];

            foreach (EffectPass pass in BlurEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Vertices1, 0, 2);
            } 
            #endregion
            
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
                GraphicsDevice.SetVertexBuffer(VertexBuffer2);

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

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Vertices2, 0, 2);
            }

            #endregion

            #region Draw to backbuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, LightCombined);

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

                //spriteBatch.Draw(EmissiveMap, EmissiveMap.Bounds, Color.White);
                //spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            spriteBatch.End();
            #endregion

            base.Draw(gameTime);
        }


        protected Vector4 ColorToVector(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }
    }
}

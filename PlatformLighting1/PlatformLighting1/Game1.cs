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

        RenderTarget2D EmissiveMap, BlurMap, ColorMap, NormalMap, LightMap, FinalMap, FinalMap2, SpecMap, DepthMap;
        RenderTarget2D CrepLightMap, CrepColorMap;

        VertexPositionColorTexture[] LightVertices;
        VertexBuffer LightVertexBuffer;

        VertexPositionColorTexture[] EmissiveVertices;
        VertexBuffer EmissiveVertexBuffer;

        static Random Random = new Random();

        #region Sprites
        Texture2D Sprite, HealDrone, HealDroneEmissive, HealDroneNormal, Texture, NormalTexture;
        Texture2D CrepuscularLightTexture;
        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        Effect RaysEffect;
        #endregion

        List<Light> LightList = new List<Light>();

        Color AmbientLight = new Color(0.25f, 0.25f, 0.25f, 1f);
        private float specularStrength = 1.0f;

        List<Sprite> SpriteList = new List<Sprite>();

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
            FinalMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            FinalMap2 = new RenderTarget2D(GraphicsDevice, 1280, 720);
            SpecMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            CrepLightMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            CrepColorMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            DepthMap = new RenderTarget2D(GraphicsDevice, 1280, 720);


            spriteBatch = new SpriteBatch(GraphicsDevice);

            Sprite = Content.Load<Texture2D>("Sprite");
            CrepuscularLightTexture = Content.Load<Texture2D>("Flare1");
            HealDrone = Content.Load<Texture2D>("HealDrone");
            HealDroneNormal = Content.Load<Texture2D>("HealDroneNormal");
            HealDroneEmissive = Content.Load<Texture2D>("HealDroneEmissive");

            SpriteList.Add(new Sprite(HealDrone, new Vector2(1280 / 2 - 32, 720 / 2 - 32), HealDroneNormal, HealDroneEmissive));

            BlurEffect = Content.Load<Effect>("Blur");
            LightCombined = Content.Load<Effect>("LightCombined");
            LightEffect = Content.Load<Effect>("LightEffect");
            LightEffect.Parameters["AmbientColor"].SetValue(new Vector4(0.0015f, 0.0015f, 0.0015f, 1.0f));



            RaysEffect = Content.Load<Effect>("Crepuscular");
            
            RaysEffect.Parameters["Projection"].SetValue(Projection);
            BlurEffect.Parameters["Projection"].SetValue(Projection);

            LightVertices = new VertexPositionColorTexture[4];
            LightVertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            LightVertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
            LightVertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            LightVertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));
            LightVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), LightVertices.Length, BufferUsage.None);
            LightVertexBuffer.SetData(LightVertices);

            EmissiveVertices = new VertexPositionColorTexture[6];
            EmissiveVertices[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            EmissiveVertices[1] = new VertexPositionColorTexture(new Vector3(1280, 0, 0), Color.White, new Vector2(1, 0));
            EmissiveVertices[2] = new VertexPositionColorTexture(new Vector3(1280, 720, 0), Color.White, new Vector2(1, 1));
            EmissiveVertices[3] = new VertexPositionColorTexture(new Vector3(1280, 720, 0), Color.White, new Vector2(1, 1));
            EmissiveVertices[4] = new VertexPositionColorTexture(new Vector3(0, 720, 0), Color.White, new Vector2(0, 1));
            EmissiveVertices[5] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            EmissiveVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), EmissiveVertices.Length, BufferUsage.None);
            EmissiveVertexBuffer.SetData(EmissiveVertices);

            Texture = Content.Load<Texture2D>("Texture");
            NormalTexture = Content.Load<Texture2D>("NormalTexture");

            LightList.Add(new Light()
            {
                Color = Color.HotPink,
                Active = true,
                Power = 0.87f,
                Position = new Vector3(100, 100, 250),
                Size = 800
            });

            LightList.Add(new Light()
            {
                Color = Color.White,
                Active = true,
                Power = 0.7f,
                Position = new Vector3(100, 100, 250),
                Size = 1200
            });

            LightList.Add(new Light()
            {
                Color = Color.Green,
                Active = true,
                Power = 1.2f,
                Position = new Vector3(800, 350, 250),
                Size = 400
            });

            LightList.Add(new Light()
            {
                Color = Color.DeepSkyBlue,
                Active = true,
                Power = 0.2f,
                Position = new Vector3(500, 650, 100),
                Size = 1200
            });


            for (int i = 0; i < 30; i++)
            {
                LightList.Add(new Light()
                {
                    Color = new Color(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255)),
                    Active = true,
                    Power = (float)Random.NextDouble(),
                    Position = new Vector3(Random.Next(0, 1280), Random.Next(0, 720), Random.Next(10, 400)),
                    Size = Random.Next(50, 600)
                });
            }
        }

        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            Vector3 LightPos;

            LightPos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 25);
            LightList[0].Position = LightPos;

            foreach (Sprite sprite in SpriteList)
            {
                sprite.Update(gameTime);
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            #region Emissive
            #region Draw to emissive map
            GraphicsDevice.SetRenderTarget(EmissiveMap);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(Sprite, new Vector2(100, 100), Color.White);
            foreach (Sprite sprite in SpriteList)
            {
                sprite.DrawEmissive(spriteBatch);
            }
            spriteBatch.End();
            #endregion
            
            #region Blur
            GraphicsDevice.SetRenderTarget(BlurMap);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetVertexBuffer(EmissiveVertexBuffer);
            BlurEffect.Parameters["InputTexture"].SetValue(EmissiveMap);
            BlurEffect.CurrentTechnique = BlurEffect.Techniques["Technique1"];

            foreach (EffectPass pass in BlurEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, EmissiveVertices, 0, 2);
            }
            #endregion
            #endregion            

            #region Draw to ColorMap
            GraphicsDevice.SetRenderTarget(ColorMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(Texture, Texture.Bounds, Color.White);
            foreach (Sprite sprite in SpriteList)
            {
                sprite.Draw(spriteBatch, Color.White);
            }
            spriteBatch.End();
            #endregion

            #region Draw to NormalMap
            GraphicsDevice.SetRenderTarget(NormalMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();

            spriteBatch.Draw(NormalTexture, NormalTexture.Bounds, Color.White);
            spriteBatch.Draw(Sprite, new Vector2(100, 100), Color.Black);
            foreach (Sprite sprite in SpriteList)
            {
                sprite.DrawNormal(spriteBatch);
            }
            spriteBatch.End();
            #endregion

            #region Draw to SpecMap
            GraphicsDevice.SetRenderTarget(SpecMap);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.End();
            #endregion

            #region Draw to DepthMap
            GraphicsDevice.SetRenderTarget(DepthMap);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            spriteBatch.End();
            #endregion

            #region Draw to LightMap
            GraphicsDevice.SetRenderTarget(LightMap);
            GraphicsDevice.Clear(Color.Transparent);

            foreach (Light light in LightList)
            {
                GraphicsDevice.SetVertexBuffer(LightVertexBuffer);

                LightEffect.Parameters["LightPosition"].SetValue(light.Position);
                LightEffect.Parameters["LightColor"].SetValue(ColorToVector(light.Color));
                LightEffect.Parameters["LightPower"].SetValue(light.Power);
                LightEffect.Parameters["LightSize"].SetValue(light.Size);



                LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];
                
                //LightEffect.Parameters["ambientColor"].SetValue(AmbientLight.ToVector4());

                LightEffect.Parameters["NormalMap"].SetValue(NormalMap);
                LightEffect.Parameters["ColorMap"].SetValue(ColorMap);
                LightEffect.CurrentTechnique.Passes[0].Apply();

                GraphicsDevice.BlendState = BlendBlack;

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, LightVertices, 0, 2);
            }

            //spriteBatch.Begin();
            //foreach (Light light in LightList)
            //{
            //    foreach (Sprite sprite in SpriteList)
            //    {
            //        Vector2 position;
            //        float dist = Vector2.Distance(sprite.Position + new Vector2(HealDrone.Width / 2, HealDrone.Height / 2), new Vector2(light.Position.X, light.Position.Y));
            //        Vector2 direction = sprite.Position - new Vector2(light.Position.X, light.Position.Y);
            //        direction.Normalize();

            //        position = sprite.Position + (direction * dist);

            //        spriteBatch.Draw(HealDrone, position, null, Color.Black * 0.5f, 0, Vector2.Zero, MathHelper.Clamp(dist * 0.01f, 1.0f, float.PositiveInfinity), SpriteEffects.None, 0);
            //    }
            //}
            //spriteBatch.End();

            #endregion
            
            #region Combine Normals, Lighting and Color
            GraphicsDevice.SetRenderTarget(FinalMap);
            GraphicsDevice.Clear(Color.CornflowerBlue);

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

            
            spriteBatch.End(); 
            #endregion

            #region Crepuscular LightMap
            GraphicsDevice.SetRenderTarget(CrepLightMap);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(CrepuscularLightTexture, Vector2.Zero, Color.White);
            spriteBatch.Draw(ColorMap, ColorMap.Bounds, Color.Black);
            foreach (Sprite sprite in SpriteList)
            {
                sprite.Draw(spriteBatch, Color.Black);
            }
            spriteBatch.End();
            #endregion

            GraphicsDevice.SetRenderTarget(FinalMap2);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            
            spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
            spriteBatch.Draw(EmissiveMap, ColorMap.Bounds, Color.White);
            spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            
            spriteBatch.End();

            #region Crepuscular ColorMap
            GraphicsDevice.SetRenderTarget(CrepColorMap);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(FinalMap2, FinalMap2.Bounds, Color.White);

            //foreach (Sprite sprite in SpriteList)
            //{
            //    sprite.Draw(spriteBatch, Color.White);
            //}
            spriteBatch.End(); 
            #endregion

            #region Draw to the BackBuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            RaysEffect.Parameters["ColorMap"].SetValue(CrepColorMap);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            //spriteBatch.Draw(EmissiveMap, ColorMap.Bounds, Color.White);
            //spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            RaysEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CrepLightMap, FinalMap.Bounds, Color.White);
            
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

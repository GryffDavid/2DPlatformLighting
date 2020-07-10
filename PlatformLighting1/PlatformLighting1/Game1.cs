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

        RenderTarget2D EmissiveMap, BlurMap, ColorMap, NormalMap, LightMap, FinalMap, FinalMap2, SpecMap, DepthMap, ShadowMap, ShadowMap2;
        RenderTarget2D CrepLightMap, CrepColorMap;

        VertexPositionColorTexture[] LightVertices;
        VertexBuffer LightVertexBuffer;

        VertexPositionColorTexture[] EmissiveVertices;
        VertexBuffer EmissiveVertexBuffer;

        static Random Random = new Random();

        #region Sprites
        Texture2D Sprite, HealDrone, HealDroneEmissive, HealDroneNormal, Texture, NormalTexture;
        Texture2D CrepuscularLightTexture;
        Texture2D BoxTexture;
        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        Effect RaysEffect;
        #endregion

        List<Light> LightList = new List<Light>();

        Color AmbientLight = new Color(0.11f, 0.11f, 0.11f, 1f);

        List<Sprite> SpriteList = new List<Sprite>();
        List<Solid> SolidList = new List<Solid>();
        List<PolygonShadow> ShadowList = new List<PolygonShadow>();
        List<myRay> RayList = new List<myRay>();

        BasicEffect BasicEffect;

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

            ShadowMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            ShadowMap2 = new RenderTarget2D(GraphicsDevice, 1280, 720);

            BasicEffect = new BasicEffect(GraphicsDevice);
            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, 1280, 720, 0, 0, 10);
            BasicEffect.VertexColorEnabled = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Sprite = Content.Load<Texture2D>("Sprite");
            CrepuscularLightTexture = Content.Load<Texture2D>("Flare1");
            HealDrone = Content.Load<Texture2D>("HealDrone");
            HealDroneNormal = Content.Load<Texture2D>("HealDroneNormal");
            HealDroneEmissive = Content.Load<Texture2D>("HealDroneEmissive");
            BoxTexture = Content.Load<Texture2D>("Box");

            SolidList.Add(new Solid(BoxTexture, new Vector2(250, 250), new Vector2(80, 80)));
            SolidList.Add(new Solid(BoxTexture, new Vector2(100, 250), new Vector2(50, 20)));
            SolidList.Add(new Solid(BoxTexture, new Vector2(500, 400), new Vector2(120, 40)));
            SolidList.Add(new Solid(BoxTexture, new Vector2(1000, 500), new Vector2(70, 80)));

            //for (int i = 0; i < 20; i++)
            //{
            //    SolidList.Add(new Solid(BoxTexture, new Vector2(Random.Next(0, 1280), Random.Next(0, 720)), new Vector2(Random.Next(16, 128), Random.Next(16, 128))));
            //}

            for (int i = 0; i < 40; i++)
            {
                SolidList.Add(new Solid(BoxTexture, new Vector2(150+(24*i), 250), new Vector2(8, 32)));
            }

            SpriteList.Add(new Sprite(HealDrone, new Vector2(1280 / 2 - 32, 720 / 2 - 32), HealDroneNormal, HealDroneEmissive));

            BlurEffect = Content.Load<Effect>("Blur");
            LightCombined = Content.Load<Effect>("LightCombined");
            LightEffect = Content.Load<Effect>("LightEffect");

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
                //Color = new Color(141, 38, 10, 42),
                Color = Color.CornflowerBlue,
                Active = true,
                Power = 1.8f,
                Position = new Vector3(100, 100, 100),
                Size = 400
            });

            LightList.Add(new Light()
            {
                Color = new Color(141, 38, 10, 42),
                //Color = Color.White,
                Active = true,
                Power = 1.8f,
                Position = new Vector3(200, 100, 100),
                Size = 800
            });

            //LightList.Add(new Light()
            //{
            //    Color = Color.White,
            //    //Color = Color.White,
            //    Active = true,
            //    Power = 1.0f,
            //    Position = new Vector3(700, 350, 100),
            //    Size = 600
            //});
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            LightList[0].Position = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 250);

            foreach (Sprite sprite in SpriteList)
            {
                sprite.Update(gameTime);
            }

            foreach (Solid solid in SolidList)
            {
                solid.Update(gameTime);
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
                LightEffect.Parameters["NormalMap"].SetValue(NormalMap);
                LightEffect.Parameters["ColorMap"].SetValue(ColorMap);

                LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];
                LightEffect.CurrentTechnique.Passes[0].Apply();                
                GraphicsDevice.BlendState = BlendBlack;

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, LightVertices, 0, 2);

                //DrawShadows(light);

                //GraphicsDevice.BlendState = PSBlendState.Multiply;
                //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                //BasicEffect.Techniques[0].Passes[0].Apply();
                //foreach (PolygonShadow shadow in ShadowList)
                //{
                //    shadow.Draw(GraphicsDevice);
                //}
                
            }

            
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
            foreach (Solid solid in SolidList)
            {
                solid.Draw(spriteBatch);
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

            foreach (Solid solid in SolidList)
            {
                solid.Draw(spriteBatch);
            }
            spriteBatch.End(); 
            #endregion

            #region Draw to the BackBuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            RaysEffect.Parameters["ColorMap"].SetValue(CrepColorMap);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            RaysEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(CrepLightMap, CrepLightMap.Bounds, Color.White);

            spriteBatch.End();
            #endregion
            
            base.Draw(gameTime);
        }


        protected Vector4 ColorToVector(Color color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }

        public class myRay
        {
            public Vector3 position, direction;
            public float length;
        }

        public static int Wrap(int index, int n)
        {
            return ((index % n) + n) % n;
        }

        public void DrawShadows(Light light)
        {
            Vector2 SourcePosition = new Vector2(light.Position.X, light.Position.Y);

            RayList.Clear();
            ShadowList.Clear();

            foreach (Solid solid in SolidList)
            {
                Vector3 lightVector, check1, check2, thing, thing2;

                for (int i = 0; i < solid.vertices.Count(); i++)
                {
                    lightVector = solid.vertices[i].Position - new Vector3(SourcePosition, 0);

                    int nextIndex, prevIndex;

                    nextIndex = Wrap(i + 1, 4);
                    prevIndex = Wrap(i - 1, 4);

                    check1 = solid.vertices[nextIndex].Position - new Vector3(SourcePosition, 0);
                    check2 = solid.vertices[prevIndex].Position - new Vector3(SourcePosition, 0);

                    thing = Vector3.Cross(lightVector, check1);
                    thing2 = Vector3.Cross(lightVector, check2);

                    if ((thing.Z <= 0 && thing2.Z <= 0) ||
                        (thing.Z >= 0 && thing2.Z >= 0))
                    {
                        RayList.Add(new myRay() { direction = lightVector, position = solid.vertices[i].Position, length = 10f });
                    }
                }

                if (RayList.Count > 1)
                {
                    int p = RayList.Count() - 2;

                    VertexPositionColor[] vertices = new VertexPositionColor[6];

                    vertices[0].Position = RayList[p].position;
                    vertices[1].Position = RayList[p].position + (RayList[p].direction * 1000);
                    vertices[2].Position = RayList[p + 1].position + (RayList[p + 1].direction * 1000);

                    vertices[3].Position = RayList[p + 1].position + (RayList[p + 1].direction * 1000);
                    vertices[4].Position = RayList[p + 1].position;
                    vertices[5].Position = RayList[p].position;

                    vertices[0].Color = Color.Black;
                    vertices[1].Color = Color.Black;
                    vertices[2].Color = Color.Black;
                    vertices[3].Color = Color.Black;
                    vertices[4].Color = Color.Black;
                    vertices[5].Color = Color.Black;

                    //vertices[0].Color = Color.Red;
                    //vertices[1].Color = Color.Red;
                    //vertices[2].Color = Color.Red;
                    //vertices[3].Color = Color.Red;
                    //vertices[4].Color = Color.Red;
                    //vertices[5].Color = Color.Red;

                    //vertices[0].Color = Color.White;
                    //vertices[1].Color = Color.White;
                    //vertices[2].Color = Color.White;
                    //vertices[3].Color = Color.White;
                    //vertices[4].Color = Color.White;
                    //vertices[5].Color = Color.White;

                    ShadowList.Add(new PolygonShadow() { Vertices = vertices });
                }
            }
        }

        private void ClearAlphaToOne()
        {
            BlendState b1 = new BlendState() { ColorWriteChannels = ColorWriteChannels.Alpha };
            BlendState b2 = new BlendState() { ColorWriteChannels = ColorWriteChannels.All };

            GraphicsDevice.BlendState = b1;
            spriteBatch.Begin();
            spriteBatch.Draw(BoxTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
            GraphicsDevice.BlendState = b2;
        }


        public static class PSBlendState
        {
            public static BlendState Multiply = new BlendState
            {
                ColorSourceBlend = Blend.DestinationColor,
                ColorDestinationBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add
            };
            public static BlendState Screen = new BlendState
            {
                ColorSourceBlend = Blend.InverseDestinationColor,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add
            };
            public static BlendState Darken = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Min
            };
            public static BlendState Lighten = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Max
            };
        } 
    }
}

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
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using PlatformLighting1.PhysicsObjects;

namespace PlatformLighting1
{    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D EmissiveMap, BlurMap, ColorMap, NormalMap, LightMap, FinalMap, SpecMap, DepthMap, ShadowMap;
        RenderTarget2D CrepLightMap, CrepColorMap, OcclusionMap;
        RenderTarget2D Buffer1, Buffer2;

        VertexPositionColorTexture[] LightVertices;
        VertexPositionColorTexture[] EmissiveVertices;
        VertexPositionColorTexture[] CrepVertices;
        
        World World;
        DrawablePhysicsObject Floor;
        List<DrawablePhysicsObject> CrateList;

        List<ToonLightning> LightningList = new List<ToonLightning>();   

        #region Sprites
        Texture2D Sprite, HealDrone, HealDroneEmissive, HealDroneNormal, Texture, NormalTexture;
        Texture2D CrepuscularLightTexture;
        Texture2D BoxTexture;
        Texture2D Laser;
        #endregion

        #region Effects
        Effect BlurEffect, LightCombined, LightEffect;
        Effect RaysEffect, DepthEffect;
        #endregion

        List<Light> LightList = new List<Light>();

        Color AmbientLight = new Color(0.1f, 0.1f, 0.1f, 1f);
    
        List<Sprite> SpriteList = new List<Sprite>();
        List<Solid> SolidList = new List<Solid>();
        List<PolygonShadow> ShadowList = new List<PolygonShadow>();
        List<myRay> RayList = new List<myRay>();

        BasicEffect BasicEffect;

        KeyboardState PreviousKeyboardState, CurrentKeyboardState;

        Vector2 SpritePos;

        List<Emitter> EmitterList = new List<Emitter>();
        Texture2D HitEffectParticle, Glowball;

        float ShotTime, CurShotTime;
        float SpecVal = 0.005f;

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
        static Random Random = new Random();

        List<CrepuscularLight> CrepLightList = new List<CrepuscularLight>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //graphics.IsFullScreen = true;
        }
        
        protected override void Initialize()
        {
            SpritePos = new Vector2(100, 100);
            ShotTime = 150;
            CurShotTime = 0;
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            CrepLightList.Add(new CrepuscularLight() 
            { 
                Position = new Vector2(1280/2, 720/2), 
                Decay = 0.9999f, 
                Exposure = 0.23f, 
                Density = 0.826f, 
                Weight = 0.358767f 
            });

            CrepLightList.Add(new CrepuscularLight()
            {
                Position = new Vector2(1280 / 2, 720 / 2),
                Decay = 0.9999f,
                Exposure = 0.23f,
                Density = 0.826f,
                Weight = 0.358767f
            });

            //for (int i = 0; i < 5; i++)
            //{
            //    CrepLightList.Add(new CrepuscularLight()
            //    {
            //        Position = new Vector2(Random.Next(0, 1280), Random.Next(0, 720)),
            //        Decay = 0.9999f,
            //        Exposure = 0.23f,
            //        Density = 0.826f,
            //        Weight = 0.358767f
            //    });
            //}

            HitEffectParticle = Content.Load<Texture2D>("HitEffectParticle");
            Glowball = Content.Load<Texture2D>("Glowball");

            Buffer2 = new RenderTarget2D(GraphicsDevice, 1280, 720, false, SurfaceFormat.Rgba64, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
            Buffer1 = new RenderTarget2D(GraphicsDevice, 1280, 720);

            OcclusionMap = new RenderTarget2D(GraphicsDevice, 1280, 720);

            EmissiveMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            BlurMap = new RenderTarget2D(GraphicsDevice, 1280, 720); 
            ColorMap = new RenderTarget2D(GraphicsDevice, 1280, 720); 
            NormalMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            LightMap = new RenderTarget2D(GraphicsDevice, 1280, 720, false, SurfaceFormat.Rgba64, DepthFormat.None, 8, RenderTargetUsage.PreserveContents);

            FinalMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            SpecMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            CrepLightMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            CrepColorMap = new RenderTarget2D(GraphicsDevice, 1280, 720);
            DepthMap = new RenderTarget2D(GraphicsDevice, 1280, 720);

            ShadowMap = new RenderTarget2D(GraphicsDevice, 1280, 720);//, false, SurfaceFormat.Rgba64, DepthFormat.None, 8, RenderTargetUsage.PreserveContents);

            BasicEffect = new BasicEffect(GraphicsDevice);
            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, 1280, 720, 0, 0, 1);
            BasicEffect.VertexColorEnabled = true;

            DepthEffect = Content.Load<Effect>("Depth");
            DepthEffect.Parameters["Projection"].SetValue(Projection);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Sprite = Content.Load<Texture2D>("Sprite");
            CrepuscularLightTexture = Content.Load<Texture2D>("Flare1");
            HealDrone = Content.Load<Texture2D>("HealDrone");
            HealDroneNormal = Content.Load<Texture2D>("HealDroneNormal");
            HealDroneEmissive = Content.Load<Texture2D>("HealDroneEmissive");
            BoxTexture = Content.Load<Texture2D>("Box");
            Laser = Content.Load<Texture2D>("Beam");

            SolidList.Add(new Solid(BoxTexture, new Vector2(400, 300), new Vector2(80, 80)));
            SolidList.Add(new Solid(BoxTexture, new Vector2(100, 250), new Vector2(50, 20)));
            SolidList.Add(new Solid(BoxTexture, new Vector2(500, 400), new Vector2(120, 40)));
            SolidList.Add(new Solid(BoxTexture, new Vector2(1000, 500), new Vector2(70, 80)));
            
            for (int i = 0; i < 40; i++)
            {
                SolidList.Add(new Solid(BoxTexture, new Vector2(150 + (24 * i), 250), new Vector2(4, 32)));
            }

            SpriteList.Add(new Sprite(HealDrone, new Vector2(1280 / 2 - 32, 720 / 2 - 32), HealDroneNormal, HealDroneEmissive));
            
            SpriteList.Add(new Sprite(HealDrone, new Vector2(100, 100), HealDroneNormal, HealDroneEmissive));
            SpriteList.Add(new Sprite(HealDrone, new Vector2(800, 500), HealDroneNormal, HealDroneEmissive));

            
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

            CrepVertices = new VertexPositionColorTexture[4];
            CrepVertices[0] = new VertexPositionColorTexture(new Vector3(-1, 1, 0), Color.White, new Vector2(0, 0));
            CrepVertices[1] = new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 0));
            CrepVertices[2] = new VertexPositionColorTexture(new Vector3(-1, -1, 0), Color.White, new Vector2(0, 1));
            CrepVertices[3] = new VertexPositionColorTexture(new Vector3(1, -1, 0), Color.White, new Vector2(1, 1));

            EmissiveVertices = new VertexPositionColorTexture[6];
            EmissiveVertices[0] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));
            EmissiveVertices[1] = new VertexPositionColorTexture(new Vector3(1280, 0, 0), Color.White, new Vector2(1, 0));
            EmissiveVertices[2] = new VertexPositionColorTexture(new Vector3(1280, 720, 0), Color.White, new Vector2(1, 1));
            EmissiveVertices[3] = new VertexPositionColorTexture(new Vector3(1280, 720, 0), Color.White, new Vector2(1, 1));
            EmissiveVertices[4] = new VertexPositionColorTexture(new Vector3(0, 720, 0), Color.White, new Vector2(0, 1));
            EmissiveVertices[5] = new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0));

            Texture = Content.Load<Texture2D>("Texture");
            NormalTexture = Content.Load<Texture2D>("NormalTexture");

            LightList.Add(new Light()
            {
                //Color = new Color(141, 38, 10, 42),
                //Color = new Color(10, 25, 70, 5),
                //Color = Color.LightGreen,
                Color = Color.Plum,
                Active = true,
                Power = 0.7f,
                Position = new Vector3(100, 100, 100),
                Size = 800
            });

            LightList.Add(new Light()
            {
                //Color = new Color(141, 38, 10, 42),
                //Color = new Color(10, 25, 70, 5),
                //Color = Color.DarkSeaGreen,
                Color = Color.Silver,
                //Color = Color.Plum,
                //Color = new Color(141, 38, 10, 42),
                //Color = Color.White,
                Active = true,
                Power = 0.8f,
                Position = new Vector3(200, 100, 100),
                Size = 400
            });

            //LightList.Add(new Light()
            //{
            //    //Color = new Color(141, 38, 10, 42),
            //    Color = Color.White,
            //    Active = true,
            //    Power = 1.8f,
            //    Position = new Vector3(500, 600, 100),
            //    Size = 600
            //});

            //LightList.Add(new Light()
            //{
            //    //Color = new Color(141, 38, 10, 42),
            //    Color = Color.LimeGreen,
            //    Active = true,
            //    Power = 1.8f,
            //    Position = new Vector3(1000, 80, 100),
            //    Size = 400
            //});

            //LightList.Add(new Light()
            //{
            //    //Color = new Color(141, 38, 10, 42),
            //    Color = Color.Purple,
            //    Active = true,
            //    Power = 0.2f,
            //    Position = new Vector3(1280 / 2, 50, 250),
            //    Size = 600
            //});

            //LightList.Add(new Light()
            //{
            //    //Color = new Color(0, 180, 255, 42),
            //    Color = Color.Goldenrod,
            //    Active = true,
            //    Power = 0.8f,
            //    Position = new Vector3(1280 - 700, 720 - 180, 250),
            //    Size = 1200
            //});


            World = new World(new Vector2(0, 9.8f));
            Floor = new DrawablePhysicsObject(World, BoxTexture, new Vector2(1280, 100), 1000);
            Floor.Position = new Vector2(1280 / 2, 720 - 50);
            Floor.body.BodyType = BodyType.Static;

            CrateList = new List<DrawablePhysicsObject>();
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Vector2 thing = new Vector2(232, 462) - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                int numSeg = (int)MathHelper.Clamp(GetEven((int)thing.Length() / 10), 4, (float)double.PositiveInfinity);
                LightningList.Clear();
                ToonLightning newLightning = new ToonLightning(numSeg, 15, new Vector2(232, 462), new Vector2(Mouse.GetState().X, Mouse.GetState().Y), new Vector2(80, 100));
                LightningList.Add(newLightning);
            }

            CurShotTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentKeyboardState.IsKeyDown(Keys.Q))
            {
                CrepLightList[0].Exposure += 0.01f;
            }


            if (CurrentKeyboardState.IsKeyDown(Keys.Y))
            {
                SpecVal += 0.001f;
                LightEffect.Parameters["specVal"].SetValue(SpecVal);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.H))
            {
                SpecVal -= 0.001f;
                LightEffect.Parameters["specVal"].SetValue(SpecVal);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.A))
            {
                CrepLightList[0].Exposure -= 0.01f;
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.G))
            {
                if (CurShotTime > ShotTime)
                {
                    Vector2 pos;
                    pos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                    Vector2 Direction;
                    Direction = LightningList[0].Direction;

                    Emitter FlashEmitter = new Emitter(HitEffectParticle, pos,
                                                            new Vector2(
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 30,
                                                            MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 30),
                                                            new Vector2(8, 12), new Vector2(100, 200), 1f, false,
                                                            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.25f, 0.25f),
                                                            Color.Yellow, Color.Orange, 0f, 0.05f, 100, 7, false, new Vector2(0, 720), true,
                                                            1.0f, null, null, null, null, null, true, new Vector2(0.25f, 0.25f), false, false,
                                                            null, false, false, false);

                    EmitterList.Add(FlashEmitter);

                    Emitter FlashEmitter2 = new Emitter(HitEffectParticle, pos,
                                                    new Vector2(
                                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) - 5,
                                                    MathHelper.ToDegrees(-(float)Math.Atan2(Direction.Y, Direction.X)) + 5),
                                                    new Vector2(12, 15), new Vector2(80, 150), 1f, false,
                                                    new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.35f, 0.35f),
                                                    Color.LightYellow, Color.Yellow, 0f, 0.05f, 100, 7, false, new Vector2(0, 720), true,
                                                    1.0f, null, null, null, null, null, true, new Vector2(0.18f, 0.18f), false, false,
                                                    null, false, false, false);

                    EmitterList.Add(FlashEmitter2);


                    LightList.Add(new Light()
                    {
                        //Color = new Color(0, 180, 255, 42),
                        Color = Color.Goldenrod,
                        Active = true,
                        Power = 0.35f,
                        Position = new Vector3(pos.X, pos.Y, 0),
                        Size = 400,
                        MaxTime = 80,
                        CurTime = 0
                    });

                    CurShotTime = 0;
                }
            }

            LightList[1].Position = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 0);

            CrepLightList[0].Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            



            foreach (Emitter emitter in EmitterList)
            {
                emitter.Update(gameTime);
            }

            foreach (ToonLightning bolt in LightningList)
            {
                bolt.Update(gameTime);
            }

            foreach (Sprite sprite in SpriteList)
            {
                sprite.Update(gameTime);
            }

            foreach (Solid solid in SolidList)
            {
                solid.Update(gameTime);
            }

            foreach (Light light in LightList)
            {
                light.Update(gameTime);
            }

            //if (CurrentKeyboardState.IsKeyDown(Keys.Space))
            //{
            //    SpawnCrate();
            //}

            SpritePos += new Vector2(
                (float)Math.Sin(2*(float)gameTime.TotalGameTime.TotalSeconds), 
                (float)Math.Cos(4*(float)gameTime.TotalGameTime.TotalSeconds))*10;

            CrepLightList[1].Position = SpritePos;

            PreviousKeyboardState = CurrentKeyboardState;

            //World.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            //************NOTE****************
            //********************************
            //THE ARTEFACTS ON THE 4TH LAYER FROM THE TOP (red stripe down the middle)
            //ARE NOT CAUSED BY A SHADER PROBLEM OR MISTAKE. THEY'RE MINOR COMPRESSION ARTEFACTS
            //FROM THE ORIGINAL TEXTURE JPEG. THEY'RE JUST AMPLIED BY THE SHADERS
            //**************NOT YOUR FAULT**************


            #region Emissive
            #region Draw to emissive map
            GraphicsDevice.SetRenderTarget(EmissiveMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, RasterizerState.CullNone);
            spriteBatch.Draw(Sprite, SpritePos, new Color(255, 255, 255, 2));
            foreach (Sprite sprite in SpriteList)
            {
                sprite.DrawEmissive(spriteBatch);
            }

            //spriteBatch.Draw(Laser, new Vector2(0, 80), new Color(255, 180, 0, 2));

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (ToonLightning lightning in LightningList)
                {
                    lightning.Draw(GraphicsDevice);
                }
            }

            spriteBatch.End();
            #endregion
            
            #region Blur
            GraphicsDevice.SetRenderTarget(BlurMap);
            GraphicsDevice.Clear(Color.Transparent);

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
                sprite.Draw(spriteBatch, Color.Pink);
            }

            spriteBatch.End();
            #endregion

            #region Draw to NormalMap
            GraphicsDevice.SetRenderTarget(NormalMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            spriteBatch.Draw(NormalTexture, NormalTexture.Bounds, Color.White);
            spriteBatch.Draw(Sprite, SpritePos, Color.Black);
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
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (Sprite sprite in SpriteList)
            {
                DepthEffect.Parameters["Texture"].SetValue(sprite.Texture);
                DepthEffect.Parameters["depth"].SetValue(sprite.Depth);

                foreach (EffectPass pass in DepthEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    sprite.Draw(spriteBatch, Color.Red);
                    //graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, spriteVertices, 0, 4, spriteIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (Solid sprite in SolidList)
            {
                DepthEffect.Parameters["Texture"].SetValue(sprite.Texture);
                DepthEffect.Parameters["depth"].SetValue(sprite.Depth);

                foreach (EffectPass pass in DepthEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    sprite.Draw(spriteBatch, Color.Red);
                    //graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, spriteVertices, 0, 4, spriteIndices, 0, 2, VertexPositionColorTexture.VertexDeclaration);
                }
            }
            spriteBatch.End();
            #endregion
            
            #region Draw to LightMap
            GraphicsDevice.SetRenderTarget(LightMap);
            GraphicsDevice.Clear(Color.Transparent);

            foreach (Light light in LightList)
            {
                if (light.Active == true)
                {
                    MyShadow(light);

                    GraphicsDevice.SetRenderTarget(LightMap);

                    LightEffect.Parameters["ShadowMap"].SetValue(ShadowMap);

                    LightEffect.Parameters["LightPosition"].SetValue(light.Position);
                    LightEffect.Parameters["LightColor"].SetValue(ColorToVector(light.Color));
                    LightEffect.Parameters["LightPower"].SetValue(light.Power);
                    LightEffect.Parameters["LightSize"].SetValue(light.Size);
                    LightEffect.Parameters["NormalMap"].SetValue(NormalMap);
                    LightEffect.Parameters["ColorMap"].SetValue(ColorMap);
                    LightEffect.Parameters["DepthMap"].SetValue(DepthMap);
                    LightEffect.Parameters["lightDepth"].SetValue(0.5f);                    

                    LightEffect.CurrentTechnique = LightEffect.Techniques["DeferredPointLight"];
                    LightEffect.CurrentTechnique.Passes[0].Apply();

                    GraphicsDevice.BlendState = BlendBlack;
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, LightVertices, 0, 2);

                }
            }

            //TODO: This is here to have the emissive sprites also "cast" light on the LightMap. 
            //Not sure if it looks as good as I'd like though
            //may need to be removed
            spriteBatch.Begin();
            spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            spriteBatch.End();

            #endregion

            #region Combine Normals, Lighting and Color
            GraphicsDevice.SetRenderTarget(FinalMap);
            GraphicsDevice.Clear(Color.DeepSkyBlue);

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

            spriteBatch.Begin();
            spriteBatch.Draw(EmissiveMap, ColorMap.Bounds, Color.White);
            spriteBatch.Draw(BlurMap, BlurMap.Bounds, Color.White);
            foreach (Solid solid in SolidList)
            {
                solid.Draw(spriteBatch, Color.Black);
            }
            spriteBatch.End();
            #endregion

            #region Occlusion Map
            GraphicsDevice.SetRenderTarget(OcclusionMap);
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            foreach (Sprite sprite in SpriteList)
            {
                sprite.Draw(spriteBatch, Color.Black);
            }

            foreach (Solid solid in SolidList)
            {
                solid.Draw(spriteBatch, Color.Black);
            }
            spriteBatch.End();
            #endregion

            #region Crepuscular ColorMap
            GraphicsDevice.SetRenderTarget(CrepColorMap);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin();
            //spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);

            //foreach (Solid solid in SolidList)
            //{
            //    solid.Draw(spriteBatch, Color.Black);
            //}
            spriteBatch.End();
            #endregion


            GraphicsDevice.SetRenderTarget(Buffer2);
            GraphicsDevice.Clear(Color.Transparent);

            RaysEffect.Parameters["ColorMap"].SetValue(CrepColorMap);
            RaysEffect.Parameters["OccMap"].SetValue(OcclusionMap);

            foreach (CrepuscularLight light in CrepLightList)
            {
                GraphicsDevice.SetRenderTarget(CrepLightMap);
                GraphicsDevice.Clear(Color.Transparent);

                spriteBatch.Begin();
                spriteBatch.Draw(CrepuscularLightTexture, new Rectangle((int)(light.Position.X), (int)(light.Position.Y), CrepuscularLightTexture.Width / 3, CrepuscularLightTexture.Height / 3), null,
                                 LightList[CrepLightList.IndexOf(light)].Color, 0, new Vector2(CrepuscularLightTexture.Width / 2, CrepuscularLightTexture.Height / 2), SpriteEffects.None, 0);
                spriteBatch.End();

                #region Buffer1
                GraphicsDevice.SetRenderTarget(Buffer1);
                GraphicsDevice.Clear(Color.Transparent);
                {
                    RaysEffect.Parameters["LightPosition"].SetValue(light.Position / new Vector2(1280, 720));
                    RaysEffect.Parameters["decay"].SetValue(light.Decay);
                    RaysEffect.Parameters["exposure"].SetValue(light.Exposure);
                    RaysEffect.Parameters["density"].SetValue(light.Density);
                    RaysEffect.Parameters["weight"].SetValue(light.Weight);
                    RaysEffect.Parameters["DepthMap"].SetValue(DepthMap);

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                    RaysEffect.CurrentTechnique.Passes[0].Apply();

                    spriteBatch.Draw(CrepLightMap, CrepLightMap.Bounds, Color.White);
                    spriteBatch.End();
                }

                GraphicsDevice.SetRenderTarget(Buffer2);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                spriteBatch.Draw(Buffer1, Buffer1.Bounds, Color.White);
                spriteBatch.End();
                #endregion
            }

            #region Draw to the BackBuffer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            #region Draw Maps on the side
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(EmissiveMap, new Rectangle(0, 0, 320, 180), Color.White);
                spriteBatch.Draw(BlurMap, new Rectangle(0, 180, 320, 180), Color.White);
                spriteBatch.Draw(NormalMap, new Rectangle(0, 360, 320, 180), Color.White);
                spriteBatch.Draw(SpecMap, new Rectangle(0, 540, 320, 180), Color.White);

                spriteBatch.Draw(DepthMap, new Rectangle(320, 0, 320, 180), Color.White);
                spriteBatch.Draw(LightMap, new Rectangle(320, 180, 320, 180), Color.White);
                spriteBatch.Draw(FinalMap, new Rectangle(320, 360, 320, 180), Color.White);
                spriteBatch.Draw(Buffer2, new Rectangle(320, 540, 320, 180), Color.White);

                spriteBatch.Draw(CrepColorMap, new Rectangle(640, 0, 320, 180), Color.White);
                spriteBatch.Draw(ColorMap, new Rectangle(640, 180, 320, 180), Color.White);
                spriteBatch.Draw(FinalMap, new Rectangle(640, 360, 320, 180), Color.White);
                spriteBatch.Draw(ShadowMap, new Rectangle(640, 540, 320, 180), Color.White);

                spriteBatch.End();

                RaysEffect.Parameters["ColorMap"].SetValue(DepthMap);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                RaysEffect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(CrepLightMap, new Rectangle(960, 0, 320, 180), Color.White);
                spriteBatch.End();
            }
            #endregion
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                spriteBatch.Draw(FinalMap, FinalMap.Bounds, Color.White);
                spriteBatch.Draw(Buffer2, FinalMap.Bounds, Color.White);

                spriteBatch.End();                
            }
            
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
            Vector3 LightPos;

            LightPos = new Vector3(Mouse.GetState().X, Mouse.GetState().Y, 250);
            LightList[0].Position = new Vector3(SpritePos.X, SpritePos.Y, 0) + new Vector3(16, 16, 0);
            //LightList[0].Position = LightPos;

            //LightList[LightList.Count - 1].Position = LightPos;

            Vector2 SourcePosition = new Vector2(light.Position.X, light.Position.Y);

            RayList.Clear();
            ShadowList.Clear();

            foreach (Solid solid in SolidList)
            {
                Vector3 lightVector, check1, check2, thing, thing2;

                for (int i = 0; i < solid.vertices.Count(); i++)
                {
                    if (CurrentKeyboardState.IsKeyDown(Keys.P) &&
                        PreviousKeyboardState.IsKeyUp(Keys.P))
                    {
                        int stop = 10;
                    }

                    lightVector = solid.vertices[i].Position - new Vector3(SourcePosition, 0);
                    //lightVector.Normalize();

                    //lightVector *= light.Size;

                    int nextIndex, prevIndex;

                    nextIndex = Wrap(i + 1, 4);
                    prevIndex = Wrap(i - 1, 4);

                    check1 = solid.vertices[nextIndex].Position - new Vector3(SourcePosition, 0);
                    check2 = solid.vertices[prevIndex].Position - new Vector3(SourcePosition, 0);
                    
                    thing = Vector3.Cross(lightVector, check1);
                    thing2 = Vector3.Cross(lightVector, check2);

                    //NOTE: THIS LINE SEEMS TO FIX THE 0 VALUE CHECK VARIABLE RESULTING IN A DISAPPEARING SHADOW
                    thing.Normalize();

                    //SHADOWS DON'T SHOW UP IF THE Y OR X VALUES FOR THE THING AND CHECK ARE THE SAME.
                    //i.e. check1.y = 158 AND thing1.y = 158. Then the next if evaluates to false and a ray isn't added.
                    //meaning that there's a blank side for the polygon
                    //The Check variables use the previous and next vertex positions to calculate a vector
                    //This can end up with the vector having a 0 in it if the light lines up with a side
                    //This makes the cross product values messed up

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
                    vertices[1].Position = RayList[p].position + (RayList[p].direction * 100);
                    vertices[2].Position = RayList[p + 1].position + (RayList[p + 1].direction * 100);

                    vertices[3].Position = RayList[p + 1].position + (RayList[p + 1].direction * 100);
                    vertices[4].Position = RayList[p + 1].position;
                    vertices[5].Position = RayList[p].position;

                    vertices[0].Color = Color.Black;
                    vertices[1].Color = Color.Black;
                    vertices[2].Color = Color.Black;
                    vertices[3].Color = Color.Black;
                    vertices[4].Color = Color.Black;
                    vertices[5].Color = Color.Black;

                    ShadowList.Add(new PolygonShadow() { Vertices = vertices });
                }
            }
        }

        public Texture2D MyShadow(Light light)
        {
            GraphicsDevice.SetRenderTarget(ShadowMap);
            GraphicsDevice.Clear(Color.White);

            DrawShadows(light);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = PSBlendState.Multiply;
            BasicEffect.Techniques[0].Passes[0].Apply();

            foreach (PolygonShadow shadow in ShadowList)
            {
                shadow.Draw(GraphicsDevice);
            }

            return ShadowMap;
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

        private void SpawnCrate()
        {
            DrawablePhysicsObject crate;
            crate = new DrawablePhysicsObject(World, BoxTexture, new Vector2(50.0f, 50.0f), 5f);
            crate.Position = new Vector2(Random.Next(50, GraphicsDevice.Viewport.Width - 50), 1);

            CrateList.Add(crate);
        }

        private int GetEven(int num)
        {
            if (num % 2 == 0)
            {
                return num;
            }
            else
            {
                return num + 1;
            }
        }

        public static double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
        }
    }
}

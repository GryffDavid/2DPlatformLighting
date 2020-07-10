using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PlatformLighting1
{
    public class Particle : Drawable
    {
        //public Texture2D Texture;
        public Vector2 CurrentPosition, Direction, Velocity, YRange, Origin, StartingPosition, Friction;
        public float Angle, Speed,
                     RotationIncrement, CurrentRotation,
                     Gravity,
                     CurrentTime, MaxTime,
                     CurrentScale, MaxScale,
                     CurrentTransparency, MaxTransparency,
                     CurrentFadeDelay, FadeDelay;
        float PercentageTime;
        float BounceY;

        public Color CurrentColor, EndColor, StartColor;
        public bool Fade, BouncedOnGround, CanBounce, Shrink, StopBounce, HardBounce, Shadow, RotateVelocity, SortDepth, Grow;
        static Random Random = new Random();
        public SpriteEffects Orientation;

        private Color _Color;
        new public Color Color
        {
            get { return _Color; }
            set
            {
                _Color = value;
                vertices[0].Color = value;
                vertices[1].Color = value;
                vertices[2].Color = value;
                vertices[3].Color = value;
            }
        }

        public float RadRotation;

        //THIS IS SO THAT SPARKS CAN BOUNCE OFF OF INVADERS
        //List<Invader> InvaderList;

        //THIS IS SO THAT SPARKS CAN BOUNCE OFF OF SOLID TRAPS
        //List<Trap> TrapList;

        //VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
        //int[] indices = new int[6];
        public Vector2[] texCoords = new Vector2[4];

        public Particle(Texture2D texture, Vector2 position, float angle, float speed, float maxTime,
            float startingTransparency, bool fade, float startingRotation, float rotationChange,
            float scale, Color startColor, Color endColor, float gravity, bool canBounce, float maxY, bool shrink,
            float? drawDepth = null, bool? stopBounce = false, bool? hardBounce = true, bool? shadow = false,
            bool? rotateVelocity = false, Vector2? friction = null, SpriteEffects? orientation = SpriteEffects.None,
            float? fadeDelay = null, bool? sortDepth = false, bool? grow = false)
        {
            Active = true;
            Texture = texture;
            CurrentPosition = position;
            StartingPosition = position;
            Angle = angle;
            Speed = speed;
            CurrentTime = 0;
            MaxTime = maxTime;
            CurrentTransparency = startingTransparency;
            MaxTransparency = startingTransparency;
            StartColor = startColor;
            CurrentColor = startColor;
            EndColor = endColor;

            if (Shrink == true)
                CurrentScale = scale;

            if (Grow == true)
                CurrentScale = 0f;

            MaxScale = scale;
            Fade = fade;
            Gravity = gravity;
            CanBounce = canBounce;
            Shrink = shrink;
            RotateVelocity = rotateVelocity.Value;

            if (fadeDelay != null)
                FadeDelay = fadeDelay.Value;
            else
                FadeDelay = 0;

            RotationIncrement = rotationChange;
            CurrentRotation = startingRotation;

            Direction.X = (float)Math.Cos(Angle);
            Direction.Y = (float)Math.Sin(Angle);

            Velocity = Direction * Speed;

            BounceY = maxY;

            if (drawDepth == null)
                DrawDepth = 0;
            else
                DrawDepth = drawDepth.Value;

            StopBounce = stopBounce.Value;

            HardBounce = hardBounce.Value;

            Shadow = shadow.Value;

            Orientation = orientation.Value;
            texCoords = GetTexCoords(Orientation);

            if (friction != null)
                Friction = friction.Value;
            else
                Friction = new Vector2(0, 0);

            Grow = grow.Value;

            SortDepth = sortDepth.Value;

            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            RadRotation = MathHelper.ToRadians(CurrentRotation);

            vertices[0] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CurrentPosition.X - (Texture.Width * CurrentScale) / 2, CurrentPosition.Y - (Texture.Height * CurrentScale) / 2, 0),
                TextureCoordinate = texCoords[0]
            };

            vertices[1] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CurrentPosition.X + (Texture.Width * CurrentScale) / 2, CurrentPosition.Y - (Texture.Height * CurrentScale) / 2, 0),
                TextureCoordinate = texCoords[1]
            };

            vertices[2] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CurrentPosition.X + (Texture.Width * CurrentScale) / 2, CurrentPosition.Y + (Texture.Height * CurrentScale) / 2, 0),
                TextureCoordinate = texCoords[2]
            };

            vertices[3] = new VertexPositionColorTexture()
            {
                Color = Color,
                Position = new Vector3(CurrentPosition.X - (Texture.Width * CurrentScale) / 2, CurrentPosition.Y + (Texture.Height * CurrentScale) / 2, 0),
                TextureCoordinate = texCoords[3]
            };

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                #region Bounce particles off objects idea
                //Bouncing particles off of invaders/traps is meant specifically for Sparks only. 
                //Those that aren't sorted by depth but are drawn additively.

                //Bounce off invader bounding boxes
                //Where from emitter to invader < max possible distance (MaxLife * MaxSpeed)
                //if (InvaderList != null)
                //foreach (Invader invader in InvaderList)
                //{

                //}

                //Bounce off trap bounding boxes
                //Where from emitter to trap < max possible distance (MaxLife * MaxSpeed)
                //if (TrapList != null)
                //foreach (Trap trap in TrapList)
                //{

                //}

                //Third colour fade
                //If ThirdColour != null change colour life to 1/3 instead of 1/2
                #endregion

                if (CurrentTime < MaxTime)
                    CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (CurrentTime >= MaxTime && CurrentFadeDelay <= FadeDelay)
                {
                    CurrentFadeDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                #region Deactivate the particle
                if (CurrentTime >= MaxTime && CurrentTransparency <= 0)
                {
                    Active = false;
                    return;
                }
                #endregion


                #region Handle particle rotation
                //These two were originally separate - keep an eye on them
                if (RotateVelocity == true)
                {
                    CurrentRotation = MathHelper.ToDegrees((float)Math.Atan2(Velocity.Y, Velocity.X));
                }
                else
                {
                    if (RotationIncrement != 0)
                    {
                        CurrentRotation += RotationIncrement * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);
                    }
                }

                //CurrentRotation = CurrentRotation % 360;
                #endregion

                #region Update vertices
                if (Velocity != Vector2.Zero)
                {
                    CurrentPosition += Velocity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                    vertices[0].Position = new Vector3(CurrentPosition.X - (Texture.Width * CurrentScale) / 2, CurrentPosition.Y - (Texture.Height * CurrentScale) / 2, 0);
                    vertices[1].Position = new Vector3(CurrentPosition.X + (Texture.Width * CurrentScale) / 2, CurrentPosition.Y - (Texture.Height * CurrentScale) / 2, 0);
                    vertices[2].Position = new Vector3(CurrentPosition.X + (Texture.Width * CurrentScale) / 2, CurrentPosition.Y + (Texture.Height * CurrentScale) / 2, 0);
                    vertices[3].Position = new Vector3(CurrentPosition.X - (Texture.Width * CurrentScale) / 2, CurrentPosition.Y + (Texture.Height * CurrentScale) / 2, 0);
                }

                Velocity.Y += Gravity * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f);

                DestinationRectangle = new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, (int)(Texture.Width * CurrentScale), (int)(Texture.Height * CurrentScale));
                #endregion

                #region Handle bouncing
                if (CanBounce == true)
                    if (CurrentPosition.Y >= BounceY && BouncedOnGround == false)
                    {
                        if (HardBounce == true)
                            CurrentPosition.Y -= Velocity.Y;

                        Velocity.Y = (-Velocity.Y / 3);
                        Velocity.X = (Velocity.X / 3);
                        RotationIncrement = (RotationIncrement * 3);
                        BouncedOnGround = true;
                    }

                if (StopBounce == true &&
                    BouncedOnGround == true &&
                    CurrentPosition.Y > BounceY)
                {
                    Velocity.Y = (-Velocity.Y / 2);

                    Velocity.X *= 0.9f;

                    RotationIncrement = MathHelper.Lerp(RotationIncrement, 0, 0.2f * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));

                    if (Velocity.Y < 0.2f && Velocity.Y > 0)
                    {
                        Velocity.Y = 0;
                    }

                    if (Velocity.Y > -0.2f && Velocity.Y < 0)
                    {
                        Velocity.Y = 0;
                    }

                    if (Velocity.X < 0.2f && Velocity.X > 0)
                    {
                        Velocity.X = 0;
                    }

                    if (Velocity.X > -0.2f && Velocity.X < 0)
                    {
                        Velocity.X = 0;
                    }
                }
                #endregion

                #region Handle bouncing rotation
                //if (Velocity.Y == 0 && CanBounce == true)
                //{
                //    if (CurrentRotation < 0)
                //        CurrentRotation += 360;
                //}
                #endregion

                #region Handle friction
                if (Friction != new Vector2(0, 0))
                {
                    Velocity.Y = MathHelper.Lerp(Velocity.Y, 0, Friction.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                    Velocity.X = MathHelper.Lerp(Velocity.X, 0, Friction.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60.0f));
                }
                #endregion

                PercentageTime = 1.0f - ((1 / MaxTime) * CurrentTime);

                //The FadeDelay should only apply to the transparency
                if (Fade == true)
                {
                    //CurrentTransparency = MaxTransparency * PercentageTime;
                    CurrentTransparency = MaxTransparency * (1.0f - ((1 / (MaxTime + FadeDelay)) * (CurrentTime + CurrentFadeDelay)));
                }
                else
                {
                    if (CurrentTime >= MaxTime)
                    {
                        Active = false;
                    }
                }

                //if (Shrink == true)
                //{
                //    CurrentScale = MaxScale * (1.0f - ((1 / (MaxTime + FadeDelay)) * (CurrentTime + CurrentFadeDelay)));
                //}


                if (Shrink == true && Grow == true)
                {
                    CurrentScale = MaxScale * (float)Math.Sin(Math.PI * PercentageTime); // MaxScale * (1.0f - ((1 / (MaxTime + FadeDelay)) * (CurrentTime + CurrentFadeDelay)));
                }

                if (Shrink == true && Grow == false)
                {
                    CurrentScale = MaxScale * (1.0f - ((1 / (MaxTime + FadeDelay)) * (CurrentTime + CurrentFadeDelay)));
                }

                if (Shrink == false && Grow == true)
                {
                    CurrentScale = MaxScale * ((1 / (MaxTime + FadeDelay)) * (CurrentTime + CurrentFadeDelay));
                }

                if (Shrink == false && Grow == false)
                {
                    CurrentScale = MaxScale;
                }



                if (SortDepth == true)
                {
                    DrawDepth = DestinationRectangle.Center.Y / 720.0f;
                }

                if (CurrentColor != EndColor)
                {
                    CurrentColor = Color.Lerp(EndColor, StartColor, PercentageTime);
                }

                Color = CurrentColor * CurrentTransparency;

                //if (RotationIncrement != 0)
                RadRotation = MathHelper.ToRadians(CurrentRotation);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                spriteBatch.Draw(Texture, DestinationRectangle, null, Color, RadRotation, Origin, Orientation, DrawDepth);
            }
        }

        public Vector2[] GetTexCoords(SpriteEffects orientation)
        {
            Vector2[] coords = new Vector2[4];

            switch (orientation)
            {
                case SpriteEffects.FlipHorizontally:
                    {
                        coords[0] = new Vector2(1, 0);
                        coords[1] = new Vector2(0, 0);
                        coords[2] = new Vector2(0, 1);
                        coords[3] = new Vector2(1, 1);
                    }
                    break;

                case SpriteEffects.FlipVertically:
                    {
                        coords[0] = new Vector2(0, 1);
                        coords[1] = new Vector2(1, 1);
                        coords[2] = new Vector2(1, 0);
                        coords[3] = new Vector2(0, 0);
                    }
                    break;

                case SpriteEffects.None:
                    {
                        coords[0] = new Vector2(0, 0);
                        coords[1] = new Vector2(1, 0);
                        coords[2] = new Vector2(1, 1);
                        coords[3] = new Vector2(0, 1);
                    }
                    break;
            }

            return coords;
        }
    }
}

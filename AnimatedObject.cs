using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGame
{
    internal abstract class AnimatedObject : MovingObject
    {
        protected int[] animSequence;   // Keeps track of the order in which frames should be displayed
        protected int currentFrame;     // The current frame to be displayed, clipped out with drawRect
        protected int frameInRow;       // Number of frames per row in the texture
        protected int animSpeed;        // Time each frame SHOULD be displayed before changing
        protected int animTimer;        // How long the current frame HAS been displayed
        protected Vector2 spriteSize;   // Size of a sprite (frame)
        protected bool animationInProgress;

        public AnimatedObject(Vector2 position, float scale = 1, float angle = 0) 
            : base(position, Color.White, scale, angle)
        {
        }

        public AnimatedObject(Vector2 position, Color color, float scale = 1, float angle = 0, float alpha = 1)
            : base(position, color, scale, angle, alpha)
        {
        }

        protected void SetFrame(int n)
        {
            drawRect.Y = (int)(spriteSize.Y * (n / frameInRow));
            drawRect.X = (int)(spriteSize.X * (n % frameInRow));
        }

        protected void SetAnimation(GameTime gameTime)
        {
            animationInProgress = true;
            animTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (animTimer > animSpeed)
            {
                animTimer -= animSpeed;
                currentFrame = animSequence[currentFrame];
                SetFrame(currentFrame);
            }
        }

        protected bool AnimationIsComplete()
        {
            // Check if the animation is complete based on your criteria
            if (currentFrame == animSequence[currentFrame])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

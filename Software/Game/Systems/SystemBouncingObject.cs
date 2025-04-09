using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;
using OpenTK.Mathematics;
using System;

namespace OpenGL_Game.Systems
{
    class SystemBouncingObject : ISystem
    {
        public string Name => "BouncingObjectAI";

        // Define horizontal bounds (flip of Room1)
        private readonly float leftBound = -10.0f;
        private readonly float rightBound = -6.25f;
        private readonly float horizontalSpeed = 2.0f; // constant horizontal speed

        // Bouncing (vertical) parameters
        private readonly float groundLevel = -1.5f; // floor level
        private readonly float amplitude = 0.5f;    // vertical bounce amplitude
        private readonly float frequency = (float)Math.PI;  // Frequency: period ~2 seconds

        public void OnAction(Entity entity)
        {
            if (entity.Name != "BouncingObject")
                return;

            var posComp = entity.GetComponent<ComponentPosition>();
            var velComp = entity.GetComponent<ComponentVelocity>();
            if (posComp == null || velComp == null)
                return;

            Vector3 pos = posComp.Position;

            // Horizontal movement:
            // If at or past left bound, ensure velocity is positive; if at or past right, velocity is negative.
            if (pos.X <= leftBound)
                velComp.Velocity = new Vector3(Math.Abs(horizontalSpeed), 0, 0);
            else if (pos.X >= rightBound)
                velComp.Velocity = new Vector3(-Math.Abs(horizontalSpeed), 0, 0);
            // (SystemPhysics updates pos.X based on velocity.)

            // Vertical bouncing: Use GameScene elapsed time
            // We need a public getter in GameScene; see next step.
            float t = (float)GameScene.gameInstance.CurrentElapsedTime;
            // Compute vertical offset using an absolute sine so it bounces up (every bounce is upward).
            float bounceY = groundLevel + amplitude * Math.Abs((float)Math.Sin(frequency * t));
            pos.Y = bounceY;

            // Update the position's Y; leave horizontal components to be updated by physics.
            posComp.Position = new Vector3(pos.X, pos.Y, pos.Z);
        }
    }
}
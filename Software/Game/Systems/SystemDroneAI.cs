using OpenGL_Game.Components;
using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace OpenGL_Game.Systems
{
    class SystemDroneAI : ISystem
    {
        public string Name => "DroneAI";

        private Vector3[] waypoints = new Vector3[]
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(9f, 0.0f, -9f),
            new Vector3(9f, 0.0f, 9f),
            new Vector3(-9f, 0.0f, -9f),
            new Vector3(-9f, 0.0f, 9f)
        };

        private int currentWaypoint = 0;
        private float speed = 1.0f;
        private float waypointThreshold = 0.5f;
        private float chaseTriggerDistance = 10f;
        private float maxChaseDistance = 15f;

        public void OnAction(Entity entity)
        {
            // Only act on the drone entity.
            if (entity.Name != "Intergalactic_Spaceship")
                return;

            var dronePos = entity.GetComponent<ComponentPosition>();
            var droneVel = entity.GetComponent<ComponentVelocity>();
            if (dronePos == null || droneVel == null)
                return;

            // If debug movement is turned off, stop the drone.
            if (!GameScene.debugMovementEnabled)
            {
                droneVel.Velocity = Vector3.Zero;
                return;
            }

            var player = GameScene.gameInstance?.entityManager.FindEntity("Player");
            if (player == null) return;

            var playerPos = player.GetComponent<ComponentPosition>();
            if (playerPos == null) return;

            // Determine if player is near the Room 4 spawn
            Vector3 room4Center = new Vector3(9f, 0f, -9f); // same as player start
            float distToRoom4 = (new Vector2(playerPos.Position.X, playerPos.Position.Z) - new Vector2(room4Center.X, room4Center.Z)).Length;

            bool shouldChase = distToRoom4 < chaseTriggerDistance;

            Vector3 target;

            if (shouldChase)
            {
                // Follow player
                target = playerPos.Position;
            }
            else
            {
                // Patrol mode
                target = waypoints[currentWaypoint];
                float distToWaypoint = (new Vector2(dronePos.Position.X, dronePos.Position.Z) - new Vector2(target.X, target.Z)).Length;

                if (distToWaypoint < waypointThreshold)
                {
                    currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                    target = waypoints[currentWaypoint]; // update target
                }
            }

            // Move toward the target
            Vector3 moveDirection = target - dronePos.Position;
            moveDirection.Y = 0;
            if (moveDirection.LengthSquared > 0.01f)
            {
                moveDirection.Normalize();
                droneVel.Velocity = moveDirection * speed;
            }
            else
            {
                droneVel.Velocity = Vector3.Zero;
            }
        }
    }
}
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenGL_Game.Managers;
using OpenGL_Game.Scenes;

namespace OpenGL_Game.Systems
{
    class SystemCollision : ISystem
    {
        public string Name => "Collision";

        // Queue entities to remove so we don't modify the list during iteration
        public List<Entity> EntitiesToRemove { get; } = new List<Entity>();

        private readonly EntityManager entityManager;

        public SystemCollision(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public void OnAction(Entity entity)
        {
            // Process only the player
            if (!(entity.Mask.HasFlag(ComponentTypes.COMPONENT_PLAYER) &&
                  entity.Mask.HasFlag(ComponentTypes.COMPONENT_POSITION) &&
                  entity.Mask.HasFlag(ComponentTypes.COMPONENT_VELOCITY)))
                return;

            var playerPos = entity.GetComponent<ComponentPosition>();
            var playerVel = entity.GetComponent<ComponentVelocity>();
            Vector2 playerXZ = new Vector2(playerPos.Position.X, playerPos.Position.Z);

            foreach (var other in entityManager.Entities())
            {
                if (other == entity)
                    continue;

                // --- Box Colliders (unchanged) ---
                var boxCollider = other.GetComponent<ComponentBoxCollider>();
                if (boxCollider != null)
                {
                    Vector3 colliderCenter = boxCollider.Center;
                    var colliderPos = other.GetComponent<ComponentPosition>();
                    if (colliderPos != null)
                        colliderCenter = colliderPos.Position;

                    Vector2 colliderCenterXZ = new Vector2(colliderCenter.X, colliderCenter.Z);
                    Vector2 halfSize = boxCollider.Size / 2.0f;

                    float left = colliderCenterXZ.X - halfSize.X;
                    float right = colliderCenterXZ.X + halfSize.X;
                    float bottom = colliderCenterXZ.Y - halfSize.Y;
                    float top = colliderCenterXZ.Y + halfSize.Y;

                    if (playerXZ.X >= left && playerXZ.X <= right &&
                        playerXZ.Y >= bottom && playerXZ.Y <= top)
                    {
                        // Overlap logic
                        float overlapLeft = playerXZ.X - left;
                        float overlapRight = right - playerXZ.X;
                        float overlapBottom = playerXZ.Y - bottom;
                        float overlapTop = top - playerXZ.Y;

                        float minOverlap = overlapLeft;
                        Vector2 pushDirection = new Vector2(-1, 0);
                        if (overlapRight < minOverlap)
                        {
                            minOverlap = overlapRight;
                            pushDirection = new Vector2(1, 0);
                        }
                        if (overlapBottom < minOverlap)
                        {
                            minOverlap = overlapBottom;
                            pushDirection = new Vector2(0, -1);
                        }
                        if (overlapTop < minOverlap)
                        {
                            minOverlap = overlapTop;
                            pushDirection = new Vector2(0, 1);
                        }

                        Vector3 newPos = playerPos.Position;
                        newPos.X += pushDirection.X * minOverlap;
                        newPos.Z += pushDirection.Y * minOverlap;
                        playerPos.Position = newPos;

                        playerVel.Velocity = Vector3.Zero;
                    }
                }

                // --- Sphere Colliders (keys, portal, etc.) ---
                var sphereCollider = other.GetComponent<ComponentSphereCollider>();
                if (sphereCollider != null)
                {
                    Vector3 colliderCenter = sphereCollider.Center;
                    var colliderPos = other.GetComponent<ComponentPosition>();
                    if (colliderPos != null)
                        colliderCenter = colliderPos.Position;

                    Vector2 diff2D = new Vector2(playerPos.Position.X - colliderCenter.X,
                                                 playerPos.Position.Z - colliderCenter.Z);
                    float distance2D = diff2D.Length;

                    if (distance2D < sphereCollider.Radius)
                    {
                        // If this is a Key...
                        if (other.Name.StartsWith("Key"))
                        {
                            if (!EntitiesToRemove.Contains(other))
                            {
                                GameScene.gameInstance.keysCollected++;
                                EntitiesToRemove.Add(other);
                            }
                            continue;
                        }
                        // If this is the Portal Frame...
                        else if (other.Name == "PortalFrame")
                        {
                            // If the player has all 3 keys, go to the EndScene
                            if (GameScene.gameInstance.keysCollected >= 3)
                            {
                                GameScene.gameInstance.GoToEndScene();
                            }
                            continue;
                        }
                        else
                        {
                            // Normal sphere collision (repelling logic)
                            if (distance2D == 0)
                            {
                                diff2D = new Vector2(1, 0);
                                distance2D = 1;
                            }
                            Vector2 pushDir2D = diff2D / distance2D;
                            float penetration = sphereCollider.Radius - distance2D;

                            Vector3 newPos = playerPos.Position;
                            newPos.X += pushDir2D.X * penetration;
                            newPos.Z += pushDir2D.Y * penetration;
                            playerPos.Position = newPos;

                            Vector3 newVel = playerVel.Velocity;
                            newVel.X = 0;
                            newVel.Z = 0;
                            playerVel.Velocity = newVel;
                        }
                    }
                }
            }
        }
    }
}
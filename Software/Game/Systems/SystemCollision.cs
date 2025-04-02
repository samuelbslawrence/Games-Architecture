using OpenTK.Mathematics;
using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenGL_Game.Managers;

namespace OpenGL_Game.Systems
{
    class SystemCollision : ISystem
    {
        public string Name => "Collision";
        private EntityManager entityManager;

        public SystemCollision(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public void OnAction(Entity entity)
        {
            // Process only the player entity (with PLAYER, POSITION, and VELOCITY components)
            if (entity.Mask.HasFlag(ComponentTypes.COMPONENT_PLAYER) &&
                entity.Mask.HasFlag(ComponentTypes.COMPONENT_POSITION) &&
                entity.Mask.HasFlag(ComponentTypes.COMPONENT_VELOCITY))
            {
                var playerPos = entity.GetComponent<ComponentPosition>();
                var playerVel = entity.GetComponent<ComponentVelocity>();
                Vector2 playerXZ = new Vector2(playerPos.Position.X, playerPos.Position.Z);

                // Iterate over all entities to check for collisions.
                foreach (var other in entityManager.Entities())
                {
                    if (other == entity)
                        continue;

                    // --- Check for Box Collider collisions (in the x–z plane) ---
                    var boxCollider = other.Components.Find(comp => comp is ComponentBoxCollider) as ComponentBoxCollider;
                    if (boxCollider != null)
                    {
                        // Determine the collider center: use ComponentPosition if available.
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
                            // Compute overlap in each direction.
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

                            // Repel the player along the axis of least penetration.
                            Vector3 newPos = playerPos.Position;
                            newPos.X += pushDirection.X * minOverlap;
                            newPos.Z += pushDirection.Y * minOverlap;
                            playerPos.Position = newPos;

                            // Stop the player's movement.
                            playerVel.Velocity = Vector3.Zero;
                        }
                    }

                    // --- Check for 2D Sphere Collider collisions (in the x–z plane) ---
                    var sphereCollider = other.Components.Find(comp => comp is ComponentSphereCollider) as ComponentSphereCollider;
                    if (sphereCollider != null)
                    {
                        // Use ComponentPosition if available; otherwise, the collider's stored center.
                        Vector3 colliderCenter = sphereCollider.Center;
                        var colliderPos = other.GetComponent<ComponentPosition>();
                        if (colliderPos != null)
                            colliderCenter = colliderPos.Position;

                        // Calculate 2D difference (x, z only).
                        Vector2 diff2D = new Vector2(playerPos.Position.X - colliderCenter.X, playerPos.Position.Z - colliderCenter.Z);
                        float distance2D = diff2D.Length;

                        if (distance2D < sphereCollider.Radius)
                        {
                            // Avoid division by zero.
                            if (distance2D == 0)
                            {
                                diff2D = new Vector2(1, 0);
                                distance2D = 1;
                            }
                            Vector2 pushDir2D = diff2D / distance2D;
                            float penetration = sphereCollider.Radius - distance2D;

                            // Adjust only x and z, leaving y unchanged.
                            Vector3 newPos = playerPos.Position;
                            newPos.X += pushDir2D.X * penetration;
                            newPos.Z += pushDir2D.Y * penetration;
                            playerPos.Position = newPos;

                            // Reset the player's horizontal velocity.
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
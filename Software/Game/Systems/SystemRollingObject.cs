using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenTK.Mathematics;

namespace OpenGL_Game.Systems
{
    class SystemRollingObject : ISystem
    {
        public string Name => "RollingObjectAI";

        // Define waypoints for the rectangular patrol route.
        // Note: Y is kept constant (–1.5) so that it rolls along the floor.
        private Vector3[] waypoints = new Vector3[]
        {
            new Vector3(10.0f, -1.5f, 10.0f),
            new Vector3(6.25f, -1.5f, 10.0f),
            new Vector3(6.25f, -1.5f, 6.25f),
            new Vector3(10.0f, -1.5f, 6.25f)
        };

        private int currentWaypoint = 0;
        private float speed = 2.0f; // Adjust speed as needed.
        private float threshold = 0.3f; // When the object is close enough to a waypoint.

        public void OnAction(Entity entity)
        {
            // Process only entities named "RollingObject"
            if (entity.Name != "RollingObject")
                return;

            var posComp = entity.GetComponent<ComponentPosition>();
            var velComp = entity.GetComponent<ComponentVelocity>();
            if (posComp == null || velComp == null)
                return;

            // Determine current target waypoint.
            Vector3 target = waypoints[currentWaypoint];
            // We only care about horizontal movement.
            Vector3 direction = target - posComp.Position;
            direction.Y = 0;
            float distance = direction.Length;

            if (distance < threshold)
            {
                // Move to next waypoint.
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                target = waypoints[currentWaypoint];
                direction = target - posComp.Position;
                direction.Y = 0;
            }
            if (direction != Vector3.Zero)
            {
                direction.Normalize();
            }
            // Set velocity toward the target.
            velComp.Velocity = direction * speed;
        }
    }
}
using OpenGL_Game.Components;
using OpenGL_Game.Objects;

namespace OpenGL_Game.Systems
{
    class SystemScale : ISystem
    {
        public string Name => "SystemScale";

        // In this simple example, the system doesn't modify the scale,
        // but you could add logic here to animate or update scales over time.
        public void OnAction(Entity entity)
        {
            // Example: If you want to gradually increase scale:
            var scaleComp = entity.GetComponent<ComponentScale>();
            if (scaleComp != null)
            {
                // Uncomment the next line to increase scale over time (for testing)
                // scaleComp.Scale *= 1.01f;
            }
        }
    }
}
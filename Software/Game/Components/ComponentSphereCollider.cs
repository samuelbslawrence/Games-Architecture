using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    internal class ComponentSphereCollider : IComponent
    {
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_COLLIDER;

        // Center of the collider (if the owning entity has a ComponentPosition, that takes precedence)
        public Vector3 Center { get; set; }
        // Radius of the sphere collider
        public float Radius { get; set; }

        public ComponentSphereCollider(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
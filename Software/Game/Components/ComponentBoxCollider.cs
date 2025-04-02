using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    internal class ComponentBoxCollider : IComponent
    {
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_COLLIDER;

        // The collider’s own center – if the owning entity has a ComponentPosition, that takes precedence.
        public Vector3 Center { get; set; }
        // Size represents the width and height of the collider in the x–z plane.
        public Vector2 Size { get; set; }
        // If true, this collider only interacts with the player.
        public bool OnlyCollideWithPlayer { get; set; }
        // If true, the collider is solid (i.e. it repels the colliding entity).
        public bool IsSolid { get; set; }

        public ComponentBoxCollider(Vector3 center, bool onlyCollideWithPlayer, Vector2 size, bool isSolid)
        {
            Center = center;
            OnlyCollideWithPlayer = onlyCollideWithPlayer;
            Size = size;
            IsSolid = isSolid;
        }
    }
}
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Components
{
    internal class ComponentPlayer : IComponent
    {
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_PLAYER;

        public Camera camera;
        public Vector3 direction;

        public ComponentPlayer(Camera camera, Vector3 direction)
        {
            this.camera = camera;
            this.direction = direction;
        }
    }
}

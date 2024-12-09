using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Components;
using OpenGL_Game.OBJLoader;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;
using OpenTK.Mathematics;

namespace OpenGL_Game.Systems
{
    class SystemPhysics : ISystem
    {
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_POSITION | ComponentTypes.COMPONENT_VELOCITY);

        public string Name => "SystemPhysics";
       

        public void OnAction(Entity entity)
        {
            if ((entity.Mask & MASK) == MASK)
            {
                List<IComponent> components = entity.Components;

                IComponent positionComponent = components.Find(delegate(IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_POSITION;
                });

                IComponent velocityComponent = components.Find(delegate (IComponent component)
                {
                    return component.ComponentType == ComponentTypes.COMPONENT_VELOCITY;
                });

                Motion((ComponentPosition)positionComponent, (ComponentVelocity)velocityComponent);
            }
        }

        public void Motion(ComponentPosition position, ComponentVelocity velocity)
        {
            position.Position += velocity.Velocity * (GameScene.dt);
        }
    }
}

using OpenGL_Game.Components;
using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Systems
{
    internal class SystemPlayer : ISystem
    {
        public string Name => "Player";

        const ComponentTypes MASK = ComponentTypes.COMPONENT_PLAYER | ComponentTypes.COMPONENT_POSITION | ComponentTypes.COMPONENT_VELOCITY;

        public void OnAction(Entity entity)
        {
            if ((entity.Mask & MASK) == MASK)
            {
                var playerComponent = entity.GetComponent<ComponentPlayer>();
                var positionComponent = entity.GetComponent<ComponentPosition>();
                var velocityComponent = entity.GetComponent<ComponentVelocity>();

                if (ControlsManager.keysPressed[(char)Keys.W])
                {
                    velocityComponent.Velocity = 2.0f * playerComponent.direction;
                }
                else if (ControlsManager.keysPressed[(char)Keys.S])
                {
                    velocityComponent.Velocity = -2.0f * playerComponent.direction;
                }
                else
                {
                    velocityComponent.Velocity = Vector3.Zero;
                }
                if (ControlsManager.keysPressed[(char)Keys.A])
                {
                    playerComponent.direction = Matrix3.CreateRotationY(-0.01f) * playerComponent.direction;
                }
                if (ControlsManager.keysPressed[(char)Keys.D])
                {
                    playerComponent.direction = Matrix3.CreateRotationY(0.01f) * playerComponent.direction;
                }

                playerComponent.camera.cameraPosition = positionComponent.Position;
                playerComponent.camera.cameraDirection = playerComponent.direction;
                playerComponent.camera.UpdateView();
            }
        }
    }
}
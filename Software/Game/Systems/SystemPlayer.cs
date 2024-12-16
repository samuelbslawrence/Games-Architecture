using OpenGL_Game.Components;
using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
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

                if (ControlsManager.keysPressed[(char)Keys.Up])
                {
                    playerComponent.camera.MoveForward(0.1f);
                }
                if (ControlsManager.keysPressed[(char)Keys.Down])
                {
                    playerComponent.camera.MoveForward(-0.1f);
                }
                if (ControlsManager.keysPressed[(char)Keys.Left])
                {
                    playerComponent.camera.RotateY(-0.01f);
                }
                if (ControlsManager.keysPressed[(char)Keys.Right])
                {
                    playerComponent.camera.RotateY(0.01f);
                }

                positionComponent.Position = playerComponent.camera.cameraPosition;
                playerComponent.direction =  playerComponent.camera.cameraDirection;
            }
        }
    }
}

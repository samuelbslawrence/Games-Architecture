using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenGL_Game.Systems;

public class SystemAudio : ISystem
{
    private const ComponentTypes MASK = (ComponentTypes.COMPONENT_POSITION | ComponentTypes.COMPONENT_AUDIO);

    public string Name => "Audio";

    void ISystem.OnAction(Entity entity)
    {
        if ((entity.Mask & MASK) == MASK)
        {
            var positionComponent = entity.GetComponent<ComponentPosition>();
            var audioComponent = entity.GetComponent<ComponentAudio>();

            if (positionComponent != null && audioComponent != null)
            {
                audioComponent.SetPosition(positionComponent.Position);
            }
        }
    }
}
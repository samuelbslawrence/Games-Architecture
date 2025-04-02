using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    class ComponentScale : IComponent
    {
        // This component holds a uniform scale value. You could also use a Vector3 for non-uniform scaling.
        public Vector3 Scale { get; set; }

        public ComponentScale(Vector3 scale)
        {
            Scale = scale;
        }

        // If your IComponent interface requires a ComponentType property, ensure you return COMPONENT_SCALE.
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_SCALE;
    }
}
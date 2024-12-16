using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using OpenGL_Game.Objects;

namespace OpenGL_Game.Components
{
    internal class ComponentSphereCollider : IComponent
    {
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }

        public ComponentSphereCollider(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_COLLIDER;
    }

    internal class ComponentAABB : IComponent
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public ComponentAABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_COLLIDER;
    }

    internal class ComponentCollider
    {
        public static bool CheckCollision(ComponentSphereCollider sphere1, ComponentSphereCollider sphere2)
        {
            float distance = Vector3.Distance(sphere1.Center, sphere2.Center);
            return distance < (sphere1.Radius + sphere2.Radius);
        }

        public static bool CheckCollision(Vector3 point, ComponentAABB aabb)
        {
            return (point.X >= aabb.Min.X && point.X <= aabb.Max.X) &&
                   (point.Y >= aabb.Min.Y && point.Y <= aabb.Max.Y) &&
                   (point.Z >= aabb.Min.Z && point.Z <= aabb.Max.Z);
        }

        public static void CheckCollisions(Camera camera, List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var sphereCollider = entity.GetComponent<ComponentSphereCollider>();
                if (sphereCollider != null)
                {
                    //if (CheckCollision(camera.cameraPosition, sphereCollider))
                    //{
                    //    Console.WriteLine($"Collision detected between Camera and Entity {entity.Name}");
                    //}
                }
            }
        }

        public static void CheckEntityCollisions(List<Entity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var sphereCollider1 = entities[i].GetComponent<ComponentSphereCollider>();
                if (sphereCollider1 == null) continue;

                for (int j = i + 1; j < entities.Count; j++)
                {
                    var sphereCollider2 = entities[j].GetComponent<ComponentSphereCollider>();
                    if (sphereCollider2 == null) continue;

                    if (CheckCollision(sphereCollider1, sphereCollider2))
                    {
                        Console.WriteLine($"Collision detected between Entity {entities[i].Name} and Entity {entities[j].Name}");
                    }
                }
            }
        }
    }
}

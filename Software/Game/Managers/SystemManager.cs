using System.Collections.Generic;
using OpenGL_Game.Systems;
using OpenGL_Game.Objects;
using System.Linq;

namespace OpenGL_Game.Managers
{
    class SystemManager
    {
        List<ISystem> systemList = new List<ISystem>();

        public SystemManager()
        {
        }

        public void ActionSystems(EntityManager entityManager)
        {
            List<Entity> entityList = entityManager.Entities();
            // Work on a copy so that removals don’t affect our iteration
            List<Entity> entitiesCopy = entityList.ToList();

            foreach (ISystem system in systemList)
            {
                foreach (Entity entity in entitiesCopy)
                {
                    system.OnAction(entity);
                }
                // If this system is the collision system, remove any entities it flagged
                if (system is SystemCollision collisionSystem)
                {
                    foreach (var e in collisionSystem.EntitiesToRemove)
                    {
                        entityList.Remove(e);
                    }
                    collisionSystem.EntitiesToRemove.Clear();
                }
                // Update the copy after removals for the next system
                entitiesCopy = entityList.ToList();
            }
        }

        public void AddSystem(ISystem system)
        {
            systemList.Add(system);
        }

        private ISystem FindSystem(string name)
        {
            return systemList.Find(system => system.Name == name);
        }
    }
}
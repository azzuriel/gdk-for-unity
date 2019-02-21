using System;
using System.Collections.Generic;
using Improbable.Gdk.Core.Commands;
using Improbable.Worker.CInterop;

namespace Improbable.Gdk.Core
{
    public class ViewDiff
    {
        public bool Disconnected { get; private set; }

        public string DisconnectMessage;

        private readonly HashSet<EntityId> entitiesAdded = new HashSet<EntityId>();
        private readonly HashSet<EntityId> entitiesRemoved = new HashSet<EntityId>();

        private readonly Dictionary<uint, IComponentDiffStorage> componentIdToComponentStorage =
            new Dictionary<uint, IComponentDiffStorage>();

        private readonly List<IComponentDiffStorage> componentStorageList = new List<IComponentDiffStorage>();

        private readonly Dictionary<uint, Dictionary<uint, ICommandDiffStorage>> componentIdToCommandIdToStorage =
            new Dictionary<uint, Dictionary<uint, ICommandDiffStorage>>();

        private Dictionary<Type, IComponentDiffStorage> typeToComponentStorage =
            new Dictionary<Type, IComponentDiffStorage>();

        private readonly List<ICommandDiffStorage> commandStorageList = new List<ICommandDiffStorage>();

        private readonly WorldCommandStorage worldCommandStorage = new WorldCommandStorage();

        public ViewDiff()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IComponentDiffStorage).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var instance = (IComponentDiffStorage) Activator.CreateInstance(type);

                        componentStorageList.Add(instance);
                        componentIdToComponentStorage.Add(instance.GetComponentId(), instance);

                        typeToComponentStorage.Add(instance.GetUpdateType(), instance);
                        foreach (var eventType in instance.GetEventTypes())
                        {
                            typeToComponentStorage.Add(eventType, instance);
                        }
                    }

                    if (typeof(ICommandDiffStorage).IsAssignableFrom(type) && !type.IsAbstract)
                    {
                        var instance = (ICommandDiffStorage) Activator.CreateInstance(type);

                        commandStorageList.Add(instance);
                        if (!componentIdToCommandIdToStorage.TryGetValue(instance.GetComponentId(),
                            out var commandIdToStorage))
                        {
                            commandIdToStorage = new Dictionary<uint, ICommandDiffStorage>();
                            componentIdToCommandIdToStorage.Add(instance.GetComponentId(), commandIdToStorage);
                        }

                        commandIdToStorage.Add(instance.GetCommandId(), instance);
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var storage in componentStorageList)
            {
                storage.Clear();
            }

            foreach (var storage in commandStorageList)
            {
                storage.Clear();
            }

            worldCommandStorage.Clear();
            entitiesAdded.Clear();
            entitiesRemoved.Clear();
        }

        public void AddEntity(long entityId)
        {
            if (!entitiesRemoved.Remove(new EntityId(entityId)))
            {
                entitiesAdded.Add(new EntityId(entityId));
            }
        }

        public void RemoveEntity(long entityId)
        {
            if (!entitiesAdded.Remove(new EntityId(entityId)))
            {
                entitiesRemoved.Add(new EntityId(entityId));
            }
        }

        public void AddComponent<T>(T component, long entityId, uint componentId) where T : ISpatialComponentUpdate
        {
            if (!componentIdToComponentStorage.TryGetValue(componentId, out var storage))
            {
                throw new ArgumentException(
                    $"Can not add component with ID {componentId} on entity with ID {entityId}. " +
                    $"Unknown component ID");
            }

            ((IDiffComponentAddedStorage<T>) storage).AddEntityComponent(entityId, component);
        }

        public void RemoveComponent(long entityId, uint componentId)
        {
            if (!componentIdToComponentStorage.TryGetValue(componentId, out var storage))
            {
                throw new ArgumentException(
                    $"Can not set remove component with ID {componentId} from entity with ID {entityId}. " +
                    $"Unknown component ID");
            }

            storage.RemoveEntityComponent(entityId);
        }

        public void SetAuthority(long entityId, uint componentId, Authority authority)
        {
            if (!componentIdToComponentStorage.TryGetValue(componentId, out var authorityStorage))
            {
                throw new ArgumentException(
                    $"Can not set authority over component with ID {componentId} for entity with ID {entityId}. " +
                    $"Unknown component ID");
            }

            ((IDiffAuthorityStorage) authorityStorage).AddAuthorityChange(
                new AuthorityChangeReceived(authority, new EntityId(entityId)));

            // Remove received command requests if authority has been lost
            if (authority == Authority.NotAuthoritative)
            {
                if (componentIdToCommandIdToStorage.TryGetValue(componentId, out var commandIdToStorage))
                {
                    foreach (var storage in commandIdToStorage)
                    {
                        storage.Value.RemoveRequests(entityId);
                    }
                }
            }
        }

        public void AddComponentUpdate<T>(T update, long entityId, uint componentId, uint updateId)
            where T : ISpatialComponentUpdate
        {
            if (!componentIdToComponentStorage.TryGetValue(componentId, out var storage))
            {
                throw new ArgumentException(
                    $"Can not update component with ID {componentId} on entity with ID {entityId}. " +
                    $"Unknown component ID");
            }

            ((IDiffUpdateStorage<T>) storage).AddUpdate(new ComponentUpdateReceived<T>(update, new EntityId(entityId),
                updateId));
        }

        public void AddEvent<T>(T ev, long entityId, uint componentId, uint updateId) where T : IEvent
        {
            if (!componentIdToComponentStorage.TryGetValue(componentId, out var storage))
            {
                throw new ArgumentException(
                    $"Can not add event from component with ID {componentId} on entity with ID {entityId}. " +
                    $"Unknown component ID");
            }

            ((IDiffEventStorage<T>) storage).AddEvent(new ComponentEventReceived<T>(ev, new EntityId(entityId),
                updateId));
        }

        public void AddCommandRequest<T>(T request, uint componentId, uint commandId) where T : IReceivedCommandRequest
        {
            if (!componentIdToCommandIdToStorage.TryGetValue(componentId, out var commandIdToStorage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown component ID {componentId}");
            }

            if (!commandIdToStorage.TryGetValue(commandId, out var storage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown command ID {commandId}");
            }

            ((IDiffCommandRequestStorage<T>) storage).AddRequest(request);
        }

        public void AddCommandResponse<T>(T response, uint componentId, uint commandId)
            where T : IRawReceivedCommandResponse
        {
            if (!componentIdToCommandIdToStorage.TryGetValue(componentId, out var commandIdToStorage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown component ID {componentId}");
            }

            if (!commandIdToStorage.TryGetValue(commandId, out var storage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown command ID {commandId}");
            }

            ((IDiffCommandResponseStorage<T>) storage).AddResponse(response);
        }

        public void AddCreateEntityResponse(CreateEntityResponseOp response)
        {
            worldCommandStorage.AddResponse(response);
        }

        public void AddDeleteEntityResponse(DeleteEntityResponseOp response)
        {
            worldCommandStorage.AddResponse(response);
        }

        public void AddReserveEntityIdsResponse(ReserveEntityIdsResponseOp response)
        {
            worldCommandStorage.AddResponse(response);
        }

        public void AddEntityQueryResponse(EntityQueryResponseOp response)
        {
            worldCommandStorage.AddResponse(response);
        }

        public void Disconnect(string message)
        {
            Disconnected = true;
            DisconnectMessage = message;
        }

        internal IComponentDiffStorage GetComponentDiffStorage(uint componentId)
        {
            if (!componentIdToComponentStorage.TryGetValue(componentId, out var storage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown component ID {componentId}");
            }

            return storage;
        }

        internal IComponentDiffStorage GetComponentDiffStorage(Type type)
        {
            if (!typeToComponentStorage.TryGetValue(type, out var storage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown type {type.FullName}");
            }

            return storage;
        }

        internal ICommandDiffStorage GetCommandDiffStorage(uint componentId, uint commandId)
        {
            if (!componentIdToCommandIdToStorage.TryGetValue(componentId, out var commandIdToStorage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown component ID {componentId}");
            }

            if (!commandIdToStorage.TryGetValue(commandId, out var storage))
            {
                throw new ArgumentException($"Can not find component diff storage. Unknown command ID {commandId}");
            }

            return storage;
        }

        internal WorldCommandStorage GetWorldCommandStorage()
        {
            return worldCommandStorage;
        }

        internal HashSet<EntityId> GetEntitiesAdded()
        {
            return entitiesAdded;
        }

        internal HashSet<EntityId> GetEntitiesRemoved()
        {
            return entitiesRemoved;
        }
    }
}
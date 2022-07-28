using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup), OrderFirst = true)]
public partial class GoInClientSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<SpawnerPaddles>();
        RequireForUpdate(GetEntityQuery(
            ComponentType.ReadOnly<NetworkIdComponent>(),
            ComponentType.Exclude<NetworkStreamInGame>()
        ));
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = new(Allocator.Temp);

        Entities
            .WithBurst()
            .WithName("Paddles")
            .WithNone<NetworkStreamInGame>()
            .ForEach((Entity entity, in NetworkIdComponent id) =>
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(entity);
                Entity request = commandBuffer.CreateEntity();
                commandBuffer.AddComponent<GoInGameRequest>(request);
                commandBuffer.AddComponent(request, new SendRpcCommandRequestComponent { TargetConnection = entity });
            }).Run();
        commandBuffer.Playback(EntityManager);
        
    }
}

using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public partial class GoInServerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<SpawnerPaddles>();
        RequireForUpdate(GetEntityQuery(
            ComponentType.ReadOnly<GoInGameRequest>(),
            ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()
        ));
    }

    private void CreatePaddle(Entity prefabPaddle, float3 startPosition)
    {
        ComponentDataFromEntity<NetworkIdComponent> networkIdFromEntity =
            GetComponentDataFromEntity<NetworkIdComponent>(true);
        
        EntityCommandBuffer commandBuffer = new(Allocator.Temp);
        Entities
            .WithBurst()
            .WithAll<GoInGameRequest>()
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity reqEntity, in ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
                Entity paddle = commandBuffer.Instantiate(prefabPaddle);
                
                commandBuffer.SetComponent(paddle,
                    new GhostOwnerComponent { NetworkId = networkIdFromEntity[reqSrc.SourceConnection].Value });
                commandBuffer.SetComponent(paddle, new Translation { Value = startPosition });
                commandBuffer.AddBuffer<PlayerInput>(paddle);
                commandBuffer.DestroyEntity(reqEntity);
            }).Run();
        commandBuffer.Playback(EntityManager);
    }
    
    protected override void OnUpdate()
    {
        CreatePaddle(GetSingleton<SpawnerPaddles>().LeftPaddle, new float3(-17, 2.5f, 0));
        
    }
}
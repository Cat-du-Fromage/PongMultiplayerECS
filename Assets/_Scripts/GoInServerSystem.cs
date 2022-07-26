using System;
using Unity.Collections;
using Unity.Entities;
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

    protected override void OnUpdate()
    {
        Entity prefabLeftPaddle = GetSingleton<SpawnerPaddles>().LeftPaddle;
        Entity prefabRightPaddle = GetSingleton<SpawnerPaddles>().RightPaddle;

        ComponentDataFromEntity<NetworkIdComponent> networkIdFromEntity =
            GetComponentDataFromEntity<NetworkIdComponent>(true);

        EntityCommandBuffer commandBuffer = new(Allocator.Temp);
        Entities
            .WithBurst()
            .WithAll<GoInGameRequest>()
            .WithNone<SendRpcCommandRequestComponent>()
            .ForEach((Entity entity, in ReceiveRpcCommandRequestComponent reqSrc) =>
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
                Debug.Log($"req source {networkIdFromEntity[reqSrc.SourceConnection].Value} plugged to in game");
                
            }).Run();
    }
}
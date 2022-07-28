using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    private ClientSimulationSystemGroup clientSimulationSystem;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        clientSimulationSystem = World.GetExistingSystem<ClientSimulationSystemGroup>();
        base.OnCreate();
    }

    private bool InitializePaddle(CommandTargetComponent targetComponent)
    {
        if (targetComponent.targetEntity != Entity.Null) return false;

        EntityCommandBuffer commandBuffer = new(Allocator.Temp);
        int localPlayerId = GetSingleton<NetworkIdComponent>().Value;
        Entity targetEntity = GetSingletonEntity<CommandTargetComponent>();

        Entities
            .WithBurst()
            .WithAll<PaddleTag>()
            .WithNone<PlayerInput>()
            .ForEach((Entity reqEntity, in GhostOwnerComponent ghostOwner) =>
            {
                if (ghostOwner.NetworkId != localPlayerId) return;
                
                commandBuffer.AddBuffer<PlayerInput>(reqEntity);
                commandBuffer.SetComponent(targetEntity, new CommandTargetComponent { targetEntity = reqEntity});
            })
            .Run();
        commandBuffer.Playback(EntityManager);

        return true;
    }


    protected override void OnUpdate()
    {
        CommandTargetComponent paddle = GetSingleton<CommandTargetComponent>();
        if ( InitializePaddle(paddle)) return;
        
        PlayerInput input = default;
        input.Tick = clientSimulationSystem.ServerTick;
        if (Keyboard.current.sKey.isPressed)
            input.Vertical -= 1;
        if (Keyboard.current.wKey.isPressed)
            input.Vertical += 1;
        
        DynamicBuffer<PlayerInput> inputBuffer = EntityManager.GetBuffer<PlayerInput>(paddle.targetEntity);
        inputBuffer.AddCommandData(input);
    }
}
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public partial class PlayerMoveSystem : SystemBase
{
    private GhostPredictionSystemGroup ghostPredictionSystemGroup;
    protected override void OnCreate()
    {
        ghostPredictionSystemGroup = World.GetExistingSystem<GhostPredictionSystemGroup>();
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        uint tick = ghostPredictionSystemGroup.PredictingTick;
        float deltatime = Time.DeltaTime;

        Entities
            .WithBurst()
            .ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation translation,
                in PredictedGhostComponent prediction) =>
            {
                if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction)) return;

                PlayerInput input;
                inputBuffer.GetDataAtTick(tick, out input);
                if (input.Vertical > 0) translation.Value.y += deltatime;
                else if (input.Vertical < 0) translation.Value.y -= deltatime;
            })
            .ScheduleParallel();
    }
}
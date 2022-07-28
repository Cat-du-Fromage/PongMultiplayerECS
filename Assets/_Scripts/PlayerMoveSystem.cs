using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using static Unity.Mathematics.math;

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

        float mapSizeY = 20f;
        float paddleSizeY = 5f;
        float bottomLimit = paddleSizeY * 0.5f;
        float topLimit = mapSizeY - (paddleSizeY * 0.5f);

        Entities
            .WithBurst()
            .ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation translation,
                in PredictedGhostComponent prediction) =>
            {
                if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction)) return;

                inputBuffer.GetDataAtTick(tick, out PlayerInput input);

                float calculatedPosition = translation.Value.y + input.Vertical * (deltatime * 10);
                translation.Value.y = clamp(calculatedPosition, bottomLimit, topLimit);
            })
            .ScheduleParallel();
    }
}
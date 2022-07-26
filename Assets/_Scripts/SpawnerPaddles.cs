using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpawnerPaddles : IComponentData
{
    public Entity LeftPaddle;
    public Entity RightPaddle;
}

using Unity.Entities;

[GenerateAuthoringComponent]
public class SpawnerPaddles : IComponentData
{
    public Entity LeftPaddle;
    public Entity RightPaddle;
    
}

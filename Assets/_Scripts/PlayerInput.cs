using Unity.NetCode;

public struct PlayerInput : ICommandData
{
    public uint Tick { get; set; }
    public int Vertical;
}
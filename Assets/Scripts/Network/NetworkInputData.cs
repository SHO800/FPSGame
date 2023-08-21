using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 CameraDirection;
    public Vector3 MoveDirection;

    public bool IsSprint;
    public bool IsJump;
    public bool IsAction;
}

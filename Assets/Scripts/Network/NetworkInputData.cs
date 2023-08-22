using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSE_BUTTON1 = 0x01;
    public const byte MOUSE_BUTTON2 = 0x02;
    public Vector3 CameraDirection;
    public Vector3 MoveDirection;

    public byte MouseButtons;
    public bool IsSprint;
    public bool IsJump;
    public bool IsAction;
    public bool IsReload;
}

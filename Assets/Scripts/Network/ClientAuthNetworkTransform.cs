using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;
[DisallowMultipleComponent]

public class ClientAuthTranform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }


}

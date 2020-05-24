using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Environment
{
    private static float _timeSpeed = 3f;

    public static float TimeSpeed { get => _timeSpeed; set { _timeSpeed = value; } }

    private static int _numberColumns = 10;

    public static int NumberColumns { get => _numberColumns; set { _numberColumns = value; } }
}

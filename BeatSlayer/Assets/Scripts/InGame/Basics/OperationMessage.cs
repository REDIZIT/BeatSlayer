﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OperationMessage
{
    public enum OperationType
    {
        Fail, Warning, Success
    }
    public OperationType Type { get; set; }
    public string Message { get; set; }

    public OperationMessage() {}
    public OperationMessage(OperationType type)
    {
        Type = type;
    }
    public OperationMessage(OperationType type, string message)
    {
        Type = type;
        Message = message;
    }
}
using System;

public class OperationResult
{
    public OperationState state;

    public object obj;

    public Exception Exception { get { return (Exception)obj; } }
}
public enum OperationState
{
    Success,
    Fail
}
public struct DevConsoleCommandResult
{
    public bool success;
    public string message;

    public DevConsoleCommandResult(bool success, string message)
    {
        this.success = success;
        this.message = message;
    }
}
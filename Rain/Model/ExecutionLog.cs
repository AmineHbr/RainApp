namespace Rain.Model
{
    public class ExecutionLog
    {
        public Guid Id { get; set; }

        public DateTime? ExecutionDate { get; set; }

        public ExecutionType Type { get; set; }

        public string Params { get; set; }

        public Status Status { get; set; }
    }

    public enum ExecutionType
    {
        Automatic,
        Manual
    }

    public enum Status
    {
        Scheduled,
        InProgress,
        Succeded,
        Failed,
        Aborted
    }
}

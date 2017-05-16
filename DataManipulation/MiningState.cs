namespace BookRecommender.DataManipulation{

    /// <summary>
    /// Mining states that represent current state of mining operation.
    /// </summary>
    public enum MiningStateType
        {
            Completed,
            Error,
            Running,
            RunningSavingToDatabase,
            RunningQueryingEndpoint,
            Started,
            Waiting,
            NotRunning


        }

        /// <summary>
        /// Represent the current state of the operation object
        /// </summary>
        public class MiningState
        {
            public MiningStateType CurrentState { get; set; } = MiningStateType.NotRunning;
            public string Message { get; set; }
            public int Count { get; set; } = 0;
            public int CurrentPosition { get; set; } = 0;
            public bool HasFinished(){
                return CurrentState == MiningStateType.Completed || CurrentState == MiningStateType.Error;
            }
            public bool IsInactive(){
                return  CurrentState == MiningStateType.Completed ||
                        CurrentState == MiningStateType.Error ||
                        CurrentState == MiningStateType.NotRunning;
            }   
        }
}
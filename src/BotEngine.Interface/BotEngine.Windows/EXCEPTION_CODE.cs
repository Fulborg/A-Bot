namespace BotEngine.Windows
{
	public enum EXCEPTION_CODE : uint
	{
		EXCEPTION_ACCESS_VIOLATION = 3221225477u,
		EXCEPTION_DATATYPE_MISALIGNMENT = 2147483650u,
		EXCEPTION_BREAKPOINT = 2147483651u,
		EXCEPTION_SINGLE_STEP = 2147483652u,
		EXCEPTION_ARRAY_BOUNDS_EXCEEDED = 3221225612u,
		EXCEPTION_INT_DIVIDE_BY_ZERO = 3221225620u,
		EXCEPTION_ILLEGAL_INSTRUCTION = 3221225501u,
		EXCEPTION_STACK_OVERFLOW = 3221225725u,
		EXCEPTION_INVALID_HANDLE = 3221225480u
	}
}

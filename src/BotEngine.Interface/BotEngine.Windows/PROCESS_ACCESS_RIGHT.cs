namespace BotEngine.Windows
{
	public enum PROCESS_ACCESS_RIGHT
	{
		PROCESS_CREATE_PROCESS = 0x80,
		PROCESS_CREATE_THREAD = 2,
		PROCESS_DUP_HANDLE = 0x40,
		PROCESS_QUERY_INFORMATION = 0x400,
		PROCESS_SET_INFORMATION = 0x200,
		PROCESS_SET_QUOTA = 0x100,
		PROCESS_SUSPEND_RESUME = 0x800,
		PROCESS_TERMINATE = 1,
		PROCESS_VM_OPERATION = 8,
		PROCESS_VM_READ = 0x10,
		PROCESS_VM_WRITE = 0x20,
		SYNCHRONIZE = 0x100000
	}
}

using System.Runtime.InteropServices;

namespace BotEngine.Windows
{
	[StructLayout(LayoutKind.Explicit)]
	public struct DEBUG_EVENT
	{
		[FieldOffset(0)]
		public DEBUG_EVENT_CODE dwDebugEventCode;

		[FieldOffset(4)]
		public uint dwProcessId;

		[FieldOffset(8)]
		public uint dwThreadId;

		[FieldOffset(12)]
		public EXCEPTION_DEBUG_INFO Exception;

		[FieldOffset(12)]
		public CREATE_THREAD_DEBUG_INFO CreateThread;

		[FieldOffset(12)]
		public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo;

		[FieldOffset(12)]
		public EXIT_THREAD_DEBUG_INFO ExitThread;

		[FieldOffset(12)]
		public EXIT_PROCESS_DEBUG_INFO ExitProcess;

		[FieldOffset(12)]
		public LOAD_DLL_DEBUG_INFO LoadDll;

		[FieldOffset(12)]
		public UNLOAD_DLL_DEBUG_INFO UnloadDll;

		[FieldOffset(12)]
		public OUTPUT_DEBUG_STRING_INFO DebugString;

		[FieldOffset(12)]
		public RIP_INFO RipInfo;
	}
}

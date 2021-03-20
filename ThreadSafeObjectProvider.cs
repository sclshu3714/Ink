//----------------------------------------------------------------------------------------
//	Copyright © 2003 - 2021 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class replicates the VB compiler-generated ThreadSafeObjectProvider class.
//----------------------------------------------------------------------------------------
internal sealed class ThreadSafeObjectProvider<T> where T : new()
{
	[System.ThreadStaticAttribute]
	private static T instance = System.Activator.CreateInstance<T>();

	static ThreadSafeObjectProvider()
	{
	}

	public T GetInstance()
	{
		return instance;
	}
}
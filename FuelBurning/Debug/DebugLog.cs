using System.Diagnostics;

namespace FuelBurning
{
    class DebugLog
    {
        public static string GetMethodName(int stackcount = 1)
        {
            StackFrame stack = new StackFrame(stackcount);
            return stack.GetMethod().ReflectedType.FullName + "." + stack.GetMethod().Name + "(): ";
        }
        public static string Sign()
        {
            return "[" + typeof(DebugLog).Namespace + "]:";
        }
    }
}

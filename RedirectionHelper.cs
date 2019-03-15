/*
The MIT License (MIT)

Copyright (c) 2015 Sebastian Schöner

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MoreEffectiveTransfer
{
    /// <summary>
    /// Helper class to deal with detours. This version is for Unity 5 x64 on Windows.
    /// We provide three different methods of detouring.
    /// </summary>
    /// 
    public struct RedirectCallsState
    {
        public byte a;

        public byte b;

        public byte c;

        public byte d;

        public byte e;

        public ulong f;
    }
    /// <summary>
    /// Helper class to deal with detours. This version is for Unity 5 x64 on Windows.
    /// We provide three different methods of detouring.
    /// </summary>
    public static class RedirectionHelper
    {
        /// <summary>
        /// Redirects all calls from method 'from' to method 'to'.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static RedirectCallsState RedirectCalls(MethodInfo from, MethodInfo to)
        {
            // GetFunctionPointer enforces compilation of the method.
            var fptr1 = from.MethodHandle.GetFunctionPointer();
            var fptr2 = to.MethodHandle.GetFunctionPointer();
            return PatchJumpTo(fptr1, fptr2);
        }

        public static void RevertRedirect(MethodInfo from, RedirectCallsState state)
        {
            var fptr1 = from.MethodHandle.GetFunctionPointer();
            DebugLog.LogToFileOnly("Revert Patching from " + fptr1 + " to " + state);
            RevertJumpTo(fptr1, state);
        }


        private static bool IsRedirected(IntPtr site, IntPtr target)
        {
            unsafe
            {
                byte* sitePtr = (byte*)site.ToPointer();
                return *sitePtr == 0x49 &&
                    *(sitePtr + 1) == 0xBB &&
                    *((ulong*)(sitePtr + 2)) == (ulong)target.ToInt64() &&
                    *(sitePtr + 10) == 0x41 &&
                    *(sitePtr + 11) == 0xFF &&
                    *(sitePtr + 12) == 0xE3;
            }
        }


        public static bool IsRedirected(MethodInfo from, MethodInfo to)
        {
            var fptr1 = from.MethodHandle.GetFunctionPointer();
            var fptr2 = to.MethodHandle.GetFunctionPointer();
            return IsRedirected(fptr1, fptr2);
        }

        /// <summary>
        /// Primitive patching. Inserts a jump to 'target' at 'site'. Works even if both methods'
        /// callers have already been compiled.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="target"></param>
        private static RedirectCallsState PatchJumpTo(IntPtr site, IntPtr target)
        {
            // R11 is volatile.
            RedirectCallsState state = new RedirectCallsState();
            unsafe
            {
                byte* sitePtr = (byte*)site.ToPointer();
                state.a = *sitePtr;
                state.b = *(sitePtr + 1);
                state.c = *(sitePtr + 10);
                state.d = *(sitePtr + 11);
                state.e = *(sitePtr + 12);
                state.f = *((ulong*)(sitePtr + 2));
                *sitePtr = 0x49; // mov r11, target
                *(sitePtr + 1) = 0xBB;
                *((ulong*)(sitePtr + 2)) = (ulong)target.ToInt64();
                *(sitePtr + 10) = 0x41; // jmp r11
                *(sitePtr + 11) = 0xFF;
                *(sitePtr + 12) = 0xE3;
            }

            return state;
        }


        private unsafe static void RevertJumpTo(IntPtr site, RedirectCallsState state)
        {
            byte* ptr = (byte*)site.ToPointer();
            *ptr = state.a;
            ptr[1] = state.b;
            *(long*)(ptr + 2) = (long)state.f;
            ptr[10] = state.c;
            ptr[11] = state.d;
            ptr[12] = state.e;
        }
    }
}
